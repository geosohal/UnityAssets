using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.UI;

public class SpecialAbility : MonoBehaviour
{
	public enum SpecialType
	{
		BasicBullet,
		Teleport,
		Burst,
		Saber,
		Laser,
		Bomb,
		Shield,
		BallMode
	};
	public float TimeToRecharge;
	public KeyCode KeyMapping;
	public SpecialType specialType;
	public float mpCost;

	protected float timeLettForRecharge;
	protected bool isCharging;

	private Text hud;

	
	// Use this for initialization
	public SpecialAbility(float timeToRecharge, KeyCode activatingKey, SpecialType eSpecial, Text hudText, float mp)
	{
		this.TimeToRecharge = timeToRecharge;
		KeyMapping = activatingKey;
		specialType = eSpecial;
		timeLettForRecharge = 0;
		isCharging = false;
		hud = hudText;
		mpCost = mp;
	}
	
	// Update is called once per frame
	public void UpdateTimer (float elapsedSeconds)
	{
		if (timeLettForRecharge > 0)
			isCharging = true;
		else if (isCharging)
		{
			hud.text = "Special Ready: " + specialType.ToString();
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
