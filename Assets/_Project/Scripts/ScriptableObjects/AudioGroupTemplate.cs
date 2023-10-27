using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Templates/Audio Group")]
public class AudioGroupTemplate : ScriptableObject
{
    public bool ignorePause = false;
    public AudioMixerGroup mixer;
    [Range(0, 1f)]
    public float volume;
    [Range(-3f, 3f)]
    public float pitch;
    [Range(-1f, 1f)]
    public float pitchRandMin;
    [Range(-1f, 1f)]
    public float pitchRandMax;
    public AudioClip[] clips;

    //Ease-of-use Methods
    public AudioClip playClip
    {
        get
        {
            if (clips.Length > 0)
                return clips[Random.Range(0, clips.Length)];
            else
                return null;
        }
    }
    public float playPitch
    {
        get
        {
            return pitch + Random.Range(pitchRandMin, pitchRandMax);
        }
    }
}
/*
            if (GUILayout.Button("Fix Clips"))
            {
                for (int i = 0; i < table.arraySize; i++)
                {
                    SerializedProperty rec = table.GetArrayElementAtIndex(i);
                    string assetPath = "Assets/_Project/Templates/AudioGroups/" + rec.FindPropertyRelative("name").stringValue + ".asset";
                    AudioGroupTemplate so = AssetDatabase.LoadAssetAtPath<AudioGroupTemplate>(assetPath);

                    if (so != null)
                    {
                        SerializedProperty clipsArray = rec.FindPropertyRelative("clips");
                        if (clipsArray.arraySize > 0)
                        {
                            so.clips = new AudioClip[clipsArray.arraySize];
                            for (int j = 0; j < clipsArray.arraySize; j++)
                                so.clips[j] = (AudioClip)clipsArray.GetArrayElementAtIndex(j).objectReferenceValue;
                        }

                        EditorUtility.SetDirty(so);
                    }
                }
            }
*/