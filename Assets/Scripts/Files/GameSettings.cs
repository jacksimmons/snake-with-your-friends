using System;


[Serializable]
public struct GameSettingsData
{
    public EGameMode GameMode;
    public float TimeToMove;
    public bool FriendlyFire;
    public byte[] FoodSettingsData;
    public MapData Map;


    public GameSettingsData(BitField foodSettingsData)
    {
        GameMode = EGameMode.SnakeRoyale;
        TimeToMove = GameSettings.DEFAULT_TIME_TO_MOVE;
        FriendlyFire = GameSettings.DEFAULT_FRIENDLY_FIRE;
        Map = new();

        FoodSettingsData = foodSettingsData.Data;
    }


    public GameSettingsData(EGameMode mode, float timeToMove, bool friendlyFire, BitField foodSettingsData,
        MapData map)
    {
        GameMode = mode;
        TimeToMove = timeToMove;
        FriendlyFire = friendlyFire;
        Map = map;

        FoodSettingsData = foodSettingsData.Data;
    }
}


[Serializable]
public class GameSettings : ICached
{
    public const float DEFAULT_TIME_TO_MOVE = 0.6f;
    public const bool DEFAULT_FRIENDLY_FIRE = true;
    private const int NUM_BYTES = 2;

    public static GameSettings Saved;
    public GameSettingsData Data;
    public BitField FoodSettings;


    public GameSettings()
    {
        Data = new(new(NUM_BYTES));

        // Set FoodSettings.Data to be a reference to the byte[] array in FoodSettingsData.
        FoodSettings = new(Data.FoodSettingsData);

        for (int i = 0; i < Enum.GetValues(typeof(EFoodType)).Length; i++)
        {
            FoodSettings.SetBit(i, true);
        }

    }


    public GameSettings(GameSettings other)
    {
        Data = other.Data;

        // Need to copy all reference types contained in Data
        other.Data.FoodSettingsData.CopyTo(Data.FoodSettingsData, 0);

        // Now link the FoodSettings to the byte[] array in FoodSettingsData.
        FoodSettings = new(Data.FoodSettingsData);
    }


    public GameSettings(GameSettingsData data)
    {
        Data = data;

        // Need to copy all reference types contained in this data
        data.FoodSettingsData.CopyTo(Data.FoodSettingsData, 0);

        // Now link the FoodSettings to the byte[] array in FoodSettingsData.
        FoodSettings = new(Data.FoodSettingsData);
    }


    public void Cache() { Saved = this; }
}