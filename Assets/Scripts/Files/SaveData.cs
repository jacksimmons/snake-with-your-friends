using System;

[Serializable]
public class SaveData : ICached
{
    public static SaveData Saved = null;
    public static readonly byte MaxPuzzleLevel = 2;

    public byte PuzzleLevel { get; set; }


    public SaveData(SaveData other)
    {
        PuzzleLevel = other.PuzzleLevel;
    }


    public SaveData()
    {
        PuzzleLevel = 1;
    }


    public void Cache()
    {
        Saved = this;
    }
}