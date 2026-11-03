using System;
using UnityEngine;

// Gera sons simples de onda quadrada/ruído por código. Só placeholder!
public static class SfxGenerator
{
    const int SampleRate = 44100;
    static readonly System.Random rng = new System.Random(1234);

    static AudioClip Make(string name, float duration, Func<float, float> sample)
    {
        int n = Mathf.CeilToInt(SampleRate * duration);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SampleRate;
            float envelope = 1f - t / duration;
            data[i] = sample(t) * envelope * envelope;
        }
        var clip = AudioClip.Create(name, n, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Fase de uma onda cuja frequência vai de f0 a f1 linearmente em "duration".
    static float Sweep(float f0, float f1, float duration, float t) => f0 * t + 0.5f * (f1 - f0) / duration * t * t;
    static float Square(float phase) => phase - Mathf.Floor(phase) < 0.5f ? 1f : -1f;
    static float Sine(float phase) => Mathf.Sin(phase * Mathf.PI * 2f);
    static float Noise() => (float)rng.NextDouble() * 2f - 1f;

    public static AudioClip Shoot() => Make("sfx_shoot", 0.12f, t => 0.25f * Square(Sweep(900f, 350f, 0.12f, t)));
    public static AudioClip Hit() => Make("sfx_hit", 0.07f, t => 0.35f * Noise());
    public static AudioClip EnemyDie() => Make("sfx_enemy_die", 0.2f, t => 0.25f * Noise() + 0.2f * Square(Sweep(300f, 60f, 0.2f, t)));
    public static AudioClip Pickup() => Make("sfx_pickup", 0.09f, t => 0.35f * Sine(Sweep(900f, 1600f, 0.09f, t)));
    public static AudioClip PlayerHurt() => Make("sfx_hurt", 0.25f, t => 0.3f * Square(Sweep(420f, 90f, 0.25f, t)));
    public static AudioClip Select() => Make("sfx_select", 0.08f, t => 0.3f * Sine(660f * t));

    public static AudioClip LevelUp()
    {
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
        return Make("sfx_levelup", 0.48f, t =>
        {
            int idx = Mathf.Min(notes.Length - 1, (int)(t / 0.12f));
            return 0.2f * Square(notes[idx] * t);
        });
    }
}
