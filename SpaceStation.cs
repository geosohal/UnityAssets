using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    MainConnector, // hub that connects large bridge and other buildings up or down
    Disk,
    Disk2,
    SmallDisk,      // also a hub but only connects small bridges
    Room
}

public class SpaceStation : MonoBehaviour {

    // heights of buildings are from top to bottom, the hub connectors are halfway between top and bottom
    // small disk should be slightly less than half but will work for now.
    public static Dictionary<BuildingType, float> buildingHeightTable;
    public static float bigBridgeLength = 162f;
    public static float maxHeightTop = 30f;
    public static float maxHeightBtm = -200f;
	// Use this for initialization
	void Start () {
		if (buildingHeightTable == null)
        {
            buildingHeightTable = new Dictionary<BuildingType, float>();
            buildingHeightTable.Add(BuildingType.SmallDisk, 20f);
            buildingHeightTable.Add(BuildingType.Room, 53f);
            buildingHeightTable.Add(BuildingType.MainConnector, 25f);
            buildingHeightTable.Add(BuildingType.Disk, 18f);
            buildingHeightTable.Add(BuildingType.Disk2, 25f);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static bool IsHub(BuildingType type)
    {
        if (type == BuildingType.MainConnector || type == BuildingType.SmallDisk)
            return true;
        return false;
    }

}
