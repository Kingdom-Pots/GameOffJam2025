using System.Collections.Generic;
using UnityEngine;

public class WaveSpawnController: MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public GameObject enemyPrefab;
        public int count;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";
        [Tooltip("List of specific enemies to spawn in this wave")]
        public List<EnemySpawnInfo> enemies = new List<EnemySpawnInfo>();
        [Tooltip("Delay before this wave starts")]
        public float delayBeforeWave = 2f;
    }

    [Header("Wave Configuration")]
    [Tooltip("List of all waves")]
    public List<Wave> waves = new List<Wave>();

    [Tooltip("Delay after all enemies die before next wave")]
    public float delayBetweenWaves = 5f;
    public bool autoStartNextWave = true;

    [Header("Spawn Settings")]
    [Tooltip("Enemy Spawn Area (Box Collider)")]
    public BoxCollider spawnArea;

    [Tooltip("Minimum distance between spawned enemies")]
    public float minSpawnDistance = 2f;
    public int maxSpawnAttempts = 30;

    [Tooltip("Height offset from bottom of collider")]
    public float spawnHeightOffset = 0f;

    [Tooltip("Parent spawned enemies to this object")]
    public bool parentToSpawner = true;

    [Header("Debug Stuff")]
    public bool showDebugInfo = true;
    public bool showSpawnArea = true;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool waitingForWaveClear = false;
    private Transform enemyContainer;

    void Start()
    {
        // Get spawn area
        if (spawnArea == null)
        {
            spawnArea = GetComponent<BoxCollider>();
            if (spawnArea == null)
            {
                Debug.LogError("No BoxCollider assigned for spawn area!");
                enabled = false;
                return;
            }
        }

        // Create enemy container
        if (parentToSpawner)
        {
            enemyContainer = new GameObject("Wave Enemies").transform;
            enemyContainer.SetParent(transform);
            enemyContainer.localPosition = Vector3.zero;
        }

        //Start enemy spawns
        StartWaves();
    }

    void Update()
    {
        // Check if waiting for wave to clear
        if (waitingForWaveClear && autoStartNextWave)
        {
            CleanUpDeadEnemies();

            if (activeEnemies.Count == 0)
            {
                if (showDebugInfo)
                    Debug.Log("All enemies defeated! Starting next wave...");

                waitingForWaveClear = false;
                Invoke(nameof(SpawnNextWave), delayBetweenWaves);
            }
        }
    }

    public void StartWaves()
    {
        if (!isSpawning && waves.Count > 0)
        {
            currentWaveIndex = 0;
            isSpawning = true;
            activeEnemies.Clear();
            Invoke(nameof(SpawnNextWave), waves[0].delayBeforeWave);
        }
    }

    void SpawnNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            isSpawning = false;
            if (showDebugInfo)
                Debug.Log("All waves completed!");
            return;
        }

        Wave wave = waves[currentWaveIndex];
        if (showDebugInfo)
            Debug.Log($"Spawning Wave {currentWaveIndex + 1}: {wave.waveName}");

        // Spawn all enemies in this wave
        SpawnWaveEnemies(wave);

        currentWaveIndex++;

        // Setup next wave trigger
        if (autoStartNextWave && currentWaveIndex < waves.Count)
        {
            waitingForWaveClear = true;
        }
        else if (!autoStartNextWave && currentWaveIndex < waves.Count)
        {
            // Time-based waves
            Invoke(nameof(SpawnNextWave), delayBetweenWaves + waves[currentWaveIndex].delayBeforeWave);
        }
        else
        {
            // Last wave - just wait for clear
            waitingForWaveClear = true;
        }
    }

    void SpawnWaveEnemies(Wave wave)
    {
        int totalSpawned = 0;

        foreach (EnemySpawnInfo enemyInfo in wave.enemies)
        {
            if (enemyInfo.enemyPrefab == null)
            {
                Debug.LogWarning("Enemy prefab is null! Skipping...");
                continue;
            }

            for (int i = 0; i < enemyInfo.count; i++)
            {
                Vector3 spawnPos = FindValidSpawnPosition();

                if (spawnPos != Vector3.zero)
                {
                    GameObject enemy = Instantiate(enemyInfo.enemyPrefab, spawnPos, Quaternion.identity);

                    if (parentToSpawner && enemyContainer != null)
                    {
                        enemy.transform.SetParent(enemyContainer);
                    }

                    activeEnemies.Add(enemy);
                    totalSpawned++;

                    if (showDebugInfo)
                        Debug.Log($"Spawned {enemyInfo.enemyPrefab.name} at {spawnPos}");
                }
                else
                {
                    Debug.LogWarning($"Failed to find spawn position for {enemyInfo.enemyPrefab.name}");
                }
            }
        }

        if (showDebugInfo)
            Debug.Log($"Wave spawned {totalSpawned} total enemies. Tracking {activeEnemies.Count} active.");
    }

    Vector3 FindValidSpawnPosition()
    {
        Bounds bounds = spawnArea.bounds;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Generates random position
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = bounds.min.y + spawnHeightOffset;
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 position = new Vector3(randomX, randomY, randomZ);

            // Checks if the spawn position is valid
            if (IsValidSpawnPosition(position))
            {
                return position;
            }
        }

        return Vector3.zero; // Failed to find position
    }

    bool IsValidSpawnPosition(Vector3 position)
    {
        // Checks distance to all active enemies
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < minSpawnDistance)
                {
                    return false;
                }
            }
        }

        return true;
    }

    void CleanUpDeadEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    public void StopWaves()
    {
        CancelInvoke();
        isSpawning = false;
        waitingForWaveClear = false;
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
    }

    public int GetActiveEnemyCount()
    {
        CleanUpDeadEnemies();
        return activeEnemies.Count;
    }

    public int GetCurrentWaveNumber()
    {
        return currentWaveIndex;
    }

    public int GetTotalWaves()
    {
        return waves.Count;
    }

    public int GetTotalEnemiesInWave(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= waves.Count) return 0;

        int total = 0;
        foreach (EnemySpawnInfo info in waves[waveIndex].enemies)
        {
            total += info.count;
        }
        return total;
    }

    // Visualize spawn area
    void OnDrawGizmos()
    {
        if (!showSpawnArea || spawnArea == null) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.matrix = spawnArea.transform.localToWorldMatrix;
        Gizmos.DrawCube(spawnArea.center, spawnArea.size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
    }

    void OnDrawGizmosSelected()
    {
        if (spawnArea == null) return;

        Gizmos.color = Color.green;
        Gizmos.matrix = spawnArea.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
    }
}