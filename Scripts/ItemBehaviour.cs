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
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// a snapshot of values received over the network
//public struct NetworkState
//{
//    public Vector3 pos;
//    public float totalMs;
//    public NetworkState( Vector3 pos, float time )
//    {
//        this.pos = pos;
//        this.totalMs = time;
//    }
//}


public class ItemBehaviour : MonoBehaviour
{
    public Text actorText;
    public GameObject actorView;
    public GameObject actorViewExit;
    public GameObject playerCam;
    public int maxHealth = 500;


    public Item item;
    private int currHealth;

    private float lastMoveUpdateTime;
    private float lastlastMoveUpdateTime;

    private Vector3 lastMoveUpdate;
    private Vector2 lastlastMoveUpdate;
    

    public GameObject bulletSpawnObj;
    public GameObject bullet;
    public float waitTime = .3f; // time between bullets
    private float timeSinceShot = 0;
    public SimpleHealthBar healthBar;
    private float shipTilt;
    public float tiltDamper=.62f;
    private float timeBetweenUpdates =.05f; // time for updates from server

    public float mlaserTimeLeft = 2.1f;
    public float msaberTimeLeft = 10f;


    public float laserTimeLeft = -1;
    public float saberTimeLeft = -1;
    
    private Vector3 lastPos;
    private float displacement2LastFrame;	// displacement made this last frame
    private float displacement2Last2Frame;	// displacement made 2 frames ago

    private float lastTime;
    private float timeSincePosChange = 0;

    public static int pbuffsize = 80;
    private NStateBuffer nbuffer;
    private float velocityChangeAllowance2 = .005f;

    public bool isThirdPerson;
    private GameObject hudObj;

    public bool isSuperFast;

    private MeshRenderer wireBall;
    private bool firstUpdate;

    private Vector3 prevMouse;

    public bool IsWASDfirst;

    public GameObject vectorGrid;

    public void Destroy()
    {
        Destroy(this); // childs and components get destroyed, too
    }


    public void Initialize(Game mmoGame, Item actorItem, string name, Radar worldRadar)
    {
        isSuperFast = false;
        this.item = actorItem;
        this.name = name;
        transform.position = new Vector3(this.item.Position.X, transform.position.y, this.item.Position.Y) *
                             RunBehaviour.WorldToUnityFactor;
        currHealth = maxHealth;
        healthBar = GameObject.FindWithTag("hpbar").GetComponent<SimpleHealthBar>();
        if (this.item.IsMine)
        {
            playerCam = GameObject.FindWithTag("MainCamera");
            playerCam.GetComponent<CameraController>().playerShip = this.gameObject;
            vectorGrid = GameObject.FindWithTag("vectorgrid");
            //vectorGrid.GetComponent<VectorGrid>().
        }
        //        GameObject.FindWithTag("MainCamera").GetComponent<MSCameraController>().playerObject = this.gameObject;
            
        ShowActor(false);
        lastTime = 0;
        nbuffer = new NStateBuffer(pbuffsize);
        hudObj = GameObject.FindGameObjectWithTag("Hud");
        firstUpdate = true;
        // turn off mesh render for wireball
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in mrs)
        {
            if (mr.gameObject.tag == "wireball")
            {
                wireBall = mr;
                wireBall.enabled = false;
                Debug.Log("wireball INIT " + wireBall.enabled.ToString());
                break;
            }
        }

        prevMouse = Input.mousePosition;
    }

    public void ToggleWireBallMode(bool val)
    {
        wireBall.enabled = val;
        isSuperFast = val;
    }
    public void ToggleWireBall()
    {
        wireBall.enabled = !wireBall.enabled;
        isSuperFast = !isSuperFast;
        Debug.Log("ball toggled to " + wireBall.enabled.ToString());
    }

    public void ApplyGridForce(float force, float radius)
    {
        vectorGrid.GetComponent<VectorGrid>().AddGridForce(this.transform.position, force, radius, Color.cyan, true);
    }


    /// <summary>
    /// Updates to item logic once per frame (called by Unity).
    /// </summary>
    public void LateUpdate()
    {
        if (this.item == null || !this.item.IsUpToDate)
        {
            ShowActor(false);
            return;
        }
        float elapsedSec = Time.time - lastTime;


        lastPos = transform.position;
        if (Math.Abs(shipTilt) > .25f)
            shipTilt *= tiltDamper;
        // set rotation of our ship
        if (this.item.IsMine)
        {
            if (!isThirdPerson)
            {
                Cursor.visible = true;
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

                    float deltaAngle = -Vector3.SignedAngle(transform.forward, newForward, Vector3.up);
                    hudObj.GetComponent<Text>().text = deltaAngle.ToString();


                    if (isThirdPerson)
                    {
                        if (deltaAngle > 20f || deltaAngle < -20f
                        ) // change ship angle only if mouse is out of center line
                        {
                            newForward = Vector3.Lerp(transform.forward, newForward,
                                (Math.Abs(deltaAngle) - 20f) / 350f);
                            hudObj.GetComponent<Text>().text = deltaAngle.ToString() + "r";
                        }
                        else
                        {
                            newForward = Vector3.Lerp(transform.forward, newForward, Math.Abs(deltaAngle) / 1350f);

                        }

                        deltaAngle *= .1f;
                    }

                    if (Math.Abs(shipTilt + deltaAngle) < 40f)
                        shipTilt += deltaAngle;

                    transform.rotation = Quaternion.LookRotation(newForward, Vector3.up) *
                                         Quaternion.AngleAxis(shipTilt, Vector3.forward);
                    SetRotation(newForward); // set Item's rotation so other players can get update

                }
            }
            else if (isThirdPerson)
            {
                Cursor.visible = false;
                
                Vector3 deltaMousePos = Input.mousePosition - prevMouse;
                deltaMousePos *= .01f;
                float deltaX = deltaMousePos.x;
                if (Input.GetKey(KeyCode.A))
                    deltaX -= .03f;
                if (Input.GetKey(KeyCode.D))
                    deltaX += .03f;
                Vector3 newFwd = Quaternion.EulerRotation(0, deltaX, 0) * transform.forward;

                if (Math.Abs(shipTilt + deltaX) < 40f)
                    shipTilt += - deltaX*30f;

                transform.rotation = Quaternion.LookRotation(newFwd, Vector3.up) *
                                     Quaternion.AngleAxis(shipTilt, Vector3.forward);
                SetRotation(newFwd); // set Item's rotation so other players can get update

                prevMouse = Input.mousePosition;
            }
                        
            // shooting
            if (Input.GetMouseButton(1))
            {
                if (timeSinceShot > waitTime)
                {
                    Debug.Log("wireball TEST " + wireBall.enabled.ToString());
                    Shoot();
                    timeSinceShot = 0;
                }
            }
            // update timer for measuring time since last shot
            if (timeSinceShot < waitTime)
                timeSinceShot += Time.deltaTime;
        }

        else if (this.item.IsMine == false) 
        {
            // set rotations of other ships using their item rotations
            Vector3 shipForward = new Vector3(this.item.Rotation.X, 0, this.item.Rotation.Y);
            transform.rotation = Quaternion.LookRotation(shipForward.normalized, Vector3.up);
            
            // detect if other ship is using thrusters by if their velocity changed
            if (this.SeemsToBeThrusting())
            {
                ParticleSystem[] psystems = GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in psystems)
                {
                    if (ps.gameObject.CompareTag("engineThrust"))
                    {
                        ps.transform.rotation =
                            transform.rotation * Quaternion.AngleAxis(180, Vector3.up);
//                        if (isMegaThrusting)
//                        {
//                            ps.startColor = new Color(.2f,.4f,1,1);
//                            ps.Emit(3);
//                        }
//                        else
                        {
                            ps.startColor = Color.yellow;
                            ps.Emit(1);
                        }

                    }
                    //  if (isMegaThrusting && ps.gameObject.CompareTag("megaThrust"))
                    {
                        //   ps.Emit(1);
                        //        ps.transform.rotation =
                        //           clientsPlayer.transform.rotation * Quaternion.AngleAxis(-90, Vector3.forward);
                    }
                }
            }

        }

        // if ship is in ball mode then rotate its wireball mesh
      //  if (wireBall.enabled)
        {
      //      wireBall.gameObject.transform.RotateAround(Vector3.left, elapsedSec*60f);
        }
       // healthBar.UpdateBar(currHealth, maxHealth);
            

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

            nbuffer.AddNetworkState(newPos,Time.time);

        }

        bool moveAbsolute = ShowActor(true);
        transform.position =  nbuffer.GetRewindedPos(Time.time - .1f);

        if (this.item.IsMine)
            playerCam.GetComponent<CameraController>().SetPlayerPos();


//        // move smoothly
//        float lerpT = (Time.time - this.lastMoveUpdateTime) / timeBetweenUpdates;
//        Debug.Log("lerpt " + lerpT.ToString() +" lmupt " + this.lastMoveUpdateTime.ToString() +
//                  "tm " + Time.time.ToString());
//        bool moveAbsolute = ShowActor(true);
//        moveAbsolute = false;
//        if (moveAbsolute)
//        {
//            // Debug.Log("move absolute: " + newPos);
//            transform.position = newPos;
//        }
//        else if (newPos != transform.position)
//        {
//            // Debug.Log("move lerp: " + newPos);
//            transform.position = Vector3.Lerp(transform.position, newPos, lerpT);
//        }

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
        
        if (firstUpdate)
        {
            //     wireBall.enabled = !wireBall.enabled;
            firstUpdate = false;
            Debug.Log("wireball fu " + wireBall.enabled.ToString());
        }

        if (wireBall.enabled)
            pbuffsize = 80;
        
        displacement2Last2Frame = displacement2LastFrame;
        displacement2LastFrame = (transform.position - lastPos).sqrMagnitude;
        lastTime = Time.time;
    }
    // detect if other ship is using thrusters by if their velocity changed by a significant enough amount
    private bool SeemsToBeThrusting()
    {
        if (displacement2Last2Frame - displacement2LastFrame > velocityChangeAllowance2)
            return true;
        return false;
    }
    
    
    public bool SeemsToBeTeleporting()
    {
        if (displacement2LastFrame > 400)
        {
            if (displacement2Last2Frame > 400)
                return false;
            return true;
        }

        return false;
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
                if (render.gameObject.tag == "laser" || render.gameObject.tag == "saber" || render.gameObject.tag == "wireball")
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
        FlySparks();

    }


    public void TakeDamage(int amount)
    {
        currHealth -= amount;
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

}