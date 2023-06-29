using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBehaviour : NetworkBehaviour
{
    [SerializeField]
    private Sprite _spriteCoffee;
    [SerializeField]
    private Sprite _spriteBooze;
    [SerializeField]
    private Sprite _spriteApple;
    [SerializeField]
    private Sprite _spriteOrange;
    [SerializeField]
    private Sprite _spriteBanana;
    [SerializeField]
    private Sprite _spriteDragonfruit;
    [SerializeField]
    private Sprite _spriteDrumstick;
    [SerializeField]
    private Sprite _spriteBone;
    [SerializeField]
    private Sprite _spriteCheese;
    [SerializeField]
    private Sprite _spritePizza;
    [SerializeField]
    private Sprite _spritePineapple;
    [SerializeField]
    private Sprite _spritePineapplePizza;
    [SerializeField]
    private Sprite _spriteIceCream;
    [SerializeField]
    private Sprite _spriteCrapALot;
    [SerializeField]
    private Sprite _spriteBalti;
    [SerializeField]
    private Sprite _spriteBrownie;

    [SerializeField]
    private PlayerMovementController _player;
    [SerializeField]
    private GameObject _fireball;
    [SerializeField]
    private GameObject _staticShit;

    private List<BodyPartStatus> _bodyPartStatuses;

    public List<Effect> ActiveInputEffects { get; private set; } = new List<Effect>();
    public List<Effect> ActivePassiveEffects { get; private set; } = new List<Effect>();

    private float _passiveEffectCooldownMax = 0f;
    private float _passiveEffectCooldown = 0f;

    private float _criticalSpeedDebuff = 1.9f; // +90% counter time
    private float _majorSpeedDebuff = 1.5f; // +50% counter time
    private float _minorSpeedDebuff = 1.1f; // +10% counter time

    private float _criticalSpeedBuff = 1 / 1.9f; // +90% movement speed
    private float _majorSpeedBuff = 1 / 1.5f; // +50% movement speed
    private float _minorSpeedBuff = 1 / 1.1f; // +10% movement speed

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

    private void Update()
    {
        if (ActiveInputEffects.Count > 0)
        {
            Effect effect = ActiveInputEffects[0];
            if (!effect.SubtractTime(Time.deltaTime))
            {
                AddCausedEffect(effect);
                RemoveInputEffect(0);
            }
            effect.SubtractCooldown(Time.deltaTime);
        }
        for (int i = 0; i < ActivePassiveEffects.Count; i++)
        {
            Effect effect = ActivePassiveEffects[i];
            if (!effect.SubtractTime(Time.deltaTime))
            {
                AddCausedEffect(effect);
                RemovePassiveEffect(i);
                i--;
            }
            effect.SubtractCooldown(Time.deltaTime);
        }

        // Powerups
        if (ActiveInputEffects.Count > 0)
        {
            if (Input.GetKey(KeyCode.Space))
                HandleInput();
        }
        HandleStatus();
        HandlePassive();
    }

    private void HandleStatus()
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

    public void HandleInput()
    {
        Effect effect = ActiveInputEffects[0];
        if (effect.Cooldown <= 0)
        {
            effect.ResetCooldown();
            Projectile proj;
            switch (effect.EffectName)
            {
                case e_Effect.BreathingFire:
                    GameObject fireball = Instantiate(_fireball, GameObject.Find("Projectiles").transform);
                    fireball.transform.position = _player.head.Position + (Vector3)_player.head.Direction;
                    proj = fireball.GetComponent<Projectile>();
                    proj.Create(5, _player.head.Direction, _player.head.Rotation, PlayerMovementController.DEFAULT_COUNTER_MAX * _majorSpeedDebuff, _player.head.Transform.gameObject);
                    break;
            }
            // Execute a OneOff effect only once its cooldown (which it typically won't have) reaches 0.
            // Then as they must have a duration of 0s, they will get cleaned up before being executed again.
            effect.IsOneOff = false;
        }
    }

    public void HandlePassive()
    {
        for (int i = 0; i < ActivePassiveEffects.Count; i++)
        {
            Effect effect = ActivePassiveEffects[i];
            if (effect.Cooldown <= 0)
            {
                effect.ResetCooldown();
                Projectile proj;
                switch (effect.EffectName)
                {
                    case e_Effect.NoSpeedBoost:
                        _player.CounterMax = PlayerMovementController.DEFAULT_COUNTER_MAX;
                        break;
                    case e_Effect.MinorSpeedBoost:
                        _player.CounterMax = Mathf.CeilToInt(PlayerMovementController.DEFAULT_COUNTER_MAX * _minorSpeedBuff);
                        break;
                    case e_Effect.MajorSpeedBoost:
                        _player.CounterMax = Mathf.CeilToInt(PlayerMovementController.DEFAULT_COUNTER_MAX * _minorSpeedBuff);
                        break;
                    case e_Effect.RocketShitting:
                        GameObject shit = Instantiate(_staticShit, GameObject.Find("Projectiles").transform);
                        shit.transform.position = _player.tail.Position - (Vector3)_player.tail.Direction;
                        proj = shit.GetComponent<Projectile>();
                        proj.Create(5, -_player.tail.Direction, _player.tail.Rotation, PlayerMovementController.DEFAULT_COUNTER_MAX * _majorSpeedDebuff);
                        break;
                    case e_Effect.SoberUp:
                        NumPints--;
                        break;
                }

                // Execute a OneOff effect only once its cooldown (which it typically won't have) reaches 0.
                // Then as they must have a duration of 0s, they will get cleaned up before being executed again.
                effect.IsOneOff = false;
            }
        }
    }

    public void AddInputEffect(Effect effect)
    {
        // Clear the old effect for the new one
        _player.CounterMax = PlayerMovementController.DEFAULT_COUNTER_MAX;
        if (ActiveInputEffects.Count > 0)
            ClearInputEffects();
        ActiveInputEffects.Add(effect);
    }

    public void AddPassiveEffect(Effect effect)
    {
        ActivePassiveEffects.Add(effect);
        _passiveEffectCooldown = 0;
        _passiveEffectCooldownMax = 0;
    }

    private void AddCausedEffect(Effect effect)
    {
        if (effect.Causes != null)
        {
            foreach (Effect cause in effect.Causes)
            {
                if (cause != null)
                {
                    if (effect.BCausesInputEffect)
                        AddInputEffect(cause);
                    else
                        AddPassiveEffect(cause);
                }
            }
        }
    }

    private void UndoEffect(Effect effect)
    {
        //switch (effect.EffectName)
        //{
        //}
    }

    private void ClearInputEffectImage()
    {
        Image powerupImg = GameObject.FindWithTag("PowerupUI").GetComponent<Image>();
        powerupImg.sprite = null;
        powerupImg.color = Color.clear;
    }

    private void RemoveInputEffect(int i)
    {
        ClearInputEffectImage();
        Effect effect = ActiveInputEffects[i];
        UndoEffect(effect);
        ActiveInputEffects.RemoveAt(i);
    }

    private void RemovePassiveEffectImage(int i)
    {
    }

    private void ClearPassiveEffectImages()
    {
        for (int i = 0; i < ActivePassiveEffects.Count; i++)
            RemovePassiveEffectImage(i);
    }

    private void RemovePassiveEffect(int i)
    {
        Effect effect = ActivePassiveEffects[i];
        UndoEffect(effect);
        ActivePassiveEffects.RemoveAt(i);
    }

    /// <summary>
    /// Disables all input status effects.
    /// </summary>
    public void ClearInputEffects()
    {
        ClearInputEffectImage();
        ActiveInputEffects.Clear();
    }

    /// <summary>
    /// Disables all passive status effects, and resets all passive counters.
    /// </summary>
    public void ClearPassiveEffects()
    {
        ActivePassiveEffects.Clear();

        NumPints = 0;
        PotassiumLevels = 0;

        _player.CounterMax = PlayerMovementController.DEFAULT_COUNTER_MAX;
    }

    public Dictionary<string, string> GetStatusDebug()
    {
        Dictionary<string, string> statuses = new Dictionary<string, string>();
        foreach (Effect effect in ActiveInputEffects)
            statuses[Enum.GetName(typeof(e_Effect), effect.EffectName)] = "True";
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
        Image powerupImg = GameObject.FindWithTag("PowerupUI").GetComponent<Image>();

        switch (food)
        {
            case EFoodType.Coffee:
                DrinkCoffee();
                break;
            case EFoodType.Booze:
                DrinkBooze();
                break;
            case EFoodType.Apple:
                EatApple();
                break;
            case EFoodType.Orange:
                EatOrange();
                break;
            case EFoodType.Banana:
                EatBanana();
                break;
            case EFoodType.Dragonfruit:
                powerupImg.color = Color.white;
                powerupImg.sprite = _spriteDragonfruit;
                EatDragonfruit();
                break;
            case EFoodType.Drumstick:
                EatDrumstick();
                break;
            case EFoodType.Bone:
                EatBone();
                break;
            case EFoodType.Cheese:
                EatCheese();
                break;
            case EFoodType.Pizza:
                EatPizza();
                break;
            case EFoodType.Pineapple:
                EatPineapple();
                break;
            case EFoodType.PineapplePizza:
                EatPineapplePizza();
                break;
            case EFoodType.IceCream:
                EatIceCream();
                break;
            case EFoodType.CrapALot:
                EatCrapALot();
                break;
            case EFoodType.Balti:
                EatBalti();
                break;
            case EFoodType.Brownie:
                EatBrownie();
                break;
        }
    }

    private void DrinkCoffee()
    {
        Effect noSpdBoost = new Effect(e_Effect.NoSpeedBoost);
        Effect resetMovSpd = new Effect(e_Effect.None, lifetime: 10, new Effect[] { noSpdBoost }, false);

        Effect major = new Effect(e_Effect.MajorSpeedBoost);
        AddPassiveEffect(resetMovSpd);
        AddPassiveEffect(major);
    }

    private void DrinkBooze()
    {
        NumPints++;

        Effect soberUp = new Effect(e_Effect.SoberUp);
        Effect drunkAPint = new Effect(e_Effect.None, lifetime: 20, new Effect[] { soberUp }, false);

        Effect pissing = new Effect(e_Effect.Pissing, lifetime:5, cooldown:0.1f);
        Effect needToPee = new Effect(e_Effect.None, lifetime:10, new Effect[] { pissing }, true);
        AddPassiveEffect(needToPee);
        AddPassiveEffect(drunkAPint);
    }

    private void EatApple()
    {
        ClearInputEffects();
        ClearPassiveEffects();
        GameObject foreground = GameObject.FindWithTag("Foreground");
        foreach (Transform fgObj in foreground.transform)
            Destroy(fgObj.gameObject);
    }

    private void EatOrange()
    {
        // Produce projectile
    }

    private void EatBanana()
    {
        // Produce peel
        PotassiumLevels++;
        if (PotassiumLevels >= 3)
        {
            // Die ... ?
        }
    }

    private void EatDragonfruit()
    {
        Effect fireBreath = new Effect(e_Effect.BreathingFire, lifetime:5f, cooldown:1f);
        AddInputEffect(fireBreath);
    }

    private void EatDrumstick()
    {
        Effect buff = new Effect(e_Effect.Buff, lifetime: 20);
        AddPassiveEffect(buff);
        EatBone();
    }

    private void EatBone()
    {
        // Rupture asshole
    }

    private void EatCheese()
    {
        // Get to eat it again
    }

    private void EatPizza()
    {
        // Get to eat it again twice
    }

    private void EatPineapple()
    {
        // ?
    }

    private void EatPineapplePizza()
    {
        // Die
    }

    private void EatIceCream()
    {
        Effect brainFreeze = new Effect(e_Effect.BrainFreeze, 3);
        Effect unicorn = new Effect(e_Effect.Unicorn, 3, new Effect[] { brainFreeze }, false, 0);
        AddPassiveEffect(unicorn);
    }

    private void EatCrapALot()
    {
        Effect laxative = new Effect(e_Effect.Laxative, 20, 1);
        AddInputEffect(laxative);
    }

    private void EatBalti()
    {
        Effect rocketShit = new Effect(e_Effect.RocketShitting, lifetime: 10, cooldown: 0.05f);

        Effect noSpdBoost = new Effect(e_Effect.NoSpeedBoost);
        Effect resetMovSpd = new Effect(e_Effect.None, lifetime: 10, new Effect[] { noSpdBoost }, false);
        Effect majorSpdBoost = new Effect(e_Effect.MajorSpeedBoost);

        Effect balti = new Effect(e_Effect.None, lifetime: 2, new Effect[] { rocketShit, resetMovSpd, majorSpdBoost }, false, cooldown: 0.05f);

        AddPassiveEffect(balti);
    }

    private void EatBrownie()
    {
        // Sleep for 5 turns
    }
}