using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LocationState
{
    #region Sub-classes
    [System.Serializable]
    public struct ObjectState
    {
        //public int id = -1;
        //public int state = 0;
        public int id;
        public int state;

        public ObjectState(int id, int state)
        {
            this.id = id;
            this.state = state;
        }
    }
    [System.Serializable]
    public class ObjectStorage
    {
        public int id;
        public Inventory inventory;

        public ObjectStorage()
        {
            id = -1;
            inventory = ScriptableObject.CreateInstance<Inventory>();
        }
        public ObjectStorage(int id, Inventory inventory)
        {
            this.id = id;
            this.inventory = inventory;
        }
        public ObjectStorage(LocationStateData.ObjectStorageData objectStorageData)
        {
            id = objectStorageData.id;
            inventory = ScriptableObject.CreateInstance<Inventory>();
            inventory.SetSaveItems(objectStorageData.inventory);
        }
    }
    [System.Serializable]
    public struct PortalAccess
    {
        public int portalId;
        public bool enabled;

        public PortalAccess(int portalId, bool enabled)
        {
            this.portalId = portalId;
            this.enabled = enabled;
        }
    }
    #endregion

    #region Properties
    public int locationId;

    public List<ObjectState> objectStateList;
    public List<ObjectStorage> objectStorageList;
    public List<PortalAccess> portalAccesList;
    #endregion

    #region Methods
    public LocationState(int locationId = -1)
    {
        this.locationId = locationId;
        objectStateList = new List<ObjectState>();
        objectStorageList = new List<ObjectStorage>();
        portalAccesList = new List<PortalAccess>();
    }
    public LocationState(LocationStateData locationStateData)
    {
        locationId = locationStateData.locationId;

        //Convert array back to list
        if (locationStateData.objectStateArray != null)
            objectStateList = new List<ObjectState>(locationStateData.objectStateArray);
        else
            objectStateList = new List<ObjectState>();

        //Convert array back to list
        if (locationStateData.objectStorageArray != null)
        {
            objectStorageList = new List<ObjectStorage>(locationStateData.objectStorageArray.Length);
            for (int i = 0; i < locationStateData.objectStorageArray.Length; i++)
                objectStorageList.Add(locationStateData.objectStorageArray[i].unpack());
        }
        else
            objectStorageList = new List<ObjectStorage>();

        //Convert array back to list
        if (locationStateData.portalAccessArray != null)
            portalAccesList = new List<PortalAccess>(locationStateData.portalAccessArray);
    }

    //Portal Access Methods
    public int getPortalAccessIndex(int portalId)
    {
        int paIndex = portalAccesList.
            FindIndex(x => x.portalId == portalId);

        //Create new entry
        if (paIndex < 0)
        {
            portalAccesList.Add(new PortalAccess(portalId, false));
            paIndex = portalAccesList.Count - 1;
        }

        return paIndex;
    }
    public void setPortalAccess(int portalId, bool enabled)
    {
        int paIndex = getPortalAccessIndex(portalId);
        portalAccesList[paIndex] = new PortalAccess(portalId, enabled);
    }

    //Object State Methods
    public int getObjectStateIndex(int id)
    {
        //See if one exists already
        int index = objectStateList.
            FindIndex(x => x.id == id);

        //If none exists then... 
        if (index < 0)
        {
            //Create new entry and add it to the list
            objectStateList.Add(new ObjectState(id, 0));

            //Use newly created entry's index
            index = objectStateList.Count - 1;
        }

        //Return index
        return index;
    }
    public int getObjectState(int id)
    {
        //Fetch state index and return state property
        return objectStateList[getObjectStateIndex(id)].state;
    }
    public void setObjectState(int id, int state)
    {
        //Fetch entry index
        int index = getObjectStateIndex(id);

        //Replace entry in list
        objectStateList[index] = new ObjectState(id, state);
    }
    #endregion
}
