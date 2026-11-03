using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameScene = "Game";
    public GameObject quitButton;

    void Start()
    {
        Time.timeScale = 1f;
        // No navegador (WebGL/itch.io) não existe "sair do jogo".
        if (quitButton && Application.platform == RuntimePlatform.WebGLPlayer) quitButton.SetActive(false);
    }

    public void Play()
    {
        AudioManager.Play(Sfx.Select);
        SceneManager.LoadScene(gameScene);
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
