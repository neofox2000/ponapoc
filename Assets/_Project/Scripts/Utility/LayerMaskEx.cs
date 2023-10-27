using UnityEngine;

public static class LayerMaskEx
{
    public static bool hasLayer(this LayerMask layerMask, int layer)
    {
        if (layerMask == (layerMask | (1 << layer)))
        {
            return true;
        }

        return false;
    }

    public static bool[] hasLayers(this LayerMask layerMask)
    {
        var hasLayers = new bool[32];

        for (int i = 0; i < 32; i++)
        {
            if (layerMask == (layerMask | (1 << i)))
            {
                hasLayers[i] = true;
            }
        }

        return hasLayers;
    }
}