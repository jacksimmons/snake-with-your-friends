using Mirror;

public class NetworkGameSettings : NetworkBehaviour
{
    [SyncVar]
    public EGameMode GameMode;
    [SyncVar]
    public int GameSize;
    [SyncVar]
    public float TimeToMove;
    [SyncVar]
    public bool FriendlyFire;

    [SyncVar]
    public bool Received = false;

    public readonly SyncList<EFoodType> DisabledFoods = new();


    public void LoadGameSettings(GameSettings settings)
    {
        GameMode = settings.GameMode;
        GameSize = settings.GameSize;
        TimeToMove = settings.TimeToMove;
        FriendlyFire = settings.FriendlyFire;
        Received = true;

        DisabledFoods.Clear();
        foreach (EFoodType food in settings.DisabledFoods)
        {
            DisabledFoods.Add(food);
        }
    }
}