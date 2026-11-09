using UnityEngine;

// Anima o personagem com a spritesheet exportada do Aseprite:
// 24 animações de 8 frames, na ordem (idle, walk, run) x (down, up, left, right,
// down_left, down_right, up_left, up_right). Em movimento usa sempre run; os frames
// de walk continuam na spritesheet, mas não são tocados.
[RequireComponent(typeof(PlayerController))]
public class CharacterAnimator : MonoBehaviour
{
    public SpriteRenderer target;
    [Tooltip("Os 192 frames da spritesheet, na ordem.")]
    public Sprite[] frames;
    public int framesPerAnimation = 8;

    [Header("Duração de cada frame (segundos, igual ao Aseprite)")]
    public float idleFrameTime = 0.13f;
    public float runFrameTime = 0.07f;

    [Tooltip("Volume do passo, tocado quando o pé encosta no chão (quadros 1 e 5 da corrida).")]
    public float stepVolume = 0.3f;

    enum State { Idle = 0, Run = 2 }

    // Índice da direção na spritesheet para cada setor de 45° (começando na direita, sentido anti-horário).
    static readonly int[] SectorToRow = { 3, 7, 1, 6, 2, 4, 0, 5 };

    PlayerController controller;
    int direction; // começa olhando para baixo
    int currentAnimation = -1;
    int frame;
    float timer;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        if (!target && controller.visual) target = controller.visual.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!target || frames == null || frames.Length == 0) return;

        Vector2 move = controller.IsDashing ? controller.LastDirection : controller.MoveInput;
        State state = State.Idle;
        if (move.sqrMagnitude > 0.01f)
        {
            direction = DirectionIndex(move);
            state = State.Run;
        }

        int animation = (int)state * 8 + direction;
        if (animation != currentAnimation)
        {
            // Troca de direção mantém o passo; troca de estado recomeça.
            if (currentAnimation < 0 || currentAnimation / 8 != animation / 8) { frame = 0; timer = 0f; }
            currentAnimation = animation;
        }

        float frameTime = state == State.Idle ? idleFrameTime : runFrameTime;
        timer += Time.deltaTime;
        while (timer >= frameTime)
        {
            timer -= frameTime;
            frame = (frame + 1) % framesPerAnimation;
            if (state == State.Run && !controller.IsDashing && (frame == 1 || frame == 5))
                AudioManager.Play(Sfx.Step, stepVolume, 0.12f);
        }

        int index = animation * framesPerAnimation + frame;
        if (index < frames.Length) target.sprite = frames[index];
    }

    public static int DirectionIndex(Vector2 v)
    {
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        int sector = Mathf.RoundToInt(angle / 45f);
        sector = ((sector % 8) + 8) % 8;
        return SectorToRow[sector];
    }
}
