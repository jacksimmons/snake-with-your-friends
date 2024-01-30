using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public partial class PlayerBehaviour
{
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


    private void Start_Status()
    {
        m_pc.Gameplay.Powerup.performed += ctx => UsePowerup();

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


    private void UsePowerup()
    {
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
                {
                    if (effect.EffectName == EEffect.None) continue;
                    return true;
                }
            }

            return false;
        }

        // First check if we can apply the new effect.
        // Second, if that isn't possible, check if we can fire ActiveInputEffect.
        if (ItemSlotEffect == null) return;

        if (!ItemSlotEffect.IsInputEffect)
            TryUseItem();
        else
        {
            ActiveInputEffect ??= ItemSlotEffect;
            UseInputEffect();
        }
    }


    private void Update()
    {
        HandleTime();
        HandlePassiveEffects();
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

    public void UseInputEffect()
    {
        Effect effect = ActiveInputEffect;
        if (effect.Cooldown <= 0 || effect.IsOneOff)
        {
            effect.ResetCooldown();
            switch (effect.EffectName)
            {
                case EEffect.BreathingFire:
                    m_pn.Spawn(EEffect.BreathingFire);
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
                    case EEffect.CureAll:
                        ClearInputEffects();
                        ClearPassiveEffects();

                        // Clear concealing objects
                        GameObject foreground = GameObject.FindWithTag("Foreground");
                        foreach (Transform fgObj in foreground.transform)
                            Destroy(fgObj.gameObject);
                        break;

                    case EEffect.SpeedBoost:
                        m_timeBetweenMoves = GameSettings.Saved.Data.TimeToMove /
                            Effect.GetSpeedMultFromSignedLevel(effect.EffectLevel);

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

                    case EEffect.RocketShitting:
                        m_pn.Spawn(EEffect.RocketShitting);
                        statusUI.ToggleShitIcon(true);
                        break;

                    case EEffect.Drunk:
                        NumPints++;
                        break;

                    case EEffect.SoberUp:
                        NumPints--;
                        break;

                    case EEffect.Sleeping:
                        Frozen = true;
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
            m_timeBetweenMoves = GameSettings.Saved.Data.TimeToMove;
            if (ActiveInputEffect != null)
                ClearInputEffects();
            ActiveInputEffect = effect;
        }
        else
        {
            ActivePassiveEffects.Add(effect);
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
            case EEffect.SpeedBoost:
                m_timeBetweenMoves = GameSettings.Saved.Data.TimeToMove;
                statusUI.DisableAllSpeedIcons();
                break;
            case EEffect.RocketShitting:
                statusUI.ToggleShitIcon(false);
                break;
            case EEffect.Sleeping:
                Frozen = false;
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

        m_timeBetweenMoves = GameSettings.Saved.Data.TimeToMove;
    }

    public Dictionary<string, string> GetStatusDebug()
    {
        Dictionary<string, string> statuses = new Dictionary<string, string>();

        if (ActiveInputEffect != null)
            statuses[Enum.GetName(typeof(EEffect), ActiveInputEffect.EffectName)] = "True";
        foreach (Effect effect in ActivePassiveEffects)
            statuses[Enum.GetName(typeof(EEffect), effect.EffectName)] = "True";
        foreach (string e_name in Enum.GetNames(typeof(EEffect)))
        {
            if (!statuses.ContainsKey(e_name))
                statuses[e_name] = "False";
        }

        statuses["numPints"] = NumPints.ToString();
        statuses["potassiumLevels"] = PotassiumLevels.ToString();
        statuses["NumPieces"] = BodyParts.Count.ToString();
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
                ItemSlotEffect = new Effect(EEffect.CureAll, isOneOff: true);
                break;

            case EFoodType.Balti:
                // 5 cycles of slow, then fast & shitting
                Effect GenerateEpisode(Effect nextEpisode = null)
                {
                    Effect speedBoost;
                    if (nextEpisode != null)
                        speedBoost = new Effect(EEffect.SpeedBoost, level: 5, lifetime: 2, causes: new Effect[] { nextEpisode });
                    else
                        speedBoost = new Effect(EEffect.SpeedBoost, level: 5, lifetime: 2);
                    Effect rocketShit = new Effect(EEffect.RocketShitting, lifetime: 5, cooldown: 0.05f);
                    Effect episode = new Effect(EEffect.SpeedBoost, level: -2, lifetime: 2, causes: new Effect[] { rocketShit, speedBoost });
                    return episode;
                }

                ItemSlotEffect = GenerateEpisode();
                break;

            case EFoodType.Booze:
                // Drunk effect, then piss then sober up
                Effect soberUp = new Effect(EEffect.SoberUp, isOneOff: true);

                Effect pissing = new Effect(EEffect.Pissing, lifetime: 5,
                    cooldown: 0.1f, isInputEffect: true, causes: new Effect[] { soberUp });

                Effect internalProcessing = new Effect(EEffect.None, lifetime: 20, causes:
                    new Effect[] { pissing });

                Effect drunk = new Effect(EEffect.Drunk, lifetime: 0, isOneOff: true);

                ItemSlotEffect = new Effect(EEffect.None, lifetime: 0, causes:
                    new Effect[] { drunk, internalProcessing });

                break;

            case EFoodType.Doughnut:
                // Sleep for 5 turns
                ItemSlotEffect = new Effect(EEffect.Sleeping, lifetime: 5);
                break;

            case EFoodType.Dragonfruit:
                ItemSlotEffect =
                    new(EEffect.BreathingFire, isInputEffect: true, isOneOff: true);
                break;
        }
    }
}
