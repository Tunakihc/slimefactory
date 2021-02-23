using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "LocationsConfig", menuName = "Configs/LocationsConfig")]
public class LocationsConfig : ScriptableObject
{
    public List<LocationConfig> LocationConfigs = new List<LocationConfig>();

    public LocationConfig GetNextConfig(LocationType currentLocation)
    {
        for (int i = 0; i < LocationConfigs.Count; i++)
        {
            if (LocationConfigs[i].LocationType == currentLocation && i + 1 < LocationConfigs.Count)
            {
                currentLocation = LocationConfigs[i + 1].LocationType;
                break;
            }
        }
        
        return GetConfig(currentLocation);
    }

    public int GetLocationNum(LocationType loc)
    {
        for (int i = 0; i < LocationConfigs.Count; i++)
        {
            if (LocationConfigs[i].LocationType == loc)
                return i;
        }

        return 0;
    }

    public LocationConfig GetConfig(LocationType type)
    {
        return LocationConfigs.Find(l => l.LocationType == type);
    }
}
