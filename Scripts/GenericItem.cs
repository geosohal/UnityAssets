using System;
using Photon.MmoDemo.Client;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// deprecated class
public class GenericItem : MonoBehaviour {

	public Item item;
	
	protected float lastMoveUpdateTime;
	protected Vector3 lastMoveUpdate;
	private float timeBetweenUpdates;


	public void Initialize(Game mmoGame, Item actorItem, string name, Radar worldRadar)
	{
		this.item = actorItem;
		this.name = name;

		transform.position = new Vector3(this.item.Position.X, transform.position.y, this.item.Position.Y) *
		                     RunBehaviour.WorldToUnityFactor;
		timeBetweenUpdates = .05f;
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
		}
	}
}
