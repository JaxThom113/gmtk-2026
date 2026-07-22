using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI Reference")]
	[SerializeField] private TextMeshProUGUI waveCounter;

    [Header("Player Reference")]
	[SerializeField] private GameObject player;

    [Header("Enemy References")]
	[SerializeField] private GameObject enemy1;
	[SerializeField] private GameObject enemy2;
	[SerializeField] private GameObject enemy3;

    [Header("Wave 1")]
    [SerializeField]
    [SerializedDictionary("Enemy", "Amount")]
	SerializedDictionary<GameObject, int> wave1;

    [Header("Wave 2")]
    [SerializeField]
    [SerializedDictionary("Enemy", "Amount")]
	SerializedDictionary<GameObject, int> wave2;

    [Header("Wave 3")]
    [SerializeField]
    [SerializedDictionary("Enemy", "Amount")]
	SerializedDictionary<GameObject, int> wave3;

    private int wave;

    void Start()
    {
        wave = 1;
        StartCoroutine(StartWaves());
    }

    private IEnumerator StartWaves()
    {
        waveCounter.text = $"Starting Wave {wave}...";
        yield return new WaitForSeconds(3f);

        waveCounter.text = $"Wave {wave}";
        yield return SpawnEnemies(wave1);
        wave++;
        waveCounter.text = $"Starting Wave {wave}...";
        yield return new WaitForSeconds(3f);

        waveCounter.text = $"Wave {wave}";
        yield return SpawnEnemies(wave2);
        wave++;
        waveCounter.text = $"Starting Wave {wave}...";
        yield return new WaitForSeconds(3f);

        waveCounter.text = $"Wave {wave}";
        yield return SpawnEnemies(wave3);
        waveCounter.text = $"Victory!";
    }
    
    private IEnumerator SpawnEnemies(Dictionary<GameObject, int> wave)
    {
        Dictionary<GameObject, int> remaining = new Dictionary<GameObject, int>(wave);

        // spawn a random enemy every second from the given wave
        while (remaining.Count > 0)
        {
            // Pick a random enemy type
            List<GameObject> enemies = new List<GameObject>(remaining.Keys);
            GameObject enemy = enemies[Random.Range(0, enemies.Count)];

            // instantiate the enemy and give it a reference to the player's position
            Vector3 randPos = new Vector3(Random.Range(0, 15), 1, Random.Range(0, 15));
            GameObject enemyObj = Instantiate(enemy, randPos, Quaternion.identity);
            enemyObj.GetComponent<Enemy>()?.Initialize(player.transform);

            // decrement the amount of the enemy type just spawned
            remaining[enemy]--;

            // remove the enemy type if all of them have been spawned
            if (remaining[enemy] <= 0)
                remaining.Remove(enemy);

            yield return new WaitForSeconds(1f);
        }
    }
}
