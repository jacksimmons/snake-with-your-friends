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
    public short x, y;
    public ETileType type;


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
    public byte objId;

    public short x;
    public short y;
    public float rotation;

    public int spawnIndex;


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
    public MapTileData[] groundData;
    public MapTileData[] wallData;
    public MapObjectData[] objectData;
    public int bgIndex;


    public MapData(MapTileData[] groundData, MapTileData[] wallData, MapObjectData[] objectData, int bgIndex)
    {
        this.groundData = groundData;
        this.wallData = wallData;
        this.objectData = objectData;
        this.bgIndex = bgIndex;
    }
}