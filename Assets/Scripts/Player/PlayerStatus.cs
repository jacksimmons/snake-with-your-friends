using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

public class PlayerStatus : NetworkBehaviour
{
    // Constants
    private const float PROJ_SPEED_FAST = 0.25f;
    private const float PROJ_SPEED_SLOW = 0.1f;

    [Range(0f, 360f)]
    // An angle either side of the player defining the random range of RocketShitting.
    // Recommended range: 0-90. Past 90 will give very shitty results.
    private const float SHIT_EXPLOSIVENESS = 45;

    [SerializeField]
    private Sprite _spriteApple;
    [SerializeField]
    private Sprite _spriteBalti;
    [SerializeField]
    private Sprite _spriteBanana;
    [SerializeField]
    private Sprite _spriteBone;
    [SerializeField]
    private Sprite _spriteBooze;
    [SerializeField]
    private Sprite _spriteCheese;
    [SerializeField]
    private Sprite _spriteCoffee;
    [SerializeField]
    private Sprite _spriteDoughnut;
    [SerializeField]
    private Sprite _spriteDragonfruit;
    [SerializeField]
    private Sprite _spriteDrumstick;
    [SerializeField]
    private Sprite _spriteIceCream;
    [SerializeField]
    private Sprite _spriteOrange;
    [SerializeField]
    private Sprite _spritePineapple;
    [SerializeField]
    private Sprite _spritePineapplePizza;
    [SerializeField]
    private Sprite _spritePizza;

    private Dictionary<EFoodType, Sprite> _foodSprites;

    [SerializeField]
    private PlayerMovement _player;
    [SerializeField]
    private GameObject _fireball;
    [SerializeField]
    private GameObject _staticShit;

    private List<BodyPartStatus> _bodyPartStatuses;

    public Effect ActiveInputEffect { get; private set; } = null;
    public List<Effect> ActivePassiveEffects { get; private set; } = new List<Effect>();

    private Effect _itemSlotEffect = null;
    public Effect ItemSlotEffect
    {
        get { return _itemSlotEffect; }
        private set
        {
            if (value == null)
                ClearPowerupIcon();
            _itemSlotEffect = value;
        }
    }

    private float _passiveEffectCooldownMax = 0f;
    private float _passiveEffectCooldown = 0f;

    // Counters
    private int _numPints = 0;
    public int NumPints
    {
        get
        {
            return _numPints;
        }
        private set
        {
            _numPints = value;
            GameObject effects = GameObject.FindWithTag("Effects");
            TooManyPints tmp = effects.transform.Find("TooManyPints").GetComponent<TooManyPints>();
            tmp.UpdatePints(value);
        }
    }
    public float SpeedIncrease { get; private set; } = 0f;
    public int PotassiumLevels { get; private set; } = 0;

    /// <summary>
    /// Handles spawning of projectiles, determined by the effect enum passed.
    /// Some objects are synced with the server, some just have synced spawn times.
    /// </summary>
    /// <param name="effect">The projectile is based on the effect.</param>
    [Command]
    private void CmdSpawn(e_Effect effect)
    {
        ProjectileBehaviour proj;
        switch (effect)
        {
            case e_Effect.RocketShitting:
                ClientSpawnUnsynced(effect);
                break;
            case e_Effect.BreathingFire:
                BodyPart head = _player.BodyParts[0];

                GameObject fireball = Instantiate(_fireball, GameObject.Find("Projectiles").transform);
                fireball.transform.position = head.Position + (Vector3)head.Direction;
                proj = fireball.GetComponent<ProjectileBehaviour>();
                proj.Proj = new Projectile(
                    lifetime: 5,
                    velocity: head.Direction * PROJ_SPEED_FAST,
                    rotation: head.RegularAngle,
                    immunityDuration: 0.5f
                );
                NetworkServer.Spawn(fireball);
                break;
        }
    }

    /// <summary>
    /// Spawns an unsynced object, at a synced time (as every client does the same
    /// thing).
    /// </summary>
    /// <param name="effect">The projectile is based on the effect.</param>
    [ClientRpc]
    private void ClientSpawnUnsynced(e_Effect effect)
    {
        switch (effect)
        {
            case e_Effect.RocketShitting:
                float randomRotation = Random.Range(-SHIT_EXPLOSIVENESS, SHIT_EXPLOSIVENESS);
                
                GameObject shit = Instantiate(_staticShit, GameObject.Find("Projectiles").transform);
                shit.transform.position = _player.BodyParts[^1].Position - (Vector3)_player.BodyParts[^1].Direction;
                shit.transform.Rotate(Vector3.forward * randomRotation);

                ProjectileBehaviour proj;
                proj = shit.GetComponent<ProjectileBehaviour>();
                proj.Proj = new Projectile(
                    lifetime: 5,
                    velocity: Vectors.Rotate(-_player.BodyParts[^1].Direction, randomRotation) * PROJ_SPEED_SLOW,
                    rotation: _player.BodyParts[^1].RegularAngle,
                    immunityDuration: 0.2f
                );
                break;
        }
    }

    private void Awake()
    {
        _foodSprites = new()
        {
            { EFoodType.Apple, _spriteApple },
            { EFoodType.Balti, _spriteBalti },
            { EFoodType.Booze, _spriteBooze },
            { EFoodType.Cheese, _spriteCheese },
            { EFoodType.Coffee, _spriteCoffee },
            { EFoodType.Doughnut, _spriteDoughnut },
            { EFoodType.Dragonfruit, _spriteDragonfruit },
            { EFoodType.Drumstick, _spriteDrumstick },
            { EFoodType.IceCream, _spriteIceCream },
            { EFoodType.Orange, _spriteOrange },
            { EFoodType.Pineapple, _spritePineapple },
            { EFoodType.PineapplePizza, _spritePineapplePizza },
            { EFoodType.Pizza, _spritePizza },
        };
    }

    private void Update()
    {
        HandleTime();

        HandleVisualEffects();
        HandlePassiveEffects();

        void TryUseItem()
        {
            if (!FindConflictingPassiveEffect())
            {
                AddEffect(ItemSlotEffect);

                // For input effects, we keep the icon visible.
                if (!ItemSlotEffect.IsInputEffect)
                    ItemSlotEffect = null;
            }
        }

        // false => No conflict; true => Conflict
        bool FindConflictingPassiveEffect()
        {
            foreach (Effect effect in ActivePassiveEffects)
            {
                if (effect.EffectName == ItemSlotEffect.EffectName)
                    return true;
            }

            return false;
        }

        // Powerup Input

        // First check if we can apply the new effect.
        // Second, if that isn't possible, check if we can fire ActiveInputEffect.
        if (Input.GetButtonDown("Powerup"))
        {
            if (ItemSlotEffect == null) return;

            if (!ItemSlotEffect.IsInputEffect)
                TryUseItem();
            else
            {
                if (ActiveInputEffect == null)
                    ActiveInputEffect = ItemSlotEffect;
                UseInputEffect();
            }
        }
    }

    private void HandleTime()
    {
        if (ActiveInputEffect != null)
        {
            ActiveInputEffect.SubtractCooldown(Time.deltaTime);
            if (!ActiveInputEffect.SubtractTime(Time.deltaTime))
            {
                AddCausedEffects(ActiveInputEffect);
                RemoveInputEffect();
            }
        }

        for (int i = 0; i < ActivePassiveEffects.Count; i++)
        {
            Effect effect = ActivePassiveEffects[i];
            if (!effect.SubtractTime(Time.deltaTime))
            {
                AddCausedEffects(effect);
                RemovePassiveEffect(i);
                i--;
            }
            effect.SubtractCooldown(Time.deltaTime);
        }
    }

    private void HandleVisualEffects()
    {
        Transform tooManyPints = transform.Find("TooManyPints");
        if (NumPints > 0 && tooManyPints != null)
        {
            // Add the effect as a child
            GameObject go = new GameObject("TooManyPints");
            go.transform.parent = transform;
            go.layer = LayerMask.NameToLayer("Effects");
            go.AddComponent<TooManyPints>();
            go.GetComponent<TooManyPints>();
        }
    }

    public void UseInputEffect()
    {
        Effect effect = ActiveInputEffect;
        if (effect.Cooldown <= 0)
        {
            effect.ResetCooldown();
            switch (effect.EffectName)
            {
                case e_Effect.BreathingFire:
                    CmdSpawn(e_Effect.BreathingFire);
                    break;
            }
            // Execute a OneOff effect only once its cooldown (which it typically won't have) reaches 0.
            // Then as they must have a duration of 0s, they will get cleaned up before being executed again.
            effect.IsOneOff = false;
        }
    }

    public void HandlePassiveEffects()
    {
        for (int i = 0; i < ActivePassiveEffects.Count; i++)
        {
            Effect effect = ActivePassiveEffects[i];
            if (effect.Cooldown <= 0)
            {
                effect.ResetCooldown();
                StatusEffectUI statusUI = GameObject.FindWithTag("StatusUI").GetComponent<StatusEffectUI>();

                switch (effect.EffectName)
                {
                    case e_Effect.CureAll:
                        print("Hi");
                        ClearInputEffects();
                        ClearPassiveEffects();

                        // Clear concealing objects
                        GameObject foreground = GameObject.FindWithTag("Foreground");
                        foreach (Transform fgObj in foreground.transform)
                            Destroy(fgObj.gameObject);
                        break;

                    case e_Effect.SpeedBoost:
                        float counterMaxVal= PlayerMovement.DEFAULT_COUNTER_MAX / 
                            SpeedEffect.GetSpeedMultFromSignedLevel(effect.EffectLevel);

                        if (float.IsInfinity(counterMaxVal))
                            _player.CounterMax = int.MaxValue;
                        else
                            _player.CounterMax = Mathf.CeilToInt(counterMaxVal);

                        statusUI.DisableAllSpeedIcons();
                        if (effect.EffectLevel >= 0)
                        {
                            statusUI.ChangeIconActive(true, "Fast", effect.EffectLevel, true);
                        }
                        else
                        {
                            statusUI.ChangeIconActive(false, "Slow", -effect.EffectLevel, true);
                        }
                        break;

                    case e_Effect.RocketShitting:
                        CmdSpawn(e_Effect.RocketShitting);
                        statusUI.ToggleShitIcon(true);
                        break;

                    case e_Effect.Drunk:
                        NumPints++;
                        break;

                    case e_Effect.SoberUp:
                        NumPints--;
                        break;

                    case e_Effect.Sleeping:
                        _player.frozen = true;
                        statusUI.ToggleSleepingIcon(true);
                        break;
                }

                // Execute a OneOff effect only once its cooldown (which it typically won't have) reaches 0.
                // Then as they must have a duration of 0s, they will get cleaned up before being executed again.
                effect.IsOneOff = false;
            }
        }
    }

    public void AddEffect(Effect effect)
    {
        if (effect.IsInputEffect)
        {
            // Clear the old effect for the new one
            _player.CounterMax = PlayerMovement.DEFAULT_COUNTER_MAX;
            if (ActiveInputEffect != null)
                ClearInputEffects();
            ActiveInputEffect = effect;
        }
        else
        {
            ActivePassiveEffects.Add(effect);
            _passiveEffectCooldown = 0;
            _passiveEffectCooldownMax = 0;
        }
    }

    private void AddCausedEffects(Effect effect)
    {
        if (effect.Causes != null)
        {
            foreach (Effect cause in effect.Causes)
            {
                AddEffect(cause);
            }
        }
    }

    private void UndoEffect(Effect effect)
    {
        //switch (effect.EffectName)
        //{
        //}
    }

    private void ClearPowerupIcon()
    {
        Image powerupImg = GameObject.FindWithTag("PowerupUI").GetComponent<Image>();
        powerupImg.sprite = null;
        powerupImg.color = Color.clear;
    }

    private void RemoveInputEffect()
    {
        UndoEffect(ActiveInputEffect);
        ItemSlotEffect = null;
        ActiveInputEffect = null;
    }

    private void RemovePassiveEffect(int i)
    {
        Effect effect = ActivePassiveEffects[i];
        StatusEffectUI statusUI = GameObject.FindWithTag("StatusUI").GetComponent<StatusEffectUI>();
        switch (effect.EffectName)
        {
            case e_Effect.SpeedBoost:
                _player.CounterMax = PlayerMovement.DEFAULT_COUNTER_MAX;
                statusUI.DisableAllSpeedIcons();
                break;
            case e_Effect.RocketShitting:
                statusUI.ToggleShitIcon(false);
                break;
            case e_Effect.Sleeping:
                _player.frozen = false;
                statusUI.ToggleSleepingIcon(false);
                break;
        }

        UndoEffect(effect);
        ActivePassiveEffects.RemoveAt(i);
    }

    /// <summary>
    /// Disables all input status effects.
    /// </summary>
    public void ClearInputEffects()
    {
        ItemSlotEffect = null;
        ActiveInputEffect = null;
    }

    /// <summary>
    /// Disables all passive status effects, and resets all passive counters.
    /// </summary>
    public void ClearPassiveEffects()
    {
        StatusEffectUI statusUI = GameObject.FindWithTag("StatusUI").GetComponent<StatusEffectUI>();
        statusUI.DisableAllIcons();

        ActivePassiveEffects.Clear();

        NumPints = 0;
        PotassiumLevels = 0;

        _player.CounterMax = PlayerMovement.DEFAULT_COUNTER_MAX;
    }

    public Dictionary<string, string> GetStatusDebug()
    {
        Dictionary<string, string> statuses = new Dictionary<string, string>();
        
        if (ActiveInputEffect != null)
            statuses[Enum.GetName(typeof(e_Effect), ActiveInputEffect.EffectName)] = "True";
        foreach (Effect effect in ActivePassiveEffects)
            statuses[Enum.GetName(typeof(e_Effect), effect.EffectName)] = "True";
        foreach (string e_name in Enum.GetNames(typeof(e_Effect)))
        {
            if (!statuses.ContainsKey(e_name))
                statuses[e_name] = "False";
        }

        statuses["numPints"] = NumPints.ToString();
        statuses["potassiumLevels"] = PotassiumLevels.ToString();
        statuses["NumPieces"] = _player.BodyParts.Count.ToString();
        return statuses;
    }

    public void Eat(EFoodType food)
    {
        if (ItemSlotEffect != null) return;

        Image powerupImg = GameObject.FindWithTag("PowerupUI").GetComponent<Image>();

        // Make the sprite visible (Color.clear is assigned on removal of icon)
        powerupImg.sprite = _foodSprites[food];
        powerupImg.color = Color.white;

        switch (food)
        {
            case EFoodType.Apple:
                ItemSlotEffect = new Effect(e_Effect.CureAll);
                break;

            case EFoodType.Balti:
                // 5 cycles of slow, then fast & shitting
                Effect GenerateEpisode(Effect nextEpisode = null)
                {
                    Effect speedBoost;
                    if (nextEpisode != null)
                        speedBoost = new Effect(e_Effect.SpeedBoost, level: 5, lifetime: 2, causes: new Effect[] { nextEpisode });
                    else
                        speedBoost = new Effect(e_Effect.SpeedBoost, level: 5, lifetime: 2);
                    Effect rocketShit = new Effect(e_Effect.RocketShitting, lifetime: 5, cooldown: 0.05f);
                    Effect episode = new Effect(e_Effect.SpeedBoost, level: -2, lifetime: 2, causes: new Effect[] { rocketShit, speedBoost });
                    return episode;
                }

                Effect episode5 = GenerateEpisode();
                Effect episode4 = GenerateEpisode(episode5);
                Effect episode3 = GenerateEpisode(episode4);
                Effect episode2 = GenerateEpisode(episode3);

                ItemSlotEffect = GenerateEpisode(episode2);
                break;

            case EFoodType.Booze:
                // Drunk effect, then piss then sober up
                Effect soberUp = new Effect(e_Effect.SoberUp);

                Effect pissing = new Effect(e_Effect.Pissing, lifetime: 5,
                    cooldown: 0.1f, isInputEffect: true, causes: new Effect[] { soberUp });

                Effect internalProcessing = new Effect(e_Effect.None, lifetime: 20,
                    new Effect[] { pissing });

                Effect drunk = new Effect(e_Effect.Drunk);

                ItemSlotEffect = new Effect(e_Effect.None, lifetime: 0,
                    new Effect[] { drunk, internalProcessing });

                break;

            case EFoodType.Coffee:
                DrinkCoffee();
                break;

            case EFoodType.Doughnut:
                // Sleep for 5 turns
                ItemSlotEffect = new Effect(e_Effect.Sleeping, lifetime: 5);
                break;

            case EFoodType.Dragonfruit:
                ItemSlotEffect = 
                    new Effect(e_Effect.BreathingFire, lifetime: 5f, cooldown: 1f, isInputEffect: true);
                break;
        }
    }

    private void DrinkCoffee()
    {
        Effect major = new Effect(e_Effect.SpeedBoost, level: 3, lifetime: 10);
        AddEffect(major);
    }

    private void EatDrumstick()
    {
        Effect buff = new Effect(e_Effect.Buff, lifetime: 20);
        AddEffect(buff);
        EatBone();
    }

    private void EatBone()
    {
        // Rupture asshole
    }

    private void EatIceCream()
    {
        Effect brainFreeze = new Effect(e_Effect.BrainFreeze, 3);
        Effect unicorn = new Effect(e_Effect.Unicorn, 3, new Effect[] { brainFreeze });
        AddEffect(unicorn);
    }
}
