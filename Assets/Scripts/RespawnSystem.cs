using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RespawnSystem : MonoBehaviour
{

    [SerializeField] private Transform[] spawnPositions;
    private Transform farthestSpawnPoint;
    private HashSet<Transform> usedSpawnPoints = new HashSet<Transform>();
    [SerializeField] private GameObject EnemyPrefab;
	private int currentNumberEnemys;
	[SerializeField] private int maxNumberEnemysSpawn = 8;
    private Transform playerPosition;
	[SerializeField] private float spawnIntervalTime = 5f;
    private float spawnTimer;

    // Start is called before the first frame update
    void Start()
    {
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentNumberEnemys <= maxNumberEnemysSpawn)
        {
            spawnTimer += Time.deltaTime;

            if(spawnTimer >= spawnIntervalTime)
            {
                spawnTimer = 0f;
                Instantiate(EnemyPrefab, SpawnAtFarthestPoint().position, Quaternion.identity);
                currentNumberEnemys++;
                usedSpawnPoints.Add(SpawnAtFarthestPoint());

			}
           
        }
    }




    Transform SpawnAtFarthestPoint()
    {
        if(usedSpawnPoints.Count == spawnPositions.Length)
            usedSpawnPoints.Clear();

        float maxDistance = 0f;
        foreach(Transform position in spawnPositions)
        {
			if (usedSpawnPoints.Contains(position))
				continue;

			float distance = Vector3.Distance(playerPosition.position, position.position);
			if (distance > maxDistance)
			{
				maxDistance = distance;
				farthestSpawnPoint = position;
			}
		}


        return farthestSpawnPoint;

         
    }

    public void RemoveSpawnPositionUsed(Transform _spawnPoint)
    {
        usedSpawnPoints.Remove(_spawnPoint);

	}
}
