using System;
using UnityEngine;

public enum Direction { Left = -1, None = 0, Right = 1 }
public enum SubmitState { Normal, Negative, Positive }

public class CommonStructures
{

    #region Structures
    [Serializable]
    public struct FloatRange
    {
        public float min;
        public float max;
    }
    [Serializable]
    public struct EffectSpawnRelative
    {
        public float scale;
        public GameObject prefab;
        public Vector3 offset;

        public EffectSpawnRelative(float scale, GameObject prefab, Vector3 offset)
        {
            this.scale = scale;
            this.prefab = prefab;
            this.offset = offset;
        }
    }
    [Serializable]
    public class Bounds2d
    {
        public Vector2 horizontal;
        public Vector2 vertical;
        public Vector2 center;

        public bool contains(Vector2 pos, float padding = 0, bool inclusive = true)
        {
            return containsHorizontal(pos.x, padding, inclusive) && containsVertical(pos.y, padding, inclusive);
        }
        public bool containsHorizontal(float x, float padding = 0, bool inclusive = true)
        {
            if (inclusive)
                return (
                    ((x + padding) >= horizontal.x) &&
                    ((x - padding) <= horizontal.y)
                );
            else
                return (
                    ((x + padding) > horizontal.x) &&
                    ((x - padding) < horizontal.y)
                );
        }
        public bool containsVertical(float y, float padding = 0, bool inclusive = true)
        {
            if (inclusive)
                return (
                    ((y + padding) >= vertical.x) &&
                    ((y - padding) <= vertical.y)
                );
            else
                return (
                    ((y + padding) > vertical.x) &&
                    ((y - padding) < vertical.y)
                );
        }
    }
    [Serializable]
    public struct ProceduralObjectGroup
    {
        public string name;
        public float spawnChance;
        public GameObject[] objects;
    }
    #endregion
}