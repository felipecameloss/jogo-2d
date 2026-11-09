using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string gameScene = "Game";
    public GameObject quitButton;

    void Start()
    {
        Time.timeScale = 1f;
        UIStyle.ApplyToScene();
        // Nome e frase vêm de GameInfo: vale também para a cena já gerada.
        SetText("Title", GameInfo.Title.ToUpperInvariant());
        SetText("Subtitle", GameInfo.Tagline);
        // Opções logo abaixo de "Jogar", copiando o botão "Sair" antes de ele sumir no navegador.
        if (quitButton) UIStyle.CloneButton(quitButton, "Opções", quitButton.transform.GetSiblingIndex(), () => OptionsMenu.Open());
        // No navegador (WebGL/itch.io) não existe "sair do jogo".
        if (quitButton && Application.platform == RuntimePlatform.WebGLPlayer) quitButton.SetActive(false);
    }

    void SetText(string child, string value)
    {
        var found = transform.Find(child);
        var text = found ? found.GetComponent<Text>() : null;
        if (text) text.text = value;
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
