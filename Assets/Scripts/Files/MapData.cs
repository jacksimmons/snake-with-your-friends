using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


public enum ETileType
{
    LightGround,
    DarkGround,

    Wall
}


[Serializable]
public class MapTileData
{
    public readonly ETileType type;
    public readonly Vector3Int position;


    public MapTileData(TileBase tile, Vector3Int pos)
    {
        position = pos;
    }
}


[Serializable]
public class MapObjectData
{
    public readonly EObjectType objectType;
    public readonly string prefabName;
    public readonly Vector3 position;
    public readonly Quaternion rotation;


    public MapObjectData(ObjectBehaviour obj)
    {
        objectType = obj.Type;
        prefabName = obj.gameObject.name;
        position = obj.transform.position;
    }


    public GameObject ToGameObject()
    {
        string resourcesFolder = "Objects";
        switch (objectType)
        {
            case EObjectType.Food:
                resourcesFolder += "/Food";
                break;
            case EObjectType.Projectile:
                resourcesFolder += "/Projectile";
                break;
        }
        return Resources.Load<GameObject>(resourcesFolder + $"/{prefabName}");
    }
}


[Serializable]
public class MapData
{
    public readonly MapTileData[] groundLayer;
    public readonly MapTileData[] wallLayer;
    public readonly MapObjectData[] objectLayer;


    public MapData(Tilemap groundLayer, Tilemap wallLayer, GameObject objectLayerObj)
    {
        Transform objectLayer = objectLayerObj.transform;

        this.groundLayer = GetAllTiles(groundLayer);
        this.wallLayer = GetAllTiles(wallLayer);
        this.objectLayer = new MapObjectData[objectLayer.childCount];

        for (int i = 0; i < objectLayer.childCount; i++)
        {
            this.objectLayer[i] = new(objectLayer.GetChild(i).GetComponent<ObjectBehaviour>());
        }
    }


    private MapTileData[] GetAllTiles(Tilemap tilemap)
    {
        MapTileData[] tiles = new MapTileData[tilemap.GetTilesBlock(tilemap.cellBounds).Length];
        int iMax = tilemap.cellBounds.yMax - tilemap.cellBounds.yMin;
        int jMax = tilemap.cellBounds.xMax - tilemap.cellBounds.xMin;
        for (int i = 0; i < iMax; i++)
        {
            for (int j = 0; j < jMax; j++)
            {
                tiles[i * jMax + j] = new(null, new(i + tilemap.cellBounds.yMin, j + tilemap.cellBounds.xMin));
            }
        }
        return tiles;
    }
}