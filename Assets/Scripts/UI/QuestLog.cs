using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Missões no canto esquerdo, abaixo dos materiais.
// Possui card compacto, painel completo rolável [M] e popup contextual de tutorial.
public class QuestLog : MonoBehaviour
{
    struct VisualEntry
    {
        public string summary;
        public string detailedLine;
        public bool hasPosition;
        public Vector3 position;
        public bool eligibleCompact;
    }

    GameObject canvasObject;
    GameObject compactRoot;
    Text summaryText;
    Text distanceText;

    GameObject fullPanelRoot;
    ScrollRect fullScrollRect;
    RectTransform fullViewport;
    RectTransform fullContent;
    Text fullBodyText;
    bool isPanelOpen;

    GameObject tutRoot;
    RectTransform tutFrame;
    Text tutTitleText;
    Text tutBodyText;
    Text tutEnterText;
    RectTransform tutViewport;
    RectTransform tutContent;

    readonly List<VisualEntry> entries = new List<VisualEntry>();
    readonly StringBuilder builder = new StringBuilder();

    void Start()
    {
        // sortingOrder 50 para permitir rolagem com o mouse e ficar abaixo de OptionsMenu (300).
        var canvas = UIStyle.Canvas(transform, "QuestLog", 50, true);
        canvasObject = canvas.gameObject;

        BuildCompactCard(canvas);
        BuildFullPanel(canvas);
        BuildTutorialPopup(canvas);
    }

    void BuildCompactCard(Transform canvas)
    {
        // Título e atalho ficam no próprio card; objetivo e distância ocupam a segunda linha.
        var inner = UIStyle.Panel(canvas, "MissaoCompacta", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -108f), new Vector2(520f, 120f));
        compactRoot = inner.parent.gameObject;

        var title = UIStyle.Bold(UIStyle.Label(inner, "MISSÕES", UIStyle.Body, TextAnchor.MiddleLeft, UIStyle.Gold));
        var titleRect = title.rectTransform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(UIStyle.Padding, -8f);
        titleRect.sizeDelta = new Vector2(210f, 38f);

        var openHint = UIStyle.Label(inner, "[M] ver todas", UIStyle.Body, TextAnchor.MiddleRight, UIStyle.Info);
        var hintRect = openHint.rectTransform;
        hintRect.anchorMin = hintRect.anchorMax = new Vector2(1f, 1f);
        hintRect.pivot = new Vector2(1f, 1f);
        hintRect.anchoredPosition = new Vector2(-UIStyle.Padding, -8f);
        hintRect.sizeDelta = new Vector2(250f, 38f);
        openHint.resizeTextForBestFit = true;
        openHint.resizeTextMinSize = 24;
        openHint.resizeTextMaxSize = 32;

        var divider = UIStyle.Image(inner, "Divisor", UIStyle.BorderColor);
        var dividerRect = divider.rectTransform;
        dividerRect.anchorMin = new Vector2(0f, 1f);
        dividerRect.anchorMax = new Vector2(1f, 1f);
        dividerRect.offsetMin = new Vector2(UIStyle.Padding, -50f);
        dividerRect.offsetMax = new Vector2(-UIStyle.Padding, -48f);

        summaryText = UIStyle.Label(inner, "", UIStyle.Body, TextAnchor.MiddleLeft, Color.white);
        var sRect = summaryText.rectTransform;
        sRect.anchorMin = new Vector2(0f, 0f);
        sRect.anchorMax = new Vector2(1f, 0f);
        sRect.offsetMin = new Vector2(UIStyle.Padding, 10f);
        sRect.offsetMax = new Vector2(-(110f + UIStyle.Padding), 58f);
        summaryText.resizeTextForBestFit = true;
        summaryText.resizeTextMinSize = 24;
        summaryText.resizeTextMaxSize = 32;

        distanceText = UIStyle.Label(inner, "", UIStyle.Body, TextAnchor.MiddleRight, UIStyle.Gold);
        var dRect = distanceText.rectTransform;
        dRect.anchorMin = dRect.anchorMax = new Vector2(1f, 0f);
        dRect.pivot = new Vector2(1f, 0.5f);
        dRect.offsetMin = new Vector2(-(110f + UIStyle.Padding), 10f);
        dRect.offsetMax = new Vector2(-UIStyle.Padding, 58f);
        distanceText.resizeTextForBestFit = true;
        distanceText.resizeTextMinSize = 24;
        distanceText.resizeTextMaxSize = 32;
    }

    void BuildFullPanel(Transform canvas)
    {
        // 1080×620 no centro da tela.
        var inner = UIStyle.Panel(canvas, "PainelMissoes", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1080f, 620f));
        fullPanelRoot = inner.parent.gameObject;
        fullPanelRoot.SetActive(false);

        const float pad = 24f;
        const float headerHeight = 44f;

        // Título e dica fora do viewport.
        var title = UIStyle.Bold(UIStyle.Label(inner, "TODAS AS MISSÕES", UIStyle.Body, TextAnchor.MiddleLeft, UIStyle.Gold));
        var tRect = title.rectTransform;
        tRect.anchorMin = new Vector2(0f, 1f);
        tRect.anchorMax = new Vector2(1f, 1f);
        tRect.pivot = new Vector2(0f, 1f);
        tRect.offsetMin = new Vector2(pad, -(pad + headerHeight));
        tRect.offsetMax = new Vector2(-150f, -pad);

        var hint = UIStyle.Label(inner, "[M] fechar", UIStyle.Body, TextAnchor.MiddleRight, UIStyle.Muted);
        var hRect = hint.rectTransform;
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(1f, 1f);
        hRect.offsetMin = new Vector2(pad, -(pad + headerHeight));
        hRect.offsetMax = new Vector2(-pad, -pad);

        // Viewport com RectMask2D e ScrollRect vertical.
        var vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        vpGo.transform.SetParent(inner, false);
        fullViewport = vpGo.GetComponent<RectTransform>();
        fullViewport.anchorMin = Vector2.zero;
        fullViewport.anchorMax = Vector2.one;
        fullViewport.offsetMin = new Vector2(pad, pad);
        fullViewport.offsetMax = new Vector2(-pad, -(pad + headerHeight + 10f));

        var vpImg = vpGo.GetComponent<Image>();
        vpImg.color = Color.clear;
        vpImg.raycastTarget = true;

        fullScrollRect = fullPanelRoot.AddComponent<ScrollRect>();
        fullScrollRect.vertical = true;
        fullScrollRect.horizontal = false;
        fullScrollRect.scrollSensitivity = 32f;
        fullScrollRect.movementType = ScrollRect.MovementType.Clamped;
        fullScrollRect.viewport = fullViewport;

        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(fullViewport, false);
        fullContent = contentGo.GetComponent<RectTransform>();
        fullContent.anchorMin = new Vector2(0f, 1f);
        fullContent.anchorMax = new Vector2(1f, 1f);
        fullContent.pivot = new Vector2(0.5f, 1f);
        fullContent.anchoredPosition = Vector2.zero;
        fullContent.sizeDelta = new Vector2(0f, 400f);
        fullScrollRect.content = fullContent;

        fullBodyText = UIStyle.Label(fullContent, "", UIStyle.Body, TextAnchor.UpperLeft, Color.white);
        fullBodyText.lineSpacing = 1.15f;
        var bRect = fullBodyText.rectTransform;
        bRect.anchorMin = new Vector2(0f, 1f);
        bRect.anchorMax = new Vector2(1f, 1f);
        bRect.pivot = new Vector2(0.5f, 1f);
        bRect.anchoredPosition = Vector2.zero;
        bRect.sizeDelta = new Vector2(0f, 400f);
    }

    void BuildTutorialPopup(Transform canvas)
    {
        // 520 de largura em (20, -244) no topo esquerdo, altura calculada de 160 a 300.
        var inner = UIStyle.Panel(canvas, "TutorialPopup", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -244f), new Vector2(520f, 160f));
        tutFrame = (RectTransform)inner.parent;
        tutRoot = tutFrame.gameObject;
        tutRoot.SetActive(false);

        tutTitleText = UIStyle.Bold(UIStyle.Label(inner, "", UIStyle.Body, TextAnchor.MiddleLeft, UIStyle.Gold));
        var tRect = tutTitleText.rectTransform;
        tRect.anchorMin = new Vector2(0f, 1f);
        tRect.anchorMax = new Vector2(1f, 1f);
        tRect.pivot = new Vector2(0f, 1f);
        tRect.offsetMin = new Vector2(UIStyle.Padding, -(UIStyle.Padding + 32f));
        tRect.offsetMax = new Vector2(-UIStyle.Padding, -UIStyle.Padding);

        tutEnterText = UIStyle.Label(inner, "[Enter] pular tutorial", UIStyle.Body, TextAnchor.MiddleLeft, UIStyle.Muted);
        var eRect = tutEnterText.rectTransform;
        eRect.anchorMin = new Vector2(0f, 0f);
        eRect.anchorMax = new Vector2(1f, 0f);
        eRect.pivot = new Vector2(0f, 0f);
        eRect.offsetMin = new Vector2(UIStyle.Padding, UIStyle.Padding);
        eRect.offsetMax = new Vector2(-UIStyle.Padding, UIStyle.Padding + 32f);

        var vpGo = new GameObject("TutViewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        vpGo.transform.SetParent(inner, false);
        tutViewport = vpGo.GetComponent<RectTransform>();
        tutViewport.anchorMin = Vector2.zero;
        tutViewport.anchorMax = Vector2.one;
        tutViewport.offsetMin = new Vector2(UIStyle.Padding, UIStyle.Padding + 36f);
        tutViewport.offsetMax = new Vector2(-UIStyle.Padding, -(UIStyle.Padding + 36f));

        var vpImg = vpGo.GetComponent<Image>();
        vpImg.color = Color.clear;
        vpImg.raycastTarget = true;

        var tutScroll = tutRoot.AddComponent<ScrollRect>();
        tutScroll.vertical = true;
        tutScroll.horizontal = false;
        tutScroll.scrollSensitivity = 32f;
        tutScroll.movementType = ScrollRect.MovementType.Clamped;
        tutScroll.viewport = tutViewport;

        var contentGo = new GameObject("TutContent", typeof(RectTransform));
        contentGo.transform.SetParent(tutViewport, false);
        tutContent = contentGo.GetComponent<RectTransform>();
        tutContent.anchorMin = new Vector2(0f, 1f);
        tutContent.anchorMax = new Vector2(1f, 1f);
        tutContent.pivot = new Vector2(0.5f, 1f);
        tutContent.anchoredPosition = Vector2.zero;
        tutScroll.content = tutContent;

        tutBodyText = UIStyle.Label(tutContent, "", UIStyle.Body, TextAnchor.UpperLeft, Color.white);
        tutBodyText.lineSpacing = 1.05f;
        var bRect = tutBodyText.rectTransform;
        bRect.anchorMin = new Vector2(0f, 1f);
        bRect.anchorMax = new Vector2(1f, 1f);
        bRect.pivot = new Vector2(0.5f, 1f);
        bRect.anchoredPosition = Vector2.zero;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        bool active = gm && gm.State == GameState.Playing && gm.Player;
        if (!active)
        {
            if (canvasObject.activeSelf) canvasObject.SetActive(false);
            if (isPanelOpen)
            {
                isPanelOpen = false;
                fullPanelRoot.SetActive(false);
            }
            return;
        }

        if (!canvasObject.activeSelf) canvasObject.SetActive(true);

        var cycle = DayNightCycle.Instance;
        bool night = cycle && cycle.IsNight;

        if (night)
        {
            // Na noite, o card compacto e o painel somem.
            if (isPanelOpen)
            {
                isPanelOpen = false;
                fullPanelRoot.SetActive(false);
            }
            if (compactRoot.activeSelf) compactRoot.SetActive(false);
        }
        else
        {
            // Durante o dia, M alterna o painel completo.
            if (GameInput.MissionsPressed())
            {
                isPanelOpen = !isPanelOpen;
                fullPanelRoot.SetActive(isPanelOpen);
                if (isPanelOpen)
                {
                    fullScrollRect.verticalNormalizedPosition = 1f;
                    fullContent.anchoredPosition = Vector2.zero;
                }
            }

            UpdateDaytimeMissions(gm);
        }

        UpdateTutorialPopup();
    }

    void UpdateDaytimeMissions(GameManager gm)
    {
        entries.Clear();
        var player = gm.Player.transform.position;

        var camp = CampSystem.Instance;
        if (camp)
        {
            foreach (var person in camp.Persons)
            {
                if (!person) continue;
                var offset = person.transform.position - player;
                if (person.State == PersonState.Lost)
                {
                    entries.Add(new VisualEntry
                    {
                        summary = $"Resgatar {person.Name}",
                        detailedLine = $"<color=#A6D9FF>> Resgatar {person.Name} ({offset.magnitude:0} m, {UIStyle.Heading(offset)})</color>",
                        hasPosition = true,
                        position = person.transform.position,
                        eligibleCompact = true
                    });
                }
                else if (person.State == PersonState.Returning)
                {
                    entries.Add(new VisualEntry
                    {
                        summary = $"{person.Title} a caminho",
                        detailedLine = $"<color=#B8A68C>> {person.Title} a caminho</color>",
                        hasPosition = false,
                        position = Vector3.zero,
                        eligibleCompact = false
                    });
                }
            }

            foreach (var spot in camp.Spots)
            {
                if (spot.IsStation && !spot.Locked && !spot.Current)
                {
                    var offset = spot.transform.position - player;
                    entries.Add(new VisualEntry
                    {
                        summary = $"Construir {spot.StationName}",
                        detailedLine = $"> Construir a {spot.StationName.ToLower()}",
                        hasPosition = true,
                        position = spot.transform.position,
                        eligibleCompact = true
                    });
                }
            }
        }

        var chests = ChestSystem.Instance;
        if (chests && chests.TryGetClosest(player, out string chestName, out var chestPosition))
        {
            var offset = chestPosition - player;
            bool hasKey = chests.Inventory && chests.Inventory.Keys > 0;
            string key = hasKey ? "" : " <color=#B8A68C>(ache uma chave)</color>";
            entries.Add(new VisualEntry
            {
                summary = $"Abrir {chestName}",
                detailedLine = $"<color=#FFD46B>> Abrir o {chestName.Replace("Baú", "baú")} ({offset.magnitude:0} m, {UIStyle.Heading(offset)})</color>{key}",
                hasPosition = true,
                position = chestPosition,
                eligibleCompact = true
            });
        }

        // Coleta diurna (sem posição: "Explorar")
        entries.Add(new VisualEntry
        {
            summary = "Coletar madeira e pedra",
            detailedLine = "<color=#B8A68C>> Colete madeira e pedra</color>",
            hasPosition = false,
            position = Vector3.zero,
            eligibleCompact = true
        });

        // O primeiro elegível torna-se o objetivo ativo
        VisualEntry? activeEntry = null;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].eligibleCompact)
            {
                activeEntry = entries[i];
                break;
            }
        }

        if (activeEntry.HasValue)
        {
            if (!compactRoot.activeSelf) compactRoot.SetActive(true);
            summaryText.text = activeEntry.Value.summary;
            if (activeEntry.Value.hasPosition)
            {
                float dist = (activeEntry.Value.position - player).magnitude;
                distanceText.text = $"{dist:0} m";
            }
            else
            {
                distanceText.text = "Explorar";
            }
        }
        else
        {
            if (compactRoot.activeSelf) compactRoot.SetActive(false);
        }

        if (isPanelOpen)
        {
            builder.Clear();
            for (int i = 0; i < entries.Count; i++)
            {
                if (i > 0) builder.Append('\n');
                builder.Append(entries[i].detailedLine);
            }
            fullBodyText.text = builder.ToString();
            float textH = fullBodyText.preferredHeight;
            float viewH = fullViewport.rect.height;
            float targetH = Mathf.Max(viewH, textH);
            fullBodyText.rectTransform.sizeDelta = new Vector2(0f, targetH);
            fullContent.sizeDelta = new Vector2(0f, targetH);
        }
    }

    void UpdateTutorialPopup()
    {
        // O painel completo cobre esta coluna; restaurar a dica ao fechá-lo.
        if (isPanelOpen)
        {
            if (tutRoot.activeSelf) tutRoot.SetActive(false);
            return;
        }

        var tutorial = Tutorial.Current;
        if (!tutorial)
        {
            if (tutRoot.activeSelf) tutRoot.SetActive(false);
            return;
        }

        if (!tutRoot.activeSelf) tutRoot.SetActive(true);

        tutTitleText.text = tutorial.Title;
        tutTitleText.color = tutorial.JustDone ? new Color(0.62f, 1f, 0.5f) : UIStyle.Gold;
        tutBodyText.text = tutorial.Text;

        // Calcular altura pelo preferredHeight + padding, mínima 160 e máxima 300.
        // Se exceder, usar viewport rolável em vez de cortar.
        float titleH = tutTitleText.preferredHeight;
        float bodyH = tutBodyText.preferredHeight;
        float enterH = tutEnterText.preferredHeight;

        const float pad = UIStyle.Padding;
        float desiredHeight = titleH + bodyH + enterH + pad * 2f + UIStyle.Border * 2f + 16f;
        float panelHeight = Mathf.Clamp(desiredHeight, 160f, 300f);
        tutFrame.sizeDelta = new Vector2(520f, panelHeight);

        float viewH = tutViewport.rect.height;
        float contentH = Mathf.Max(viewH, bodyH);
        tutBodyText.rectTransform.sizeDelta = new Vector2(0f, contentH);
        tutContent.sizeDelta = new Vector2(0f, contentH);
    }
}
