using Extensions;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GameBehaviour : MonoBehaviour
{
	[SerializeField]
	private Tile _lightTile;
	[SerializeField]
	private Tile _darkTile;
	[SerializeField]
	private Tile _wallTile;

	[SerializeField]
	private GameObject[] _players;
	private bool[] _objects;

	[SerializeField]
	private GameObject _foodTemplate;
	[SerializeField]
	private GameObject _menuSelectTemplate;

	public enum e_WorldSize : int
	{
		LOBBY = 10,
		GAME_SMALL = 15,
		GAME_MEDIUM = 30,
		GAME_LARGE = 45
	}
	[SerializeField]
	private e_WorldSize groundSize;

	// Soft limit is preferred, but if it is too small, the hard limit is used (1 tile).
	// The minimum ratio between the distance between two snakes, and the WORLD_SIZE, before an inner square must be established.
	private const float SOFT_MIN_DIST_WORLD_SIZE_RATIO = 0.2f;
	private const float HARD_MIN_DIST = 10f;

	void Start()
	{
		if (groundSize == e_WorldSize.LOBBY)
		{
			Vector2Int custBL = Vector2Int.left * ((int)groundSize + 1);

			// Create the customise area tilemaps
			Tilemap custGT = CreateAndReturnTilemap(gridName: "CustGround", hasCollider: false);
			Tilemap custWT = CreateAndReturnTilemap(gridName: "CustWall", hasCollider: true);

			CreateGroundTilemap(custGT, custBL);
			CreateWallTilemap(custWT, custBL);

			Vector2Int readyBL = Vector2Int.up * ((int)groundSize + 1);

			// Create the ready area tilemaps
			Tilemap readyGT = CreateAndReturnTilemap(gridName: "ReadyGround", hasCollider: false);
			Tilemap readyWT = CreateAndReturnTilemap(gridName: "ReadyWall", hasCollider: true);

			CreateGroundTilemap(readyGT, readyBL);
			CreateWallTilemap(readyWT, readyBL);

			Vector2Int baseBL = Vector2Int.zero;

			// Create the base area tilemaps
			Tilemap baseGT = CreateAndReturnTilemap(gridName: "BaseGround", hasCollider: false);
			Tilemap baseWT = CreateAndReturnTilemap(gridName: "BaseWall", hasCollider: true);

			CreateGroundTilemap(baseGT, baseBL);
			CreateWallTilemap(baseWT, baseBL);

			PlaceSnakes(1, _players, baseGT, baseBL);

			// Create the object array (the size of the map has been defined now)
			_objects = new bool[(int)groundSize * (int)groundSize];

			CreateTeleportingMenuPair(
				"Customise", "Back",
				new Vector3(baseBL.x + 1.5f + ((int)groundSize / 2), baseBL.y + 1.5f, 0),
				new Vector3(custBL.x + 1.5f + ((int)groundSize / 2), custBL.y + (int)groundSize + 0.5f, 0));

			CreateTeleportingMenuPair(
				"Ready", "Unready",
				new Vector3(baseBL.x + 1.5f + ((int)groundSize / 2), baseBL.y + (int)groundSize + 0.5f, 0),
				new Vector3(readyBL.x + 1.5f + ((int)groundSize / 2), readyBL.y + 1.5f, 0));
		}
	}

	Tilemap CreateAndReturnTilemap(string gridName, bool hasCollider)
	{
		GameObject gridObject = new GameObject(gridName);
		gridObject.AddComponent<Grid>();

		GameObject tilemapObject = new GameObject("Tilemap");
		tilemapObject.AddComponent<Tilemap>();
		tilemapObject.AddComponent<TilemapRenderer>();

		if (hasCollider)
		{
			tilemapObject.AddComponent<TilemapCollider2D>();
		}

		tilemapObject.transform.parent = gridObject.transform;

		Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();

		return tilemap;
	}

	void CreateGroundTilemap(Tilemap groundTilemap, Vector2Int bl)
	{
		// Bounds are an inner square of the 51x51 wall bounds starting at 0,0
		BoundsInt bounds = new BoundsInt(
			(Vector3Int)(bl + Vector2Int.one),
			(Vector3Int)((int)groundSize * Vector2Int.one) + Vector3Int.forward);
		Tile[] tiles = new Tile[(int)groundSize * (int)groundSize];
		for (int i = 0; i < (int)groundSize; i++)
		{
			for (int j = 0; j < (int)groundSize; j++)
			{
				if (i % 2 == 0)
				{
					// Even row -> starts with light (i.e. Even cols are light)
					if (j % 2 == 0)
						tiles[(int)groundSize * i + j] = _lightTile;
					else
						tiles[(int)groundSize * i + j] = _darkTile;
				}
				else
				{
					// Odd row -> starts with dark (i.e. Odd cols are light)
					if (j % 2 == 0)
						tiles[(int)groundSize * i + j] = _darkTile;
					else
						tiles[(int)groundSize * i + j] = _lightTile;
				}
			}
		}
		groundTilemap.SetTilesBlock(bounds, tiles);
	}

	void CreateWallTilemap(Tilemap wallTilemap, Vector2Int bl)
	{
		// This square is (int)groundSize + 2 squared, since it is one bigger on each side of the x and y edges of the inner square
		BoundsInt bounds = new BoundsInt(
			(Vector3Int)bl,
			(Vector3Int)(((int)groundSize + 2) * Vector2Int.one) + Vector3Int.forward);
		Tile[] tiles = new Tile[((int)groundSize + 2) * ((int)groundSize + 2)];
		print(bounds.size);
		for (int i = 0; i < (int)groundSize + 2; i++)
		{
			for (int j = 0; j < (int)groundSize + 2; j++)
			{
				if (i == 0 || i == (int)groundSize + 1)
				{
					// We are on the top or bottom row, so guaranteed placement of wall
					tiles[((int)groundSize + 2) * i + j] = _wallTile;
				}
				else if (j == 0 || j == (int)groundSize + 1)
				{
					// We are on the leftmost or rightmost column, so place wall
					tiles[((int)groundSize + 2) * i + j] = _wallTile;
				}
			}
		}

		wallTilemap.SetTilesBlock(bounds, tiles);
	}

	void PlaceSnakes(int depth, GameObject[] remainingPlayers, Tilemap groundTilemap, Vector2Int bl)
	{
		// Outer snakes (along the walls)
		// Calculate the maximum distance between snakes.
		// If this distance is too small, spawn inner snakes.

		float minDist = (int)groundSize * SOFT_MIN_DIST_WORLD_SIZE_RATIO;
		if (minDist < HARD_MIN_DIST)
			minDist = HARD_MIN_DIST;

		Vector3 BL = groundTilemap.CellToWorld((Vector3Int)(bl + (depth + 1) * Vector2Int.one));
		Vector3 BR = groundTilemap.CellToWorld((Vector3Int)(bl + new Vector2Int((int)groundSize - depth + 1, depth + 1)));
		Vector3 TL = groundTilemap.CellToWorld((Vector3Int)(bl + new Vector2Int(depth + 1, (int)groundSize - depth + 1)));
		Vector3 TR = groundTilemap.CellToWorld((Vector3Int)(bl + ((int)groundSize - depth + 1) * Vector2Int.one));

		Vector3[] corners = { BL, BR, TL, TR };

		for (int i = 0; i < remainingPlayers.Length; i++)
		{
			_players[i].transform.position = corners[i % 4] + (Vector3)(Vector2.one * groundTilemap.cellSize / 2);
			if (i % 4 == 0 && i < remainingPlayers.Length - 1)
			{
				int newDepth = depth + (int)Mathf.Floor(minDist);
				print(newDepth);
				if (newDepth >= (int)groundSize / 2)
				{
					throw new System.Exception("The players do not fit in the map provided.");
				}
				else
				{
					PlaceSnakes(newDepth, Arrays.SubArray(remainingPlayers, 4), groundTilemap, bl);
				}
			}
		}
	}

	GameObject CreateObjectOnGrid(Vector2Int gridPos, GameObject obj)
	{
		int index = ((int)groundSize - 1) * gridPos.y + gridPos.x;
		if (!_objects[index])
		{
			_objects[index] = true;
			return Instantiate(obj, (Vector3)(Vector2)gridPos, obj.transform.rotation);
		}
		else
		{
			// Unsuccessful; already an object there
			return null;
		}
	}

	void CreateTeleportingMenuPair(
		string text1, string text2,
		Vector3 from, Vector3 to)
	{
		GameObject menuSelect = Instantiate(_menuSelectTemplate);

		Teleporter teleporter = menuSelect.GetComponentInChildren<Teleporter>();

		teleporter.A.transform.position = from;
		teleporter.A.GetComponentInChildren<TextMeshProUGUI>().text = text1;
		teleporter.B.transform.position = to;
		teleporter.B.GetComponentInChildren<TextMeshProUGUI>().text = text2;
	}
}
