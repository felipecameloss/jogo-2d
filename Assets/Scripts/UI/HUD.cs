using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Image xpFill;
    public Text levelText;
    public Text timerText;
    public Text killsText;
    public Text dashText;
    public Text goldText;
    [Tooltip("Mostra 'DIA 1' / 'NOITE 1' e quanto falta para o próximo período.")]
    public Text periodText;
    [Tooltip("Aviso grande no meio da tela quando o período muda.")]
    public Text bannerText;
    public float bannerDuration = 3f;

    static readonly Color DayColor = new Color(1f, 0.88f, 0.4f);
    static readonly Color NightColor = new Color(0.6f, 0.72f, 1f);
    static readonly Color BossColor = new Color(1f, 0.35f, 0.3f);

    PlayerExperience xp;
    PlayerController controller;
    PlayerWallet wallet;
    int shownGold = -1;
    float goldPunch;
    DayNightCycle cycle;
    float bannerTimer;

    // Barra de vida do chefe (criada por código para não precisar regerar a cena).
    RectTransform bossBar;
    RectTransform bossFill;
    Image bossFillImage;
    Text bossNameText;

    void Start()
    {
        cycle = DayNightCycle.Instance;
        if (cycle) cycle.OnPeriodChanged += ShowBanner;
        Boss.Spawned += OnBossSpawned;
        Boss.Defeated += OnBossDefeated;
        if (bannerText) bannerText.gameObject.SetActive(false);
        if (cycle) ShowBanner(cycle.Period);
        BuildBossBar();
    }

    void OnDestroy()
    {
        if (cycle) cycle.OnPeriodChanged -= ShowBanner;
        Boss.Spawned -= OnBossSpawned;
        Boss.Defeated -= OnBossDefeated;
    }

    void ShowBanner(DayPeriod period)
    {
        if (!cycle) return;
        bool night = period == DayPeriod.Night;
        var spawner = EnemySpawner.Instance;
        if (night && spawner && spawner.IsBossNight(cycle.NightNumber))
        {
            bool big = spawner.BossKindForNight(cycle.NightNumber) == BossKind.Big;
            string hint = big ? "A noite mais escura chegou..." : "Algo grande se aproxima...";
            ShowBanner($"NOITE {cycle.NightNumber}\n<size=40>{hint}</size>", BossColor);
            return;
        }
        ShowBanner(night ? $"NOITE {cycle.NightNumber}\n<size=40>Os monstros estão chegando...</size>"
                         : $"DIA {cycle.DayNumber}\n<size=40>Prepare-se para a noite</size>",
                   night ? NightColor : DayColor);
    }

    void ShowBanner(string text, Color color)
    {
        if (!bannerText) return;
        bannerText.text = text;
        bannerText.color = color;
        bannerText.gameObject.SetActive(true);
        bannerTimer = bannerDuration;
    }

    void OnBossSpawned(Boss boss)
    {
        ShowBanner($"{boss.DisplayName.ToUpper()}\n<size=40>Derrote-o para o sol nascer</size>", BossColor);
        if (bossNameText) bossNameText.text = boss.DisplayName;
    }

    void OnBossDefeated(Boss boss)
    {
        ShowBanner(boss.Kind == BossKind.Big
                ? $"{boss.DisplayName.ToUpper()} CAIU!\n<size=40>Você venceu o chefe final! A run continua...</size>"
                : $"{boss.DisplayName.ToUpper()} DERROTADO!\n<size=40>O sol está nascendo</size>",
            DayColor);
    }

    void BuildBossBar()
    {
        var go = new GameObject("BossBar", typeof(RectTransform));
        bossBar = (RectTransform)go.transform;
        bossBar.SetParent(transform, false);
        bossBar.anchorMin = bossBar.anchorMax = new Vector2(0.5f, 1f);
        bossBar.pivot = new Vector2(0.5f, 1f);
        bossBar.anchoredPosition = new Vector2(0f, -200f);
        bossBar.sizeDelta = new Vector2(900f, 28f);
        var bg = go.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.7f);
        bg.raycastTarget = false;

        var fillGo = new GameObject("Fill", typeof(RectTransform));
        bossFill = (RectTransform)fillGo.transform;
        bossFill.SetParent(bossBar, false);
        bossFill.anchorMin = Vector2.zero;
        bossFill.anchorMax = Vector2.one;
        bossFill.offsetMin = new Vector2(4f, 4f);
        bossFill.offsetMax = new Vector2(-4f, -4f);
        bossFillImage = fillGo.AddComponent<Image>();
        bossFillImage.color = BossColor;
        bossFillImage.raycastTarget = false;

        var nameGo = new GameObject("Name", typeof(RectTransform));
        var nameRt = (RectTransform)nameGo.transform;
        nameRt.SetParent(bossBar, false);
        nameRt.anchorMin = new Vector2(0f, 1f);
        nameRt.anchorMax = new Vector2(1f, 1f);
        nameRt.pivot = new Vector2(0.5f, 0f);
        nameRt.anchoredPosition = new Vector2(0f, 4f);
        nameRt.sizeDelta = new Vector2(0f, 40f);
        bossNameText = nameGo.AddComponent<Text>();
        bossNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bossNameText.fontSize = 30;
        bossNameText.alignment = TextAnchor.LowerCenter;
        bossNameText.color = Color.white;
        bossNameText.raycastTarget = false;
        nameGo.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);

        go.SetActive(false);
    }

    void UpdateBossBar()
    {
        if (!bossBar) return;
        var boss = Boss.Current;
        bool show = boss && !boss.Enemy.IsDead;
        if (bossBar.gameObject.activeSelf != show) bossBar.gameObject.SetActive(show);
        if (!show) return;

        // O preenchimento encolhe da direita para a esquerda conforme o chefe perde vida.
        var max = bossFill.anchorMax;
        max.x = boss.HealthFraction;
        bossFill.anchorMax = max;
        bossFillImage.color = boss.Enraged
            ? Color.Lerp(BossColor, Color.white, 0.25f + 0.25f * Mathf.Sin(Time.time * 10f))
            : BossColor;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (!gm || !gm.Player) return;
        if (!xp) xp = gm.Player.GetComponent<PlayerExperience>();
        if (!controller) controller = gm.Player.GetComponent<PlayerController>();
        if (!wallet) wallet = gm.Player.GetComponent<PlayerWallet>();

        if (xpFill) xpFill.fillAmount = xp.CurrentXP / xp.XPToNext;
        if (levelText) levelText.text = $"Nv {xp.Level}";
        if (timerText) timerText.text = cycle ? cycle.ClockText : GameManager.FormatTime(gm.ElapsedTime);
        if (periodText && cycle)
        {
            if (cycle.IsNight && cycle.HoldNight)
            {
                periodText.text = Boss.Current ? $"NOITE {cycle.NightNumber}  •  derrote o {Boss.Current.DisplayName}"
                                               : $"NOITE {cycle.NightNumber}  •  um chefe se aproxima...";
                periodText.color = BossColor;
            }
            else
            {
                string left = GameManager.FormatTime(cycle.TimeLeftInPeriod);
                periodText.text = cycle.IsNight ? $"NOITE {cycle.NightNumber}  •  amanhece em {left}"
                                                : $"DIA {cycle.DayNumber}  •  anoitece em {left}";
                periodText.color = cycle.IsNight ? NightColor : DayColor;
            }
            if (timerText) timerText.color = periodText.color;
        }
        if (bannerText && bannerTimer > 0f)
        {
            bannerTimer -= Time.deltaTime;
            var c = bannerText.color;
            c.a = Mathf.Clamp01(bannerTimer / 0.6f); // some no final
            bannerText.color = c;
            if (bannerTimer <= 0f) bannerText.gameObject.SetActive(false);
        }
        UpdateBossBar();
        if (killsText) killsText.text = $"Abates: {gm.Kills}";
        if (goldText && wallet)
        {
            // "Pulinho" no texto quando ganha ouro.
            if (wallet.Gold != shownGold)
            {
                if (shownGold >= 0 && wallet.Gold > shownGold) goldPunch = 1f;
                shownGold = wallet.Gold;
                goldText.text = $"Ouro: {shownGold}";
            }
            goldPunch = Mathf.MoveTowards(goldPunch, 0f, Time.unscaledDeltaTime * 5f);
            goldText.rectTransform.localScale = Vector3.one * (1f + goldPunch * 0.2f);
        }
        if (dashText && controller)
        {
            int charges = controller.DashCharges;
            float wait = controller.DashRechargeRemaining;
            string text = charges > 0 ? $"DASH x{charges}" : "DASH";
            if (wait > 0f) text += $"  ({wait:0.0}s)";
            dashText.text = text;
            dashText.color = charges > 0 ? Color.white : new Color(1f, 1f, 1f, 0.4f);
        }
    }
}
