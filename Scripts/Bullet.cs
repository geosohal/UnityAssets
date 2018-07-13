using System.Collections;
using System.Collections.Generic;
using Photon.MmoDemo.Client;
using UnityEngine;
using Forge3D;

public class Bullet : MonoBehaviour
{

	public float speed = 7f;
	public float maxTime = 5f;
	public float currTime = 0;
	public float damage = 1f;
	public uint playerID; // player who created bullet

	private Item item;

	private float lastMoveUpdateTime;
	private Vector3 lastMoveUpdate;
	private bool firstUpdate;
	private float secondsTillUpdate = .05f; // based on how long server delays in sec to update bullet pos

	public static int pbuffsize = 80;
	private NStateBuffer nbuffer;
	
	private GameObject vectorGrid;
	private VectorGrid gridComponent;
	private Vector2 velocityEstimate; // estimate of velocity used for grid force
    public F3DFXController fxController;


    public Item Item
    {
        get { return item; }
        set { item = value;
            this.name = item.Id;
            velocityEstimate = new Vector2(item.Rotation.X, item.Rotation.Y) * 2f; }
    }

    // Use this for initialization
    void Start () {
		
	}

	public void Initialize(Item _item, F3DFXController f3d )
	{
        fxController = f3d;
		this.item = _item;
		this.name = _item.Id;
		firstUpdate = true;
		nbuffer = new NStateBuffer(pbuffsize);
		Show(false);
		
		vectorGrid = GameObject.FindWithTag("vectorgrid");
		gridComponent = vectorGrid.GetComponentInChildren<VectorGrid>();
		velocityEstimate = new Vector2(item.Rotation.X, item.Rotation.Y)*2f;
	}


    public void Initialize(F3DFXController f3d)
    {
        fxController = f3d;
        firstUpdate = true;
        nbuffer = new NStateBuffer(pbuffsize);
        Show(false);

        vectorGrid = GameObject.FindWithTag("vectorgrid");
        gridComponent = vectorGrid.GetComponentInChildren<VectorGrid>();
      //  velocityEstimate = new Vector2(item.Rotation.X, item.Rotation.Y) * 2f;
    }

    // Update is called once per frame
    void Update () {
		
		// if item was destroyed
		if (this.item == null)
		{
			Debug.Log("bullet waz destroyed");
			//Destroy(this);
			return;
			
		}


		Vector3 newPos = new Vector3(this.item.Position.X, transform.position.y, this.item.Position.Y)*RunBehaviour.WorldToUnityFactor;

		if (firstUpdate)
		{
			transform.position = newPos;
			firstUpdate = false;
		}


		if (newPos != this.lastMoveUpdate)
		{
			this.lastMoveUpdate = newPos;
			this.lastMoveUpdateTime = Time.time;
			
			nbuffer.AddNetworkState(newPos,lastMoveUpdateTime);
			if (nbuffer.posSetCount > 0)
				Show(true);
			ApplyGridForce(velocityEstimate,5);
		}
		
		transform.position =  nbuffer.GetRewindedPos(Time.time - .1f);
		
//		if (newPos != transform.position)
//		{
//			// Debug.Log("move lerp: " + newPos);
//			// move smoothly
//			float lerpT = (Time.time - this.lastMoveUpdateTime)/secondsTillUpdate;
//			transform.position = Vector3.Lerp(transform.position, newPos, lerpT);
//		}



	}
	
	public void ApplyGridForce(float force, float radius)
	{
		if (vectorGrid != null)
			gridComponent.AddGridForce(this.transform.position, force, radius, Color.cyan, true, false);
	}
	public void ApplyGridForce(Vector2 force, float radius)
	{
		if (vectorGrid != null)
		{
			Color alphared = Color.cyan;
			alphared.a = .2f;
			gridComponent.AddGridForce(this.transform.position, force, radius, alphared, true, true);
		}
	}



    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    // MOVED TO SERVER
    /*
    if (collision.rigidbody.tag == "Enemy")
    {
        triggeringEnemy = collision.rigidbody.gameObject;
        triggeringEnemy.GetComponent<Enemy>().health -= damage;
        Destroy(this.gameObject);
    }

    if (collision.rigidbody.tag == "Asteroid")
    {
        GameObject triggeringAst = collision.rigidbody.gameObject;
        triggeringAst.GetComponent<Asteroid>().InflictDamage(damage);
        Destroy(this.gameObject);
    }
    //	}
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Enemy")
            {
                triggeringEnemy = other.gameObject;
                triggeringEnemy.GetComponent<Enemy>().health -= damage;
                Destroy(this.gameObject);
            }

            if (other.tag == "Asteroid")
            {
                GameObject triggeringAst = other.gameObject;
                triggeringAst.GetComponent<Asteroid>().health -= damage;
                Destroy(this.gameObject);
            }
        }*/

    //private void OnCollisionEnter(Collision collision)
    //{


    //    // check if other game object is an asteroid
    //    Asteroid ast = collision.gameObject.GetComponent<Asteroid>();
    //    Asteroid ast2 = collision.transform.GetComponent<Asteroid>();
    //    //        Asteroid ast3 = collision.rigidbody.GetComponent<Asteroid>();
    //    // Asteroid ast4 = collision.
    //    if (ast != null || ast2 != null || ast2 != null)
    //    {
    //        Vector3 pos = this.GetComponent<Collider>().transform.position;
    //        Debug.Log("collide at " + pos.ToString());
    //        fxController.PlasmaGunImpact(collision.transform.position);
    //        // todoj tell server we destroyed the asteroid
    //        ast.InflictDamage(damage);

    //        Destroy(this);
    //    }


    //}


    private void OnTriggerEnter(Collider other)
    {
        Vector3 pos = this.GetComponent<Collider>().transform.position;
        Debug.Log("collide at " + pos.ToString());
        fxController.PlasmaGunImpact(other.transform.position);
        ApplyGridForce(5f, 3f);
        ParticleSystem[] psystems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in psystems)
        {
            Destroy(ps);
        }
        // check if other game object is an asteroid
        Asteroid ast = other.gameObject.GetComponent<Asteroid>();
        if (ast != null)
        {
            // todoj tell server we destroyed the asteroid
            ast.InflictDamage(damage);
        }

        Destroy(this);
    }


    private bool Show(bool show)
	{
		var renderers = GetComponentsInChildren<Renderer>();
		if (renderers[0].enabled != show)
		{
			foreach (var render in renderers)
			{
				render.enabled = show;
			}

			return true;
		}
		return false;
	}
}
