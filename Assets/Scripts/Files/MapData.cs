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
public class MapTileData
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


public enum EObjectType : byte
{
    None, // 0

    Food, // EFoodType
    Projectile, // EProjectileType
}


[Serializable]
public class MapObjectData
{
    public readonly EObjectType type;

    public readonly short x;
    public readonly short y;

    public readonly float rotation;


    public MapObjectData(EObjectType type, short x, short y)
    {
        this.type = type;
        this.x = x;
        this.y = y;
    }
}


[Serializable]
public class MapData
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


    public MapData() { }
}