using UnityEditor;
using UnityEngine;
using System.IO;

namespace GameDB
{
    [CustomEditor(typeof(GameDatabase))]
    public class GameDatabaseEditor : Editor
    {
        const string dbScriptPathBase = @"Assets/_Project/Scripts/Database/";
        const string dbScriptPathTables = "Tables/";
        const string dbScriptPathRows = "Rows/";
        const string dbScriptPathEditor = "Editor/";
        const string dbScriptPathPropertyDrawer = "Attributes/Editor/";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add all Quests"))
            {
                //Get Assets
                string[] guids = AssetDatabase.FindAssets("t:QuestTemplate");

                GameDatabase db = OpenDB();

                if (guids.Length > 0)
                {
                    db.quests = new QuestTemplate[guids.Length];
                    //Use paths to get object references
                    for (int i = 0; i < guids.Length; i++)
                    {
                        //Convert to path
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                        //Load asset from path
                        QuestTemplate template = AssetDatabase.LoadAssetAtPath<QuestTemplate>(assetPath);

                        //Add asset to list
                        db.quests[i] = template;
                    }
                }
                else
                {
                    db.quests = new QuestTemplate[0];
                    Debug.Log("No Quest Templates found");
                }

                //Mark the database as modified so that it will be saved when the dev hits Ctrl-S
                EditorUtility.SetDirty(db);
            }
            if (GUILayout.Button("Add all Skills"))
            {
                //Get Assets
                string[] guids = AssetDatabase.FindAssets("t:AttributeTemplate");

                GameDatabase db = OpenDB();

                if (guids.Length > 0)
                {
                    db.skills = new RPGData.AttributeTemplate[guids.Length];
                    //Use paths to get object references
                    for (int i = 0; i < guids.Length; i++)
                    {
                        //Convert to path
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                        if (assetPath.IndexOf("Skill") > -1)
                        {
                            //Load asset from path
                            RPGData.AttributeTemplate template =
                                AssetDatabase.LoadAssetAtPath<RPGData.AttributeTemplate>(assetPath);

                            //Add asset to list
                            db.skills[i] = template;
                        }
                    }
                }
                else
                {
                    db.skills = new RPGData.AttributeTemplate[0];
                    Debug.Log("No Skill Templates found");
                }

                //Mark the database as modified so that it will be saved when the dev hits Ctrl-S
                EditorUtility.SetDirty(db);
            }
            if (GUILayout.Button("Add all Items"))
            {
                //Get Assets
                string[] guids = AssetDatabase.FindAssets("t:ItemTemplate");

                GameDatabase db = OpenDB();

                if (guids.Length > 0)
                {
                    db.items = new ItemTemplate[guids.Length];
                    //Use paths to get object references
                    for (int i = 0; i < guids.Length; i++)
                    {
                        //Convert to path
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                        //Load asset from path
                        ItemTemplate template =
                            AssetDatabase.LoadAssetAtPath<ItemTemplate>(assetPath);

                        //Add asset to list
                        db.items[i] = template;
                    }

                    //Sort by saveID
                    System.Array.Sort(db.items, new ItemSortBySaveID());
                }
                else
                {
                    db.items = new ItemTemplate[0];
                    Debug.Log("No ItemTemplates found");
                }

                //Mark the database as modified so that it will be saved when the dev hits Ctrl-S
                EditorUtility.SetDirty(db);
            }
            if (GUILayout.Button("Add all World Locations"))
            {
                //Get Assets
                string[] guids = AssetDatabase.FindAssets("t:WorldLocationTemplate");

                GameDatabase db = OpenDB();

                if (guids.Length > 0)
                {
                    db.worldLocations = new WorldLocationTemplate[guids.Length];
                    //Use paths to get object references
                    for (int i = 0; i < guids.Length; i++)
                    {
                        //Convert to path
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                        //Load asset from path
                        WorldLocationTemplate template =
                            AssetDatabase.LoadAssetAtPath<WorldLocationTemplate>(assetPath);

                        //Add asset to list
                        db.worldLocations[i] = template;
                    }

                    //Sort by saveID
                    System.Array.Sort(
                        db.worldLocations,
                        new WorldLocationTemplate.SortBySaveID());
                }
                else
                {
                    db.worldLocations = new WorldLocationTemplate[0];
                    Debug.Log("No WorldLocationTemplates found");
                }

                //Mark the database as modified so that it will be saved when the dev hits Ctrl-S
                EditorUtility.SetDirty(db);
            }
            EditorGUILayout.EndHorizontal();
        }

        #region Database Methods
        public static GameDatabase OpenDB()
        {
            GameDatabase database = (GameDatabase)AssetDatabase.LoadAssetAtPath(GameDatabase.dbFile_editor, typeof(GameDatabase));
            if (database == null)
            {
                //Create a new database
                database = CreateInstance<GameDatabase>();
                AssetDatabase.CreateAsset(database, GameDatabase.dbFile_editor);
                AssetDatabase.SaveAssets();
            }

            return database;
        }
        #endregion
    }
}
