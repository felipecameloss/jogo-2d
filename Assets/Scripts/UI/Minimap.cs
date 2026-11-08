using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Radar de orientação centrado no jogador, sozinho no canto superior direito. O mapa é infinito,
// então o alcance é fixo: o que está além dele fica preso na borda, menor, indicando a direção.
// Mostra só a fogueira e os baús fechados; as chaves continuam tendo de ser achadas.
public class Minimap : MonoBehaviour
{
    const float PanelSize = 240f;
    const float EdgeMargin = 10f;
    const float MarkerSize = 12f;
    const float WorldRange = 24f; // Unidades do centro até a borda; cobre os baús vistos da fogueira.
    const float FarScale = 0.65f;

    static readonly Color CampColor = new Color(1f, 0.42f, 0.12f);
    static readonly Color ChestColor = new Color(1f, 0.85f, 0.3f);

    readonly List<Image> chestMarkers = new List<Image>();
    readonly List<Vector3> chests = new List<Vector3>();
    GameObject root;
    RectTransform panel;
    Image camp, player;

    void Start()
    {
        var canvas = UIStyle.Canvas(transform, "MinimapUI", -1); // Os menus continuam por cima.
        root = canvas.gameObject;
        var inner = UIStyle.Panel(canvas, "Minimapa", Vector2.one, Vector2.one, new Vector2(-20f, -20f), Vector2.one * (PanelSize + UIStyle.Border * 2f));
        inner.GetComponent<Image>().color = new Color(0.16f, 0.21f, 0.12f); // chão
        panel = inner;
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
        marker.rectTransform.localScale = Vector3.one * (far ? FarScale : 1f);
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
