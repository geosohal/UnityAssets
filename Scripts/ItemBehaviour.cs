// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemBehaviour.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The item behaviour.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Photon.MmoDemo.Client;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class ItemBehaviour : MonoBehaviour
{
    public Text actorText;
    public GameObject actorView;
    public GameObject actorViewExit;
    public int maxHealth = 9000;


    public Item item;
    private int currHealth;

    private float lastMoveUpdateTime;

    private Vector3 lastMoveUpdate;

    public GameObject bulletSpawnObj;
    public GameObject bullet;
    public float waitTime = .3f; // time between bullets
    private float timeSinceShot = 0;
    public SimpleHealthBar healthBar;
    private float shipTilt;
    public float tiltDamper=.62f;
    private float timeBetweenUpdates; // time for updates from server

    public float mlaserTimeLeft = 1f;
    public float msaberTimeLeft = 14f;


    public float laserTimeLeft = -1;
    public float saberTimeLeft = -1;
    

    public void Destroy()
    {
        Destroy(this); // childs and components get destroyed, too
    }


    public void Initialize(Game mmoGame, Item actorItem, string name, Radar worldRadar)
    {
        this.item = actorItem;
        this.name = name;
        timeBetweenUpdates = .012f;
        transform.position = new Vector3(this.item.Position.X, transform.position.y, this.item.Position.Y) *
                             RunBehaviour.WorldToUnityFactor;
        currHealth = maxHealth;
        healthBar = GameObject.FindWithTag("hpbar").GetComponent<SimpleHealthBar>();
        if (this.item.IsMine)
            GameObject.FindWithTag("MainCamera").GetComponent<CameraController>().playerShip = this.gameObject;
        ShowActor(false);
        
    }


    /// <summary>
    /// Updates to item logic once per frame (called by Unity).
    /// </summary>
    public void Update()
    {
        if (this.item == null || !this.item.IsUpToDate)
        {
            ShowActor(false);
            return;
        }

        if (Math.Abs(shipTilt) > .25f)
            shipTilt *= tiltDamper;
        // set rotation of our ship
        if (this.item.IsMine)
        {
            // make ship face mouse
            Plane playerPlane = new Plane(Vector3.up, transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float hitDist = 0.0f;

            if (playerPlane.Raycast(ray, out hitDist))
            {
                Vector3 targetPoint = ray.GetPoint(hitDist);
                targetPoint.y = transform.position.y;
                Vector3 newForward = targetPoint - transform.position;
                newForward = newForward.normalized;
                SetRotation(newForward); // set Item's rotation so other players can get update
                float deltaAngle = -Vector3.SignedAngle(transform.forward, newForward, Vector3.up);
                if (Math.Abs(shipTilt + deltaAngle) < 40f)
                    shipTilt += deltaAngle;
                transform.rotation = Quaternion.LookRotation(newForward, Vector3.up) * Quaternion.AngleAxis(shipTilt, Vector3.forward);
            }

            // update timer for measuring time since last shot
            if (timeSinceShot < waitTime)
                timeSinceShot += Time.deltaTime;
                
            
            // shooting
            if (Input.GetMouseButton(1))
            {
                if (timeSinceShot > waitTime)
                {
                    Shoot();
                    timeSinceShot = 0;
                }
            }
        }
        else if (this.item.IsMine == false) // set rotations of other ships using their item rotations
        {
            Vector3 shipForward = new Vector3(this.item.Rotation.X, 0, this.item.Rotation.Y);
            transform.rotation = Quaternion.LookRotation(shipForward.normalized, Vector3.up);
        }


        // you could update the radar more often by using available info about items close-by:
        // this.radar.OnRadarUpdate(this.item.Id, this.item.Type, this.item.Position);


        byte[] colorBytes = BitConverter.GetBytes(this.item.Color);
        SetActorColor(new Color((float) colorBytes[2] / byte.MaxValue, (float) colorBytes[1] / byte.MaxValue,
            (float) colorBytes[0] / byte.MaxValue));


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
        bool moveAbsolute = ShowActor(true);

        if (moveAbsolute)
        {
            // Debug.Log("move absolute: " + newPos);
            transform.position = newPos;
        }
        else if (newPos != transform.position)
        {
            // Debug.Log("move lerp: " + newPos);
            transform.position = Vector3.Lerp(transform.position, newPos, lerpT);
        }

        // view distance
        if (this.item.ViewDistanceEnter.X > 0 && this.item.ViewDistanceEnter.Y > 0)
        {
            this.actorView.transform.localScale =
                new Vector3(this.item.ViewDistanceEnter.X, 0,
                    this.item.ViewDistanceEnter.Y) * RunBehaviour.WorldToUnityFactor;
            this.actorView.transform.localScale *=
                2; // ViewDistanceEnter is +/- units from the item's position. So we have to double the quad-scale.
        }
        else
        {
            this.actorView.transform.localScale *= 0;
        }

        // exit view distance
        if (this.item.ViewDistanceExit.X > 0 && this.item.ViewDistanceExit.Y > 0)
        {
            this.actorViewExit.transform.localScale =
                new Vector3(this.item.ViewDistanceExit.X, 0, this.item.ViewDistanceExit.Y) *
                RunBehaviour.WorldToUnityFactor;
            this.actorViewExit.transform.localScale *=
                2; // ViewDistanceExit is +/- units from the item's position. So we have to double the quad-scale.
        }
        else
        {
            this.actorViewExit.transform.localScale *= 0;
        }


        // update text
        actorText.text = this.item.Text;
        if (this.item.IsMine)
        {
//            actorText.text = string.Format("{0}\n{1:0.0}:{2:0.0}\n{3:0.0}:{4:0.0}", this.item.Text, this.item.Position.X,
//               this.item.Position.Y, this.transform.position.x, this.transform.position.z);

            actorText.text = this.item.Text;
            actorText.fontSize = 9;
            

            // update ship's angle
        }
        else
        {
            actorText.text = string.Format("{0}", this.item.Text);
        }
    }

    private void SetRotation(Vector3 newForward)
    {
        //  item.SetRotation(newForward.x, newForward.y, newForward.z);
        ((RunBehaviour) GameObject.FindGameObjectWithTag("GameController").GetComponent<RunBehaviour>())
            .DoRotationOnly(newForward);
    }


    private void SetActorColor(Color actorColor)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var render in renderers)
        {
            render.material.color = actorColor;
        }

        this.actorText.color = actorColor;

        actorColor.a = 0.10f;
        this.actorView.GetComponent<Renderer>().material.color = actorColor;
        actorColor.a = 0.05f;
        this.actorViewExit.GetComponent<Renderer>().material.color = actorColor;
    }


    private bool ShowActor(bool show)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        if (renderers[0].enabled != show)
        {
            foreach (var render in renderers)
            {
                // dont toggle renderer for specials, only the specials toggle those
                if (render.gameObject.tag == "laser" || render.gameObject.tag == "saber")
                    continue;
                render.enabled = show;
            }

            return true;
        }

        this.actorText.enabled = show;

        return false;
    }

    void Shoot()
    {
        if (this.item.IsMine)
        {
            // Instantiate(bullet, bulletSpawnObj.transform.position, transform.rotation );
        }

        Vector3 fwd = transform.forward;
        ((RunBehaviour) GameObject.FindGameObjectWithTag("GameController").GetComponent<RunBehaviour>()).DoSpawnBullet(
            bulletSpawnObj.transform.position, fwd.x, fwd.z);
    }

    public void TakeDamageAndUpdateBar(int amount)
    {
        currHealth -= amount;
        healthBar.UpdateBar(currHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        currHealth -= amount;
    }
}