using System;
using UnityEngine;

public enum PersonRole { WeaponSmith, ArmorSmith }
public enum PersonState { Lost, Returning, Resident }

// Pessoa perdida pelo mapa. Resgatada com F, anda sozinha até o acampamento e vira residente.
// Residente leva dano de monstros encostados (eles não a procuram); derrotada, volta a ficar perdida.
public class LostPerson : MonoBehaviour
{
    public const float MaxHealth = 40f;
    const float WalkSpeed = 3.5f;
    const float TouchRadius = 0.6f;
    const float HitInterval = 2f, HitFactor = 0.5f; // igual ao dano em construções
    const float IdleFrameTime = 0.13f, RunFrameTime = 0.07f;
    const int FramesPerAnimation = 8, IdleAnimation = 0, RunAnimation = 2;

    public PersonRole Role { get; private set; }
    public PersonState State { get; private set; }
    public float Health { get; private set; } = MaxHealth;
    // Ponto do acampamento onde fica; com a estação construída, aparece no desenho dela.
    public Vector3 Home { get; set; }
    public bool Hidden { set { if (sr) sr.enabled = !value; } }
    public string Name => Role == PersonRole.WeaponSmith ? "o ferreiro" : "a ferreira";
    public string Title => Role == PersonRole.WeaponSmith ? "Ferreiro" : "Ferreira";
    public string Ending => Role == PersonRole.WeaponSmith ? "o" : "a"; // concordância: resgatado/resgatada

    public event Action<LostPerson> OnStateChanged;

    SpriteRenderer sr;
    Sprite[] frames;
    Color tint;
    int direction;
    int frame;
    float frameTimer, flashTimer, nextHit;

    public void Setup(PersonRole role, Sprite[] sheet, Color color)
    {
        Role = role;
        frames = sheet;
        tint = color;
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        sr.spriteSortPoint = SpriteSortPoint.Pivot;
        sr.color = tint;
        State = PersonState.Lost;
        Animate(IdleAnimation, IdleFrameTime);
    }

    public void Rescue()
    {
        if (State != PersonState.Lost) return;
        Health = MaxHealth;
        Hidden = false;
        SetState(PersonState.Returning);
        FxManager.Burst(transform.position, new Color(1f, 0.9f, 0.5f), 14, 4f);
        FxManager.Popup(transform.position + Vector3.up, $"{Title} resgatad{Ending}!", new Color(1f, 0.9f, 0.5f));
        AudioManager.Play(Sfx.LevelUp, 0.6f);
    }

    // Volta a ficar perdida em outro ponto do mapa.
    public void Defeat(Vector3 newPosition)
    {
        if (State != PersonState.Resident) return;
        FxManager.Burst(transform.position, new Color(1f, 0.4f, 0.3f), 18, 5f);
        FxManager.Popup(transform.position + Vector3.up, $"{Title} foi derrotad{Ending} e se perdeu!", new Color(1f, 0.5f, 0.4f));
        AudioManager.Play(Sfx.PlayerHurt, 0.8f);
        transform.position = newPosition;
        Hidden = false;
        SetState(PersonState.Lost);
    }

    void SetState(PersonState state)
    {
        State = state;
        OnStateChanged?.Invoke(this);
    }

    void Update()
    {
        if (State == PersonState.Returning) Walk();
        else Animate(IdleAnimation, IdleFrameTime);
        if (State == PersonState.Resident) CheckHits();

        if (flashTimer > 0f) flashTimer -= Time.deltaTime;
        if (sr) sr.color = flashTimer > 0f ? new Color(1f, 0.45f, 0.4f) : tint;
    }

    // Atravessa muros e portões: anda direto, sem colisor.
    void Walk()
    {
        Vector3 offset = Home - transform.position;
        if (offset.sqrMagnitude < 0.01f)
        {
            transform.position = Home;
            SetState(PersonState.Resident);
            FxManager.Popup(transform.position + Vector3.up, $"{Title} chegou ao acampamento", new Color(1f, 0.9f, 0.5f));
            return;
        }
        if (Time.deltaTime > 0f) direction = CharacterAnimator.DirectionIndex(offset);
        transform.position = Vector3.MoveTowards(transform.position, Home, WalkSpeed * Time.deltaTime);
        Animate(RunAnimation, RunFrameTime);
    }

    // Monstros encostados batem como em construções: metade do dano de contato a cada 2 s.
    void CheckHits()
    {
        if (Time.time < nextHit) return;
        float damage = 0f;
        foreach (var enemy in Enemy.Active)
            if (enemy && !enemy.IsDead && ((Vector2)(enemy.transform.position - transform.position)).sqrMagnitude < TouchRadius * TouchRadius)
                damage += enemy.contactDamage * HitFactor;
        if (damage <= 0f) return;
        nextHit = Time.time + HitInterval;
        Health = Mathf.Max(0f, Health - damage);
        flashTimer = 0.15f;
        FxManager.Popup(transform.position + Vector3.up * 0.8f, $"{Title} -{damage:0}", new Color(1f, 0.55f, 0.45f), 0.8f);
        AudioManager.Play(Sfx.Hit, 0.3f);
        if (Health <= 0f && CampSystem.Instance) CampSystem.Instance.DefeatPerson(this);
    }

    void Animate(int animation, float frameTime)
    {
        if (!sr || frames == null || frames.Length == 0) return;
        frameTimer += Time.deltaTime;
        while (frameTimer >= frameTime)
        {
            frameTimer -= frameTime;
            frame = (frame + 1) % FramesPerAnimation;
        }
        int index = ((animation * 8) + direction) * FramesPerAnimation + frame;
        sr.sprite = frames[Mathf.Clamp(index, 0, frames.Length - 1)];
    }
}
