using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


public enum ETileType : byte
{
    LightGround,
    DarkGround,

    Wall
}


[Serializable]
public struct MapTileData
{
    public readonly short x, y;
    public readonly ETileType type;


    public MapTileData(short x, short y, ETileType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
    }
}


[Serializable]
public struct MapObjectData
{
    public readonly byte objId;

    public readonly short x;
    public readonly short y;
    public readonly float rotation;

    public readonly int spawnIndex;


    public MapObjectData(byte objId, short x, short y, float rotation, int spawnIndex)
    {
        this.objId = objId;
        this.x = x;
        this.y = y;
        this.rotation = rotation;
        this.spawnIndex = spawnIndex;
    }
}


[Serializable]
public struct MapData
{
    public readonly MapTileData[] groundData;
    public readonly MapTileData[] wallData;
    public readonly MapObjectData[] objectData;
    public readonly int bgIndex;


    public MapData(MapTileData[] groundData, MapTileData[] wallData, MapObjectData[] objectData, int bgIndex)
    {
        this.groundData = groundData;
        this.wallData = wallData;
        this.objectData = objectData;
        this.bgIndex = bgIndex;
    }
}