public enum EFoodType
{
    None, // 0

    Apple,
    Balti,
    Banana,
    Bone,
    Booze,
    Cheese,
    Coffee,
    Doughnut,
    Dragonfruit,
    Drumstick,
    IceCream,
    Orange,
    Pineapple,
    PineapplePizza,
    Pizza, // 15

    // !Final index must be less than 32 (for FoodSettings bitfield)
}
public enum EEffect
{
    None,

    // Apple
    CureAll,

    // Dragonfruit
    BreathingFire,

    // Booze
    Drunk,
    Pissing,

    // Balti
    SpeedBoost,
    RocketShitting,

    // Doughnut
    Sleeping,

    SoberUp,
    Hallucination,
    Unicorn,
    BrainFreeze,
    Buff
}