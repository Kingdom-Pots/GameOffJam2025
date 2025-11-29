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

    [System.Serializable]
    public class EnemyPool
    {
        public string poolName = "Enemy Pool";
        [Tooltip("Wave number this pool unlocks at (e.g., 10)")]
        public int unlockAtWave = 1;
        [Tooltip("Enemies available in this pool")]
        public List<GameObject> enemyPrefabs = new List<GameObject>();
    }

    [Header("Wave Configuration")]
    [Tooltip("Pre-configured waves (optional - used first if present)")]
    public List<Wave> waves = new List<Wave>();

    [Tooltip("Generate unlimited waves after pre-configured waves")]
    public bool unlimitedWaves = true;

    [Tooltip("Enemy pools for procedural wave generation")]
    public List<EnemyPool> enemyPools = new List<EnemyPool>();

    [Header("Procedural Wave Settings")]
    public int proceduralStartingEnemies = 10;
    public int enemyIncreasePerWave = 2;
    public float delayBetweenWaves = 5f;
    public bool autoStartNextWave = true;

    [Header("Spawn Settings")]
    [Tooltip("Box collider defining spawn area")]
    public BoxCollider spawnArea;

    [Tooltip("Minimum distance between spawned enemies")]
    public float minSpawnDistance = 2f;
    public int maxSpawnAttempts = 30;

    [Tooltip("Height offset from bottom of collider")]
    public float spawnHeightOffset = 0f;

    [Tooltip("Parent spawned enemies to this object")]
    public bool parentToSpawner = true;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showSpawnArea = true;

    [Tooltip("Automatically start waves on Start")]
    public bool startOnAwake = true;

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

        // Start waves automatically if enabled
        if (startOnAwake)
        {
            StartWaves();
        }
    }

    void Update()
    {
        // Check if waiting for wave to clear
        if (waitingForWaveClear && autoStartNextWave)
        {
            CleanUpDeadEnemies();

            // Check if all enemies are dead (health <= 0) instead of destroyed
            if (AreAllEnemiesDead())
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
        // Check if using pre-configured waves
        if (currentWaveIndex < waves.Count)
        {
            // Use pre-configured wave
            Wave wave = waves[currentWaveIndex];
            if (showDebugInfo)
                Debug.Log($"Spawning Pre-Configured Wave {currentWaveIndex + 1}: {wave.waveName}");

            SpawnWaveEnemies(wave);
        }
        else if (unlimitedWaves)
        {
            // Generate procedural wave
            Wave proceduralWave = GenerateProceduralWave(currentWaveIndex + 1);
            if (showDebugInfo)
                Debug.Log($"Spawning Procedural Wave {currentWaveIndex + 1}: {proceduralWave.waveName}");

            SpawnWaveEnemies(proceduralWave);
        }
        else
        {
            // No more waves
            isSpawning = false;
            if (showDebugInfo)
                Debug.Log("All waves completed!");
            return;
        }

        currentWaveIndex++;

        // Setup next wave trigger
        if (currentWaveIndex < waves.Count || unlimitedWaves)
        {
            waitingForWaveClear = true;
        }
        else
        {
            isSpawning = false;
        }
    }

    Wave GenerateProceduralWave(int waveNumber)
    {
        Wave procWave = new Wave();
        procWave.waveName = $"Wave {waveNumber}";
        procWave.delayBeforeWave = 2f;
        procWave.enemies = new List<EnemySpawnInfo>();

        // Calculates how many waves past the predetermined ones
        int proceduralWaveNumber = waveNumber - waves.Count;

        // Calculate total enemies for this wave
        int totalEnemies = proceduralStartingEnemies + (proceduralWaveNumber * enemyIncreasePerWave);
        totalEnemies = Mathf.Max(1, totalEnemies);

        if (showDebugInfo)
            Debug.Log($"Generating procedural wave {waveNumber} with {totalEnemies} total enemies");

        // Get all unlocked enemy types from all pools
        List<GameObject> availableEnemyTypes = new List<GameObject>();
        List<EnemyPool> availablePools = GetAvailablePoolsForWave(waveNumber);

        foreach (EnemyPool pool in availablePools)
        {
            foreach (GameObject enemyPrefab in pool.enemyPrefabs)
            {
                if (enemyPrefab != null && !availableEnemyTypes.Contains(enemyPrefab))
                {
                    availableEnemyTypes.Add(enemyPrefab);
                }
            }
        }

        if (availableEnemyTypes.Count == 0)
        {
            Debug.LogWarning($"No enemy types available for wave {waveNumber}!");
            return procWave;
        }

        // Randomly distribute total enemies across all available enemy types
        int[] enemyCounts = new int[availableEnemyTypes.Count];

        // Give at least 1 to each type
        for (int i = 0; i < availableEnemyTypes.Count; i++)
        {
            enemyCounts[i] = 1;
        }

        int remainingEnemies = totalEnemies - availableEnemyTypes.Count;

        // Randomly distribute remaining enemies
        for (int i = 0; i < remainingEnemies; i++)
        {
            int randomIndex = Random.Range(0, availableEnemyTypes.Count);
            enemyCounts[randomIndex]++;
        }

        // Create spawn info for each enemy type
        for (int i = 0; i < availableEnemyTypes.Count; i++)
        {
            EnemySpawnInfo spawnInfo = new EnemySpawnInfo();
            spawnInfo.enemyPrefab = availableEnemyTypes[i];
            spawnInfo.count = enemyCounts[i];
            procWave.enemies.Add(spawnInfo);

            if (showDebugInfo)
                Debug.Log($"  - {enemyCounts[i]}x {availableEnemyTypes[i].name}");
        }

        return procWave;
    }

    List<EnemyPool> GetAvailablePoolsForWave(int waveNumber)
    {
        List<EnemyPool> available = new List<EnemyPool>();

        foreach (EnemyPool pool in enemyPools)
        {
            if (waveNumber >= pool.unlockAtWave)
            {
                available.Add(pool);
            }
        }

        return available;
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
            // Generate random position
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = bounds.min.y + spawnHeightOffset;
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 position = new Vector3(randomX, randomY, randomZ);

            // Check if valid
            if (IsValidSpawnPosition(position))
            {
                return position;
            }
        }

        return Vector3.zero; // Failed to find position
    }

    bool IsValidSpawnPosition(Vector3 position)
    {
        // Check distance to all active enemies
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
        // Remove null enemies (if they do get destroyed)
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    bool AreAllEnemiesDead()
    {
        // If no enemies in list, consider wave complete
        if (activeEnemies.Count == 0)
            return true;

        // Check if all enemies have 0 health or are dead
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy == null) continue;

            // Check for EnemyController component
            EnemyController controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                // If enemy is still alive, wave is not complete
                if (!controller.IsDead())
                {
                    return false;
                }
            }
        }

        // All enemies are dead
        return true;
    }

    public int GetAliveEnemyCount()
    {
        int aliveCount = 0;

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                EnemyController controller = enemy.GetComponent<EnemyController>();
                if (controller != null && !controller.IsDead())
                {
                    aliveCount++;
                }
            }
        }

        return aliveCount;
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
        return GetAliveEnemyCount();
    }

    public int GetCurrentWaveNumber()
    {
        return currentWaveIndex;
    }

    public int GetTotalWaves()
    {
        if (unlimitedWaves)
            return -1; // Infinite
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