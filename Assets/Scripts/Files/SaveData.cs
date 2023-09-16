using System;

[Serializable]
public class SaveData
{
    public static SaveData Saved = new();
    public static readonly byte MaxPuzzleLevel = 1;

    public byte PuzzleLevel { get; set; }


    public SaveData()
    {
        PuzzleLevel = 1;
    }
}