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

    PlayerExperience xp;
    PlayerController controller;
    PlayerWallet wallet;
    int shownGold = -1;
    float goldPunch;
    DayNightCycle cycle;
    float bannerTimer;

    void Start()
    {
        cycle = DayNightCycle.Instance;
        if (cycle) cycle.OnPeriodChanged += ShowBanner;
        if (bannerText) bannerText.gameObject.SetActive(false);
        if (cycle) ShowBanner(cycle.Period);
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

        if (xpFill) xpFill.fillAmount = xp.CurrentXP / xp.XPToNext;
        if (levelText) levelText.text = $"Nv {xp.Level}";
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
