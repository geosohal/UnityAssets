using System;
using System.Collections.Generic;
using Photon.MmoDemo.Client;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class BotBehaviour : MonoBehaviour {

	public Item item;
	
	protected float lastMoveUpdateTime;
	protected Vector3 lastMoveUpdate;
	protected Vector3 lastRotation;
	private float timeBetweenUpdates; // time for updates from server
	private int maxHealth = 30;
	private int currHealth;
	public SimpleHealthBar healthBar;
	private bool isMother;


	public void Initialize(Game mmoGame, Item botItem, string name, Radar worldRadar, bool isMother)
	{
		this.item = botItem;
		this.name = name;
		this.isMother = isMother;
		transform.position = new Vector3(this.item.Position.X, transform.position.y, this.item.Position.Y) *
		                     RunBehaviour.WorldToUnityFactor;
		timeBetweenUpdates = .05f;
		currHealth = maxHealth;
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
		}

		// healthBar.UpdateBar(currHealth, maxHealth);

		// move smoothly
		float lerpT = (Time.time - this.lastMoveUpdateTime) / timeBetweenUpdates;
		
		if (newPos != transform.position)
		{
			// Debug.Log("move lerp: " + newPos);
			transform.position = Vector3.Lerp(transform.position, newPos, lerpT);
			
			Vector3 currRot = new Vector3(item.Rotation.X, 0, item.Rotation.Y);
			if (currRot != lastRotation)
			{
				lastRotation = currRot;
				transform.rotation = Quaternion.LookRotation(currRot);
			}
		}
	}
	
	public void TakeDamage(int amount)
	{
		//Debug.Log(("bot taking dmg " + amount.ToString()));
		currHealth -= amount;
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
