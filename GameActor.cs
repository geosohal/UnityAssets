using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameActor : MonoBehaviour
{
	public static Dictionary<long, GameActor> Actors = new
		Dictionary<long, GameActor>();
	public long ActorID;
	
	void SetActorID( long actorID )
	{
		this.ActorID = actorID;
		Actors.Add( this.ActorID, this );
	}
	public void Destruct()
	{
		Actors.Remove( this.ActorID );
		Destroy( gameObject );
	}
}

