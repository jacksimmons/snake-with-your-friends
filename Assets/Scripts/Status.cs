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
	private bool _isBreathingFire;
	private bool _isPissing;

	// Passive effects (can have as many as you want)
	private bool _isFast;
	private bool _isBrainFreezed;
	private bool _isHallucinating;
	private bool _isAppleTeethed;
	private bool _isWobbling;

	// Counters
	private float _speedIncrease;
	private int _potassiumLevels;

	public Status(List<BodyPartStatus> bpsx)
	{
		_bodyPartStatuses = bpsx;
		p_NumPieces = 2;
	}

	public void Update(float delta)
	{
		if (_hasPositiveActive)
		{
		}
	}

	// ! need to update ui after
	/// <summary>
	/// Disables all positive active effects.
	/// </summary>
	public void ClearPositiveActive()
	{
		_isBreathingFire = false;
		_isPissing = false;
	}

	public void ClearPassive()
	{
		_isFast = false;
		_isBrainFreezed = false;
		_isHallucinating = false;
		_isAppleTeethed = false;
		_isWobbling = false;

		_speedIncrease = 0;
		_potassiumLevels = 0;
	}

	public Dictionary<string, string> GetStatusDebug()
	{
		Dictionary<string, string> statuses = new Dictionary<string, string>();
		statuses["isBreathingFire"] = _isBreathingFire.ToString();
		statuses["isPissing"] = _isPissing.ToString();
		statuses["isFast"] = _isFast.ToString();
		statuses["isBrainFreezed"] = _isBrainFreezed.ToString();
		statuses["isHallucinating"] = _isHallucinating.ToString();
		statuses["isAppleTeethed"] = _isAppleTeethed.ToString();
		statuses["isWobbling"] = _isWobbling.ToString();

		statuses["speedIncrease"] = _speedIncrease.ToString();
		statuses["potassiumLevels"] = _potassiumLevels.ToString();

		return statuses;
	}

	/// <summary>
	/// Resets all positive active effects, and handles setting
	/// the values required for a new positive active effect to
	/// be in place.
	/// </summary>
	/// <param name="duration">The length of time the new active
	/// effect will last.</param>
	public void NewPositiveActive(float duration)
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
		}
	}

	private void DrinkCoffee()
	{
		NewPositiveActive(10);
		_isFast = true;
		_speedIncrease += 0.1f;
	}

	private void DrinkBooze()
	{
		NewPositiveActive(10);
		_isPissing = true;
	}

	private void EatApple()
	{
		ClearPassive();
		_isAppleTeethed = true;
	}

	private void EatOrange()
	{
	}

	private void EatBanana()
	{
	}

	private void EatFireFruit()
	{
		NewPositiveActive(5);
		_isBreathingFire = true;
	}

	private void EatDrumstick()
	{
	}

	private void EatBone()
	{
	}

	private void EatCheese()
	{
	}

	private void EatPizza()
	{
	}

	private void EatPineapple()
	{
	}

	private void EatPineapplePizza()
	{
	}

	private void EatIceCream()
	{
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