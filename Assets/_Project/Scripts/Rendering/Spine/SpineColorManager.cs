using UnityEngine;
using System.Collections.Generic;
using Spine.Unity;

public class SpineColorManager : MonoBehaviour 
{
    const float shadowDensity = 0.75f;

    [System.Serializable]
    public class ColorSet
    {
        public string name;
        public bool shadowed = false;
        [SpineSlot]
        public List<string> slotNames;

        [HideInInspector]
        public List<Spine.Slot> slots = null;
    }
    [System.Serializable]
    public class ColorLayer
    {
        public string name;
        public Color color = new Color(1, 1, 1, 1);
        public List<ColorSet> colorSets;
    }

    public bool debug = false;
    public bool initialized = false;
    public List<ColorLayer> colorLayers;

    //List<Spine.Slot> slots = null;
    SkeletonRenderer skeletonRenderer;

    void init()
    {
        skeletonRenderer = GetComponentInChildren<SkeletonRenderer>();
        if (skeletonRenderer.skeleton != null)
        {
            //slots = new List<Spine.Slot>(colorLayers.Count);

            //Cache all slots
            foreach (ColorLayer cl in colorLayers)
                foreach (ColorSet cs in cl.colorSets)
                {
                    //Initialize slot list
                    if (cs.slots == null)
                        cs.slots = new List<Spine.Slot>(colorLayers.Count);
                    else
                        cs.slots.Clear();

                    //Fetch slots from skeleton
                    foreach (string slotName in cs.slotNames)
                        cs.slots.Add(skeletonRenderer.skeleton.FindSlot(slotName));
                }
        }
        else
            Debug.LogWarning(name + ": No skeleton found!");

        initialized = true;
    }
    public void updateColors()
    {
        if (!initialized)
            init();

        Color color;
        foreach (ColorLayer cl in colorLayers)
            foreach (ColorSet cs in cl.colorSets)
            {
                color = cl.color;
                if (cs.shadowed)
                {
                    color = new Color(
                        Mathf.Max(0, color.r * shadowDensity),
                        Mathf.Max(0, color.g * shadowDensity),
                        Mathf.Max(0, color.b * shadowDensity),
                        color.a);
                }

                if (cs.slots != null)
                {
                    for (int i = 0; i < cs.slots.Count; i++)
                    {
                        if (cs.slots[i] != null)
                            cs.slots[i].SetColor(color);
                        else
                            Debug.LogWarning("Invalid slotName: " + cs.slotNames[i]);
                    }
                }
                else
                    Debug.LogWarning("SpineColorManager has no slots!");
            }
    }
    
    void Start()
    {
        if (debug)
            updateColors();
    }
}
