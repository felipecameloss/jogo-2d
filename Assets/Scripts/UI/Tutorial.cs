using System;
using UnityEngine;

// Tutorial do primeiro dia: objetivos curtos, um de cada vez, sem pausar o jogo. Aparece no
// card próprio abaixo das missões (QuestLog). O guia das opções reutiliza os mesmos textos.
// Termina ao anoitecer, ao cumprir tudo ou com Enter. Depois de
// concluído ou pulado, não aparece mais (dá para reativar nas opções).
public class Tutorial : MonoBehaviour
{
    const string SeenKey = "tutorial_visto";
    const float DoneDelay = 1.2f, NightMessageTime = 6f;

    public const int GuidePageCount = 5;

    const string CollectInstruction = "Segure [E] perto de uma árvore e de uma rocha para coletar madeira e pedra.";
    const string BuildInstruction = "Volte à fogueira e aperte [F] numa marca do chão para erguer uma cerca.";
    const string RescueInstruction = "Há pessoas perdidas pelo mapa. Siga a missão de resgate e aperte [F] perto delas.";
    const string ChestInstruction = "Ache uma chave pelo mapa e abra um baú com [F]: ele dá armas e equipamentos.";
    const string NightInstruction = "Os monstros vão atrás da fogueira. Defenda-a e repare o que eles quebrarem com [F].";

    public static string GuidePageTitle(int page)
    {
        switch (page)
        {
            case 0: return "1/5 - COLETA";
            case 1: return "2/5 - CONSTRUÇÃO";
            case 2: return "3/5 - RESGATE";
            case 3: return "4/5 - BAÚS";
            case 4: return "5/5 - DEFESA NOTURNA";
            default: return "";
        }
    }

    public static string GuidePageText(int page)
    {
        switch (page)
        {
            case 0: return CollectInstruction;
            case 1: return BuildInstruction;
            case 2: return RescueInstruction;
            case 3: return ChestInstruction;
            case 4: return NightInstruction;
            default: return "";
        }
    }

    public static bool Seen
    {
        get => PlayerPrefs.GetInt(SeenKey, 0) == 1;
        set { PlayerPrefs.SetInt(SeenKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    struct Step
    {
        public string Text;
        public Func<bool> Done;
        public Step(string text, Func<bool> done) { Text = text; Done = done; }
    }

    public static Tutorial Current { get; private set; }
    public string Title { get; private set; } = "";
    public string Text { get; private set; } = "";
    public bool JustDone => doneTimer >= 0f;

    Step[] steps;
    int index;
    float doneTimer = -1f, nightTimer = -1f;
    bool sawWood, sawStone;
    ResourceInventory bag;

    void Awake() => Current = this;

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    void Start()
    {
        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        bag = player ? player.GetComponent<ResourceInventory>() : null;
        steps = new[]
        {
            new Step(CollectInstruction, () => sawWood && sawStone),
            new Step(BuildInstruction, HasWall),
            new Step(RescueInstruction, RescuedSomeone),
            new Step(ChestInstruction, () => ChestSystem.Instance && ChestSystem.Instance.ClosedChests < 3),
        };
        Show();
    }

    static bool HasWall()
    {
        foreach (var structure in Structure.Active)
            if (structure.Kind != StructureKind.Heart && structure.Kind != StructureKind.Station) return true;
        return false;
    }

    static bool RescuedSomeone()
    {
        var camp = CampSystem.Instance;
        if (!camp) return false;
        foreach (var person in camp.Persons)
            if (person && person.State != PersonState.Lost) return true;
        return false;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (!gm || gm.State != GameState.Playing) return;

        if (GameInput.SkipTutorialPressed()) { Finish(); return; }
        // A bolsa pode ser gasta logo depois: guarda que o jogador já coletou cada material.
        if (bag) { sawWood |= bag.Wood > 0; sawStone |= bag.Stone > 0; }

        var cycle = DayNightCycle.Instance;
        if (cycle && cycle.IsNight)
        {
            if (nightTimer < 0f)
            {
                nightTimer = NightMessageTime;
                Title = "A NOITE CHEGOU";
                Text = NightInstruction;
            }
            nightTimer -= Time.deltaTime;
            if (nightTimer <= 0f) Finish();
            return;
        }

        if (doneTimer >= 0f)
        {
            doneTimer -= Time.deltaTime;
            if (doneTimer < 0f) Advance();
            return;
        }
        if (steps[index].Done())
        {
            doneTimer = DoneDelay;
            Title = "OBJETIVO CUMPRIDO!";
            AudioManager.Play(Sfx.LevelUp, 0.4f);
        }
    }

    void Advance()
    {
        index++;
        if (index >= steps.Length) { Finish(); return; }
        Show();
    }

    void Show()
    {
        Title = $"TUTORIAL {index + 1}/{steps.Length}";
        Text = steps[index].Text;
    }

    void Finish()
    {
        Seen = true;
        Destroy(this);
    }
}
