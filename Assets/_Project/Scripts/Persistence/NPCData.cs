using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Management class for persistent NPC data
/// </summary>
[System.Serializable]
public class NPCData
{
    List<SaveNPC> npcs;

    public NPCData()
    {
        npcs = new List<SaveNPC>();
    }

    public SaveNPC Fetch(int savedStateID)
    {
        return npcs.Find(x => x.savedStateID == savedStateID);
    }
    public void Store(NPCController npc)
    {
        if (npc.persistentID != -1)
        {
            //Convert npc to SaveNPC data
            SaveNPC newNPC = new SaveNPC(npc);

            //Look for an existing npc record
            SaveNPC existingNPC = Fetch(npc.persistentID);

            //Add or replace record
            if(existingNPC == null)
                npcs.Add(newNPC);
            else
            {
                int index = npcs.IndexOf(existingNPC);
                npcs[index] = newNPC;
            }
        }
        else
            Debug.LogWarning("NPC has not been assigned a Saved State ID - this NPC will not be saved!");
    }

    public SaveNPCData Pack()
    {
        SaveNPCData saveData = new SaveNPCData();

        if (npcs.Count > 0)
            saveData.npcs = npcs.ToArray();

        return saveData;
    }
    public void Unpack(SaveNPCData savedData)
    {
        npcs.Clear();
        npcs.Capacity = savedData.npcs.Length;
        for (int i = 0; i < savedData.npcs.Length; i++)
            npcs.Add(savedData.npcs[i]);
    }
}