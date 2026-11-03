using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Image xpFill;
    public Text levelText;
    public Text timerText;
    public Text killsText;

    PlayerExperience xp;

    void Update()
    {
        var gm = GameManager.Instance;
        if (!gm || !gm.Player) return;
        if (!xp) xp = gm.Player.GetComponent<PlayerExperience>();

        if (xpFill) xpFill.fillAmount = xp.CurrentXP / xp.XPToNext;
        if (levelText) levelText.text = $"Nv {xp.Level}";
        if (timerText) timerText.text = GameManager.FormatTime(gm.ElapsedTime);
        if (killsText) killsText.text = $"Abates: {gm.Kills}";
    }
}
