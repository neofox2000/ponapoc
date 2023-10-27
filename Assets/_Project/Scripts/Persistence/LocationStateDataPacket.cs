using System.Collections.Generic;

[System.Serializable]
public class LocationStateDataPacket
{
    public LocationStateData[] dataz = null;

    public LocationStateDataPacket()
    {
        dataz = new LocationStateData[0];
    }
    public LocationStateDataPacket(List<LocationState> locationStateDataList)
    {
        if (locationStateDataList != null)
        {
            dataz = new LocationStateData[locationStateDataList.Count];
            for (int i = 0; i < dataz.Length; i++)
                dataz[i] = new LocationStateData(locationStateDataList[i]);
        }
    }
    public List<LocationState> unpack()
    {
        if (dataz != null)
        {
            List<LocationState> ret = new List<LocationState>(dataz.Length);
            for (int i = 0; i < dataz.Length; i++)
                ret.Add(dataz[i].unpack());

            return ret;
        }
        else
            return new List<LocationState>();
    }
}