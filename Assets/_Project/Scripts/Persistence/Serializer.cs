using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class Serializer
{
    public static bool saveToDisk(string filename, object obj)
    {
        bool success = false;
        BinaryFormatter BF = new BinaryFormatter();

#if !UNITY_WEBPLAYER
        FileStream file = File.Create(filename);

        try
        {
            BF.Serialize(file, obj);
            success = true;
        }
        catch (System.Exception e)
        { Debug.Log("Error in <Serializer.saveToDisk>: " + e.Message); }

        file.Close();
#else
        try
        {
            MemoryStream memoryStream = new MemoryStream ();
            BF.Serialize(memoryStream, obj);
            string tmp = System.Convert.ToBase64String (memoryStream.ToArray ());
            PlayerPrefs.SetString (filename, tmp);
            success = true;
        }
        catch
        {

        }
#endif

        return success;
    }
    public static object loadFromDisk(string filename)
    {
        object obj = null;

        if (Common.saveExists(filename))
        {
            BinaryFormatter BF = new BinaryFormatter();

#if !UNITY_WEBPLAYER
            FileStream file = File.Open(filename, FileMode.Open);

            try
            {
                obj = (object)BF.Deserialize(file);
            }
            catch (System.Exception e)
            {
                obj = null;
                Debug.Log("Error in <Serializer.loadFromDisk>: " + e.Message);
            }

            file.Close();
#else
            try
            {
                string tmp = PlayerPrefs.GetString(filename, string.Empty);
                MemoryStream memoryStream = new MemoryStream(
                    System.Convert.FromBase64String(tmp));
                obj = (object) BF.Deserialize(memoryStream);
            }
            catch
            {
                obj = null;
            }
#endif
        }

        return obj;
    }
}
