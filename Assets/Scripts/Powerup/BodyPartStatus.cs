using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
