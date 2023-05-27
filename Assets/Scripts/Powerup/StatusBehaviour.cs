using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusBehaviour : MonoBehaviour
{
    [SerializeField]
    private PlayerBehaviour _player;
    [SerializeField]
    private GameObject _fireball;
    [SerializeField]
    private GameObject _static_shit;

    private List<BodyPartStatus> _bodyPartStatuses;

    public List<Effect> p_ActiveInputEffects { get; private set; } = new List<Effect>();
    public List<Effect> p_ActivePassiveEffects { get; private set; } = new List<Effect>();

    private float _inputEffectCooldownMax = 0f;
    private float _inputEffectCooldown = 0f;

    private float _majorSpeedBoost = 3f;
    private float _minorSpeedBoost = 1f;

    // Counters
    private int _numPints = 0;
    public int p_NumPints
    {
        get { return _numPints; }
    }

    private float _speedIncrease = 0f;
    public float p_SpeedIncrease
    {
        get { return _speedIncrease; }
    }

    private int _potassiumLevels = 0;
    public int p_PotassiumLevels
    {
        get { return _potassiumLevels; }
    }

    private void Update()
    {
        _inputEffectCooldown -= Time.deltaTime;
        if (p_ActiveInputEffects.Count > 0)
        {
            Effect effect = p_ActiveInputEffects[0];
            if (!effect.SubtractTime(Time.deltaTime))
            {
                AddCausedEffect(effect);
                RemoveInputEffect(0);
            }
        }
        for (int i = 0; i < p_ActivePassiveEffects.Count; i++)
        {
            Effect effect = p_ActivePassiveEffects[i];
            if (!effect.SubtractTime(Time.deltaTime))
            {
                AddCausedEffect(effect);
                RemovePassiveEffect(i);
                i--;
            }
        }

        // Powerups
        if (p_ActiveInputEffects.Count > 0)
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
        if (p_NumPints > 0 && tooManyPints != null)
        {
            // Add the effect as a child
            GameObject go = new GameObject("TooManyPints");
            go.transform.parent = transform;
            go.layer = LayerMask.NameToLayer("Effects");
            go.AddComponent<TooManyPints>();
            go.GetComponent<TooManyPints>();
        }

        if (tooManyPints != null)
        {
            if (p_NumPints > 0)
            {
                tooManyPints.GetComponent<TooManyPints>().UpdatePints(p_NumPints);
            }
            else
            {
                Destroy(tooManyPints.gameObject);
            }
        }
    }

    public void HandleInput()
    {
        if (_inputEffectCooldown <= 0f)
        {
            _inputEffectCooldown = _inputEffectCooldownMax;
            Projectile proj;
            switch (p_ActiveInputEffects[0].p_EffectName)
            {
                case e_Effect.BreathingFire:
                    GameObject fireball = Instantiate(_fireball, GameObject.Find("Projectiles").transform);
                    proj = fireball.GetComponent<Projectile>();
                    proj.Create(Mathf.Infinity, _player.head.p_Position + (Vector3)_player.head.p_Direction,
                        _player.head.p_Direction, _player.head.p_Rotation, 0.2f);
                    break;
            }
        }
    }

    public void HandlePassive()
    {
        for (int i = 0; i < p_ActivePassiveEffects.Count; i++)
        {
            Effect effect = p_ActivePassiveEffects[i];
            if (_inputEffectCooldown <= 0f)
            {
                _inputEffectCooldown = _inputEffectCooldownMax;
                Projectile proj;
                switch (effect.p_EffectName)
                {
                    case e_Effect.RocketShitting:
                        GameObject shit = Instantiate(_static_shit, GameObject.Find("Projectiles").transform);
                        proj = shit.GetComponent<Projectile>();
                        proj.Create(Mathf.Infinity, _player.tail.p_Position - (Vector3)_player.tail.p_Direction,
                            -_player.tail.p_Direction, _player.tail.p_Rotation, 0.2f);
                        _player.MovementSpeed = _player.DefaultMovementSpeed + _majorSpeedBoost;
                        break;
                }
            }
        }
    }

    public void AddInputEffect(Effect effect, float cooldown)
    {
        // Clear the old effect for the new one
        _player.MovementSpeed = _player.DefaultMovementSpeed;
        if (p_ActiveInputEffects.Count > 0)
            ClearInputEffects();
        p_ActiveInputEffects.Add(effect);
        _inputEffectCooldown = 0;
        _inputEffectCooldownMax = cooldown;
    }

    public void AddPassiveEffect(Effect effect, float cooldown = 0)
    {
        p_ActivePassiveEffects.Add(effect);
        _inputEffectCooldown = 0;
        _inputEffectCooldownMax = cooldown;
    }

    private void AddCausedEffect(Effect effect)
    {
        Effect cause = effect.p_Causes;
        if (cause != null)
        {
            if (effect.p_CausesInputEffect)
                AddInputEffect(cause, effect.p_CausesCooldown);
            else
                AddPassiveEffect(cause, effect.p_CausesCooldown);
        }
    }

    private void UndoEffect(Effect effect)
    {
        switch (effect.p_EffectName)
        {
            case e_Effect.RocketShitting:
                _player.MovementSpeed -= _majorSpeedBoost; break;
        }
    }

    private void RemoveInputEffect(int i)
    {
        Effect effect = p_ActiveInputEffects[i];
        UndoEffect(effect);
        p_ActiveInputEffects.RemoveAt(i);
    }

    private void RemovePassiveEffect(int i)
    {
        Effect effect = p_ActivePassiveEffects[i];
        UndoEffect(effect);
        p_ActivePassiveEffects.RemoveAt(i);
    }

    /// <summary>
    /// Disables all input status effects.
    /// </summary>
    public void ClearInputEffects()
    {
        p_ActiveInputEffects.Clear();
        _inputEffectCooldown = 0f;
    }

    /// <summary>
    /// Disables all passive status effects, and resets all passive counters.
    /// </summary>
    public void ClearPassiveEffects()
    {
        p_ActivePassiveEffects.Clear();

        _numPints = 0;
        _potassiumLevels = 0;

        _player.MovementSpeed = _player.DefaultMovementSpeed;
    }

    public Dictionary<string, string> GetStatusDebug()
    {
        Dictionary<string, string> statuses = new Dictionary<string, string>();
        foreach (Effect effect in p_ActiveInputEffects)
            statuses[Enum.GetName(typeof(e_Effect), effect.p_EffectName)] = "True";
        foreach (Effect effect in p_ActivePassiveEffects)
            statuses[Enum.GetName(typeof(e_Effect), effect.p_EffectName)] = "True";
        foreach (string e_name in Enum.GetNames(typeof(e_Effect)))
        {
            if (!statuses.ContainsKey(e_name))
                statuses[e_name] = "False";
        }

        statuses["numPints"] = _numPints.ToString();
        statuses["potassiumLevels"] = _potassiumLevels.ToString();
        statuses["NumPieces"] = _player.BodyParts.Count.ToString();
        return statuses;
    }

    public void Eat(e_Food food)
    {
        switch (food)
        {
            case e_Food.Coffee:
                DrinkCoffee();
                break;
            case e_Food.Booze:
                DrinkBooze();
                break;
            case e_Food.Apple:
                EatApple();
                break;
            case e_Food.Orange:
                EatOrange();
                break;
            case e_Food.Banana:
                EatBanana();
                break;
            case e_Food.Dragonfruit:
                EatDragonfruit();
                break;
            case e_Food.Drumstick:
                EatDrumstick();
                break;
            case e_Food.Bone:
                EatBone();
                break;
            case e_Food.Cheese:
                EatCheese();
                break;
            case e_Food.Pizza:
                EatPizza();
                break;
            case e_Food.Pineapple:
                EatPineapple();
                break;
            case e_Food.PineapplePizza:
                EatPineapplePizza();
                break;
            case e_Food.IceCream:
                EatIceCream();
                break;
            case e_Food.CrapALot:
                EatCrapALot();
                break;
            case e_Food.Balti:
                EatBalti();
                break;
            case e_Food.Brownie:
                EatBrownie();
                break;
        }
    }

    private void DrinkCoffee()
    {
        Effect minor = new Effect(e_Effect.MinorSpeedBoost, Mathf.Infinity);
        Effect major = new Effect(e_Effect.MajorSpeedBoost, 10, minor, false, 0);
        AddPassiveEffect(major);
    }

    private void DrinkBooze()
    {
        Effect drunk = new Effect(e_Effect.Drunk, 100);
        Effect pissing = new Effect(e_Effect.Pissing, 10);
        AddInputEffect(pissing, 1);
        AddPassiveEffect(drunk);
    }

    private void EatApple()
    {
        ClearPassiveEffects();
    }

    private void EatOrange()
    {
        // Produce projectile
    }

    private void EatBanana()
    {
        // Produce peel
        _potassiumLevels++;
        if (_potassiumLevels >= 3)
        {
            // Die ... ?
        }
    }

    private void EatDragonfruit()
    {
        Effect fireBreath = new Effect(e_Effect.BreathingFire, 5);
        AddInputEffect(fireBreath, 0.3f);
    }

    private void EatDrumstick()
    {
        Effect buff = new Effect(e_Effect.Buff, 20);
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
        Effect unicorn = new Effect(e_Effect.Unicorn, 3, brainFreeze, false, 0);
        AddPassiveEffect(unicorn);
    }

    private void EatCrapALot()
    {
        Effect laxative = new Effect(e_Effect.Laxative, 20);
        AddInputEffect(laxative, 1);
    }

    private void EatBalti()
    {
        // Add rocket shit for 1 second after 10 seconds
        Effect rocketShit = new Effect(e_Effect.RocketShitting, 10);
        Effect balti = new Effect(e_Effect.None, 2, rocketShit, false, 0.05f);
        AddPassiveEffect(balti);
    }

    private void EatBrownie()
    {
        // Sleep for 5 turns
    }
}