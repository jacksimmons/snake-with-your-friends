using Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameBehaviour : MonoBehaviour
{
	public static bool loaded = false;

	public Tilemap groundTilemap;
	public Tilemap wallTilemap;

	[SerializeField]
	private Tile _lightTile;
	[SerializeField]
	private Tile _darkTile;
	[SerializeField]
	private Tile _wallTile;

	[SerializeField]
	private GameObject[] _players;

	private Food food;

	private const int WORLD_SIZE = 25; // The world size is a square, WORLD_SIZE * WORLD_SIZE

	// Soft limit is preferred, but if it is too small, the hard limit is used (1 tile).
	// The minimum ratio between the distance between two snakes, and the WORLD_SIZE, before an inner square must be established.
	private const float SOFT_MIN_DIST_WORLD_SIZE_RATIO = 0.2f;
	private const float HARD_MIN_DIST = 10f;

	void Awake()
	{
		// Create the tilemaps
		groundTilemap = CreateAndReturnTilemap(gridName: "Ground", hasCollider: false);
		wallTilemap = CreateAndReturnTilemap(gridName: "Wall", hasCollider: true);
	}

	void Start()
	{
		CreateGroundTilemap();
		CreateWallTilemap();
		PlaceSnakes(1, _players);

		loaded = true;
	}

	Tilemap CreateAndReturnTilemap(string gridName, bool hasCollider)
	{
		GameObject gridObject = new GameObject(gridName);
		gridObject.AddComponent<Grid>();

		GameObject tilemapObject = new GameObject("Tilemap");
		tilemapObject.AddComponent<Tilemap>();
		tilemapObject.AddComponent<TilemapRenderer>();

		if (hasCollider)
			tilemapObject.AddComponent<TilemapCollider2D>();

		tilemapObject.transform.parent = gridObject.transform;

		Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();

		return tilemap;
	}

	void CreateGroundTilemap()
	{
		// Bounds are an inner square of the 51x51 wall bounds starting at 0,0
		BoundsInt bounds = new BoundsInt(new Vector3Int(1, 1, 0), new Vector3Int(WORLD_SIZE, WORLD_SIZE, 1));
		Tile[] tiles = new Tile[WORLD_SIZE * WORLD_SIZE];
		for (int i = 0; i < WORLD_SIZE; i++)
		{
			for (int j = 0; j < WORLD_SIZE; j++)
			{
				if (i % 2 == 0)
				{
					// Even row -> starts with light (i.e. Even cols are light)
					if (j % 2 == 0)
						tiles[WORLD_SIZE * i + j] = _lightTile;
					else
						tiles[WORLD_SIZE * i + j] = _darkTile;
				}
				else
				{
					// Odd row -> starts with dark (i.e. Odd cols are light)
					if (j % 2 == 0)
						tiles[WORLD_SIZE * i + j] = _darkTile;
					else
						tiles[WORLD_SIZE * i + j] = _lightTile;
				}
			}
		}

		groundTilemap.SetTilesBlock(bounds, tiles);
	}

	void CreateWallTilemap()
	{
		// This square is WORLD_SIZE + 2 squared, since it is one bigger on each side of the x and y edges of the inner square
		BoundsInt bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(WORLD_SIZE + 2, WORLD_SIZE + 2, 1));
		Tile[] tiles = new Tile[(WORLD_SIZE + 2) * (WORLD_SIZE + 2)];
		for (int i = 0; i < WORLD_SIZE + 2; i++)
		{
			for (int j = 0; j < WORLD_SIZE + 2; j++)
			{
				if (i == 0 || i == WORLD_SIZE + 1)
				{
					// We are on the top or bottom row, so guaranteed placement of wall
					tiles[(WORLD_SIZE + 2) * i + j] = _wallTile;
				}
				else if (j == 0 || j == WORLD_SIZE + 1)
				{
					// We are on the leftmost or rightmost column, so place wall
					tiles[(WORLD_SIZE + 2) * i + j] = _wallTile;
				}
			}
		}

		wallTilemap.SetTilesBlock(bounds, tiles);
	}

	void PlaceSnakes(int depth, GameObject[] remainingPlayers)
	{
		// Outer snakes (along the walls)
		// Calculate the maximum distance between snakes.
		// If this distance is too small, spawn inner snakes.

		float minDist = WORLD_SIZE * SOFT_MIN_DIST_WORLD_SIZE_RATIO;
		if (minDist < HARD_MIN_DIST)
			minDist = HARD_MIN_DIST;

		Vector3 BL = groundTilemap.CellToWorld(new Vector3Int(depth + 1, depth + 1, 0));
		Vector3 BR = groundTilemap.CellToWorld(new Vector3Int(WORLD_SIZE - depth + 1, depth + 1, 0));
		Vector3 TL = groundTilemap.CellToWorld(new Vector3Int(depth + 1, WORLD_SIZE - depth + 1, 0));
		Vector3 TR = groundTilemap.CellToWorld(new Vector3Int(WORLD_SIZE - depth + 1, WORLD_SIZE - depth + 1, 0));

		Vector3[] corners = { BL, BR, TL, TR };

		for (int i = 0; i < remainingPlayers.Length; i++)
		{
			_players[i].transform.position = corners[i % 4] + (Vector3)(Vector2.one * groundTilemap.cellSize / 2);
			if (i % 4 == 0 && i < remainingPlayers.Length - 1)
			{
				int newDepth = depth + (int)Mathf.Floor(minDist);
				print(newDepth);
				if (newDepth >= WORLD_SIZE / 2)
				{
					throw new System.Exception("The players do not fit in the map provided.");
				}
				else
				{
					PlaceSnakes(newDepth, Arrays.SubArray(remainingPlayers, 4));
				}
			}
		}
	}
}
