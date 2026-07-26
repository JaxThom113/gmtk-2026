using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using TMPro;
using Sezylrin.SimplePooling;

public class GameManager : MonoBehaviour
{
    [Header("Player Reference")]
	[SerializeField] private Vector3 playerSpawnPoint;
	[SerializeField] private Transform playerPoint;
	[SerializeField] private GameObject player;

    [Header("Menu References")]
	[SerializeField] private MainMenu mainMenu;
	[SerializeField] private DeathScreen deathScreen;
	[SerializeField] private WinScreen winScreen;
	[SerializeField] private PauseMenu pauseMenu;

    [Header("Manager References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CameraManager cameraManager;

    [Header("SO References")]
	[SerializeField] private BoolSO gamePlaying;
	[SerializeField] private IntSO gameWave;
	[SerializeField] private BoolSO playerDead;
	[SerializeField] private BoolSO gameWin;
	[SerializeField] private IntSO playerMaxHealth;
	[SerializeField] private IntSO playerSpeed;
	[SerializeField] private IntSO playerLevel;
    [SerializeField] private IntSO playerExperience;
    [SerializeField] private IntSO playerExperienceToNextLevel;
	[SerializeField] private IntSO playerSelectedWeapon;
	[SerializeField] private IntSO playerWeaponCount;
	[SerializeField] private BoolSO playerWeaponFull;

    [Header("Game to Play")]
    [SerializeField] private GameSO game;

    [Header("Resetter")]
	[SerializeField] private ResetterOBJ resetter;

    private Coroutine startWaves;
    private GameObject enemyContainer;
    private GameObject spawnedPlayer;

    void OnEnable()
    {
        mainMenu.OnStartGame += StartGame;

        deathScreen.OnDeathStartGame += RefreshGame;
        winScreen.OnWinStartGame += RefreshGame;

        deathScreen.OnEndGame += EndGame;
        winScreen.OnWinGame += EndGame;
        pauseMenu.OnEndGamePause += EndGame;
    }

    void OnDisable()
    {
        mainMenu.OnStartGame -= StartGame;
        deathScreen.OnEndGame -= EndGame;
        winScreen.OnWinGame -= EndGame;
    }

    private void StartGame()
    {
        gamePlaying.Bool = true;
        playerDead.Bool = false;
        cameraManager.ActivateCamera(1);

        // reset stats
        resetter.ResetValues();
        // gameWave.Int = 1;
        // playerMaxHealth.Int = 30;
        // playerSpeed.Int = 8;
        // playerLevel.Int = 1;
        // playerExperience.Int = 0;
        // playerExperienceToNextLevel.Int = 100;
        // playerSelectedWeapon.Int = 0;
        // playerWeaponCount.Int = 0;
        // playerWeaponFull.Bool = false;

        enemyContainer = new GameObject("EnemyContainer");

        // spawn the player, give its InputManager a reference to the PauseMenu
        spawnedPlayer = Instantiate(player, playerSpawnPoint, Quaternion.identity);
        var inputController = spawnedPlayer.GetComponent<InputController>();
        inputController?.SetPauseMenu(pauseMenu);

        // attach the camera follow point
        playerPoint.position = playerSpawnPoint;
        playerPoint.SetParent(spawnedPlayer.transform, true);

        uiManager.gameObject.SetActive(true);
        
        startWaves = StartCoroutine(PlayGame());
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

    private void RefreshGame()
    {
        EndGame();
        StartGame();
    }

    private IEnumerator PlayGame()
    {
        foreach (var wave in game.waves)
        {
            yield return PlayWave(wave);
            gameWave.Int++;
            yield return new WaitForSeconds(game.waveDelay);
        }

        // win state
        gameWin.Bool = true;
    }
    
    private IEnumerator PlayWave(WaveSO wave)
    {
        foreach (var spawn in wave.spawns)
        {
            // spawn however many enemies requested on the SpawnSO
            for (int i = 0; i < spawn.count; i++)
            {
                Vector3 randPos = new Vector3(Random.Range(0, 15), 1, Random.Range(0, 15));
                Pooler.GetObject<Enemy>(
                    spawn.enemy,
                    randPos,
                    Quaternion.identity,
                    enemyContainer.transform,
                    onNewInstance: (e) => e.Initialize(spawnedPlayer != null ? spawnedPlayer.transform : playerPoint.transform),
                    onGet: (e) => e.ResetObj()
                );
            }

            yield return new WaitForSeconds(spawn.delay);
        }
    }
}
