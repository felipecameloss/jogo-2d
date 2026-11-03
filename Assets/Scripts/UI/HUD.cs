using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Image xpFill;
    public Text levelText;
    public Text timerText;
    public Text killsText;
    public Text dashText;

    PlayerExperience xp;
    PlayerController controller;

    void Update()
    {
        var gm = GameManager.Instance;
        if (!gm || !gm.Player) return;
        if (!xp) xp = gm.Player.GetComponent<PlayerExperience>();
        if (!controller) controller = gm.Player.GetComponent<PlayerController>();

        if (xpFill) xpFill.fillAmount = xp.CurrentXP / xp.XPToNext;
        if (levelText) levelText.text = $"Nv {xp.Level}";
        if (timerText) timerText.text = GameManager.FormatTime(gm.ElapsedTime);
        if (killsText) killsText.text = $"Abates: {gm.Kills}";
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
