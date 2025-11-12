using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("List of enemy prefabs to randomly spawn from")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Tooltip("Number of enemies to spawn")]
    public int enemiesToSpawn = 10;

    [Tooltip("Minimum distance between spawned enemies")]
    public float minSpawnDistance = 2f;

    [Tooltip("Height offset from bottom of collider")]
    public float spawnHeightOffset = 0f;

    [Header("Spawn Area")]
    [Tooltip("Box collider defining the spawn area")]
    public BoxCollider spawnArea;

    [Header("Advanced Options")]
    [Tooltip("Maximum attempts to find valid spawn position")]
    public int maxSpawnAttempts = 30;

    [Tooltip("Spawn enemies on Start")]
    public bool spawnOnStart = true;

    [Tooltip("Parent spawned enemies to this transform")]
    public bool parentToSpawner = true;

    private List<Vector3> spawnedPositions = new List<Vector3>();
    private Transform enemyContainer;

    void Start()
    {
        // Use attached box collider if not assigned
        if (spawnArea == null)
        {
            spawnArea = GetComponent<BoxCollider>();
            if (spawnArea == null)
            {
                Debug.LogError("No BoxCollider assigned or found on GameObject!");
                return;
            }
        }

        // Create container for spawned enemies
        if (parentToSpawner)
        {
            enemyContainer = new GameObject("Spawned Enemies").transform;
            enemyContainer.SetParent(transform);
            enemyContainer.localPosition = Vector3.zero;
        }

        if (spawnOnStart)
        {
            SpawnEnemies();
        }
    }

    public void SpawnEnemies()
    {
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogError("No enemy prefabs assigned to spawn!");
            return;
        }

        ClearSpawnedEnemies();
        spawnedPositions.Clear();

        int successfulSpawns = 0;
        int totalAttempts = 0;
        int maxTotalAttempts = enemiesToSpawn * maxSpawnAttempts;

        while (successfulSpawns < enemiesToSpawn && totalAttempts < maxTotalAttempts)
        {
            Vector3 spawnPos = GetRandomPositionInBox();

            if (IsValidSpawnPosition(spawnPos))
            {
                GameObject enemyPrefab = GetRandomEnemy();
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                if (parentToSpawner && enemyContainer != null)
                {
                    enemy.transform.SetParent(enemyContainer);
                }

                spawnedPositions.Add(spawnPos);
                successfulSpawns++;

                Debug.Log($"Spawned {enemyPrefab.name} at {spawnPos}");
            }

            totalAttempts++;
        }

        if (successfulSpawns < enemiesToSpawn)
        {
            Debug.LogWarning($"Only spawned {successfulSpawns}/{enemiesToSpawn} enemies. " +
                           "Consider increasing spawn area or decreasing minSpawnDistance.");
        }
        else
        {
            Debug.Log($"Successfully spawned {successfulSpawns} enemies!");
        }
    }

    Vector3 GetRandomPositionInBox()
    {
        // Get the bounds of the box collider in world space
        Bounds bounds = spawnArea.bounds;

        // Generate random position within bounds
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = bounds.min.y + spawnHeightOffset;
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(randomX, randomY, randomZ);
    }

    bool IsValidSpawnPosition(Vector3 position)
    {
        // Check if position is too close to other spawned enemies
        foreach (Vector3 spawnedPos in spawnedPositions)
        {
            if (Vector3.Distance(position, spawnedPos) < minSpawnDistance)
            {
                return false;
            }
        }

        return true;
    }

    GameObject GetRandomEnemy()
    {
        int randomIndex = Random.Range(0, enemyPrefabs.Count);
        return enemyPrefabs[randomIndex];
    }

    public void ClearSpawnedEnemies()
    {
        if (enemyContainer != null)
        {
            foreach (Transform child in enemyContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    // Public method to spawn specific amount
    public void SpawnEnemies(int amount)
    {
        int originalAmount = enemiesToSpawn;
        enemiesToSpawn = amount;
        SpawnEnemies();
        enemiesToSpawn = originalAmount;
    }

    // Visualize spawn area in editor
    void OnDrawGizmos()
    {
        if (spawnArea != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.matrix = spawnArea.transform.localToWorldMatrix;
            Gizmos.DrawCube(spawnArea.center, spawnArea.size);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (spawnArea != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = spawnArea.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
        }
    }
}