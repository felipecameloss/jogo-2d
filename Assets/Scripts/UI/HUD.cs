using UnityEngine;
using UnityEngine.UI;

// HUD da run. Os textos vêm da cena gerada; no Start eles são reorganizados em painéis opacos:
// nível e XP no canto superior esquerdo, relógio no topo, dash embaixo.
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

    PlayerExperience xp;
    PlayerController controller;
    PlayerWallet wallet;
    int shownGold = -1;
    float goldPunch;
    DayNightCycle cycle;
    float bannerTimer;
    Text xpLabel;

    void Start()
    {
        Layout();
        cycle = DayNightCycle.Instance;
        if (cycle) cycle.OnPeriodChanged += ShowBanner;
        if (bannerText) bannerText.gameObject.SetActive(false);
        if (cycle) ShowBanner(cycle.Period);
    }

    void Layout()
    {
        var root = transform;
        // Nível, abates e barra de XP rotulada.
        var player = UIStyle.Panel(root, "Jogador", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(460f, 120f));
        if (levelText)
        {
            Place(levelText, player, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(UIStyle.Padding, -56f), new Vector2(-UIStyle.Padding, -6f));
            levelText.alignment = TextAnchor.MiddleLeft;
            UIStyle.Style(levelText, UIStyle.Large);
            levelText.color = UIStyle.Gold;
        }
        if (killsText)
        {
            Place(killsText, player, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(UIStyle.Padding, -56f), new Vector2(-UIStyle.Padding, -6f));
            killsText.alignment = TextAnchor.MiddleRight;
            UIStyle.Style(killsText, UIStyle.Body);
        }
        if (xpFill)
        {
            var bar = (RectTransform)xpFill.transform.parent;
            bar.SetParent(player, false);
            bar.anchorMin = new Vector2(0f, 0f);
            bar.anchorMax = new Vector2(1f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.offsetMin = new Vector2(UIStyle.Padding, UIStyle.Padding);
            bar.offsetMax = new Vector2(-UIStyle.Padding, UIStyle.Padding + 36f);
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
            xpLabel = UIStyle.Label(bar, "", UIStyle.Body, TextAnchor.MiddleCenter, Color.white);
            UIStyle.Stretch(xpLabel.rectTransform);
        }

        // Relógio e período no topo.
        var clock = UIStyle.Panel(root, "Relogio", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(460f, 120f));
        if (timerText)
        {
            Place(timerText, clock, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -64f), new Vector2(0f, -4f));
            timerText.alignment = TextAnchor.MiddleCenter;
            UIStyle.Style(timerText, UIStyle.Title);
        }
        if (periodText)
        {
            Place(periodText, clock, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 8f), new Vector2(0f, 48f));
            periodText.alignment = TextAnchor.MiddleCenter;
            UIStyle.Style(periodText, UIStyle.Body);
        }

        // O ouro foi para o painel de materiais; a dica de teclas está nas opções.
        if (goldText) goldText.gameObject.SetActive(false);
        var hint = root.Find("Hint");
        if (hint) hint.gameObject.SetActive(false);
        if (dashText)
        {
            UIStyle.Style(dashText, UIStyle.Body);
            dashText.rectTransform.anchoredPosition = new Vector2(0f, 24f);
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
    }

    void ShowBanner(DayPeriod period)
    {
        if (!bannerText || !cycle) return;
        bool night = period == DayPeriod.Night;
        bannerText.text = night ? $"NOITE {cycle.NightNumber}\n<size=40>Os monstros estão chegando...</size>"
                                : $"DIA {cycle.DayNumber}\n<size=40>Prepare-se para a noite</size>";
        bannerText.color = night ? NightColor : DayColor;
        bannerText.gameObject.SetActive(true);
        bannerTimer = bannerDuration;
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
            string left = GameManager.FormatTime(cycle.TimeLeftInPeriod);
            periodText.text = cycle.IsNight ? $"NOITE {cycle.NightNumber}  •  amanhece em {left}"
                                            : $"DIA {cycle.DayNumber}  •  anoitece em {left}";
            periodText.color = cycle.IsNight ? NightColor : DayColor;
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
        if (killsText) killsText.text = $"{gm.Kills} abates";
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
