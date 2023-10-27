[System.Serializable]
public class LocationStateData
{
    #region Subclasses
    [System.Serializable]
    public struct ObjectStorageData
    {
        public int id;
        public SaveItem[] inventory;

        /*
        public ObjectStorageData()
        {
            id = -1;
            inventory = null;
        }
        */
        public ObjectStorageData(LocationState.ObjectStorage objectStorage)
        {
            //Store id
            id = objectStorage.id;

            //Convert all inventory items into basic item data
            inventory = objectStorage.inventory.GetSaveItems();
        }

        public LocationState.ObjectStorage unpack()
        {
            return new LocationState.ObjectStorage(this);
        }
    }
    #endregion

    #region Properties
    public int locationId;

    public LocationState.ObjectState[] objectStateArray;
    public ObjectStorageData[] objectStorageArray;
    public LocationState.PortalAccess[] portalAccessArray;
    #endregion

    #region Methods
    public LocationStateData()
    {
        locationId = -1;
        objectStateArray = null;
        objectStorageArray = null;
        portalAccessArray = null;
    }
    public LocationStateData(LocationState locationState)
    {
        //Store id
        locationId = locationState.locationId;

        //Convert already-simple data from a list into an array
        objectStateArray = locationState.objectStateList.ToArray();

        //Convert complex data into simple data
        objectStorageArray = new ObjectStorageData[locationState.objectStorageList.Count];
        for (int i = 0; i < objectStorageArray.Length; i++)
            objectStorageArray[i] = new ObjectStorageData(locationState.objectStorageList[i]);

        //Convert already-simple data from a list into an array
        portalAccessArray = locationState.portalAccesList.ToArray();
    }
    public LocationState unpack()
    {
        return new LocationState(this);
    }
    #endregion
}