//using System.Collections.Generic;
//using Photon.MmoDemo.Client;
//using UnityEngine;
//
//public class MotherBotBehaviour : BotBehaviour {
//
//	public SimpleHealthBar healthBar;
//	
//	// Use this for initialization
//	public void Initialize(Game mmoGame, Item botItem, string name, Radar worldRadar) {
//		base.Initialize(mmoGame,botItem,name, worldRadar);
//
//		healthBar = GetComponentInChildren<SimpleHealthBar>();
//	}
//	
//	void Update () {
//
//		CustomUpdate();
//
//		
//	}
//	
//	// Update is called once per frame
//	protected void CustomUpdate ()
//	{
//		/base.CustomUpdate();
//
//		healthBar.UpdateBar(currHealth, maxHealth);
//		healthBar.transform.position = transform.position + new Vector3(0,3,0);
//	}
//	
//		
//	public void TakeDamage(int amount)
//	{
//		base.TakeDamage(amount);
//		healthBar.UpdateBar(currHealth, maxHealth);
//	}
//}
