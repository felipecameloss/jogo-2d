using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState { Playing, Paused, LevelUp, GameOver }

// Controla o estado da run (infinita): tempo, abates, pausa, level up e derrota.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public string mainMenuScene = "MainMenu";

    [Header("UI")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public Text gameOverTitle;
    public Text gameOverStats;

    public GameState State { get; private set; } = GameState.Playing;
    public float ElapsedTime { get; private set; }
    public int Kills { get; private set; }
    public PlayerStats Player { get; private set; }

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        Player = FindAnyObjectByType<PlayerStats>();
        if (pausePanel) pausePanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    void Start()
    {
        // Também funciona nas cenas já existentes: não é preciso regenerar o protótipo.
        if (Player && DayNightCycle.Instance && !GetComponent<DayResourceSystem>())
            gameObject.AddComponent<DayResourceSystem>();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (GameInput.PausePressed())
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        if (State != GameState.Playing) return;

        // A run é infinita: dura enquanto o jogador aguentar as noites.
        ElapsedTime += Time.deltaTime;
    }

    public void RegisterKill() => Kills++;

    public void Pause()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
        if (pausePanel) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
        if (pausePanel) pausePanel.SetActive(false);
    }

    public void EnterLevelUp()
    {
        if (State != GameState.Playing) return;
        State = GameState.LevelUp;
        Time.timeScale = 0f;
    }

    public void ExitLevelUp()
    {
        if (State != GameState.LevelUp) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void EndRun()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;
        Time.timeScale = 0f;
        if (pausePanel) pausePanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(true);

        var xp = Player ? Player.GetComponent<PlayerExperience>() : null;
        var cycle = DayNightCycle.Instance;
        if (gameOverTitle)
            gameOverTitle.text = cycle && cycle.IsNight ? $"VOCÊ CAIU NA NOITE {cycle.NightNumber}..." : "VOCÊ CAIU...";
        if (gameOverStats)
        {
            string nights = cycle ? $"\nNoites enfrentadas: {cycle.NightNumber}" : "";
            var wallet = Player ? Player.GetComponent<PlayerWallet>() : null;
            var materials = Player ? Player.GetComponent<ResourceInventory>() : null;
            string resources = materials ? $"\nMateriais coletados: {materials.TotalCollected}" : "";
            string gold = wallet ? $"\nOuro coletado: {wallet.TotalEarned}" : "";
            gameOverStats.text = $"Tempo: {FormatTime(ElapsedTime)}{nights}\nNível: {(xp ? xp.Level : 1)}\nAbates: {Kills}{gold}{resources}";
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public static string FormatTime(float seconds)
    {
        int s = Mathf.FloorToInt(seconds);
        return $"{s / 60:00}:{s % 60:00}";
    }
}
