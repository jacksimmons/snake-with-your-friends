using System;

[Serializable]
public class SaveData : ICached
{
    public static SaveData Saved;
    public static readonly byte MaxPuzzleLevel = 1;

    public byte PuzzleLevel { get; set; }


    public SaveData(SaveData other)
    {
        PuzzleLevel = other.PuzzleLevel;
    }


    public SaveData()
    {
        PuzzleLevel = 0;
    }


    public void Cache()
    {
        Saved = this;
    }
}