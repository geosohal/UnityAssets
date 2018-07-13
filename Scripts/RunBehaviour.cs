// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RunBehaviour.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The run behaviour.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.MmoDemo.Client;
using Photon.MmoDemo.Common;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using sysr = System.Random;
using System.Collections.Generic;
using System.Diagnostics;
using Forge3D;
//using NUnit.Framework.Constraints;
using UnityEngine.Experimental.Rendering;
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
    public GameObject BotPrefab;
    public GameObject BotPrefabFast;
    public GameObject UpdateText;
    public GameObject BurstPrefab;
    public GameObject BombPrefab;
    public GameObject ShipDestroyedPrefab;
    public GameObject ShipBigDestroyedPrefab;
    public GameObject TpOutEffect;
    public GameObject TpInEffect;
    public GameObject HpBox;
    public GameObject MotherMobPrefab;
    public GameObject BallModeStartEffect;
    public GameObject PlasmaImpact;
    public SimpleHealthBar mpBar;

    public List<Tuple<GameObject,float>> Expirableffects;

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
    public ItemBehaviour clientsPlayer;    // itembehaviour belonging to the client who runs this code

    public AudioClip longLaserSound;
    public AudioClip teleportSound;
    public AudioClip burstSound;
    public AudioClip saberSound;
    public AudioClip shipExplosionSound;
    public AudioClip shipExplosionSoundBig;
    public AudioClip thrustSound;
    public AudioClip shipHitSound;
    public AudioClip shipHitSoundM;
    public AudioClip plasmaGunSound;
    
    private float lastKeyPress;
    private Vector? lastMovePosition;
    private Vector? lastRotation;
    private Vector lastRotStored;
    private Vector2 avatarVelocity; // actually is delta velocity per frame
    private float nextMoveTime;
    private float nextRotTime;
    private GameObject actorGo;
    private Dictionary<string, ItemBehaviour> actorTable;
    public Dictionary<string, BotBehaviour> botTable;
    private float lastTime;
    
    // speed boost related members
    private float currMegaThrust = 1f;
    private float currMaxVel;
    private float currMaxVelSq;
    private bool isMegaThrusting;
    private List<SpecialAbility> abilities;
    
    private bool wasTeleported = false;

    private bool isBursting;
    private int timeToIncreaseView = 6;
    
    private float hudTime;    // make shift hud vars
    private float timeToFadehud = 1.5f;
    private string lastText;
    private bool hudon = false;

    public float maxMP = 200f;
    public float MPpersec = 3f;
    private float currMP;

    public float thrustCostPerSec = 4f;
    public float tpCost = 9f;
    public float burstCost = 7f;
    public float saberCost = 10f;
    public float laserCost = 7f;
    public float bombCost = 10f;
    public float ballCost = 7f;
    public float bulletCost = 3f;
    public float shieldCost = 6f;
    
    private float timeToDestroyExpirableEffect = 8f;
    public float secTillUpdate = .05f;

    private AudioSource audioSource;

    public bool isFirstUpdate;

    
    public float moveForceRadius;
    public float moveForce;

    public bool isBuildMode;
    public List<Asteroid> asteroids;

    private List<Bullet> bullets;

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
        botTable = new Dictionary<string, BotBehaviour>();
        avatarVelocity = new Vector2(0,0);
        maxVelSq = MaxVel * MaxVel;
        lastRotStored = new Vector(1,0);
     //   controlsType = ControlsType.DirectionFirst;
        maxVelSq = MaxVel * MaxVel;
        currMaxVelSq = maxVelSq;
        currMaxVel = MaxVel;
        abilities = new List<SpecialAbility>();
        Text hudText = UpdateText.GetComponent<Text>();
        SpecialAbility tpability = new SpecialAbility(2f, KeyCode.Mouse0, SpecialAbility.SpecialType.Teleport, hudText, tpCost);
        abilities.Add(tpability);
        SpecialAbility burstAbility = new SpecialAbility(5f, KeyCode.Space, SpecialAbility.SpecialType.Burst, hudText, burstCost);
        abilities.Add((burstAbility));
        SpecialAbility saberAbility = new SpecialAbility(12f, KeyCode.F, SpecialAbility.SpecialType.Saber, hudText, saberCost);
        abilities.Add(saberAbility);
        SpecialAbility laserAbility = new SpecialAbility(4f, KeyCode.Q, SpecialAbility.SpecialType.Laser, hudText, laserCost );
        abilities.Add((laserAbility));
        SpecialAbility bombAbility = new SpecialAbility(3f, KeyCode.Z, SpecialAbility.SpecialType.Bomb, hudText, bombCost );
        abilities.Add(bombAbility);
        SpecialAbility shieldAbility = new SpecialAbility(6f, KeyCode.C, SpecialAbility.SpecialType.Shield, hudText, bombCost );
        abilities.Add(shieldAbility);
        SpecialAbility ballAbility = new SpecialAbility(1f, KeyCode.X, SpecialAbility.SpecialType.BallMode, hudText, ballCost );
        abilities.Add(ballAbility);
        wasTeleported = false;
        isMegaThrusting = false;
        Expirableffects = new List<Tuple<GameObject, float>>();
        currMP = maxMP;

        audioSource = GetComponent<AudioSource>();
        isFirstUpdate = true;
        isBuildMode = false;
        asteroids = new List<Asteroid>();
        bullets = new List<Bullet>();
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

        lastText = UpdateText.GetComponent<Text>().text;
        hudTime = 0;
        mpBar = GameObject.FindWithTag("mpbar").GetComponent<SimpleHealthBar>();
        
        
    }


    public void Update()
    {
        float elapsedSec = Time.time - lastTime;
        Game.Update();

        currMP += MPpersec * elapsedSec;
        mpBar.UpdateBar(currMP, maxMP );
        // some temp hud updating
        // if hud was just changed
        if (hudTime <= 0 && !lastText.Equals(UpdateText.GetComponent<Text>().text) && !lastText.Equals(" "))
        {
            hudTime = timeToFadehud;
            hudon = true;
        }

        if (hudon && hudTime > 0)
            hudTime -= elapsedSec;
        if (hudon && hudTime < 0)
        {
            UpdateText.GetComponent<Text>().text = " ";
            hudon = false;
            hudTime = 0;

        }

        lastText = UpdateText.GetComponent<Text>().text;
            

        if (timeToIncreaseView > 0)
        {
            timeToIncreaseView--;
            IncreaseViewDistance();
            
        }

        if (Game.WorldEntered)
        {
            // send queued velocity 20 times/sec
            SendVelocityRot();

            if (!isBuildMode)
            {
                ReadKeyboardInput(elapsedSec);

                // update special abilities and activate if their key is pressed
                foreach (SpecialAbility sa in abilities)
                {
                    sa.UpdateTimer(elapsedSec);
                    if (sa.IsChargedAndKeyPressed() && currMP > sa.mpCost)
                    {
                        currMP -= sa.mpCost;
                        sa.ResetChargeTimer();
                        switch (sa.specialType)
                        {
                            case SpecialAbility.SpecialType.Teleport:
                                Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                                RaycastHit hit;

                                // play teleport out effect
                                GameObject neweffect = Instantiate(TpOutEffect, clientsPlayer.transform.position, Quaternion.identity);
                                Expirableffects.Add(new Tuple<GameObject, float>(neweffect, timeToDestroyExpirableEffect));

                                MoveActorToMousePosition();
                                if (this.lastRotation.HasValue)
                                    Game.Avatar.MoveAbsolute(this.lastMovePosition.Value, (Vector)this.lastRotation);
                                else
                                    Game.Avatar.MoveAbsolute(this.lastMovePosition.Value, Game.Avatar.Rotation);

                                wasTeleported = true;

                                // play teleport in effect

                                Vector3 newPos = new Vector3(lastMovePosition.Value.X, 0, lastMovePosition.Value.Y) * WorldToUnityFactor;

                                GameObject effect = Instantiate(TpInEffect, newPos, Quaternion.identity);
                                effect.name = "Foll"; // set effect to follow player pos
                                Expirableffects.Add(new Tuple<GameObject, float>(effect, timeToDestroyExpirableEffect)); ;
                                break;

                            case SpecialAbility.SpecialType.Burst:
                                Game.Avatar.ApplyBurst();
                                GameObject burst = Instantiate(BurstPrefab, clientsPlayer.transform.position,
                                    clientsPlayer.transform.rotation);
                                burst.transform.localScale = new Vector3(2f, 2f, 2f);
                                Expirableffects.Add(new Tuple<GameObject, float>(burst, timeToDestroyExpirableEffect));
                                break;
                            case SpecialAbility.SpecialType.Saber:
                                Game.Avatar.FireSaber();
                                break;
                            case SpecialAbility.SpecialType.Laser:
                                Game.Avatar.FireLaser();
                                break;
                            case SpecialAbility.SpecialType.Bomb:
                                if (controlsType == ControlsType.DirectionFirst)
                                    Game.Avatar.LaunchBomb(new Vector(clientsPlayer.transform.forward.x, clientsPlayer.transform.forward.z));
                                else if (controlsType == ControlsType.WasdFirst)
                                    Game.Avatar.LaunchBomb(clientsPlayer.GetMouseForward());
                                break;
                            case SpecialAbility.SpecialType.BallMode:
                                if (!clientsPlayer.isSuperFast)
                                {
                                    clientsPlayer.ToggleWireBall();
                                    Game.Avatar.StartSuperFast();
                                    Vector3 nextPos = clientsPlayer.transform.position;
                                    GameObject balleffect = Instantiate(BallModeStartEffect, nextPos, Quaternion.identity);
                                    balleffect.name = "Foll"; // set effect to follow player pos
                                    Expirableffects.Add(new Tuple<GameObject, float>(balleffect,
                                        timeToDestroyExpirableEffect));
                                }
                                else
                                {
                                    Game.Avatar.EndSuperFast();
                                    clientsPlayer.ToggleWireBall();
                                }

                                break;
                        }
                    }
                }
            }

            // update specials timers in each actor/ship for View effects
            foreach (KeyValuePair<string,ItemBehaviour> kvp in actorTable)
            {
                ItemBehaviour ib = kvp.Value;    // ib of ship
                
                // if saber was just turned on for this ship/actor
                if (ib.saberTimeLeft <= 0 && ib.item.IsSaberFiring)
                    OnSaberFired(ib);
                // if laser was just turned on for this ship/actor
                if (ib.laserTimeLeft <= 0 && ib.item.IsLaserFiring)
                    OnLaserFired(ib);
                
                if (ib.laserTimeLeft > 0)
                {
                    ib.laserTimeLeft -= elapsedSec;
                    // laser just turned off so turn off its graphics for the players ship
                    if (ib.laserTimeLeft <= 0)
                    {
                        ib.laserbeam.StopFire();
                        ib.item.IsLaserFiring = false;
                        if (controlsType == ControlsType.WasdFirst)
                            ib.laserObject.GetComponent<MeshRenderer>().enabled = false;
                        if (controlsType == ControlsType.DirectionFirst)
                        {
                            MeshRenderer[] mrs = ib.GetComponentsInChildren<MeshRenderer>();
                            foreach (MeshRenderer mr in mrs)
                            {
                                if (mr.gameObject.tag == "laser")
                                {
                                    mr.enabled = false;
                                    break;
                                }
                            }
                        }
                    } // if (ib.laserTimeLeft <= 0)
                }

                if (ib.saberTimeLeft > 0)
                {
                    ib.saberTimeLeft -= elapsedSec;
                    // saber just turned off so turn off its graphics for the players ship
                    if (ib.saberTimeLeft <= 0)
                    {
                        ib.item.IsSaberFiring = false;
                        if (controlsType == ControlsType.WasdFirst)
                            ib.saberObject.GetComponent<MeshRenderer>().enabled = false;
                        if (controlsType == ControlsType.DirectionFirst)
                        {
                            MeshRenderer[] mrs = ib.GetComponentsInChildren<MeshRenderer>();
                            foreach (MeshRenderer mr in mrs)
                            {
                                if (mr.gameObject.tag == "saber")
                                {
                                    mr.enabled = false;
                                    break;
                                }
                            }
                        }
                    } // if (ib.laserTimeLeft <= 0)
                }
            } // foreach ib (actor) in actor table

            if (isFirstUpdate)
            {
            }

        } // if gameworld entered

        UpdateExpirableEffects(elapsedSec);
        DetectTeleportAndPlayEffect();
        
        lastTime = Time.time;

    }

    public void DetectTeleportAndPlayEffect()
    {
        foreach (var player in actorTable)
        {
            if (player.Value.item.PreviousPosition != null)
            {
                if (player.Value.SeemsToBeTeleporting())
                {
                    Vector3 prevPos = new Vector3(((Vector)player.Value.item.PreviousPosition).X,0,
                        ((Vector)player.Value.item.PreviousPosition).Y) * WorldToUnityFactor;
                    GameObject neweffect = Instantiate(TpOutEffect, prevPos, Quaternion.identity);
                    Expirableffects.Add(new Tuple<GameObject, float>(neweffect, timeToDestroyExpirableEffect));
                    
                    Vector3 nextPos = new Vector3(player.Value.item.Position.X,0,player.Value.item.Position.Y)
                        * WorldToUnityFactor;
                    GameObject effect = Instantiate(TpInEffect, nextPos, Quaternion.identity);
                    Expirableffects.Add(new Tuple<GameObject, float>(effect, timeToDestroyExpirableEffect));;
                    
                    audioSource.PlayOneShot(teleportSound, 1f);
                }
            }
        }
    }

    public void UpdateExpirableEffects(float elapsedSec)
    {
        foreach (var effect in Expirableffects)
        {
            if (effect.second < 0)
            {
                Destroy(effect.first);
                Expirableffects.Remove(effect);
                return;
            }

            effect.second -= elapsedSec;
            
            // if this is a teleport in effect sets it's position with the player who made it 
            if (effect.first.name.StartsWith("TPin"))
            {
                string ownerId = effect.first.name.Substring(4);
                if (actorTable.ContainsKey(ownerId))
                {
                    Debug.Log("effect following player");
                    ((GameObject) effect.first).transform.position = actorTable[ownerId].transform.position;
                }
            }
        }
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
                  //  DecreaseViewDistance();
                    
                }
                else if (Event.current.delta.y > 0)
                {
                   // IncreaseViewDistance();
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

    // attach item associated with appropriate Bullet object
    private void CreateBullet(Item bulletItem)
    {
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            if (bullets[i].Item == null)
            {
                bullets[i].Item = bulletItem;
                return;
            }
        }
    }

    private void CreateBullet(Vector3 pos, Vector2 rot)
    {
        GameObject newbullet = Instantiate(BulletPrefab, pos,
            Quaternion.LookRotation(new Vector3(rot.x, 0, rot.y)));
        Bullet bull = newbullet.GetComponent<Bullet>();
        if (bull == null)
            bull = newbullet.AddComponent(typeof(Bullet)) as Bullet;
        bull.Initialize(clientsPlayer.beamController);
        newbullet.GetComponent<Rigidbody>().velocity = new Vector3(rot.x, 0, rot.y) +
            new Vector3(avatarVelocity.x,0,avatarVelocity.y);
        bullets.Add(bull);
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

    private void CreateBot(Item bot)
    {
        GameObject newbot = Instantiate(BotPrefab, new Vector3(bot.Position.X, 0, bot.Position.Y) * WorldToUnityFactor,
            Quaternion.identity);
        BotBehaviour bb = newbot.GetComponent<BotBehaviour>();
        if (bb == null)
            bb = newbot.AddComponent(typeof (BotBehaviour)) as BotBehaviour;
        bb.Initialize(this.Game, bot, this.ItemObjectName(bot), null, false);
        botTable.Add(bot.Id, bb);
        Debug.Log("made bot on client");
    }
    
    private void CreateFastBot(Item bot)
    {
        GameObject newbot = Instantiate(BotPrefabFast, new Vector3(bot.Position.X, 0, bot.Position.Y) * WorldToUnityFactor,
            Quaternion.identity);
        BotBehaviour bb = newbot.GetComponent<BotBehaviour>();
        if (bb == null)
            bb = newbot.AddComponent(typeof (BotBehaviour)) as BotBehaviour;
        bb.Initialize(this.Game, bot, this.ItemObjectName(bot), null, false);
        botTable.Add(bot.Id, bb);
        Debug.Log("made fast bot on client");
    }
    
    private void CreateMotherMob(Item bot)
    {
        GameObject newbot = Instantiate(MotherMobPrefab, new Vector3(bot.Position.X, 0, bot.Position.Y) * WorldToUnityFactor,
            Quaternion.identity);
        BotBehaviour bb = newbot.GetComponent<BotBehaviour>();
        if (bb == null)
            bb = newbot.AddComponent(typeof (BotBehaviour)) as BotBehaviour;
        bb.Initialize(this.Game, bot, this.ItemObjectName(bot), null, true);
        botTable.Add(bot.Id, bb);
        Debug.Log("made mother bot on client");
    }

    private void CreateBomb(Item bombItem)
    {
        Debug.Log("making bombt");
        GameObject newBomb = Instantiate(BombPrefab,
            new Vector3(bombItem.Position.X, 0, bombItem.Position.Y) * WorldToUnityFactor, Quaternion.identity);
        BombBehavior bb = newBomb.GetComponent<BombBehavior>();
        if (bb == null)
            bb = newBomb.AddComponent(typeof(BombBehavior)) as BombBehavior;
        bb.Initialize(bombItem);
    }

    private void CreateHP(Item hpItem)
    {
        Debug.Log("making hp box");
        GameObject newHp = Instantiate(HpBox,
            new Vector3(hpItem.Position.X, 0, hpItem.Position.Y) * WorldToUnityFactor, Quaternion.identity);
        newHp.name = hpItem.Id;
    }

    #region IGameListener

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
            else if (item.Id.StartsWith("bo"))
                CreateBot(item);
            else if (item.Id.StartsWith("bf"))
                CreateFastBot(item);
            else if (item.Id.StartsWith("mb"))
                CreateMotherMob(item);
            else if (item.Id.StartsWith("zz"))
                CreateBomb(item);
            else if (item.Id.StartsWith("hp"))
                CreateHP(item);
            else // create player
                CreateActor(item);
        }
    }
    
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


   // List<BotBehaviour> bots;

    public void OnItemRemoved(Item item)
    {
        string objName;
        if (item.Type == ItemType.Bullet)
        {
            objName = item.Id;
            foreach (Bullet b in bullets)
            {
                if (b.Item.Id == item.Id)
                {
                    bullets.Remove(b);
                    break;
                }
            }
        }
        else if (item.Type == ItemType.Bomb)
        {

            objName = item.Id;
            GameObject bomb = GameObject.Find(objName);
            bomb.GetComponent<BombBehavior>().Explode();
            return;
        }
        else if (item.Type == ItemType.Resource)
        {
            Debug.Log("removing hp box");
            objName = item.Id;
            Destroy(GameObject.Find(objName));
        }
        else if (item.Type == ItemType.Bot)
        {

            objName = this.ItemObjectName(item);
            Debug.Log("removing bot" + objName + " aka" + item.Id);
            if (botTable.ContainsKey(item.Id))
            {
                if (item.Id.StartsWith("bo") || item.Id.StartsWith("bf"))
                    OnShipExplosion(item.Position);
                else if (item.Id.StartsWith("mob"))
                    OnShipExplosionBig(item.Position);
                //botTable[item.Id].DeathAnimation(Expirableffects);
            }
        }
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


                tile.transform.GetComponent<Renderer>().enabled = false; 
                    /*= (x + y)%2 == 0
                    ? matDark
                    : mat;*/
                
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
		game.Avatar.MoveAbsolute(GetRandomPosition(false), Vector.Zero);
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
	private Vector GetRandomPosition(bool aroundMiddle)
	{
//	    if (aroundMiddle)
//	    {
//	        var d = game.WorldData.BoundingBox.Max - game.WorldData.BoundingBox.Min;
//	        var dover4 = d / 4;
//	        Vector min2 = new Vector(game.WorldData.BoundingBox.Min.X+d*2,game.WorldData.BoundingBox.Min.Y+d*2);
//	        return game.WorldData.BoundingBox.Min + new Vector { X = d.X * (float)this.random.NextDouble(), Y = d.Y * (float)this.random.NextDouble() };
//	    }
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
            
          //  if (this.avatarVelocity.sqrMagnitude > .1f)
            
            if (this.lastRotation.HasValue)
            {
                Vector lastRot = (Vector) lastRotation;
                System.Random r = new System.Random();
                // if (r.Next(100) < 3)
                //   Debug.Log("lastrot " + lastRot.ToString());
                if (controlsType == ControlsType.DirectionFirst)
                {
                    Game.Avatar.VelocityRotation(new Vector(avatarVelocity.x, avatarVelocity.y),
                        new Vector(lastRot.X, lastRot.Y), null, isMegaThrusting);
                }
                else
                {
                    Game.Avatar.VelocityRotation(new Vector(avatarVelocity.x, avatarVelocity.y),
                        new Vector(lastRot.X, lastRot.Y), clientsPlayer.GetMouseForward(), isMegaThrusting);
                }
            }
        
            else
            {
               // Debug.Log("warning null rotation shouldnt happen");
                if (controlsType == ControlsType.DirectionFirst)
                {
                    Game.Avatar.VelocityRotation(new Vector(avatarVelocity.x, avatarVelocity.y), null, null,
                        isMegaThrusting);
                }
                else
                {
                    Game.Avatar.VelocityRotation(new Vector(avatarVelocity.x, avatarVelocity.y), null, 
                        clientsPlayer.GetMouseForward(), isMegaThrusting);
                }
            }
            avatarVelocity = Vector2.zero;
        

            // up to 20 times per second
            this.nextMoveTime = Time.time + secTillUpdate;
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

        if (currMP > 0 && Input.GetKey(KeyCode.LeftShift))
        {
            isMegaThrusting = true;
            currMP -= thrustCostPerSec* elapsedSec;
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
            bool isThrusting = true;
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
            else
                isThrusting = false;
            
            if (isThrusting)
                DoThrusterEffect();
          //  else
            {
                clientsPlayer.ApplyGridForce(moveForce, moveForceRadius/2, avatarVelocity/2f, null);
            }
        }
        else if (controlsType == ControlsType.DirectionFirst)
        {
            if (Input.GetKey(KeyCode.W))
            {
                avatarVelocity += new Vector2(lastRotStored.X, lastRotStored.Y) * ThrustForce * currMegaThrust;
                DoThrusterEffect();


            }
            else if (Input.GetKey(KeyCode.S))
            {
                //  audioSource.
                avatarVelocity -= new Vector2(lastRotStored.X, lastRotStored.Y) * ThrustForce * currMegaThrust;
                DoThrusterEffect();

            }
        }

        if (Input.GetKey((KeyCode.E)))
        {
            Game.Avatar.ApplyBreak();
        }

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

    private void DoThrusterEffect()
    {
        ParticleSystem[] psystems = clientsPlayer.GetComponentsInChildren<ParticleSystem>();
       // clientsPlayer.ApplyGridForce(avatarVelocity/5f, moveForceRadius/2f);
        foreach (ParticleSystem ps in psystems)
        {
            if (ps.gameObject.CompareTag("engineThrust"))
            {
                ps.transform.rotation =
                    clientsPlayer.transform.rotation * Quaternion.AngleAxis(180, Vector3.up);
                if (isMegaThrusting)
                {
                    ps.startColor = new Color(.2f,.4f,1,1);
                    ps.Emit(3);
                }
                else
                {
                    ps.startColor = Color.yellow;
                    ps.Emit(1);
                }

            }
            if (isMegaThrusting && ps.gameObject.CompareTag("megaThrust"))
            {
                //   ps.Emit(1);
                //        ps.transform.rotation =
                //           clientsPlayer.transform.rotation * Quaternion.AngleAxis(-90, Vector3.forward);
            }
        }
    }
        
    public void OnLaserFired(ItemBehaviour ib)
    {
        Debug.Log("Laser Fired operation received for " + ib.item.Id);
      //  audioSource.PlayOneShot(longLaserSound, 1f);
        if (ib != null)
        {
            ib.laserTimeLeft = ib.mlaserTimeLeft;
            Debug.Log("lasr adv");
            ib.laserObject.GetComponent<MeshRenderer>().enabled = true;
            ib.laserbeam.StartFire();
//            MeshRenderer[] mrs = ib.GetComponentsInChildren<MeshRenderer>();
//            foreach (MeshRenderer mr in mrs)
//            {
//                if (mr.gameObject.tag == "laser")
//                {
//                    Debug.Log("lasr adv enable");
//                    mr.enabled = true;
//                    break;
//                }
//            }
        }
    }

    public void PlasmaImpactEffect(Vector3 pos)
    {
     //   GameObject explosion = Instant
        Expirableffects.Add(new Tuple<GameObject, float>(PlasmaImpact, 1.5f));
    }

    public void OnSaberFired(ItemBehaviour ib)
    {
        Debug.Log("saber Fired operation received for" + ib.item.Id);
        
        if (ib != null)
        {
            audioSource.PlayOneShot(saberSound,1f);
            ib.saberTimeLeft = ib.msaberTimeLeft;
            Debug.Log("saber adv");
            ib.saberObject.GetComponent<MeshRenderer>().enabled = true;
//            MeshRenderer[] mrs = ib.GetComponentsInChildren<MeshRenderer>();
//            foreach (MeshRenderer mr in mrs)
//            {
//                if (mr.gameObject.tag == "saber")
//                {
//                    Debug.Log("saber enabled");
//                    mr.enabled = true;
//                    break;
//                }
//            }
        }
    }

    public void OnHpChange(string itemId, int hpChange)
    {
        
        if (itemId == clientsPlayer.item.Id)
            clientsPlayer.TakeDamageAndUpdateBar(hpChange);
        else
        {
            if (itemId.StartsWith("bo") || itemId.StartsWith("bf"))    // if we're decrementing a bots hp
            {
                audioSource.PlayOneShot(shipHitSound, .4f);
                botTable[itemId].TakeDamage((hpChange));
            }
            else if (itemId.StartsWith("mb"))
            {
                if (hpChange > 0)   // only play damage sound when hp is set to decrease, otherwise it is just regenerating hp
                    audioSource.PlayOneShot(shipHitSoundM, .5f);
                botTable[itemId].TakeDamage((hpChange));
            }
            else// decrement a players hp
                actorTable[itemId].TakeDamage((hpChange));
        }
    }

    public void OnBurst(Vector pos)
    {
        audioSource.PlayOneShot(burstSound, .5f);
        Debug.Log("burst at " + pos.ToString());
        GameObject burst = Instantiate(BurstPrefab, new Vector3(pos.X,0,pos.Y) * WorldToUnityFactor, Quaternion.identity);
        burst.transform.localScale = new Vector3(2f,2f,2f);
        Expirableffects.Add(new Tuple<GameObject, float>(burst, timeToDestroyExpirableEffect));
        
    }

    public void OnShipExplosion(Vector pos)
    {
        audioSource.PlayOneShot(shipExplosionSound, .8f);
        Debug.Log("explo at " + pos.ToString());
        GameObject explo = Instantiate(ShipDestroyedPrefab, new Vector3(pos.X,0,pos.Y) * WorldToUnityFactor, Quaternion.identity);
        //burst.transform.localScale = new Vector3(2f,2f,2f);
        Expirableffects.Add(new Tuple<GameObject, float>(explo, timeToDestroyExpirableEffect));
    }
    
    public void OnShipExplosionBig(Vector pos)
    {
        audioSource.PlayOneShot(shipExplosionSoundBig, .9f);
        Debug.Log("b explo at " + pos.ToString());
        GameObject explo = Instantiate(ShipDestroyedPrefab, new Vector3(pos.X,0,pos.Y) * WorldToUnityFactor, Quaternion.identity);
        ShipDestroyedPrefab.transform.localScale = new Vector3(3.5f,3.5f,3.5f);
        Expirableffects.Add(new Tuple<GameObject, float>(explo, timeToDestroyExpirableEffect));
    }

    public void OnBombSpawn(string itemId)
    {
       // Vector pos = actorTable[itemId].item.Position;
        
    }

    public void OnBombExplode(string itemId, Vector pos)
    {
        
    }
    
    // this feeds the radar with item positions
    public void OnRadarUpdate(string itemId, ItemType itemType, Vector position, bool remove)
    {
        Radar r = GetComponent<Radar>();
        r.OnRadarUpdate(itemId, itemType, position);
    }

    public void SetBuildMode(bool val)
    {
        isBuildMode = val;
        clientsPlayer.isBuildMode = val;
        if (isBuildMode)
        {
            //Game.Avatar.ApplyBreak();
            avatarVelocity = -1f*new Vector2(((Vector)Game.Avatar.Velocity).X, ((Vector)Game.Avatar.Velocity).Y);
        }
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

        if (currMP < bulletCost)
            return;
        currMP -= bulletCost;
        Operations.FireBullet(Game,new Vector(pos.x, pos.z)/ WorldToUnityFactor, 
            new Vector(fwX, fwZ), avatarVelocity.x, avatarVelocity.y, true);
        CreateBullet(pos, new Vector2(fwX, fwZ));

        GameObject flash = (GameObject)Instantiate(Resources.Load("plasma_gun_muzzle_flash"));
        flash.transform.position = clientsPlayer.transform.position;
        flash.transform.position = flash.transform.position + new Vector3(0, .2f, 0);
        flash.transform.rotation = clientsPlayer.transform.rotation;
        Expirableffects.Add(new Tuple<GameObject, float>(flash, .14f));
        audioSource.PlayOneShot(plasmaGunSound, .3f);

    }
    
    #endregion
}