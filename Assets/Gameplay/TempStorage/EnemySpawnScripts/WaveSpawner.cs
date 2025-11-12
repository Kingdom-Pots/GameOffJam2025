using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnWave
    {
        public string waveName;
        public int enemyCount;
        public float delayBeforeWave = 2f;
    }

    public EnemySpawner enemySpawner;
    public List<SpawnWave> waves = new List<SpawnWave>();
    public float delayBetweenWaves = 5f;

    private int currentWave = 0;
    private bool isSpawning = false;

    void Start()
    {
        if (enemySpawner == null)
        {
            enemySpawner = GetComponent<EnemySpawner>();
        }
    }

    public void StartWaves()
    {
        if (!isSpawning && waves.Count > 0)
        {
            currentWave = 0;
            isSpawning = true;
            Invoke(nameof(SpawnNextWave), waves[0].delayBeforeWave);
        }
    }

    void SpawnNextWave()
    {
        if (currentWave >= waves.Count)
        {
            isSpawning = false;
            Debug.Log("All waves completed!");
            return;
        }

        SpawnWave wave = waves[currentWave];
        Debug.Log($"Spawning wave {currentWave + 1}: {wave.waveName}");

        enemySpawner.SpawnEnemies(wave.enemyCount);

        currentWave++;

        if (currentWave < waves.Count)
        {
            Invoke(nameof(SpawnNextWave), delayBetweenWaves + waves[currentWave].delayBeforeWave);
        }
        else
        {
            isSpawning = false;
        }
    }

    public void StopWaves()
    {
        CancelInvoke();
        isSpawning = false;
    }
}