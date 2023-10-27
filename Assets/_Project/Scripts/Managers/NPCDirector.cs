using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GameDB;

public class NPCDirector : MonoBehaviour
{
    #region Inspector Properties
    //Inspector Properties
    [Header("Main Settings")]
    [Tooltip("Which team is this group of enemies playing for?\n0 = Hostile to all\n1 = The Player's team\nAll other numbers are allied to those with the same number, and attack all other teams")]
    public int team;
    [Tooltip("Enemies to randomly choose from when spawning")]
    public GameObject[] enemies;
    [Tooltip("Places that enemies can spawn")]
    public List<BoxCollider2D> spawningAreas;

    [Header("Starting Spawn Settings")]
    [Tooltip("Number of enemies to spawn as soon as the level is loaded")]
    [Range(0, 99)]
    public int initalSpawnCount = 15;

    [Header("Constant Spawn Settings")]
    [Tooltip("Number of enemies to spawn on each tick")]
    [Range(0, 99)]
    public int spawnAmount = 1;
    [Tooltip("Time (in seconds) between enemy spawns")]
    [Range(1f, 120f)]
    public float spawnDelay = 10f;
    [Tooltip("When this number is reached, enemies will no longer be spawned until it decreases")]
    [Range(0, 99)]
    public int maxSpawns = 20;
    #endregion
    #region Private Properties
    float lastSpawnTimer = 0f;
    List<Actor> 
        mobs = null, 
        zombieHordeMobs = null;
    Room _room = null;
    Room room
    {
        get
        {
            if (_room == null)
                _room = GetComponentInParent<Room>();

            return _room;
        }
    }
    #endregion
    #region Events
    public Action OnZombieHordeEventStarted;
    public Action OnZombieHordeEventEnded;
    #endregion

    //Methods
    Vector3 GetValidPosition()
    {
        if (spawningAreas.Count > 0)
        {
            //Select random area
            Bounds bounds = spawningAreas[UnityEngine.Random.Range(0, spawningAreas.Count)].bounds;

            //Generate random point within the area
            return new Vector3(
                UnityEngine.Random.Range(bounds.center.x - bounds.extents.x, bounds.center.x + bounds.extents.x),
                UnityEngine.Random.Range(bounds.center.y - bounds.extents.y, bounds.center.y + bounds.extents.y),
                //Random.Range(bounds.horizontal.x + (minSpawnDistance * 1.5f), bounds.horizontal.y),
                //Random.Range(bounds.vertical.x, bounds.vertical.y),
                0);
        }
        else
            Debug.LogWarning("No spawning areas defined!");

        return Vector3.zero;
    }
    Actor SpawnEnemy(bool useSpawningLimit)
    {
        //Obey spawning limits?
        if (useSpawningLimit && mobs.Count >= maxSpawns)
            return null;
        
        //Init mob list if needed
        if (mobs == null) mobs = new List<Actor>(spawnAmount);

        //Get a spawn position
        Vector3 spawnPos = GetValidPosition();

        //Spawn mob
        Actor enemy = Common.ProducePrefab(
            GetRandomEnemy(),
            spawnPos,
            Quaternion.identity,
            false)
        .GetComponent<Actor>();
        enemy.transform.SetParent(transform);
        Actor controller = enemy as Actor;

        //Set team
        controller.team = team;

        //Register death event
        enemy.OnDied += EnemyDied;

        //Add mob to list
        mobs.Add(enemy);

        return enemy;
    }
    List<Actor> SpawnEnemies(bool useSpawningLimits, int spawnAmountOverride = -1)
    {
        int spawnCount = spawnAmountOverride == -1 ? spawnAmount : spawnAmountOverride;
        List<Actor> spawningBatch = new List<Actor>(spawnAmount);

        for (int i = 0; i < spawnCount; i++)
        {
            //Add mob to batch (in case someone wants the mobs spawned in this function alone)
            spawningBatch.Add(SpawnEnemy(useSpawningLimits));
        }

        return spawningBatch;
    }
    public void StartZombieHorde(int mobCount)
    {
        zombieHordeMobs = SpawnEnemies(false, mobCount);
        foreach (Actor mob in zombieHordeMobs)
            mob.OnDied += ZombieHordeEnemyDied;

        //Notify listeners that the event started
        //SendMessage("OnScriptedEvent", "ZombieHordeStart");
        if (OnZombieHordeEventStarted != null)
            OnZombieHordeEventStarted();
    }
    void ZombieHordeEnemyDied(Actor enemy)
    {
        //Remove event
        enemy.OnDied -= ZombieHordeEnemyDied;

        //Remove mob from list
        if (zombieHordeMobs != null)
            zombieHordeMobs.Remove(enemy);

        //Event end check (are all mobs dead?)
        if (zombieHordeMobs.Count == 0)
        {
            zombieHordeMobs = null;

            //Notify listeners that the event ended
            //SendMessage("OnScriptedEvent", "ZombieHordeEnd");
            if(OnZombieHordeEventEnded != null)
                OnZombieHordeEventEnded();
        }
    }
    void EnemyDied(Actor enemy)
    {
        //Remove event
        enemy.OnDied -= EnemyDied;

        //Remove mob from list
        if (mobs != null) mobs.Remove(enemy);

        //Remove mob from room
        if (room) room.EntityLeftRoom(enemy);
    }
    IEnumerator PreWarmEnemies()
    {
        int enemyCount = 0;
        if(mobs == null) mobs = new List<Actor>(initalSpawnCount);

        while (enemyCount < initalSpawnCount)
        {
            SpawnEnemy(true);
            enemyCount++;

            //Stagger spawns to make sure there is no 'hitch' in frame rates
            yield return new WaitForFixedUpdate();
        }
    }
    public GameObject GetRandomEnemy()
    {
        if (enemies.Length > 0)
            return enemies[UnityEngine.Random.Range(0, enemies.Length)];
        else
        {
            Debug.LogWarning("No enemies defined - spawning default");
            return null;
        }
    }

    //Mono Methods
    void Awake()
    {
        //Wait a second for other scripts to finish initialization
        lastSpawnTimer = 1f;
    }
    void Start()
    {
        if (enemies.Length > 0)
            StartCoroutine(PreWarmEnemies());
        else
        {
            Debug.Log(name + " has no enemies defined -> deactivated.");
            gameObject.SetActive(false);
        }
    }
    void Update()
    {
        if (lastSpawnTimer < 0)
        {
            SpawnEnemies(true);
            lastSpawnTimer = spawnDelay;
        }
        else
            lastSpawnTimer -= Time.deltaTime;
    }
}