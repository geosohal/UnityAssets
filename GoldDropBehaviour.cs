using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldDropBehaviour : MonoBehaviour {

    ItemBehaviour lockedPlayer;
    public float moveToPlayerSpeed = 10f;
    public float distForConsumption = 7f;
    public int goldAmount = 10;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (lockedPlayer != null)
        {
            Vector3 goldToPlayer = lockedPlayer.transform.position - transform.position;
            float distToPlayer = Vector3.Magnitude(goldToPlayer);
            if (distToPlayer < distForConsumption)
            {
                lockedPlayer.AddGold(goldAmount);
                // todoj
                // play resource collected effects
                Destroy(this);
            }
            else
                this.GetComponent<Rigidbody>().velocity = goldToPlayer.normalized * moveToPlayerSpeed;
        }
	}

    //private void OnTriggerEnter2D(Collider collision)
    //{
    //    if (collision.gameObject.tag == "Player")
    //    {
    //        lockedPlayer = collision.gameObject.GetComponent<ItemBehaviour>();
    //    }
    //}
}
