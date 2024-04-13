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


    private PlayerMovement m_pm;
    [SerializeField]
    private GameObject m_fireball;
    [SerializeField]
    private GameObject m_staticShit;
    [SerializeField]
    private GameObject m_buffArms;

    /// <summary>
    /// Effects
    /// </summary>
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

    // Status variables
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

    public bool IsBuff { get; private set; } = false;
    private GameObject m_buffArmsInstance = null;

    public float SpeedIncrease { get; private set; } = 0f;
    public int PotassiumLevels { get; private set; } = 0;

    private PlayerControls controls;


    private void Awake()
    {
        m_pm = GetComponent<PlayerMovement>();
        controls = new();
        controls.Gameplay.Powerup.performed += ctx => UsePowerup();
    }


    private void OnEnable() { controls.Gameplay.Enable(); }


    private void OnDisable() { controls.Gameplay.Disable(); }


    private void UsePowerup()
    {
        void TryUseItem()
        {
            foreach (Effect effect in FindConflictingPassiveEffects(ItemSlotEffect))
            {
                RemovePassiveEffect(effect);
            }

            AddEffect(ItemSlotEffect);

            // For input effects, we keep the icon visible.
            if (!ItemSlotEffect.IsInputEffect)
                ItemSlotEffect = null;
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


    private List<Effect> FindConflictingPassiveEffects(Effect effect)
    {
        List<Effect> conflicts = new();
        foreach (Effect other in ActivePassiveEffects)
        {
            if (other.EffectName == effect.EffectName)
            {
                if (other.EffectName == EEffect.None) continue;
                conflicts.Add(other);
            }
        }

        return conflicts;
    }


    private void Update()
    {
        HandlePassiveEffects();
        HandleTime();
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

        List<Effect> toRemove = new();
        for (int i = 0; i < ActivePassiveEffects.Count; i++)
        {
            Effect effect = ActivePassiveEffects[i];
            if (!effect.SubtractTime(Time.deltaTime))
            {
                AddCausedEffects(effect);
                RemovePassiveEffect(i);

                toRemove.Add(effect);
                continue;
            }
            effect.SubtractCooldown(Time.deltaTime);
        }

        // Remove all expired passive effects
        foreach (Effect e in toRemove)
        {
            ActivePassiveEffects.Remove(e);
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
                    CmdSpawn(EEffect.BreathingFire);
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
                    case EEffect.Buff:
                        if (IsBuff) break;
                        m_buffArmsInstance = Instantiate(m_buffArms);
                        m_buffArmsInstance.GetComponent<BuffArms>().Setup(m_pm);
                        IsBuff = true;
                        break;

                    case EEffect.CureAll:
                        ClearInputEffects();
                        ClearPassiveEffects();
                        break;

                    case EEffect.Drunk:
                        NumPints++;
                        break;

                    case EEffect.LoseGains:
                        Destroy(m_buffArmsInstance);
                        IsBuff = false;
                        break;

                    case EEffect.RocketShitting:
                        CmdSpawn(EEffect.RocketShitting);
                        statusUI.ToggleShitIcon(true);
                        break;

                    case EEffect.Sleeping:
                        m_pm.Frozen = true;
                        statusUI.ToggleSleepingIcon(true);
                        break;

                    case EEffect.SoberUp:
                        NumPints--;
                        break;

                    case EEffect.SpeedBoost:
                        GameBehaviour.Instance.CmdSetSpeedMultiplier(Effect.GetSpeedMultFromSignedLevel(effect.EffectLevel));

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
                }

                // Execute a OneOff effect only once its cooldown (which it typically won't have) reaches 0.
                // Then as they must have a duration of 0s, they will get cleaned up before being executed again.
                effect.IsOneOff = false;
            }
        }
    }


    /// <summary>
    /// Handles spawning of projectiles, determined by the effect enum passed.
    /// Some objects are synced with the server, some just have synced spawn times.
    /// </summary>
    /// <param name="effect">The projectile is based on the effect.</param>
    [Command]
    private void CmdSpawn(EEffect effect)
    {
        switch (effect)
        {
            case EEffect.BreathingFire:
                BodyPart head = m_pm.BodyParts[0];

                GameObject fireball = Instantiate(m_fireball, GameObject.Find("Projectiles").transform);
                fireball.transform.position = head.Position + (Vector3)head.Direction;
                fireball.GetComponent<ProjectileBehaviour>()
                .Proj = Projectiles.ConstructFireball(
                    head.Direction * PROJ_SPEED_FAST,
                    head.RegularAngle);

                NetworkServer.Spawn(fireball);
                break;
            case EEffect.RocketShitting:
                ClientSpawnUnsynced(effect);
                break;
        }
    }


    /// <summary>
    /// Spawns an unsynced object, at a synced time (as every client does the same
    /// thing).
    /// </summary>
    /// <param name="effect">The projectile is based on the effect.</param>
    [ClientRpc]
    private void ClientSpawnUnsynced(EEffect effect)
    {
        switch (effect)
        {
            case EEffect.RocketShitting:
                float randomRotation = Random.Range(-SHIT_EXPLOSIVENESS, SHIT_EXPLOSIVENESS);

                GameObject shit = Instantiate(m_staticShit, GameObject.Find("Projectiles").transform);
                shit.transform.position = m_pm.BodyParts[^1].Position - (Vector3)m_pm.BodyParts[^1].Direction;
                shit.transform.Rotate(Vector3.forward * randomRotation);

                shit.GetComponent<ProjectileBehaviour>()
                .Proj = Projectiles.ConstructShit(
                    Extensions.Vectors.Rotate(-m_pm.BodyParts[^1].Direction, randomRotation) * PROJ_SPEED_SLOW,
                    m_pm.BodyParts[^1].RegularAngle);
                break;
        }
    }


    public void AddEffect(Effect effect)
    {
        if (effect.IsInputEffect)
        {
            // Clear the old effect for the new one
            GameBehaviour.Instance.CmdResetSpeedMultiplier();
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


    private void ClearPowerupIcon()
    {
        Image powerupImg = GameObject.FindWithTag("PowerupUI").GetComponent<Image>();
        powerupImg.sprite = null;
        powerupImg.color = Color.clear;
    }


    private void RemoveInputEffect()
    {
        ItemSlotEffect = null;
        ActiveInputEffect = null;
    }


    private void RemovePassiveEffect(int index)
    {
        RemovePassiveEffect(ActivePassiveEffects[index]);
    }


    private void RemovePassiveEffect(Effect effect)
    {
        StatusEffectUI statusUI = GameObject.FindWithTag("StatusUI").GetComponent<StatusEffectUI>();
        switch (effect.EffectName)
        {
            case EEffect.RocketShitting:
                statusUI.ToggleShitIcon(false);
                break;

            case EEffect.Sleeping:
                m_pm.Frozen = false;
                statusUI.ToggleSleepingIcon(false);
                break;

            case EEffect.SpeedBoost:
                GameBehaviour.Instance.CmdResetSpeedMultiplier();
                statusUI.DisableAllSpeedIcons();
                break;
        }
    }


    /// <summary>
    /// Nicely clears up all status data, useful for starting a new game.
    /// </summary>
    public void ClearAll()
    {
        ClearInputEffects();
        ClearPassiveEffects();
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

        foreach (Effect effect in ActivePassiveEffects)
        {
            RemovePassiveEffect(effect);
        }
        // Clear after so as to not modify the collection during iteration
        ActivePassiveEffects.Clear();

        NumPints = 0;
        PotassiumLevels = 0;

        // Clear concealing objects
        GameObject foreground = GameObject.FindWithTag("Foreground");
        foreach (Transform fgObj in foreground.transform)
            Destroy(fgObj.gameObject);
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
        statuses["NumPieces"] = m_pm.BodyParts.Count.ToString();
        return statuses;
    }


    /// <summary>
    /// When the player eats a food, store its powerup properties in the item slot, etc.
    /// </summary>
    /// <param name="food"></param>
    public void Eat(EFoodType food, Sprite powerupSprite)
    {
        // If there already is a powerup in the slot, do not overwrite it.
        if (ItemSlotEffect != null) return;

        Image powerupImg = GameObject.FindWithTag("PowerupUI").GetComponent<Image>();

        // Make the sprite visible (Color.clear is assigned on removal of icon)
        powerupImg.sprite = powerupSprite;
        powerupImg.color = Color.white;

        // Set the item slot effect property (store in a variable until the powerup is used)
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
                        speedBoost = new Effect(EEffect.SpeedBoost, level: 5, lifetime: 5, causes: new Effect[] { nextEpisode });
                    else
                        speedBoost = new Effect(EEffect.SpeedBoost, level: 5, lifetime: 5);
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
                    cooldown: 0.1f, causes: new Effect[] { soberUp });

                Effect liquidIngested = new Effect(EEffect.None, lifetime: 20, causes:
                    new Effect[] { pissing });

                Effect drunk = new Effect(EEffect.Drunk, isOneOff: true);

                ItemSlotEffect = new Effect(EEffect.None, lifetime: 0, causes:
                    new Effect[] { drunk, liquidIngested });

                break;

            case EFoodType.Doughnut:
                // Sleep for 5 turns
                ItemSlotEffect = new Effect(EEffect.Sleeping, lifetime: 5);
                break;

            case EFoodType.Dragonfruit:
                ItemSlotEffect =
                    new(EEffect.BreathingFire, isInputEffect: true, isOneOff: true);
                break;

            case EFoodType.Drumstick:

                Effect weak = new Effect(EEffect.LoseGains, isOneOff: true);
                Effect duration = new Effect(EEffect.None, lifetime: 7.5f, causes: new Effect[] { weak });
                Effect buff = new Effect(EEffect.Buff, isOneOff: true);
                ItemSlotEffect = new Effect(EEffect.None, causes: new Effect[] { buff, duration });
                break;
        }
    }
}