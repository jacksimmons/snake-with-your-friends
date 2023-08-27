public enum EGameMode
{
    SnakeRoyale,
    SnakeKart,
    HideNSnake
}

public sealed class LobbySettings
{
    private BitField bf = new();

    public bool AppleEnabled
    {
        get { return bf.GetBit(0); }
        set { bf.SetBit(0, value); }
    }
    public bool BaltiEnabled
    {
        get { return bf.GetBit(1); }
        set { bf.SetBit(1, value); }
    }
    public bool BananaEnabled
    {
        get { return bf.GetBit(2); }
        set { bf.SetBit(2, value); }
    }
    public bool BoneAndDrumstickEnabled
    {
        get { return bf.GetBit(3); }
        set { bf.SetBit(3, value); }
    }
    public bool BoozeEnabled
    {
        get { return bf.GetBit(4); }
        set { bf.SetBit(4, value); }
    }
    public bool CheeseEnabled
    {
        get { return bf.GetBit(5); }
        set { bf.SetBit(5, value); }
    }
    public bool CoffeeEnabled
    {
        get { return bf.GetBit(6); }
        set { bf.SetBit(6, value); }
    }
    public bool DoughnutEnabled
    {
        get { return bf.GetBit(7); }
        set { bf.SetBit(7, value); }
    }
    public bool DragonfruitEnabled
    {
        get { return bf.GetBit(8); }
        set { bf.SetBit(8, value); }
    }
    public bool IceCreamEnabled
    {
        get { return bf.GetBit(9); }
        set { bf.SetBit(9, value); }
    }
    public bool OrangeEnabled
    {
        get { return bf.GetBit(10); }
        set { bf.SetBit(10, value); }
    }
    public bool PineappleEnabled
    {
        get { return bf.GetBit(11); }
        set { bf.SetBit(11, value); }
    }
    public bool PizzaEnabled
    {
        get { return bf.GetBit(12); }
        set { bf.SetBit(12, value); }
    }
    public bool PizzaWithPineappleEnabled
    {
        get { return bf.GetBit(13); }
        set { bf.SetBit(13, value); }
    }

    public bool TimeLimit
    {
        get { return bf.GetBit(25); }
        set { bf.SetBit(25, value); }
    }
    public bool Respawning // Only kart
    {
        get { return bf.GetBit(26); }
        set { bf.SetBit(26, value); }
    }
    public bool DestructibleEnvironment
    {
        get { return bf.GetBit(27); }
        set { bf.SetBit(27, value); }
    }
    public bool LeavePlayerRemains
    {
        get { return bf.GetBit(28); }
        set { bf.SetBit(28, value); }
    }
    public bool BouncingProjectiles
    {
        get { return bf.GetBit(29); }
        set { bf.SetBit(29, value); }
    }
    public bool FriendlyFire
    {
        get { return bf.GetBit(30); }
        set { bf.SetBit(30, value); }
    }
    public bool FreeMovement
    {
        get { return bf.GetBit(31); }
        set { bf.SetBit(31, value); }
    }

    private int speed;
    private EGameMode gameMode;

    public LobbySettings()
    {
        //AppleEnabled = true;
        //BaltiEnabled = true;
        //BananaEnabled = true;
        //BoneAndDrumstickEnabled = true;
        //BoozeEnabled = true;
        //CheeseEnabled = true;
        //CoffeeEnabled = true;
        //DoughnutEnabled = true;
        //DragonfruitEnabled = true;
        //IceCreamEnabled = true;
        //OrangeEnabled = true;
        //PineappleEnabled = true;
        //PizzaEnabled = true;
        //PizzaWithPineappleEnabled = true;

        //DestructibleFood = true;
        //FriendlyFire = true;
    }
}