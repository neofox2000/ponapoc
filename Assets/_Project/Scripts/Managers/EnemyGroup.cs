using UnityEngine;
using GameDB;
using System;

public class EnemyGroup : MonoBehaviour
{
    public enum SpawnState { UnSpawned, Alive, Dead }

    [Serializable]
    public struct Spawn
    {
        //Inspector props
        public GameObject enemyPrefab;

        //Internal props
        public Actor spawnedEnemy { get; set; }
        public SpawnState state { get; set; }
    }

    public Transform spawnPoint;
    public Vector2 randomOffsetX;
    public Vector2 randomOffsetY;
    public int team;
    public Direction faceDirection = Direction.Left;
    public Spawn[] spawns;

    public Action OnAllEnemiesDied;

    //This script is usually on an object in a room
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

    //Makes sure only 1 group is ever spawned
    bool spawned = false;

    // Are all enemies dead?
    bool isDefeated()
    {
        foreach (Spawn spawn in spawns)
            if (spawn.state != SpawnState.Dead)
                //An enemy is not dead yet
                return false;

        //All enemies are dead
        return true;
    }

    //Event Handlers
    void EnemyDied(Actor enemy)
    {
        //Unregister event
        enemy.OnDied -= EnemyDied;

        //Find spawn in list
        int spawnIndex = -1;
        for (int i = 0; i < spawns.Length; i++)
        {
            if(spawns[i].spawnedEnemy == enemy)
            {
                spawnIndex = i;
                break;
            }            
        }

        if (spawnIndex > -1)
        {
            //Update State
            spawns[spawnIndex].state = SpawnState.Dead;

            //Clear reference (enemy object may be reused elsewhere, making this reference possibly problematic)
            spawns[spawnIndex].spawnedEnemy = null;

            //Fire event if all enemies are dead
            if (isDefeated() && OnAllEnemiesDied != null)
                OnAllEnemiesDied();
        }
        else
            Debug.LogWarning("Enemy was not part of the spawn list!");
    }

    //Spawns all enemies in this group
    public void SpawnEnemy(Action OnDefeated)
    {
        if (spawned)
        {
            Debug.LogWarning("EnemyGroup spawn called more than once!");
            return;
        }

        //Mark the spawn so it is not called twice
        spawned = true;

        //Set the callback
        OnAllEnemiesDied = OnDefeated;

        //Cache default spawn
        Vector3 defaultPosition = transform.position;

        //Spawn all mobs
        for (int i = 0; i < spawns.Length; i++)
        {
            //spawnEnemy(ref spawns[i], transform.position);
            if (spawns[i].state == SpawnState.UnSpawned)
            {
                //Update State
                spawns[i].state = SpawnState.Alive;

                //Which spawn point to use?
                Vector3 spawnPointToUse = spawnPoint ? spawnPoint.position : defaultPosition;

                //Add randomized offset
                spawnPointToUse += new Vector3(
                    UnityEngine.Random.Range(randomOffsetX.x, randomOffsetX.y),
                    UnityEngine.Random.Range(randomOffsetY.x, randomOffsetY.y));

                //Spawn Enemy
                if (spawns[i].enemyPrefab)
                {
                    spawns[i].spawnedEnemy = Common.ProducePrefab(
                        spawns[i].enemyPrefab,
                        spawnPointToUse,
                        Quaternion.identity,
                        false)
                    .GetComponent<Actor>();

                    //Grab controller
                    Actor controller = spawns[i].spawnedEnemy as Actor;

                    //Set team
                    controller.team = team;

                    //Set facing direction
                    controller.dir = (int)faceDirection;

                    //Register death event
                    spawns[i].spawnedEnemy.OnDied += EnemyDied;
                }
                else
                    Debug.LogWarning(name + " has an empty enemy link");
            }
        }
    }
}