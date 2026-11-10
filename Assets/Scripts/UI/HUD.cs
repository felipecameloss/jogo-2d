using UnityEngine;
using UnityEngine.UI;

// HUD da run. Os textos vêm da cena gerada; no Start eles são reorganizados em painéis opacos:
// relógio no topo, nível e XP no rodapé, dash no canto inferior direito.
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
    Text xpLabel;

    // Barra de vida do chefe (criada por código para não precisar regerar a cena).
    RectTransform bossBar;
    RectTransform bossFill;
    Image bossFillImage;
    Text bossNameText;

    void Start()
    {
        Layout();
        cycle = DayNightCycle.Instance;
        if (cycle) cycle.OnPeriodChanged += ShowBanner;
        Boss.Spawned += OnBossSpawned;
        Boss.Defeated += OnBossDefeated;
        if (bannerText) bannerText.gameObject.SetActive(false);
        if (cycle) ShowBanner(cycle.Period);
        BuildBossBar();
    }

    void Layout()
    {
        var root = transform;

        // Faixa transparente do rodapé: altura 64 para caber o número de XP acima da barra fina.
        var footerGo = new GameObject("RodapeXP", typeof(RectTransform));
        var footer = footerGo.GetComponent<RectTransform>();
        footer.SetParent(root, false);
        footer.anchorMin = new Vector2(0f, 0f);
        footer.anchorMax = new Vector2(1f, 0f);
        footer.pivot = new Vector2(0.5f, 0f);
        footer.offsetMin = new Vector2(20f, 12f);
        footer.offsetMax = new Vector2(-20f, 76f);

        if (levelText)
        {
            var lRect = levelText.rectTransform;
            lRect.SetParent(footer, false);
            lRect.anchorMin = new Vector2(0f, 0f);
            lRect.anchorMax = new Vector2(0f, 1f);
            lRect.pivot = new Vector2(0f, 0.5f);
            lRect.anchoredPosition = Vector2.zero;
            lRect.sizeDelta = new Vector2(150f, 0f);
            levelText.alignment = TextAnchor.MiddleLeft;
            UIStyle.Style(levelText, UIStyle.Body);
            UIStyle.Bold(levelText);
            levelText.color = UIStyle.Gold;
        }

        if (killsText) killsText.gameObject.SetActive(false);

        if (xpFill)
        {
            var bar = (RectTransform)xpFill.transform.parent;
            bar.SetParent(footer, false);
            bar.anchorMin = new Vector2(0f, 0f);
            bar.anchorMax = new Vector2(1f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.offsetMin = new Vector2(180f, 8f);
            bar.offsetMax = new Vector2(-20f, 20f);
            var background = bar.GetComponent<Image>();
            if (background) { background.sprite = null; background.color = new Color(0.05f, 0.03f, 0.02f); }

            // Preenchimento pela âncora: funciona sem depender do sprite da cena.
            var fill = xpFill.rectTransform;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0f, 1f);
            fill.offsetMin = new Vector2(3f, 3f);
            fill.offsetMax = new Vector2(-3f, -3f);
            xpFill.sprite = null;
            xpFill.type = Image.Type.Simple;
            xpFill.color = new Color(0.3f, 0.62f, 1f);

            // Rótulo numérico acima do trilho dentro da faixa
            xpLabel = UIStyle.Label(footer, "", UIStyle.Body, TextAnchor.MiddleCenter, Color.white);
            var xlRect = xpLabel.rectTransform;
            xlRect.anchorMin = new Vector2(0f, 0f);
            xlRect.anchorMax = new Vector2(1f, 0f);
            xlRect.pivot = new Vector2(0.5f, 0f);
            xlRect.offsetMin = new Vector2(180f, 24f);
            xlRect.offsetMax = new Vector2(-20f, 56f);
        }

        // Relógio e período no centro superior: 480×104 em (0, -20).
        var clock = UIStyle.Panel(root, "Relogio", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(480f, 104f));
        if (timerText)
        {
            Place(timerText, clock, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -52f), new Vector2(0f, -4f));
            timerText.alignment = TextAnchor.MiddleCenter;
            UIStyle.Style(timerText, UIStyle.Title);
            timerText.fontSize = 48;
        }
        if (periodText)
        {
            Place(periodText, clock, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 6f), new Vector2(0f, 44f));
            periodText.alignment = TextAnchor.MiddleCenter;
            UIStyle.Style(periodText, UIStyle.Body);
            periodText.resizeTextForBestFit = true;
            periodText.resizeTextMinSize = 24;
            periodText.resizeTextMaxSize = 32;
        }

        // O ouro foi para o painel de materiais; a dica de teclas está nas opções.
        if (goldText) goldText.gameObject.SetActive(false);
        var hint = root.Find("Hint");
        if (hint) hint.gameObject.SetActive(false);
        if (dashText)
        {
            UIStyle.Style(dashText, UIStyle.Body);
            var dRect = dashText.rectTransform;
            dRect.anchorMin = new Vector2(1f, 0f);
            dRect.anchorMax = new Vector2(1f, 0f);
            dRect.pivot = new Vector2(1f, 0f);
            dRect.anchoredPosition = new Vector2(-20f, 80f);
            dRect.sizeDelta = new Vector2(300f, 48f);
            dashText.alignment = TextAnchor.MiddleRight;
        }
        if (bannerText) UIStyle.Style(bannerText, UIStyle.Huge);
    }

    static void Place(Text text, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var rect = text.rectTransform;
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
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
            ShowBanner($"NOITE {cycle.NightNumber}\n<size=32>{hint}</size>", BossColor);
            return;
        }
        ShowBanner(night ? $"NOITE {cycle.NightNumber}\n<size=32>Os monstros estão chegando...</size>"
                         : $"DIA {cycle.DayNumber}\n<size=32>Prepare-se para a noite</size>",
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
        ShowBanner($"{boss.DisplayName.ToUpper()}\n<size=32>Derrote-o para o sol nascer</size>", BossColor);
        if (bossNameText) bossNameText.text = boss.DisplayName;
    }

    void OnBossDefeated(Boss boss)
    {
        ShowBanner(boss.Kind == BossKind.Big
                ? $"{boss.DisplayName.ToUpper()} CAIU!\n<size=32>Você venceu o chefe final! A run continua...</size>"
                : $"{boss.DisplayName.ToUpper()} DERROTADO!\n<size=32>O sol está nascendo</size>",
            DayColor);
    }

    void BuildBossBar()
    {
        var go = new GameObject("BossBar", typeof(RectTransform));
        bossBar = (RectTransform)go.transform;
        bossBar.SetParent(transform, false);
        bossBar.anchorMin = bossBar.anchorMax = new Vector2(0.5f, 1f);
        bossBar.pivot = new Vector2(0.5f, 1f);
        // Abaixo da fogueira com vão de 40 (fogueira vai até y = -206).
        bossBar.anchoredPosition = new Vector2(0f, -246f);
        bossBar.sizeDelta = new Vector2(760f, 28f);
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
        UIStyle.Bold(bossNameText);
        UIStyle.Style(bossNameText, UIStyle.Body);
        bossNameText.alignment = TextAnchor.LowerCenter;
        bossNameText.color = Color.white;
        bossNameText.raycastTarget = false;

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

        if (xpFill) xpFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(xp.CurrentXP / xp.XPToNext), 1f);
        if (levelText) levelText.text = $"NÍVEL {xp.Level}";
        if (xpLabel) xpLabel.text = $"XP {Mathf.FloorToInt(xp.CurrentXP)} / {Mathf.CeilToInt(xp.XPToNext)}";
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
            dashText.color = charges > 0 ? Color.white : UIStyle.Muted;
        }
    }
}
