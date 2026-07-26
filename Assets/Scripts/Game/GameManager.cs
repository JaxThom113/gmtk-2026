using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using TMPro;
using Sezylrin.SimplePooling;
using Unity.VisualScripting;

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

    [Header("Game Settings")]
    [SerializeField] private GameSO game;
    [SerializeField] private List<Transform> spawnLocations;

    [Header("Resetter")]
	[SerializeField] private ResetterOBJ resetter;

    [Header("Environment VFX")]
    [SerializeField] private GameObject healthVfxPlane;

    private Coroutine startWaves;
    private GameObject enemyContainer;
    private GameObject spawnedPlayer;

    private AudioObj menuTrack; 
    private AudioObj gameTrack; 

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

    void Start()
    {
        // start menu music
        if (gameTrack != null && gameTrack.IsSourcePlaying())
            gameTrack.StopSound();
        menuTrack = AudioManager.Instance.PlaySound(AudioRef.MenuMusic, true);
    }

    private void StartGame()
    {
        gamePlaying.Bool = true;
        playerDead.Bool = false;
        cameraManager.ActivateCamera(1);

        // reset stats
        resetter.ResetValues();

        enemyContainer = new GameObject("EnemyContainer");

        spawnedPlayer = Instantiate(player, playerSpawnPoint, Quaternion.identity);
        var inputController = spawnedPlayer.GetComponent<InputController>();
        inputController?.SetPauseMenu(pauseMenu);

        if (healthVfxPlane != null)
            healthVfxPlane.SetActive(true);

        // attach the camera follow point
        playerPoint.position = playerSpawnPoint;
        playerPoint.SetParent(spawnedPlayer.transform, true);

        uiManager.gameObject.SetActive(true);
        
        startWaves = StartCoroutine(PlayGame());

        // start game music
        if (menuTrack != null && menuTrack.IsSourcePlaying())
            menuTrack.StopSound();

        List<AudioRef> gameTracks = new List<AudioRef>()
        {
            AudioRef.Track1,
            AudioRef.Track2,
            AudioRef.Track3,
            AudioRef.Track4,
            AudioRef.Track5
        };
        int randTrack = Random.Range(0, gameTracks.Count);
        gameTrack = AudioManager.Instance.PlaySound(gameTracks[randTrack], true);
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

        if (healthVfxPlane != null)
            healthVfxPlane.SetActive(false);

        uiManager.gameObject.SetActive(false);

        // start menu music
        if (gameTrack != null && gameTrack.IsSourcePlaying())
            gameTrack.StopSound();
        menuTrack = AudioManager.Instance.PlaySound(AudioRef.MenuMusic, true);
    }

    private void RefreshGame()
    {
        EndGame();
        StartGame();
    }

    private IEnumerator PlayGame()
    {
        yield return new WaitForSeconds(3);

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
                Vector3 randPos;
                if (spawnLocations == null)
                    randPos = new Vector3(Random.Range(0, 15), 1.25f, Random.Range(0, 15));
                else
                    randPos = spawnLocations[Random.Range(0, spawnLocations.Count)].position;
                
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
