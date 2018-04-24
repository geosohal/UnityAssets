using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAbility : MonoBehaviour
{
	public enum SpecialType
	{
		BasicBullet,
		Teleport,
		Burst,
		Saber,
		Laser
	};
	public float TimeToRecharge;
	public KeyCode KeyMapping;
	public SpecialType specialType;

	protected float timeLettForRecharge;
	protected bool isCharging;
	
	// Use this for initialization
	public SpecialAbility(float timeToRecharge, KeyCode activatingKey, SpecialType eSpecial)
	{
		this.TimeToRecharge = timeToRecharge;
		KeyMapping = activatingKey;
		specialType = eSpecial;
		timeLettForRecharge = 0;
		isCharging = false;
	}
	
	// Update is called once per frame
	public void UpdateTimer (float elapsedSeconds)
	{
		if (timeLettForRecharge > 0)
			isCharging = true;
		else
		{
			isCharging = false;
		}
		
		if (isCharging)
			timeLettForRecharge -= elapsedSeconds;
	}

	public bool IsCharged()
	{
		return !isCharging;
	}

	public void Use()
	{
		timeLettForRecharge = TimeToRecharge;
	}

	public bool IsChargedAndKeyPressed()
	{
		return IsCharged() && Input.GetKey(KeyMapping);
	}

	public void ResetChargeTimer()
	{
		timeLettForRecharge = TimeToRecharge;
		isCharging = true;
	}
}
