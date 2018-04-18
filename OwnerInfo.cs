using UnityEngine;
using System.Collections;

public class OwnerInfo : MonoBehaviour
{
	public ulong OwnerID;

	public bool IsMine
	{
		get
		{
			;
			return OwnerID == StarCollectorClient.PlayerID;
		}
	}

	void SetOwnerID( ulong ownerID )
	{
		this.OwnerID = ownerID;
	}
}