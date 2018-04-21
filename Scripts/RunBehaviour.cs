// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RunBehaviour.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The run behaviour.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System;
using ExitGames.Client.Photon;
using Photon.MmoDemo.Client;
using Photon.MmoDemo.Common;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using sysr = System.Random;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// The run behaviour. note z coordinate on client is y coord on the server
/// </summary>
public class RunBehaviour : MonoBehaviour, IGameListener
{
    
    public enum ControlsType
    {
        WasdFirst = 0,
        DirectionFirst = 1,
    }

    public ControlsType controlsType;
    public bool DebugLog; // to be set in inspector
    public GameObject ActorPrefab; // to be set in Inspector. the prefab for items/actors
    public GameObject BulletPrefab;

    /// <summary>
    /// defines how world-units get translated to unity-units.
    /// </summary>
    /// <remarks>
    /// If we want to display a whole world, then it needs to be "shrinked" by a factor.
    /// Showing a world 1:1 in unity-units and then moving the Camera away will only cause flicker.
    /// </remarks>
    public static float WorldToUnityFactor; // set by code

    public float MaxUnityUnitsForWorld = 1000; // inspector can override

    private Game game;
    public WorldData world;
    public Text WorldNameText; // Text component in new GUI set in inspector
    public Text WorldXUnitsText;
    public Text WorldYUnitsText;
    public Text AreaUnitsXText;
    public Text AreaUnitsYText;
    public float ThrustForce = 5f;
    public float MaxVel = 10f;
    public float breakDampFactor;
    public float megaThrustFactor;
    public float megaMaxVelFactor;

    public float maxVelSq;
    public float MegaThrustFadePerSec; // amount to decrease max thrust per second, value of 1 decreases it by 1 per/sec
    private float lastKeyPress;
    private Vector? lastMovePosition;
    private Vector? lastRotation;
    private Vector lastRotStored;
    private Vector2 avatarVelocity;
    private float nextMoveTime;
    private float nextRotTime;
    private GameObject actorGo;
    private ItemBehaviour clientsPlayer;    // itembehaviour belonging to the client who runs this code
    private Dictionary<string, ItemBehaviour> actorTable;
    private float lastTime;
    
    // speed boost related members
    private float currMegaThrust = 1f;
    private float currMaxVel;
    private float currMaxVelSq;
    private bool isMegaThrusting;
    private List<SpecialAbility> abilities;
    
    private bool wasTeleported = false;

    public Game Game
    {
        get { return this.game; }
        set { this.game = value; }
    }


    public bool IsDebugLogEnabled
    {
        get { return this.DebugLog; }
    }

    public void Awake()
    {
        if (IsDebugLogEnabled)
        {
            Debug.Log("Awake");
        }
        DemoSettings settings = DemoSettings.GetDefaultSettings();
        this.Game = new Game(this, settings, "unity");
        var peer = new PhotonPeer(this.Game, settings.UseTcp ? ConnectionProtocol.Tcp : ConnectionProtocol.Udp) { ChannelCount = 3 };
        this.Game.Initialize(peer);
        actorTable = new Dictionary<string, ItemBehaviour>();
        avatarVelocity = new Vector2(0,0);
        maxVelSq = MaxVel * MaxVel;
        lastRotStored = new Vector(1,0);
        controlsType = ControlsType.DirectionFirst;
        maxVelSq = MaxVel * MaxVel;
        currMaxVelSq = maxVelSq;
        currMaxVel = MaxVel;
        abilities = new List<SpecialAbility>();
        SpecialAbility tpability = new SpecialAbility(2f, KeyCode.Mouse0, SpecialAbility.SpecialType.Teleport);
        abilities.Add(tpability);
        wasTeleported = false;
        isMegaThrusting = false;
    }

    public void Start()
    {
        // Make the game run even when in background
        Application.runInBackground = true;
#if UNITY_IPHONE
            iPhoneSettings.verticalOrientation = false;
            iPhoneSettings.screenCanDarken = false;
#endif
        
        if (IsDebugLogEnabled)
        {
            Debug.Log("Start");
        }

        Debug.Log("Connect");
        
        this.Game.Connect();
        
        // set default interest area size
        InterestArea area;
                  Game.TryGetCamera(0, out area);
                  Vector viewDistance = new Vector(8001,8001);
                  viewDistance.X = Math.Max(0, viewDistance.X - (Game.WorldData.TileDimensions.X/2));
                  viewDistance.Y = Math.Max(0, viewDistance.Y - (Game.WorldData.TileDimensions.Y/2));
          
                  area.SetViewDistance(viewDistance);
                  
        // access this client's avatar via:
        // this.Game.Avatar
        lastTime = 0;
    }


    public void Update()
    {
        float elapsedSec = Time.time - lastTime;
        Game.Update();

        
        
        if (Game.WorldEntered)
        {
            // send queued velocity 20 times/sec
            SendVelocityRot();
            

            ReadKeyboardInput(elapsedSec);
            foreach (SpecialAbility sa in abilities)
            {
                sa.UpdateTimer(elapsedSec);
                if (sa.IsChargedAndKeyPressed())
                {
                    sa.ResetChargeTimer();
                    switch (sa.specialType)
                    {
                        case SpecialAbility.SpecialType.Teleport:
                            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                            RaycastHit hit;
                            MoveActorToMousePosition();
                            if (this.lastRotation.HasValue)
                                Game.Avatar.MoveAbsolute(this.lastMovePosition.Value, (Vector)this.lastRotation);
                            else
                                Game.Avatar.MoveAbsolute(this.lastMovePosition.Value, Game.Avatar.Rotation);
                            Cursor.lockState = CursorLockMode.Locked;
                            Cursor.lockState = CursorLockMode.None; 
                            wasTeleported = true;
                            break;
                    }
                }
            }
        }

//        if (avatarVelocity.sqrMagnitude > 0.1f && !wasTeleported)
//        {
//            MoveRelative(new Vector(avatarVelocity.x, avatarVelocity.y) * elapsedSec);
//        }

        
        lastTime = Time.time;

    }

    public void OnApplicationQuit()
    {
        this.Game.Disconnect();       
    }

    public void OnGUI()
    {
        if (this.Game != null && this.Game.WorldEntered && Event.current != null)
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                if (Event.current.delta.y < 0)
                {
                    DecreaseViewDistance();
                    
                }
                else if (Event.current.delta.y > 0)
                {
                    IncreaseViewDistance();
                }
            }
            else if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 2)
                {
                    InterestArea cam;
                    this.Game.TryGetCamera(0, out cam);
                    cam.ResetViewDistance();
                }
//                else if (Event.current.button == 0)
//                {
//                    this.MoveActorToMousePosition();
//                }
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
//                if (Event.current.button == 0)
//                {
//                    this.MoveActorToMousePosition();
//                }
            }
        }
    }

    private void CreateBullet(Item bulletItem)
    {
        //Operations.SpawnItem(Game, Game.Avatar.Id + "_blt_" + RandomString(5), ItemType.Bullet, new Vector(pos.x, pos.y, pos.z), new Vector(velX,0,velZ), null, true );
        GameObject newbullet = Instantiate(BulletPrefab, new Vector3(bulletItem.Position.X, 0, bulletItem.Position.Y)* WorldToUnityFactor, 
            Quaternion.LookRotation( new Vector3(bulletItem.Rotation.X, 0, bulletItem.Rotation.Y)) );
        Debug.Log("brot:" + bulletItem.Rotation.ToString());
        Bullet bull = newbullet.GetComponent<Bullet>();
        if (bull == null)
            bull = newbullet.AddComponent(typeof(Bullet)) as Bullet;
        bull.Initialize(bulletItem);
    }

    private void CreateActor(Item actorItem)
    {
        actorGo = (GameObject) Instantiate(this.ActorPrefab);
        ItemBehaviour ib = actorGo.GetComponent<ItemBehaviour>();
        if (ib == null)
        {
            ib = actorGo.AddComponent(typeof (ItemBehaviour)) as ItemBehaviour;
        }
        ib.Initialize(this.Game, actorItem, this.ItemObjectName(actorItem), null);
        Debug.Log("adding player id: " + actorGo.GetComponent<ItemBehaviour>().item.Id.ToString());
        if (!actorTable.ContainsKey(actorItem.Id))
            actorTable.Add(actorItem.Id, ib);
        if (actorItem.IsMine)
        {
            Debug.Log("adding mine player");
            clientsPlayer = ib;
        }
    }

    #region IGameListener

    public void LogDebug(object message)
    {
        Debug.Log(message);
    }


    public void LogError(object message)
    {
        Debug.LogError(message);
    }


    public void LogInfo(object message)
    {
        Debug.Log(message);
    }


    public void OnCameraAttached(string itemId)
    {
        throw new NotImplementedException();
    }


    public void OnCameraDetached()
    {
        throw new NotImplementedException();
    }


    public void OnConnect()
    {
        Debug.Log("connected");
    }


    public void OnDisconnect(StatusCode returnCode)
    {
        Debug.Log("disconnected");
    }


    public void OnItemAdded(Item item)
    {
        if (Game != null)
        {
            if (IsDebugLogEnabled)
            {
                Debug.Log("add item " + item.Id);
            }
            if (item.Id.StartsWith("bt"))
                CreateBullet(item);
            else // create player
                CreateActor(item);
        }
    }


    public void OnItemRemoved(Item item)
    {
        string objName;
        if (item.Type == ItemType.Bullet)
            objName = item.Id;
        else 
            objName = this.ItemObjectName(item);
        GameObject obj = GameObject.Find(objName);
        // if this doesnt work for bullet destroy remove the "item_" appendage
        if (obj != null)
        {
            Destroy(obj);
            if (IsDebugLogEnabled)
            {
                Debug.Log("destroy item " + objName);
            }
        }
        else
        {
            Debug.LogError("destroy item not found " + item.Id);
        }
    }

    private string ItemObjectName(Item item)
    {
        return "Item_" + item.Id;
    }


    public void OnItemSpawned(string itemId)
    {
      //  Instantiate(bullet, bulletSpawnObj.transform.position, transform.rotation );
    }


    public void OnWorldEntered()
    {
        this.world = this.Game.WorldData;
        WorldToUnityFactor = 1/(this.world.Width/this.MaxUnityUnitsForWorld);
        Debug.Log(this.world.ToString() + " factor: " + WorldToUnityFactor);


        // update UI
        if (this.WorldNameText != null)
        {
            this.WorldNameText.text = "\"" + this.world.Name + "\"";
        }
        if (this.WorldXUnitsText != null)
        {
            this.WorldXUnitsText.text = "" + this.world.Width;
        }
        if (this.WorldYUnitsText != null)
        {
            this.WorldYUnitsText.text = "" + this.world.Height;
        }
        if (this.AreaUnitsXText != null)
        {
            this.AreaUnitsXText.text = ""+this.world.TileDimensions.X;
        }
        if (this.AreaUnitsYText != null)
        {
            this.AreaUnitsYText.text = "" + this.world.TileDimensions.Y;
        }


        float tilesX = this.world.Width/this.world.TileDimensions.Y;
        float tilesY = this.world.Height/this.world.TileDimensions.X;

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.name = "Plane";
        plane.transform.position = new Vector3(0, -0.5f, 0);

        Material mat = new Material((Material) Resources.Load("TileMaterial"));
        mat.name = "TileMat Light";
        Material matDark = new Material((Material) Resources.Load("TileMaterial"));
        matDark.color = matDark.color*0.75f;
        matDark.name = "TileMat Dark";

        for (int y = 0; y < tilesY; y++)
        {
            for (int x = 0; x < tilesX; x++)
            {
                float posX = x*this.world.TileDimensions.Y;
                float posY = y*this.world.TileDimensions.X;


                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = "tile";
                tile.transform.RotateAround(tile.transform.position, new Vector3(1, 0, 0), 90);


                tile.transform.GetComponent<Renderer>().material = (x + y)%2 == 0
                    ? matDark
                    : mat;
                
                tile.transform.parent = plane.transform;
                tile.transform.localPosition = new Vector3(posX + (this.world.TileDimensions.Y/2f), 0f, posY + (this.world.TileDimensions.X/2f)) * WorldToUnityFactor;
                tile.transform.localScale = new Vector3(this.world.TileDimensions.Y/1f, this.world.TileDimensions.X/1f, this.world.TileDimensions.X/1f) * WorldToUnityFactor;
            }
        }


        // set camera in the middle of base line, calculate height with pyhtagoros (60 degree triangle)
        //float cameraHeight = Mathf.Sqrt(Mathf.Pow(maxDimension, 2) - (maxDimension / 4f));
        //cam.transform.position = new Vector3(world.Width / 2f, cameraHeight, world.Height) * WorldToUnityFactor;

        // set camera to a lower height, calculate z offset with pyhtagoros (approximate)
        // float cameraHeight = maxDimension  / 2;
        // transform.position = new Vector3( world.Width / 2f, cameraHeight, world.Height + Mathf.Sqrt( Mathf.Pow(1.6f * maxDimension, 2) - Mathf.Pow(cameraHeight, 2) ) - maxDimension);
        //cam.transform.LookAt(new Vector3(world.Width / 2f, 0, world.Height / 2f) * WorldToUnityFactor);

        Debug.Log("Adding player.");
        CreateActor(game.Avatar);
		game.Avatar.MoveAbsolute(GetRandomPosition(), Vector.Zero);
        Operations.RadarSubscribe(game.Peer, game.WorldData.Name);

        // initialize the visual radar component
        Radar r = GetComponent<Radar>();
        if (r != null)
        {
            Vector min = game.WorldData.BoundingBox.Min;
            Vector max = game.WorldData.BoundingBox.Max;
            r.worldRect = new Rect(min.X, min.Y, max.X, max.Y);
            r.selfId = game.Avatar.Id + game.Avatar.Type;
        }

    }
	
	private readonly System.Random random = new System.Random();
	private Vector GetRandomPosition()
	{
		var d = game.WorldData.BoundingBox.Max - game.WorldData.BoundingBox.Min;
		return game.WorldData.BoundingBox.Min + new Vector { X = d.X * (float)this.random.NextDouble(), Y = d.Y * (float)this.random.NextDouble() };
	}
	
    #endregion

    private void DecreaseViewDistance()
    {
        InterestArea area;
        Game.TryGetCamera(0, out area);
        Vector viewDistance = new Vector(area.ViewDistanceEnter);
        viewDistance.X = Math.Max(0, viewDistance.X - (Game.WorldData.TileDimensions.X/2));
        viewDistance.Y = Math.Max(0, viewDistance.Y - (Game.WorldData.TileDimensions.Y/2));

        area.SetViewDistance(viewDistance);
    }


    private void IncreaseViewDistance()
    {
        InterestArea area;
        Game.TryGetCamera(0, out area);
        Vector viewDistance = new Vector(area.ViewDistanceEnter);
        viewDistance.X = Math.Min(Game.WorldData.Width, viewDistance.X + (Game.WorldData.TileDimensions.X/2));
        viewDistance.Y = Math.Min(Game.WorldData.Height, viewDistance.Y + (Game.WorldData.TileDimensions.Y/2));
        area.SetViewDistance(viewDistance);
    }

    private void SendVelocityRot()
    {
        if (Time.time > this.nextMoveTime)
        {
            
            if (this.avatarVelocity.sqrMagnitude > .1f)
            {
                if (this.lastRotation.HasValue)
                {
                    Vector lastRot = (Vector) lastRotation;
                    Game.Avatar.VelocityRotation(new Vector(avatarVelocity.x, avatarVelocity.y),
                       new Vector(lastRot.X, lastRot.Y), isMegaThrusting);
                }
                else
                {
                    Debug.Log("warning null rotation shouldnt happen");
                    Game.Avatar.VelocityRotation(new Vector(avatarVelocity.x, avatarVelocity.y), null, isMegaThrusting);
                }
                avatarVelocity = Vector2.zero;
            }

            // up to 20 times per second
            this.nextMoveTime = Time.time + 0.05f;
        }
    }

    
    private void Move()
    {
        if (Time.time > this.nextMoveTime)
        {
            wasTeleported = false;
            if (this.lastMovePosition.HasValue)
            {
                if (this.lastRotation.HasValue)
                    Game.Avatar.MoveAbsolute(this.lastMovePosition.Value, (Vector)this.lastRotation);
                else
                    Game.Avatar.MoveAbsolute(this.lastMovePosition.Value, Game.Avatar.Rotation);
                this.lastMovePosition = null;
            }
            else if (this.lastRotation.HasValue)
            {
                Game.Avatar.MoveAbsolute(Game.Avatar.Position, (Vector) this.lastRotation);
                this.lastRotation = null;
            }

            // up to 20 times per second
            this.nextMoveTime = Time.time + 0.05f;
        }
    }

    private void MoveAbsolute(Vector newPosition)
    {
        OnRadarUpdate(Game.Avatar.Id, Game.Avatar.Type, newPosition, false);
        this.lastMovePosition = newPosition;
    }


    private void MoveActorToMousePosition()
    {
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, UnityEngine.Camera.main.farClipPlane, 1))
        {
            Vector newPosition = new Vector();
            newPosition.X = hit.point.x/RunBehaviour.WorldToUnityFactor;
            newPosition.Y = hit.point.z/RunBehaviour.WorldToUnityFactor;
            Debug.Log("new pos" + newPosition.ToString());
            MoveAbsolute(newPosition);
        }
    }


    private void MoveRelative(Vector offset)
    {
        Vector newPosition = new Vector(Game.Avatar.Position);
        DemoSettings settings = (DemoSettings) Game.Settings;
        newPosition.X += offset.X*settings.AutoMoveVelocity;
        newPosition.Y += offset.Y*settings.AutoMoveVelocity;
        MoveAbsolute(newPosition);
    }

    public void SetAvatarName(string name)
    {
        Debug.Log("SetAvatarName");
        Game.Avatar.SetText(name);
    }



    private void ReadKeyboardInput(float elapsedSec)
    {
        if (Input.GetKey(KeyCode.Keypad5) || Input.GetKey(KeyCode.C))
        {
            if (this.lastKeyPress + 0.3f < Time.time)
            {
                Vector newPosition = new Vector();
                newPosition.X = Game.WorldData.Width/2 + Game.WorldData.BoundingBox.Min.X;
                newPosition.Y = Game.WorldData.Height/2 + Game.WorldData.BoundingBox.Min.Y;
                Game.Avatar.MoveAbsolute(newPosition, Game.Avatar.Rotation);
                this.lastKeyPress = Time.time;
            }
        }

        DemoSettings settings = (DemoSettings) Game.Settings;

        if (Input.GetKey(KeyCode.M))
        {
            if (this.lastKeyPress + 0.3f < Time.time)
            {
                settings.AutoMove = !settings.AutoMove;
                this.lastKeyPress = Time.time;
            }
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isMegaThrusting = true;
        }
        else
        {
            isMegaThrusting = false;
        }
//            currMegaThrust = megaThrustFactor;
//            currMaxVel = MaxVel*megaMaxVelFactor;
//            currMaxVelSq = currMaxVel*currMaxVel;
//        }
//        else
//        {
//            currMegaThrust = 1f;
//            if (currMaxVel > MaxVel)
//            {
//                currMaxVel -= MegaThrustFadePerSec * elapsedSec;
//                currMaxVelSq = currMaxVel * currMaxVel;
//            }
//            else
//            {
//                currMaxVel = MaxVel;
//                currMaxVelSq = maxVelSq;
//            }
//            
//            
//        }

        if (controlsType == ControlsType.WasdFirst)
        {
            if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
                avatarVelocity = avatarVelocity + new Vector2(-1, 1) * ThrustForce * currMegaThrust;
            else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
                avatarVelocity = avatarVelocity + new Vector2(-1, -1) * ThrustForce * currMegaThrust;
            else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
                avatarVelocity = avatarVelocity + new Vector2(1, -1) * ThrustForce * currMegaThrust;
            else if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.W))
                avatarVelocity = avatarVelocity + new Vector2(1, 1) * ThrustForce * currMegaThrust;
            else if (Input.GetKey(KeyCode.Keypad8) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                avatarVelocity = avatarVelocity + Vector2.up * ThrustForce * currMegaThrust;
            else if (Input.GetKey(KeyCode.Keypad4) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                avatarVelocity = avatarVelocity + Vector2.left * ThrustForce * currMegaThrust;
            else if (Input.GetKey(KeyCode.Keypad2) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                avatarVelocity = avatarVelocity + Vector2.down * ThrustForce * currMegaThrust;
            else if (Input.GetKey(KeyCode.Keypad6) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                avatarVelocity = avatarVelocity + Vector2.right * ThrustForce * currMegaThrust;
        }
        else if (controlsType == ControlsType.DirectionFirst)
        {
            if (Input.GetKey(KeyCode.W))
            {
                avatarVelocity += new Vector2(lastRotStored.X, lastRotStored.Y) * ThrustForce * currMegaThrust;

                ParticleSystem[] psystems = clientsPlayer.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in psystems)
                {
                    ps.Emit(1);
                    ps.transform.rotation = clientsPlayer.transform.rotation * Quaternion.AngleAxis(180, Vector3.up);
                }

            }
            else if (Input.GetKey(KeyCode.S))
            {
                avatarVelocity -= new Vector2(lastRotStored.X, lastRotStored.Y) * ThrustForce * currMegaThrust;
                
                ParticleSystem[] psystems = clientsPlayer.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in psystems)
                {
                    ps.Emit(1);
                    ps.transform.rotation = clientsPlayer.transform.rotation;
                }
            }

        }
        
        if (Input.GetKey((KeyCode.E)))
        {
            avatarVelocity *= breakDampFactor;
        }
        // test code
//        if (Input.GetKey(KeyCode.S))
//        {
//            DoSpawnBullet(clientsPlayer.transform.position, clientsPlayer.transform.forward.x,
//                clientsPlayer.transform.forward.z);
//        }


//        if (avatarVelocity.sqrMagnitude > currMaxVelSq)
//        {
//            avatarVelocity = avatarVelocity.normalized * currMaxVel;
//        }


        // for some reason, a German keyboard's '+' will result in a Equals key. Anywhere else, this should be '+', too. I hope.
        if (Input.GetKey(KeyCode.RightBracket))
        {
            if (this.lastKeyPress + 0.05f < Time.time)
            {
                IncreaseViewDistance();
                this.lastKeyPress = Time.time;
            }
        }

        if (Input.GetKey(KeyCode.LeftBracket))
        {
            if (this.lastKeyPress + 0.05f < Time.time)
            {
                DecreaseViewDistance();
                this.lastKeyPress = Time.time;
            }
        }

        if (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Slash))
        {
            if (this.lastKeyPress + 0.05f < Time.time)
            {
                InterestArea cam;
                Game.TryGetCamera(0, out cam);
                cam.ResetViewDistance();
                this.lastKeyPress = Time.time;
            }
        }
    }
    
    public void OnLaserFired(string itemId, Vector position, Vector rotation)
    {
        Debug.Log("Laser Fired operation received");
    }

    public void OnHpChange(string itemId, int hpChange)
    {
        
        if (itemId == clientsPlayer.item.Id)
            clientsPlayer.TakeDamageAndUpdateBar(hpChange);
        else
        {
             actorTable[itemId].TakeDamage((hpChange));
        }
    }
    
    // this feeds the radar with item positions
    public void OnRadarUpdate(string itemId, ItemType itemType, Vector position, bool remove)
    {
        Radar r = GetComponent<Radar>();
        r.OnRadarUpdate(itemId, itemType, position);
    }
    
    #region player initiated operations
    
    // precondition: updated rotation of avatar has been set
    public void DoRotationOnly(Vector3 newforward)
    {
        this.lastRotation = new Vector(newforward.x, newforward.z);
        this.lastRotStored = new Vector(newforward.x, newforward.z);
        //Game.Avatar.MoveAbsolute(Game.Avatar.Position, Game.Avatar.Rotation);
    }

    public static string RandomString(int length)
    {
        System.Random random2 = new System.Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random2.Next(s.Length)]).ToArray());
    }
    public void DoSpawnBullet(Vector3 pos, float fwX, float fwZ)
    {
        //bulletVelocity += avatarVelocity;    // add factor of velocity that comes from ship moved to server

        Operations.FireBullet(Game,new Vector(pos.x, pos.z)/ WorldToUnityFactor, 
            new Vector(fwX, fwZ), avatarVelocity.x, avatarVelocity.y, true);
    }
    
    #endregion
}