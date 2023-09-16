using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleBehaviour : GameBehaviour
{
    SaveData data;
    PlayerMovement localPlayerMovement;

    protected override void Start()
    {
        base.Start();
    }


    [Client]
    public override void OnGameSceneLoaded(string name)
    {
        base.OnGameSceneLoaded(name);

        data = Saving.LoadFromFile<SaveData>("SaveData.dat");

        byte puzzleLevel = data.PuzzleLevel;
        if (data.PuzzleLevel > SaveData.MaxPuzzleLevel)
            puzzleLevel = SaveData.MaxPuzzleLevel;

        Instantiate(Resources.Load<GameObject>($"Puzzles/Puzzle{puzzleLevel}"));

        ClientLoadGame();
    }


    [Client]
    protected override void ClientLoadGame()
    {
        base.ClientLoadGame();

        localPlayerMovement = GameObject.Find("LocalPlayerObject").GetComponent<PlayerMovement>();
        localPlayerMovement.enabled = true;

        GameObject cam = GameObject.FindWithTag("MainCamera");
        cam.GetComponent<CamBehaviour>().Player = localPlayerMovement;

        CmdReady();
    }


    [Command]
    protected override void CmdReady()
    {
        PlacePlayers(0, 0, Vector2Int.zero);
    }


    [Server]
    public override void PlacePlayers(int depth, int playersStartIndex, Vector2Int bl)
    {
        Transform puzzleStartPoints = GameObject.FindWithTag("PuzzleStart").transform;

        for (int i = 0; i < localPlayerMovement.BodyParts.Count; i++)
        {
            Transform startPoint = puzzleStartPoints.GetChild(i);
            localPlayerMovement.BodyParts[i].Position = startPoint.position;
            localPlayerMovement.BodyParts[i].RegularAngle = startPoint.rotation.eulerAngles.z;
        }
    }
}