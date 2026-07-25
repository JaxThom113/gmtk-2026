using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using TMPro;
using Sezylrin.SimplePooling;

public class GameManager : MonoBehaviour
{
    [Header("Player Reference")]
	[SerializeField] private GameObject player;

    [Header("UI References")]
	[SerializeField] private TextMeshProUGUI waveCounter;
	[SerializeField] private DeathScreen deathScreen;

    [Header("SO References")]
	[SerializeField] private BoolSO gameStarted;
	[SerializeField] private BoolSO playerDead;
	[SerializeField] private IntSO playerHealth;
	[SerializeField] private IntSO playerLevel;
	[SerializeField] private IntSO playerSelectedWeapon;
	[SerializeField] private IntSO playerWeaponCount;
	[SerializeField] private BoolSO playerWeaponFull;

    /*
        I need to:
        reset weapons
        reset upgrades
        reset time
        reset max health
    */

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
    private bool running;
    private Coroutine startWaves;

    void Start()
    {
        running = false;
    }

    void Update()
    {
        if (!running && gameStarted.Bool)
        {
            wave = 1;
            startWaves = StartCoroutine(StartWaves());
            running = true;
        }

        if (playerDead.Bool)
        {
            StopCoroutine(startWaves);
            deathScreen.gameObject.SetActive(true);
            playerDead.Bool = false;
            gameStarted.Bool = false;
            running = false;
        }
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
            /*GameObject enemyObj = Instantiate(enemy, randPos, Quaternion.identity);
            enemyObj.GetComponent<Enemy>()?.Initialize(player.transform);*/
            Pooler.GetObject<Enemy>(enemy, randPos, Quaternion.identity, 
                onNewInstance: (e) => e.Initialize(player.transform),
                onGet: (e) => e.ResetObj());
            // decrement the amount of the enemy type just spawned
            remaining[enemy]--;

            // remove the enemy type if all of them have been spawned
            if (remaining[enemy] <= 0)
                remaining.Remove(enemy);

            yield return new WaitForSeconds(1f);
        }
    }
}
