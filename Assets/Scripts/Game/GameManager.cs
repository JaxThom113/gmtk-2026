using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using TMPro;
using Sezylrin.SimplePooling;

public class GameManager : MonoBehaviour
{
    [Header("Player Reference")]
	[SerializeField] private Transform playerPoint;
	[SerializeField] private GameObject player;

    [Header("Menu References")]
	[SerializeField] private MainMenu mainMenu;
	[SerializeField] private DeathScreen deathScreen;

    [Header("Manager References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CameraManager cameraManager;

    [Header("SO References")]
	[SerializeField] private BoolSO gamePlaying;
	[SerializeField] private IntSO gameWave;
	[SerializeField] private BoolSO playerDead;
	[SerializeField] private IntSO playerMaxHealth;
	[SerializeField] private IntSO playerSpeed;
	[SerializeField] private IntSO playerLevel;
    [SerializeField] private IntSO playerExperience;
    [SerializeField] private IntSO playerExperienceToNextLevel;
	[SerializeField] private IntSO playerSelectedWeapon;
	[SerializeField] private IntSO playerWeaponCount;
	[SerializeField] private BoolSO playerWeaponFull;

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

    private Coroutine startWaves;
    private GameObject enemyContainer;
    private GameObject spawnedPlayer;

    void OnEnable()
    {
        mainMenu.OnStartGame += StartGame;
        deathScreen.OnEndGame += EndGame;
    }

    void OnDisable()
    {
        mainMenu.OnStartGame -= StartGame;
        deathScreen.OnEndGame -= EndGame;
    }

    private void StartGame()
    {
        gamePlaying.Bool = true;
        playerDead.Bool = false;
        cameraManager.ActivateCamera(1);

        // reset stats
        gameWave.Int = 1;
        playerMaxHealth.Int = 30;
        playerSpeed.Int = 8;
        playerLevel.Int = 1;
        playerExperience.Int = 0;
        playerExperienceToNextLevel.Int = 100;
        playerSelectedWeapon.Int = 0;
        playerWeaponCount.Int = 0;
        playerWeaponFull.Bool = false;

        enemyContainer = new GameObject("EnemyContainer");

        // spawn the player, attach the camera follow point
        spawnedPlayer = Instantiate(player, new Vector3(0, 1.25f, 0), Quaternion.identity);
        playerPoint.position = new Vector3(0, 1.25f, 0);
        playerPoint.SetParent(spawnedPlayer.transform, true);

        uiManager.gameObject.SetActive(true);
        
        startWaves = StartCoroutine(StartWaves());
    }

    private void EndGame()
    {
        gamePlaying.Bool = false;
        cameraManager.ActivateCamera(0);

        StopCoroutine(startWaves);

        Destroy(enemyContainer);
        // Pooler.ClearObject<Enemy>();

        // despawn the player, detach the camera follow point
        playerPoint.SetParent(null, true);
        Destroy(spawnedPlayer);
        spawnedPlayer = null;

        uiManager.gameObject.SetActive(false);
    }

    private IEnumerator StartWaves()
    {
        yield return new WaitForSeconds(3f);

        yield return SpawnEnemies(wave1);
        gameWave.Int++;
        yield return new WaitForSeconds(3f);

        yield return SpawnEnemies(wave2);
        gameWave.Int++;
        yield return new WaitForSeconds(3f);

        yield return SpawnEnemies(wave3);
        gameWave.Int++;
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
            Pooler.GetObject<Enemy>(
                enemy,
                randPos,
                Quaternion.identity,
                enemyContainer.transform,
                onNewInstance: (e) => e.Initialize(spawnedPlayer != null ? spawnedPlayer.transform : playerPoint.transform),
                onGet: (e) => e.ResetObj()
            );

            // decrement the amount of the enemy type just spawned
            remaining[enemy]--;

            // remove the enemy type if all of them have been spawned
            if (remaining[enemy] <= 0)
                remaining.Remove(enemy);

            yield return new WaitForSeconds(1f);
        }
    }
}
