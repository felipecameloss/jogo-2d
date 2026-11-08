using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpgradeOption
{
    public string Title;
    public string Description;
    public Action Apply;
}

// Monta as opções de level up: só atributos e passivas.
// Também guarda o catálogo de armas do jogo, usado pela ferraria e pelos baús.
public class UpgradeManager : MonoBehaviour
{
    public LevelUpPanel panel;
    [Tooltip("Armas medievais do jogo: oferecidas pela ferraria e pelos baús.")]
    public Weapon[] availableWeapons;
    public int optionsPerLevel = 3;
    public int maxStatLevel = 5;

    // Lido no Awake de quem precisar: o campo serializado já está pronto, sem depender da ordem dos Start.
    public static Weapon[] Catalog()
    {
        var manager = FindAnyObjectByType<UpgradeManager>();
        return manager && manager.availableWeapons != null ? manager.availableWeapons : new Weapon[0];
    }

    PlayerStats stats;
    PlayerExperience xp;
    readonly Dictionary<string, int> statLevels = new Dictionary<string, int>();
    List<UpgradeOption> currentOptions;
    bool showing;

    void Start()
    {
        stats = GameManager.Instance.Player;
        xp = stats.GetComponent<PlayerExperience>();
        if (panel) panel.gameObject.SetActive(false);
    }

    void Update()
    {
        // Checa todo frame em vez de reagir a evento: assim um level up que aconteça
        // durante a pausa não se perde, ele só aparece quando o jogo voltar.
        if (!showing && xp.PendingLevelUps > 0 && GameManager.Instance.State == GameState.Playing) Show();
    }

    void Show()
    {
        showing = true;
        GameManager.Instance.EnterLevelUp();
        AudioManager.Play(Sfx.LevelUp, 0.7f);
        FxManager.Burst(stats.transform.position, new Color(1f, 0.9f, 0.3f), 20, 5f);

        currentOptions = RollOptions();
        panel.Show(currentOptions, xp.Level - xp.PendingLevelUps + 1, OnPicked);
    }

    void OnPicked(int index)
    {
        currentOptions[index].Apply();
        xp.ConsumePendingLevelUp();
        showing = false;
        GameManager.Instance.ExitLevelUp();
    }

    List<UpgradeOption> RollOptions()
    {
        var pool = new List<UpgradeOption>();

        // Armas vêm de baús e da ferraria; o level up só dá atributos e passivas.
        AddStat(pool, "vida", "Coração Robusto", "+20 de vida máxima", () => stats.AddMaxHealth(20f));
        AddStat(pool, "vel", "Botas Leves", "+10% de velocidade", () => stats.moveSpeed *= 1.1f);
        AddStat(pool, "dano", "Força Bruta", "+12% de dano em tudo", () => stats.damageMultiplier += 0.12f);
        AddStat(pool, "recarga", "Mente Ágil", "-8% de tempo de recarga", () => stats.cooldownMultiplier *= 0.92f);
        AddStat(pool, "area", "Expansão", "+12% de área das armas", () => stats.areaMultiplier += 0.12f);
        AddStat(pool, "ima", "Ímã", "+30% de alcance para coletar", () => stats.pickupRadius *= 1.3f);
        AddStat(pool, "regen", "Regeneração", "Recupera +0.5 de vida por segundo", () => stats.regenPerSecond += 0.5f);
        AddStat(pool, "proj", "Duplicação", "+1 virote, faca, machado e bomba", () => stats.bonusProjectiles++, 2);
        AddStat(pool, "xp", "Sabedoria", "+15% de XP ganho", () => stats.xpMultiplier += 0.15f);
        AddStat(pool, "armadura", "Armadura", "-1 de dano em cada golpe recebido", () => stats.armor += 1f);
        AddStat(pool, "crit", "Olho Afiado", "+8% de chance de crítico (dano x2)", () => stats.critChance += 0.08f);
        AddStat(pool, "duracao", "Ampulheta", "+15% de duração de poças e lentidão", () => stats.durationMultiplier += 0.15f);
        AddStat(pool, "vampiro", "Presas", "+4% de chance de curar 2 de vida ao matar", () => stats.lifestealChance += 0.04f);
        AddStat(pool, "dashcd", "Passo Rápido", "-15% de recarga do dash", () => stats.dashCooldown *= 0.85f);
        AddStat(pool, "dashcarga", "Fôlego Extra", "+1 carga de dash", () => stats.dashCharges++, 2);

        // Embaralha e pega as primeiras.
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        var result = pool.GetRange(0, Mathf.Min(optionsPerLevel, pool.Count));

        // Nunca deixa a tela de level up vazia (tudo no máximo = opção de cura).
        if (result.Count == 0)
            result.Add(new UpgradeOption { Title = "Lanche", Description = "Recupera 30 de vida", Apply = () => stats.Heal(30f) });

        return result;
    }

    void AddStat(List<UpgradeOption> pool, string id, string title, string description, Action apply, int maxLevel = -1)
    {
        if (maxLevel < 0) maxLevel = maxStatLevel;
        statLevels.TryGetValue(id, out int level);
        if (level >= maxLevel) return;

        pool.Add(new UpgradeOption
        {
            Title = $"{title} ({level + 1}/{maxLevel})",
            Description = description,
            Apply = () =>
            {
                statLevels[id] = level + 1;
                apply();
            }
        });
    }
}
