using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Forge3D
{
    public class F3DPlayerTurretController : MonoBehaviour
    {
        RaycastHit hitInfo; // Raycast structure
        public F3DTurret turret;
        bool isFiring; // Is turret currently in firing state
        public F3DFXController fxController;
        private RunBehaviour rb;
        float rangeSq = 3000;
        float lastTime;

        BotBehaviour targettedBot;
        float secondsTillScan = 1f;
        float secondsSinceLastScan = 0f;
        float dmgPerSec = 5f;
        //ItemBehaviour targettedPlayer;

        bool miningMode;
        Asteroid targettedAst;
        public SpaceStation owner;
        //       F3DFXController gfxController;
        public Transform loopToDespawn;

        private void Start()
        {
            rb = GameObject.FindGameObjectWithTag("GameController").GetComponent<RunBehaviour>();
            miningMode = true;
            F3DFXController gfxController = GetComponent<F3DFXController>();
        }
        void Update()
        {
            float elapsedSec = Time.time - lastTime;
            lastTime = Time.time;
            secondsSinceLastScan += elapsedSec;

            if (targettedAst != null)
                turret.SetNewTarget(targettedAst.transform.position);

            if (secondsSinceLastScan >= secondsTillScan)
            {
                if (miningMode)
                {
                    if (targettedAst != null)
                    {
                        //turret.SetNewTarget(targettedAst.transform.position);
                        if (turret.GetAngleToTarget() < 7)
                        {
                            targettedAst.InflictDamage(dmgPerSec * elapsedSec);
                            if (isFiring == false)
                            {
                                StartFire();
                            }
                            else if (isFiring == true && targettedAst.currHealth <= 0)
                            {
                                // send resource to owning space station
                                owner.gold += 5;
                                targettedAst = null;
                                StopFire();
                            }
                        }
                    }
                    else
                        CheckForAsteroids();
                }
                else
                {
                    if (targettedBot != null)
                    {
                        targettedBot.TakeDamage(1);
                        // todoj if bot's health is <= 0 then send itemdestroyed operation.
                        //if (targettedBot.c)
                    }
                    CheckForNearbyBots();
                }
            }



            //    CheckForTurn();
            //     CheckForFire();
        }

        // called when targettedAst is null (there is no current target)
        void CheckForAsteroids()
        {
          //  bool wasFiringBefore = (targettedAst != null);
            foreach (Asteroid ast in rb.asteroids)
            {
                if (IsPosInRange(ast.transform.position))
                {
                    targettedAst = ast;
                    break;
                }
            }
            if (isFiring && targettedAst == null)
                StopFire();
            else if (!isFiring && targettedAst != null)
                turret.SetNewTarget(targettedAst.transform.position);
        }

        bool IsPosInRange(Vector3 pos)
        {
            return Vector3.SqrMagnitude(pos - transform.position) < rangeSq;
        }

        void CheckForNearbyBots()
        {
            bool wasFiringBefore = (targettedBot != null);
            foreach (KeyValuePair<string, BotBehaviour> bb in rb.botTable)
            {
                if (IsPosInRange(bb.Value.transform.position))
                {
                    targettedBot = bb.Value;
                }
            }
            if (wasFiringBefore && targettedBot == null)
            {
                StopFire();
            }
            else if (!wasFiringBefore && targettedBot != null)
            {
                StartFire(targettedBot.transform.position);
            }
        }

        public void StopFire()
        {
            Debug.Log("stop Fire");
            fxController.Stop();

                //GetComponentInChildren<F3DDespawn>();
                F3DDespawn[] despawnables = GetComponentsInChildren<F3DDespawn>();
                foreach (F3DDespawn dsp in despawnables)
                {
                    dsp.Despawn();
                }
                if (loopToDespawn != null)
                    ((F3DDespawn)loopToDespawn.GetComponent<F3DDespawn>()).Despawn();

          //  F3DPoolManager.Pools["GeneratedPool"].Despawn(fxController.)
            isFiring = false;
            targettedAst = null;
           // turret.StopAnimation();
        }

        void StartFire()
        {
            Debug.Log("Start Fire");
            isFiring = true;
            fxController.Fire(this);
         //   turret.PlayAnimationLoop();
        }

        void StartFire(Vector3 targetPos)
        {
            
            turret.SetNewTarget(targetPos);
            
        }

        void CheckForFire()
        {
            // Fire turret
            if (!isFiring && Input.GetKeyDown(KeyCode.Mouse0))
            {
                StartFire();
            }

            // Stop firing
            if (isFiring && Input.GetKeyUp(KeyCode.Mouse0))
            {
                StopFire();
            }
        }

        void CheckForTurn()
        {
            // Construct a ray pointing from screen mouse position into world space
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Raycast
            if (Physics.Raycast(cameraRay, out hitInfo, 500f))
            {
                turret.SetNewTarget(hitInfo.point);
            }
        }
    }
}