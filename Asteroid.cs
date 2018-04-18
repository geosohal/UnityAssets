using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{

	public float health = 30f;

	private float currHealth;
	
	// Use this for initialization
	void Start ()
	{
		currHealth = health;
	}
	
	// Update is called once per frame
	void Update () {
		if (currHealth <= 0)
		{
			Die();
		}
	}


	public void InflictDamage(float amount)
	{
		currHealth -= amount;
	}

	private void Die()
	{
		// instantiate energy object
		GameObject hydroObj = (GameObject)Instantiate(Resources.Load("Hydrogen"));
		hydroObj.transform.position = transform.position;
		hydroObj.GetComponent<FreeHydrogen>().quantity = (uint) (health / 3);
		
		
		// remove asteroid object
		Destroy(this.gameObject);
	}
}
