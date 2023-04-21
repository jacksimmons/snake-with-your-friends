using System;
using System.Collections.Generic;
using UnityEngine;

public class Status
{
	public int p_NumPieces { get; set; }

	private List<BodyPartStatus> _bodyPartStatuses;

	// Positive active effects (only one at a time)
	private bool _hasPositiveActive;
	private float _positiveTimer;

	private Dictionary<string, bool> _active_effects = new Dictionary<string, bool>
	{
		{ "breathing_fire", false },
		{ "buff", false },
		{ "pissing", false }
	};

	private Dictionary<string, bool> _passive_effects = new Dictionary<string, bool>
	{
		{ "caffeinated", false },
		{ "brain_freeze", false },
		{ "hallucination", false },
		{ "too_many_pints", false },
		{ "unicorn", false },
		{ "laxative", false },
		{ "rocket_shitting", false }
	};

	// Counters
	private int _shit_o_counter;
	private float _speedIncrease;
	private int _potassiumLevels;

	public Status(List<BodyPartStatus> bpsx)
	{
		_bodyPartStatuses = bpsx;
		_shit_o_counter = 0;
		_speedIncrease = 0;
		_potassiumLevels = 0;
		p_NumPieces = 2;
	}

	public void Update(float delta)
	{
		if (_hasPositiveActive)
		{
		}
	}

	/// <summary>
	/// Disables all active effects.
	/// </summary>
	public void ClearActive()
	{
		foreach (var key in _active_effects.Keys)
		{
			_active_effects[key] = false;
		}
	}

	public void ClearPassive()
	{
		foreach (var key in _passive_effects.Keys)
		{
			_passive_effects[key] = false;
		}

		_shit_o_counter = 0;
		_speedIncrease = 0;
		_potassiumLevels = 0;
	}

	public Dictionary<string, string> GetStatusDebug()
	{
		Dictionary<string, string> statuses = new Dictionary<string, string>();
		foreach (var key in _active_effects.Keys)
			statuses[key] = _active_effects[key].ToString();
		foreach (var key in _passive_effects.Keys)
			statuses[key] = _passive_effects[key].ToString();

		statuses["shit_o_counter"] = _shit_o_counter.ToString();
		statuses["speedIncrease"] = _speedIncrease.ToString();
		statuses["potassiumLevels"] = _potassiumLevels.ToString();
		statuses["NumPieces"] = p_NumPieces.ToString();
		return statuses;
	}

	/// <summary>
	/// Handles setting
	/// the values required for a new active effect to
	/// be in place.
	/// </summary>
	/// <param name="duration">The length of time the new active
	/// effect will last.</param>
	public void NewActive(float duration)
	{
		_hasPositiveActive = true;
		_positiveTimer = duration;
	}

	public void Eat(Food food)
	{
		switch (food)
		{
			case Food.Coffee:
				DrinkCoffee();
				break;
			case Food.Booze:
				DrinkBooze();
				break;
			case Food.Apple:
				EatApple();
				break;
			case Food.Orange:
				EatOrange();
				break;
			case Food.Banana:
				EatBanana();
				break;
			case Food.FireFruit:
				EatFireFruit();
				break;
			case Food.Drumstick:
				EatDrumstick();
				break;
			case Food.Bone:
				EatBone();
				break;
			case Food.Cheese:
				EatCheese();
				break;
			case Food.Pizza:
				EatPizza();
				break;
			case Food.Pineapple:
				EatPineapple();
				break;
			case Food.PineapplePizza:
				EatPineapplePizza();
				break;
			case Food.IceCream:
				EatIceCream();
				break;
			case Food.CrapALot:
				EatCrapALot();
				break;
			case Food.Balti:
				EatBalti();
				break;
			case Food.Brownie:
				EatBrownie();
				break;
		}
	}

	private void DrinkCoffee()
	{
		NewActive(10);
		_active_effects["caffeinated"] = true;
		_speedIncrease += 0.1f;
	}

	private void DrinkBooze()
	{
		NewActive(10);
		_active_effects["pissing"] = true;
	}

	private void EatApple()
	{
		ClearPassive();
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

	private void EatFireFruit()
	{
		NewActive(5);
		_active_effects["breathing_fire"] = true;
	}

	private void EatDrumstick()
	{
		NewActive(15);
		_active_effects["buff"] = true;
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
		_passive_effects["unicorn"] = true;
	}

	private void EatCrapALot()
	{
		_passive_effects["laxative"] = true;
	}

	private void EatBalti()
	{
		// Add rocket shit for 1 second after 10 seconds
	}

	private void EatBrownie()
	{
		// Sleep for 5 turns
	}
}

public class BodyPartStatus
{
	public bool isBurning = false;
	public bool isSteel = false;

	public BodyPartStatus(bool isBurning, bool isSteel)
	{
		this.isBurning = isBurning;
		this.isSteel = isSteel;
	}
}