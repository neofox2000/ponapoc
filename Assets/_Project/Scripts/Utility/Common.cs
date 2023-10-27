using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public static class Common
{
    static readonly string[] ignoredProperties = { "minVolume", "maxVolume", "rolloffFactor" };
    public const string poolName = "Pool";

    public static float lowestAudioDBSetting = 50f;
    public static int akhShowing = Animator.StringToHash("Showing");
    public static string defaultFloatFormat = "#,0.00";
    public static string defaultFloatPercentFormat = "#,0.00%";
    public static string defaultIntFormat = "#,0";
    public static string defaultIntPercentFormat = "#,0%";

    //Useful stuff
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }

    //Component Cloning
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if ((pinfo.CanWrite) && (!ignoredProperties.Contains(pinfo.Name)))
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }
    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }

    //Pool Management Methods
    public static void poolDespawn(Transform instance)
    {
        Lean.LeanPool.Despawn(instance);
    }
    public static Transform poolSpawn(GameObject instance)
    {
        return Lean.LeanPool.Spawn(instance).transform;
    }
    public static Transform poolSpawn(GameObject instance, Vector3 position, Quaternion rotation, Transform parent)
    {
        return Lean.LeanPool.Spawn(instance, position, rotation, parent)
            .transform;
    }
    public static void initProceduralObjects(CommonStructures.ProceduralObjectGroup[] procedurallyPlacedObjects)
    {
        foreach (CommonStructures.ProceduralObjectGroup PO in procedurallyPlacedObjects)
            foreach (GameObject GO in PO.objects)
                GO.SetActive(PO.spawnChance > UnityEngine.Random.Range(0f, 1f));
    }
    public static Transform SpawnEffect(CommonStructures.EffectSpawnRelative effectData, Transform anchor, int dir, int sortingOrder)
    {
        if (effectData.prefab)
        {
            //Spawn prefab
            Transform trans = ProducePrefab(
                effectData.prefab, 
                anchor, 
                true,
                false,
                false).transform;

            //Set scale
            trans.localScale = effectData.prefab.transform.localScale * effectData.scale;

            //Set position
            trans.position = calcDirectionalOffset(trans.position, effectData.offset, dir);

            //Orient object's direction relative to provided anchor?
            copyObjectDirection(
                trans,
                anchor,
                effectData.prefab.transform);

            //Set sorting order (if applicable)
            SortingGroup sg = trans.GetComponent<SortingGroup>();
            if (sg) sg.sortingOrder = sortingOrder;

            return trans;
        }

        return null;
    }
    public static void flipObject(Transform objectToFlip, bool useRotation = true)
    {
        if (useRotation)
            objectToFlip.localRotation =
                Quaternion.Euler(
                objectToFlip.localRotation.eulerAngles.x,
                objectToFlip.localRotation.eulerAngles.y + 180,
                objectToFlip.localRotation.eulerAngles.z);
        else
            objectToFlip.localScale = new Vector3(
                objectToFlip.localScale.x * -1,
                objectToFlip.localScale.y,
                objectToFlip.localScale.z);
    }
    public static int convertScaleToDir(float s)
    {
        return (Mathf.Abs(s) < 0) ? (-1) : (1);
    }
    public static int convertRotationToDir(float r)
    {
        //return (Mathf.Abs(Mathf.RoundToInt(r)) == 180) ? (-1) : (1);
        return Mathf.RoundToInt(Mathf.Cos(r));
    }
    public static void copyObjectDirection(Transform source, Transform target, Transform prefabTrans, bool useRotation = true)
    {
        int prefabDir = 1;
        int sourceDir = 1;
        int targetDir = 1;

        if (useRotation)
        {
            prefabDir = convertRotationToDir(prefabTrans.rotation.eulerAngles.y);
            sourceDir = convertRotationToDir(source.localRotation.eulerAngles.y);
            targetDir = convertRotationToDir(target.rotation.eulerAngles.y);
        }
        else
        {
            prefabDir = convertScaleToDir(prefabTrans.lossyScale.x);
            sourceDir = convertScaleToDir(source.lossyScale.x);
            targetDir = convertScaleToDir(target.lossyScale.x);
        }

        //Make sure the directions work out
        if ((sourceDir * prefabDir) != (targetDir))
            flipObject(source);
    }
    public static Vector3 calcDirectionalOffset(Vector3 normalPosition, Vector3 offset, int dir)
    {
        return
            new Vector3(
                normalPosition.x + (offset.x * dir),
                normalPosition.y + offset.y,
                normalPosition.z + offset.z);
    }
    public static void ChangeLayersRecursively(Transform trans, int layer)
    {
        trans.gameObject.layer = layer;
        foreach (Transform child in trans)
            ChangeLayersRecursively(child, layer);
    }
    public static GameObject ProducePrefab(GameObject prefab, Transform parentTrans, bool usePool = true, bool flip = false, bool attachToParent = true)
    {
        GameObject ret = null;

        if (!usePool)
        {
            ret = (GameObject)MonoBehaviour.Instantiate(
                prefab,
                Vector3.zero,
                prefab.transform.rotation);
        }
        else
        {
            ret = Lean.LeanPool.Spawn(
                prefab,
                Vector3.zero,
                prefab.transform.rotation);
        }

        if (parentTrans)
        {
            if (attachToParent)
                ret.transform.SetParent(parentTrans, false);
            else
                ret.transform.position = parentTrans.position;
        }

        if (flip) flipObject(ret.transform);

        //Fix Z Rotation (during animation, parent rotations may change causing the world-Z rotation to be non-0)
        if (parentTrans)
            ret.transform.localRotation = Quaternion.Euler(
                ret.transform.localRotation.eulerAngles.x,
                ret.transform.localRotation.eulerAngles.y,
                parentTrans.rotation.eulerAngles.z);

        return ret;
    }
    public static GameObject ProducePrefab(GameObject prefab, Vector3 position, Quaternion rotation, bool usePool = true, Transform parentTrans = null)
    {
        GameObject ret = null;

        //Spawn or instantiate
        if (!usePool)
            ret = (GameObject)MonoBehaviour.Instantiate(prefab, position, rotation);
        else
            ret = Lean.LeanPool.Spawn(prefab, position, rotation);

        //Set parent
        if (parentTrans != null) ret.transform.SetParent(parentTrans, true);

        //Return spawned/instantiated object
        return ret;
    }
    public static void cleanupPrefab(GameObject prefab, bool usePool = true)
    {
        if (usePool)
            poolDespawn(prefab.transform);
        else
            MonoBehaviour.Destroy(prefab);
    }
    public static void showStandardAnimatorObject(Animator animator, bool showing = true)
    {
        animator.SetBool(akhShowing, showing);
    }
    public static void SetupIcon(Image icon, Sprite sprite, Color32 iconColor)
    {
        if (sprite != null)
        {
            icon.sprite = sprite;
            icon.color = iconColor;
        }
        else
        {
            icon.sprite = GameDatabase.core.defaultIcon;
            icon.color = Color.white;
        }
    }
    public static void setMixerVolume(AudioMixer mixer, string key, float newValue)
    {
        if (newValue == 0)
            newValue = -80;
        else
            newValue = (newValue * lowestAudioDBSetting) - lowestAudioDBSetting;

        mixer.SetFloat(key, newValue);
    }
    public static float getMixerVolume(AudioMixer mixer, string key)
    {
        float value;
        mixer.GetFloat(key, out value);
        if (value < (-lowestAudioDBSetting))
            return 0;
        else
            return (value / lowestAudioDBSetting) + 1;
    }

    public static bool saveExists(string fn)
    {
#if !UNITY_WEBPLAYER
        return File.Exists(fn);
#else
        return (PlayerPrefs.GetString(fn, string.Empty) != string.Empty);
#endif
    }

    public static float smoothFunctionFloat(float target, float Base, float multiplier)
    {
        return Mathf.Log10(target + Base) * multiplier;
    }
    public static int smoothFunctionInt(float target, float Base, float multiplier)
    {
        return Mathf.RoundToInt(smoothFunctionFloat(target, Base, multiplier));
    }
    public static float invSmoothFunctionFloat(float target, float Base, float multiplier)
    {
        return Mathf.Pow(10, (target / multiplier)) - Base;
    }
    public static int invSmoothFunctionInt(float target, float Base, float multiplier)
    {
        return Mathf.RoundToInt(invSmoothFunctionFloat( target,  Base,  multiplier));
    }
    public static bool mouseInsideGameWindow()
    {
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        return screenRect.Contains(Input.mousePosition);
    }
    public static bool getControlState()
    {
        return InputX.Pressed(InputCode.LeftControl);
    }
    public static bool getShiftState()
    {
        return InputX.Pressed(InputCode.LeftShift);
    }
    public static SubmitState getKeyboardSubmitState()
    {
        SubmitState state = SubmitState.Normal;
        if (Common.getControlState()) state = SubmitState.Negative;
        if (Common.getShiftState()) state = SubmitState.Positive;

        return state;
    }
}