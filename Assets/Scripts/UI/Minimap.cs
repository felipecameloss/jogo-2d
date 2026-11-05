using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Radar de orientação centrado no jogador. O mapa é infinito, então o alcance é fixo: o que
// está além dele fica preso na borda, mais apagado, indicando a direção. Mostra só a fogueira
// e os baús fechados; as chaves continuam tendo de ser achadas.
public class Minimap : MonoBehaviour
{
    const float PanelSize = 220f;
    const float EdgeMargin = 10f;
    const float MarkerSize = 12f;
    const float WorldRange = 24f; // Unidades do centro até a borda; cobre os baús vistos da fogueira.
    const float FarAlpha = 0.55f;

    static readonly Color CampColor = new Color(1f, 0.42f, 0.12f);
    static readonly Color ChestColor = new Color(1f, 0.85f, 0.3f);

    readonly List<Image> chestMarkers = new List<Image>();
    readonly List<Vector3> chests = new List<Vector3>();
    GameObject root;
    RectTransform panel;
    Image camp, player;

    void Start()
    {
        root = new GameObject("MinimapUI", typeof(Canvas), typeof(CanvasScaler));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1; // Os menus continuam por cima.
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Abaixo do ouro e da direção do baú.
        var background = NewImage("Panel", root.transform, new Color(0.04f, 0.09f, 0.12f, 0.8f), PanelSize);
        panel = background.rectTransform;
        panel.anchorMin = panel.anchorMax = Vector2.one;
        panel.anchoredPosition = new Vector2(-130f, -325f);
        camp = NewImage("Camp", panel, CampColor, MarkerSize * 1.4f);
        player = NewImage("Player", panel, Color.white, MarkerSize);
    }

    void Update()
    {
        var game = GameManager.Instance;
        if (!root || !game || !game.Player) return;
        bool playing = game.State == GameState.Playing;
        root.SetActive(playing);
        if (!playing) return;

        Vector3 origin = game.Player.transform.position;
        if (CampSystem.Instance) Place(camp, CampSystem.Instance.Center - origin);

        chests.Clear();
        if (ChestSystem.Instance) ChestSystem.Instance.GetClosedChests(chests);
        for (int i = 0; i < chests.Count; i++)
        {
            if (i == chestMarkers.Count) chestMarkers.Add(NewChestMarker());
            chestMarkers[i].gameObject.SetActive(true);
            Place(chestMarkers[i], chests[i] - origin);
        }
        for (int i = chests.Count; i < chestMarkers.Count; i++) chestMarkers[i].gameObject.SetActive(false);
    }

    void Place(Image marker, Vector2 worldOffset)
    {
        float edge = PanelSize * 0.5f - EdgeMargin;
        Vector2 position = worldOffset * (edge / WorldRange);
        float distance = Mathf.Max(Mathf.Abs(position.x), Mathf.Abs(position.y));
        bool far = distance > edge;
        if (far) position *= edge / distance; // Mantém a direção ao prender na borda.
        marker.rectTransform.anchoredPosition = position;
        var color = marker.color;
        color.a = far ? FarAlpha : 1f;
        marker.color = color;
    }

    Image NewChestMarker()
    {
        var marker = NewImage("Chest", panel, ChestColor, MarkerSize);
        marker.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f); // Losango, para não depender só da cor.
        player.transform.SetAsLastSibling(); // O jogador fica sempre por cima.
        return marker;
    }

    static Image NewImage(string name, Transform parent, Color color, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        image.rectTransform.sizeDelta = Vector2.one * size;
        return image;
    }
}
