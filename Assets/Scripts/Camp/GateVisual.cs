using UnityEngine;

// Portão abre quando o jogador chega perto, para o lado oposto ao dele, e fecha quando ele se afasta.
// Só visual: a colisão não muda (o jogador sempre passa, os monstros nunca).
public class GateVisual : MonoBehaviour
{
    const int Columns = 6;
    const float FrameTime = 0.09f;
    const float OpenRange = 1.8f;
    // Linhas da folha: 0 abre para o sul, 1 para o oeste, 2 para o norte, 3 para o leste.
    const int OpenSouth = 0, OpenWest = 1, OpenNorth = 2, OpenEast = 3;

    SpriteRenderer sr;
    Sprite[] frames;
    bool horizontal;
    int row;
    float progress; // 0 fechado, Columns - 1 aberto

    public void Setup(SpriteRenderer renderer, bool isHorizontal)
    {
        sr = renderer;
        horizontal = isHorizontal;
        row = horizontal ? OpenSouth : OpenWest;
    }

    public void SetFrames(Sprite[] sheet)
    {
        frames = sheet;
        Show();
    }

    void Update()
    {
        if (!sr || frames == null) return;
        var player = CampSystem.Instance ? CampSystem.Instance.PlayerCollider : null;
        bool near = player && ((Vector2)(player.transform.position - transform.position)).sqrMagnitude < OpenRange * OpenRange;
        // O lado só muda com o portão fechado, para a folha não pular no meio da animação.
        if (near && progress <= 0f) row = AwayFrom(player.transform.position);
        progress = Mathf.MoveTowards(progress, near ? Columns - 1 : 0f, Time.deltaTime / FrameTime);
        Show();
    }

    int AwayFrom(Vector3 position)
    {
        Vector3 offset = position - transform.position;
        if (horizontal) return offset.y < 0f ? OpenNorth : OpenSouth;
        return offset.x > 0f ? OpenWest : OpenEast;
    }

    void Show()
    {
        if (sr && frames != null) sr.sprite = CampArt.Pick(frames, row * Columns + Mathf.RoundToInt(progress));
    }
}
