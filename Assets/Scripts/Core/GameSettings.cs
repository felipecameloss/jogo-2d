using System;
using UnityEngine;

// Preferências do jogador, guardadas em PlayerPrefs (no navegador, ficam no IndexedDB).
public static class GameSettings
{
    const string MusicKey = "volume_musica", SfxKey = "volume_efeitos";
    public const float DefaultMusic = 0.4f, DefaultSfx = 0.5f;

    public static event Action Changed;

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicKey, DefaultMusic);
        set { PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value)); Changed?.Invoke(); }
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxKey, DefaultSfx);
        set { PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value)); Changed?.Invoke(); }
    }

    // Grava em disco. Chamado ao fechar as opções, não a cada movimento do slider.
    public static void Save() => PlayerPrefs.Save();
}
