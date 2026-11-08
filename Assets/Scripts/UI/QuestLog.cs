using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Missões no canto esquerdo, abaixo dos materiais: tutorial, resgates, estações, baú mais
// próximo e a meta do período. Substitui as setas de direção soltas pela tela.
public class QuestLog : MonoBehaviour
{
    const float Width = UIStyle.ColumnWidth, TitleHeight = 40f;

    RectTransform frame;
    Text body;
    GameObject root;
    readonly StringBuilder builder = new StringBuilder();

    void Start()
    {
        var canvas = UIStyle.Canvas(transform, "QuestLog", -1);
        root = canvas.gameObject;
        var panel = UIStyle.Panel(canvas, "Missoes", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -318f), new Vector2(Width, 200f));
        frame = (RectTransform)panel.parent;
        var title = UIStyle.Bold(UIStyle.Label(panel, "MISSÕES", UIStyle.Body, TextAnchor.UpperLeft, UIStyle.Gold));
        UIStyle.Stretch(title.rectTransform, UIStyle.Padding);
        body = UIStyle.Label(panel, "", UIStyle.Body, TextAnchor.UpperLeft, Color.white);
        body.rectTransform.anchorMin = new Vector2(0f, 1f);
        body.rectTransform.anchorMax = new Vector2(1f, 1f);
        body.rectTransform.pivot = new Vector2(0.5f, 1f);
        body.rectTransform.anchoredPosition = new Vector2(0f, -(UIStyle.Padding + TitleHeight));
        body.rectTransform.sizeDelta = new Vector2(-2f * UIStyle.Padding, 100f);
        body.lineSpacing = 1f;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        bool show = gm && gm.State == GameState.Playing && gm.Player;
        root.SetActive(show);
        if (!show) return;

        builder.Clear();
        var player = gm.Player.transform.position;
        var cycle = DayNightCycle.Instance;
        bool night = cycle && cycle.IsNight;

        var tutorial = Tutorial.Current;
        if (tutorial)
        {
            string color = tutorial.JustDone ? "#9DFF80" : "#FFD46B";
            Line($"<color={color}>{tutorial.Title}</color>");
            Line(tutorial.Text);
            Line("<color=#B8A68C>[Enter] pular tutorial</color>");
            Line("");
        }

        if (night && cycle) Line($"<color=#FF8A70>> Defenda a fogueira ({GameManager.FormatTime(cycle.TimeLeftInPeriod)})</color>");

        var camp = CampSystem.Instance;
        if (camp)
        {
            foreach (var person in camp.Persons)
            {
                if (!person) continue;
                var offset = person.transform.position - player;
                if (person.State == PersonState.Lost)
                    Line($"<color=#A6D9FF>> Resgatar {person.Name} ({offset.magnitude:0} m, {UIStyle.Heading(offset)})</color>");
                else if (person.State == PersonState.Returning)
                    Line($"<color=#B8A68C>> {person.Title} a caminho</color>");
            }
            foreach (var spot in camp.Spots)
                if (spot.IsStation && !spot.Locked && !spot.Current)
                    Line($"> Construir a {spot.StationName.ToLower()}");
        }

        var chests = ChestSystem.Instance;
        if (chests && chests.TryGetClosest(player, out string chestName, out var chestPosition))
        {
            var offset = chestPosition - player;
            bool hasKey = chests.Inventory && chests.Inventory.Keys > 0;
            string key = hasKey ? "" : " <color=#B8A68C>(ache uma chave)</color>";
            Line($"<color=#FFD46B>> Abrir o {chestName.Replace("Baú", "baú")} ({offset.magnitude:0} m, {UIStyle.Heading(offset)})</color>{key}");
        }

        if (!night) Line("<color=#B8A68C>> Colete madeira e pedra</color>");

        body.text = builder.ToString().TrimEnd('\n');
        float textHeight = body.preferredHeight;
        body.rectTransform.sizeDelta = new Vector2(-2f * UIStyle.Padding, textHeight);
        float height = textHeight + TitleHeight + UIStyle.Padding * 2f + UIStyle.Border * 2f;
        frame.sizeDelta = new Vector2(Width, height);
    }

    void Line(string text) => builder.Append(text).Append('\n');
}
