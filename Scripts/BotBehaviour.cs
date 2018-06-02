using System;
using System.Collections.Generic;
using Photon.MmoDemo.Client;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class BotBehaviour : MonoBehaviour {

	public Item item;
	
	private float lastMoveUpdateTime;
	private Vector3 lastMoveUpdate;
	private Vector3 lastRotation;
	private float timeBetweenUpdates; // time for updates from server
	private int maxHealth = 660;
	private int currHealth;
	public SimpleHealthBar healthBar;
	private bool isMother;	
	public static int pbuffsize = 80;
	private NStateBuffer nbuffer;
	private Canvas guiCanvas;

	public void Initialize(Game mmoGame, Item botItem, string name, Radar worldRadar, bool isMother)
	{
		this.item = botItem;
		this.name = name;
		this.isMother = isMother;
		transform.position = new Vector3(this.item.Position.X, transform.position.y, this.item.Position.Y) *
		                     RunBehaviour.WorldToUnityFactor;
		timeBetweenUpdates = .05f;
		currHealth = maxHealth;
		nbuffer = new NStateBuffer(pbuffsize);
		
		healthBar = GetComponentInChildren<SimpleHealthBar>();
		if (isMother)
		{
			guiCanvas = GameObject.FindGameObjectWithTag("gui").GetComponentInChildren<Canvas>();
			healthBar.transform.parent = guiCanvas.transform;
		}		
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 newPos = new Vector3(this.item.Position.X, transform.position.y, this.item.Position.Y) *
		                 RunBehaviour.WorldToUnityFactor;

		if (newPos != this.lastMoveUpdate)
		{
			this.lastMoveUpdate = newPos;
			this.lastMoveUpdateTime = Time.time;
			
			nbuffer.AddNetworkState(newPos,Time.time);
			Vector3 currRot = new Vector3(item.Rotation.X, 0, item.Rotation.Y);
			if (currRot != lastRotation)
			{
				lastRotation = currRot;
				transform.rotation = Quaternion.LookRotation(currRot);
			}
		}

		// healthBar.UpdateBar(currHealth, maxHealth);

		transform.position =  nbuffer.GetRewindedPos(Time.time - .1f);
	}
	
	public void TakeDamage(int amount)
	{
		//Debug.Log(("bot taking dmg " + amount.ToString()));
		currHealth -= amount;
		if (isMother)
			healthBar.UpdateBar(currHealth, maxHealth);
		FlySparks();
	}
	
	private void FlySparks()
	{
		ParticleSystem[] psystems = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem ps in psystems)
		{
			if (ps.gameObject.CompareTag("damageSpark") && !ps.isPlaying)
			{
				ps.Play();
				return;
			}
		}
	}

	public void DeathAnimation(List<Tuple<GameObject,float>> expirables)
	{
		ParticleSystem[] psystems = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem ps in psystems)
		{
			if (ps.gameObject.CompareTag("damageSpark"))
			{
				GameObject newObj = new GameObject();
				ParticleSystem copyps = newObj.AddComponent<ParticleSystem>();
				copyps.Stop();
				HelperUtility.GetCopyOf(copyps, ps);
				copyps.Play();
				expirables.Add( new Tuple<GameObject,float>(newObj, 5f) ) ;
				return;
			}
		}
	}
}
