using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Menu "Survivors > Gerar sprites, prefabs e cenas".
// Monta o protótipo inteiro com arte placeholder gerada por código.
// Depois de gerado, vocês podem editar tudo normalmente no Editor
// (trocar sprites, mexer em valores dos prefabs etc). Rodar de novo SOBRESCREVE Assets/Generated e as cenas.
public static class SurvivorsSetup
{
    const string GenRoot = "Assets/Generated";
    const string SpriteDir = GenRoot + "/Sprites";
    // Spritesheets exportadas do Aseprite (arte de verdade, não é gerada aqui).
    const string ArtDir = "Assets/Art/Sprites";
    const int ArtPixelsPerUnit = 24;
    const string PrefabDir = GenRoot + "/Prefabs";
    const string SceneDir = "Assets/Scenes";
    const string MenuScenePath = SceneDir + "/MainMenu.unity";
    const string GameScenePath = SceneDir + "/Game.unity";

    static Font font;
    static Sprite sSquare;

    [MenuItem("Survivors/Gerar sprites, prefabs e cenas")]
    public static void GenerateFromMenu()
    {
        if (!EditorUtility.DisplayDialog("Survivors",
                "Isso vai (re)criar Assets/Generated e as cenas MainMenu e Game, sobrescrevendo mudanças feitas nelas. Continuar?",
                "Gerar", "Cancelar")) return;
        GenerateAll();
        EditorSceneManager.OpenScene(GameScenePath);
    }

    // Também pode ser chamado em linha de comando: -executeMethod SurvivorsSetup.GenerateAll
    public static void GenerateAll()
    {
        EnsureFolder(GenRoot);
        EnsureFolder(SpriteDir);
        EnsureFolder(PrefabDir);
        EnsureFolder(SceneDir);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ---------- Sprites ----------
        var sCircle = MakeSprite("Circle", 64, 64, Shapes.Circle);
        var sBlob = MakeSprite("Blob", 64, 64, Shapes.Blob);
        sSquare = MakeSprite("Square", 64, 64, Shapes.Square);
        var sDiamond = MakeSprite("Diamond", 64, 64, Shapes.Diamond);
        var sCross = MakeSprite("Cross", 64, 64, Shapes.Cross);
        var sRing = MakeSprite("Ring", 64, 64, Shapes.Ring);
        var sGlow = MakeSprite("Glow", 128, 128, Shapes.Glow);
        var sPlain = MakeSprite("Pixel", 8, 8, (u, v, px) => Color.white);
        var sGround = MakeGroundSprite();

        // ---------- Arte (Aseprite) ----------
        // Pivô nos pés / base do tronco, para ordenar pela altura (quem está mais embaixo fica na frente).
        var playerFrames = ImportSheet($"{ArtDir}/player.png", 32, 32, new Vector2(0.5f, 0.25f));
        ImportSheet("Assets/Art/Resources/Characters/character_women.png", 32, 32, new Vector2(0.5f, 0.25f)); // NPC: carregada em jogo por Resources
        var bushAFrames = ImportSheet($"{ArtDir}/bush_a.png", 32, 32, new Vector2(0.5f, 0.15f));
        var bushBFrames = ImportSheet($"{ArtDir}/bush_b.png", 32, 32, new Vector2(0.5f, 0.15f));
        var treeFrames = ImportSheet($"{ArtDir}/tree.png", 64, 80, new Vector2(0.5f, 0.12f));
        // Pedras coletáveis: cada uma sorteia uma dessas variações.
        var rockSprites = new[]
        {
            ImportSheet($"{ArtDir}/rock_1_small.png", 16, 14, new Vector2(0.5f, 0.14f)),
            ImportSheet($"{ArtDir}/rock_2_round.png", 24, 20, new Vector2(0.5f, 0.1f)),
            ImportSheet($"{ArtDir}/rock_3_flat.png", 32, 20, new Vector2(0.58f, 0.15f)),
            ImportSheet($"{ArtDir}/rock_4_mossy.png", 32, 28, new Vector2(0.48f, 0.11f)),
            ImportSheet($"{ArtDir}/rock_5_cluster.png", 40, 30, new Vector2(0.48f, 0.17f)),
        }.Where(f => f.Length > 0).Select(f => f[0]).ToArray();
        // A árvore não é decoração: é o recurso de madeira coletável durante o dia.
        var resourceTree = MakeDecoration("Resource_Tree", treeFrames);
        var decorations = new[]
        {
            new DecorationEntry { prefab = MakeDecoration("Deco_BushA", bushAFrames), weight = 1.5f },
            new DecorationEntry { prefab = MakeDecoration("Deco_BushB", bushBFrames), weight = 1.5f },
        };

        // ---------- Pickups ----------
        var gem = MakePickup("XPGem", sDiamond, PickupType.Experience, new Color(0.35f, 0.7f, 1f), 0.32f, 1f);
        var heal = MakePickup("HealOrb", sCross, PickupType.Heal, new Color(1f, 0.35f, 0.4f), 0.45f, 30f);
        var magnet = MakePickup("Magnet", sRing, PickupType.Magnet, new Color(0.6f, 0.8f, 1f), 0.5f, 0f);
        var gold = MakePickup("Gold", sCircle, PickupType.Gold, new Color(1f, 0.8f, 0.2f), 0.3f, 1f);

        // ---------- Inimigos ----------
        var slime = MakeEnemy("Slime", sBlob, new Color(0.45f, 0.9f, 0.4f), 0.8f, hp: 10, speed: 1.8f, damage: 6, xp: 1, gem, heal, magnet, gold, 0.2f, 1, 2);
        var bat = MakeEnemy("Bat", sDiamond, new Color(0.7f, 0.45f, 1f), 0.6f, hp: 6, speed: 3.4f, damage: 4, xp: 1, gem, heal, magnet, gold, 0.15f, 1, 2);
        var golem = MakeEnemy("Golem", sSquare, new Color(0.85f, 0.5f, 0.35f), 1.3f, hp: 60, speed: 1.2f, damage: 12, xp: 5, gem, heal, magnet, gold, 0.6f, 3, 6);
        golem.knockbackResistance = 0.7f;
        PrefabUtility.SavePrefabAsset(golem.gameObject);

        // ---------- Armas ----------
        var projectile = MakeProjectile("Projectile", sCircle, new Color(1f, 0.95f, 0.5f), new Vector3(0.4f, 0.28f, 1f));
        var knife = MakeProjectile("Knife", sDiamond, new Color(0.85f, 0.9f, 1f), new Vector3(0.6f, 0.18f, 1f));
        var blade = MakeBlade(sDiamond);

        var boltGo = new GameObject("W_MagicBolt");
        var bolt = boltGo.AddComponent<MagicBoltWeapon>();
        bolt.weaponName = "Besta";
        bolt.description = "Dispara virotes no inimigo mais próximo.";
        bolt.damage = 10f;
        bolt.cooldown = 0.9f;
        bolt.projectilePrefab = projectile;
        bolt = SavePrefab<MagicBoltWeapon>(boltGo, "W_MagicBolt");

        var orbitGo = new GameObject("W_Orbit");
        var orbit = orbitGo.AddComponent<OrbitWeapon>();
        orbit.weaponName = "Lâminas Orbitais";
        orbit.description = "Lâminas giram ao seu redor.";
        orbit.damage = 8f;
        orbit.bladePrefab = blade;
        orbit = SavePrefab<OrbitWeapon>(orbitGo, "W_Orbit");

        var auraGo = new GameObject("W_Aura");
        var aura = auraGo.AddComponent<AuraWeapon>();
        aura.weaponName = "Aura Sagrada";
        aura.description = "Causa dano contínuo em quem chega perto.";
        aura.damage = 5f;
        aura.cooldown = 0.6f;
        var auraVisual = NewSprite("Visual", auraGo.transform, sGlow, new Color(1f, 0.85f, 0.4f, 0.35f), -1);
        aura.visual = auraVisual;
        aura = SavePrefab<AuraWeapon>(auraGo, "W_Aura");

        var knifeGo = new GameObject("W_Knife");
        var knifeW = knifeGo.AddComponent<KnifeWeapon>();
        knifeW.weaponName = "Facas";
        knifeW.description = "Arremessa facas na direção em que você anda.";
        knifeW.damage = 7f;
        knifeW.cooldown = 0.6f;
        knifeW.projectilePrefab = knife;
        knifeW = SavePrefab<KnifeWeapon>(knifeGo, "W_Knife");

        var lightningGo = new GameObject("W_Lightning");
        var lightning = lightningGo.AddComponent<LightningWeapon>();
        lightning.weaponName = "Raio Celeste";
        lightning.description = "Raios caem em inimigos aleatórios.";
        lightning.damage = 14f;
        lightning.cooldown = 1.8f;
        lightning.impactSprite = sGlow;
        lightning = SavePrefab<LightningWeapon>(lightningGo, "W_Lightning");

        var boomerangGo = new GameObject("W_Boomerang");
        var boomerang = boomerangGo.AddComponent<BoomerangWeapon>();
        boomerang.weaponName = "Machado de arremesso";
        boomerang.description = "Vai até o inimigo e volta, acertando duas vezes.";
        boomerang.damage = 9f;
        boomerang.cooldown = 1.6f;
        boomerang.sprite = sCross;
        boomerang = SavePrefab<BoomerangWeapon>(boomerangGo, "W_Boomerang");

        var flaskGo = new GameObject("W_Flask");
        var flask = flaskGo.AddComponent<FlaskWeapon>();
        flask.weaponName = "Frasco Venenoso";
        flask.description = "Cria poças de veneno embaixo dos inimigos.";
        flask.damage = 4f;
        flask.cooldown = 2.5f;
        flask.zoneSprite = sGlow;
        flask = SavePrefab<FlaskWeapon>(flaskGo, "W_Flask");

        var whipGo = new GameObject("W_Whip");
        var whip = whipGo.AddComponent<WhipWeapon>();
        whip.weaponName = "Espada";
        whip.description = "Golpe de espada forte na sua frente.";
        whip.damage = 13f;
        whip.cooldown = 1.3f;
        whip.slashSprite = sDiamond;
        whip = SavePrefab<WhipWeapon>(whipGo, "W_Whip");

        var novaGo = new GameObject("W_FrostNova");
        var nova = novaGo.AddComponent<FrostNovaWeapon>();
        nova.weaponName = "Nova Gélida";
        nova.description = "Explosão de gelo que empurra e deixa os inimigos lentos.";
        nova.damage = 8f;
        nova.cooldown = 4f;
        nova.ringSprite = sRing;
        nova = SavePrefab<FrostNovaWeapon>(novaGo, "W_FrostNova");

        var bombGo = new GameObject("W_Bomb");
        var bomb = bombGo.AddComponent<BombWeapon>();
        bomb.weaponName = "Bombas";
        bomb.description = "Larga bombas que explodem depois de um tempo.";
        bomb.damage = 30f;
        bomb.cooldown = 3f;
        bomb.bombSprite = sCircle;
        bomb.explosionSprite = sGlow;
        bomb = SavePrefab<BombWeapon>(bombGo, "W_Bomb");

        // ---------- Cenas ----------
        BuildGameScene(sSquare, sPlain, sGround, playerFrames, decorations, resourceTree, rockSprites, new[] { slime, bat, golem }, bolt, new Weapon[] { bolt, whip, knifeW, boomerang, bomb });
        BuildMenuScene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("[Survivors] Projeto gerado! Abra Assets/Scenes/MainMenu ou Game e aperte Play.");
    }

    // ===================================================================== Cena do jogo

    static void BuildGameScene(Sprite sSquare, Sprite sPlain, Sprite sGround, Sprite[] playerFrames, DecorationEntry[] decorations, GameObject resourceTree, Sprite[] rockSprites, Enemy[] enemies, Weapon startWeapon, Weapon[] allWeapons)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Câmera
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.14f, 0.1f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        // Ordenação top-down: no mesmo sortingOrder, quem está mais para baixo na tela é desenhado na frente.
        // (Fica no Graphics Settings do projeto; o campo da câmera não é salvo na cena.)
        UnityEngine.Rendering.GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis;
        UnityEngine.Rendering.GraphicsSettings.transparencySortAxis = Vector3.up;
        camGo.AddComponent<AudioListener>();
        var follow = camGo.AddComponent<CameraFollow>();

        // Chão
        var groundGo = new GameObject("Ground");
        var groundSr = groundGo.AddComponent<SpriteRenderer>();
        groundSr.sprite = sGround;
        groundSr.drawMode = SpriteDrawMode.Tiled;
        groundSr.size = new Vector2(80f, 80f);
        groundSr.sortingOrder = -100;
        var bg = groundGo.AddComponent<InfiniteBackground>();
        bg.follow = camGo.transform;
        bg.tileSize = 2f;

        // Árvores e arbustos
        var decoGo = new GameObject("Decorations");
        var scatter = decoGo.AddComponent<DecorationScatter>();
        scatter.follow = camGo.transform;
        scatter.entries = decorations;

        // Jogador
        var player = new GameObject("Player");
        player.tag = "Player";
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.mass = 5f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.AddComponent<CircleCollider2D>().radius = 0.35f;

        var visual = NewSprite("Visual", player.transform, playerFrames.Length > 0 ? playerFrames[0] : null, Color.white, 10);
        visual.spriteSortPoint = SpriteSortPoint.Pivot;

        var stats = player.AddComponent<PlayerStats>();
        stats.visual = visual;
        var controller = player.AddComponent<PlayerController>();
        controller.visual = visual.transform;
        var anim = player.AddComponent<CharacterAnimator>();
        anim.target = visual;
        anim.frames = playerFrames;
        player.AddComponent<PlayerExperience>();
        player.AddComponent<PlayerWallet>();
        var holder = player.AddComponent<WeaponHolder>();
        holder.startingWeapon = startWeapon;

        var barRoot = new GameObject("HealthBar");
        barRoot.transform.SetParent(player.transform, false);
        barRoot.transform.localPosition = new Vector3(0f, -0.42f, 0f);
        var barBg = NewSprite("Background", barRoot.transform, sPlain, new Color(0.1f, 0.1f, 0.1f, 0.8f), 11);
        barBg.transform.localScale = new Vector3(0.95f, 0.13f, 1f);
        var barFill = NewSprite("Fill", barRoot.transform, sPlain, new Color(0.9f, 0.25f, 0.3f), 12);
        barFill.transform.localScale = new Vector3(0.9f, 0.09f, 1f);
        var bar = barRoot.AddComponent<WorldHealthBar>();
        bar.stats = stats;
        bar.fill = barFill.transform;

        follow.target = player.transform;

        // UI
        var canvas = MakeCanvas("Canvas");
        MakeEventSystem();
        var hud = BuildHud(canvas.transform);
        var levelUp = BuildLevelUpPanel(canvas.transform);

        // Sistemas
        var systems = new GameObject("GameSystems");
        var gm = systems.AddComponent<GameManager>();
        var upgrades = systems.AddComponent<UpgradeManager>();
        upgrades.panel = levelUp;
        upgrades.availableWeapons = allWeapons;
        systems.AddComponent<FxManager>().particleSprite = sSquare;
        systems.AddComponent<AudioManager>();
        systems.AddComponent<DayNightCycle>().ground = groundSr;
        var resources = systems.AddComponent<DayResourceSystem>();
        resources.treePrefab = resourceTree;
        resources.rockSprites = rockSprites;
        systems.AddComponent<ChestSystem>();
        var spawner = systems.AddComponent<EnemySpawner>();
        spawner.entries = new[]
        {
            new SpawnEntry { prefab = enemies[0], startTime = 0f, weight = 3f },
            new SpawnEntry { prefab = enemies[1], startTime = 45f, weight = 2f },
            new SpawnEntry { prefab = enemies[2], startTime = 150f, weight = 1f },
        };

        BuildPausePanel(canvas.transform, gm);
        BuildGameOverPanel(canvas.transform, gm);
        hud.transform.SetAsFirstSibling();

        EditorSceneManager.SaveScene(scene, GameScenePath);
    }

    static HUD BuildHud(Transform canvas)
    {
        var root = NewUI("HUD", canvas);
        Stretch(root);
        var hud = root.gameObject.AddComponent<HUD>();

        // Barra de XP no topo
        var xpBg = NewUI("XPBar", root);
        xpBg.anchorMin = new Vector2(0f, 1f);
        xpBg.anchorMax = new Vector2(1f, 1f);
        xpBg.pivot = new Vector2(0.5f, 1f);
        xpBg.sizeDelta = new Vector2(-40f, 28f);
        xpBg.anchoredPosition = new Vector2(0f, -12f);
        var bgImg = xpBg.gameObject.AddComponent<Image>();
        bgImg.sprite = sSquare;
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0f, 0f, 0f, 0.6f);

        var fill = NewUI("Fill", xpBg);
        Stretch(fill, 4f);
        var fillImg = fill.gameObject.AddComponent<Image>();
        fillImg.sprite = sSquare;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0f;
        fillImg.color = new Color(0.35f, 0.7f, 1f);
        hud.xpFill = fillImg;

        hud.levelText = MakeText(xpBg, "Level", "Nv 1", 24, TextAnchor.MiddleRight, Color.white);
        Stretch(hud.levelText.rectTransform, 0f);
        hud.levelText.rectTransform.offsetMax = new Vector2(-12f, 0f);

        hud.timerText = MakeText(root, "Timer", "00:00", 52, TextAnchor.UpperCenter, Color.white);
        Anchor(hud.timerText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(300f, 70f));

        hud.periodText = MakeText(root, "Period", "DIA 1", 28, TextAnchor.UpperCenter, new Color(1f, 0.88f, 0.4f));
        Anchor(hud.periodText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(700f, 40f));

        hud.bannerText = MakeText(root, "Banner", "DIA 1", 96, TextAnchor.MiddleCenter, Color.white);
        hud.bannerText.supportRichText = true;
        Anchor(hud.bannerText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 180f), new Vector2(1400f, 220f));

        hud.killsText = MakeText(root, "Kills", "Abates: 0", 30, TextAnchor.UpperRight, Color.white);
        Anchor(hud.killsText.rectTransform, new Vector2(1f, 1f), new Vector2(-170f, -55f), new Vector2(300f, 50f));

        hud.dashText = MakeText(root, "Dash", "DASH x1", 30, TextAnchor.LowerCenter, Color.white);
        Anchor(hud.dashText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(400f, 50f));

        hud.goldText = MakeText(root, "Gold", "Ouro: 0", 30, TextAnchor.UpperRight, new Color(1f, 0.85f, 0.25f));
        Anchor(hud.goldText.rectTransform, new Vector2(1f, 1f), new Vector2(-170f, -100f), new Vector2(300f, 50f));

        var hint = MakeText(root, "Hint", "WASD/Setas: mover    Shift: dash    ESC: pausar", 22, TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.6f));
        Anchor(hint.rectTransform, new Vector2(0f, 0f), new Vector2(330f, 30f), new Vector2(640f, 40f));

        return hud;
    }

    static LevelUpPanel BuildLevelUpPanel(Transform canvas)
    {
        var root = MakeOverlay("LevelUpPanel", canvas, 0.75f);
        var panel = root.gameObject.AddComponent<LevelUpPanel>();

        panel.headerText = MakeText(root, "Header", "NÍVEL 2!", 72, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.3f));
        Anchor(panel.headerText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(1000f, 120f));
        var sub = MakeText(root, "Sub", "Escolha um upgrade (clique ou teclas 1, 2, 3)", 28, TextAnchor.MiddleCenter, Color.white);
        Anchor(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(1000f, 50f));

        var row = NewUI("Options", root);
        Anchor(row, new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(1500f, 320f));
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 40f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = true;

        panel.optionButtons = new Button[3];
        panel.optionTitles = new Text[3];
        panel.optionDescriptions = new Text[3];
        for (int i = 0; i < 3; i++)
        {
            var btn = MakeButton(row, "Option" + (i + 1), "", new Color(0.18f, 0.2f, 0.3f), out var label);
            Object.DestroyImmediate(label.gameObject);

            var title = MakeText(btn.transform, "Title", "Título", 34, TextAnchor.UpperCenter, new Color(1f, 0.9f, 0.4f));
            title.rectTransform.anchorMin = new Vector2(0f, 0.55f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.offsetMin = new Vector2(20f, 0f);
            title.rectTransform.offsetMax = new Vector2(-20f, -30f);
            title.horizontalOverflow = HorizontalWrapMode.Wrap;

            var desc = MakeText(btn.transform, "Description", "Descrição", 28, TextAnchor.UpperCenter, Color.white);
            desc.rectTransform.anchorMin = new Vector2(0f, 0f);
            desc.rectTransform.anchorMax = new Vector2(1f, 0.55f);
            desc.rectTransform.offsetMin = new Vector2(20f, 20f);
            desc.rectTransform.offsetMax = new Vector2(-20f, 0f);
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;

            panel.optionButtons[i] = btn;
            panel.optionTitles[i] = title;
            panel.optionDescriptions[i] = desc;
        }

        root.gameObject.SetActive(false);
        return panel;
    }

    static void BuildPausePanel(Transform canvas, GameManager gm)
    {
        var root = MakeOverlay("PausePanel", canvas, 0.7f);
        var title = MakeText(root, "Title", "PAUSADO", 80, TextAnchor.MiddleCenter, Color.white);
        Anchor(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(800f, 120f));

        var col = MakeButtonColumn(root, new Vector2(0f, -40f));
        UnityEventTools.AddPersistentListener(MakeButton(col, "Continuar", "Continuar", new Color(0.2f, 0.45f, 0.3f), out _).onClick, gm.Resume);
        UnityEventTools.AddPersistentListener(MakeButton(col, "Reiniciar", "Reiniciar", new Color(0.25f, 0.27f, 0.35f), out _).onClick, gm.Restart);
        UnityEventTools.AddPersistentListener(MakeButton(col, "Menu", "Menu Principal", new Color(0.25f, 0.27f, 0.35f), out _).onClick, gm.GoToMenu);

        gm.pausePanel = root.gameObject;
        root.gameObject.SetActive(false);
    }

    static void BuildGameOverPanel(Transform canvas, GameManager gm)
    {
        var root = MakeOverlay("GameOverPanel", canvas, 0.8f);
        gm.gameOverTitle = MakeText(root, "Title", "VOCÊ CAIU...", 80, TextAnchor.MiddleCenter, new Color(1f, 0.4f, 0.4f));
        Anchor(gm.gameOverTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 280f), new Vector2(1200f, 120f));
        gm.gameOverStats = MakeText(root, "Stats", "", 36, TextAnchor.MiddleCenter, Color.white);
        Anchor(gm.gameOverStats.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(800f, 180f));

        var col = MakeButtonColumn(root, new Vector2(0f, -150f));
        UnityEventTools.AddPersistentListener(MakeButton(col, "JogarNovamente", "Jogar Novamente", new Color(0.2f, 0.45f, 0.3f), out _).onClick, gm.Restart);
        UnityEventTools.AddPersistentListener(MakeButton(col, "Menu", "Menu Principal", new Color(0.25f, 0.27f, 0.35f), out _).onClick, gm.GoToMenu);

        gm.gameOverPanel = root.gameObject;
        root.gameObject.SetActive(false);
    }

    // ===================================================================== Menu principal

    static void BuildMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.09f, 0.14f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        var canvas = MakeCanvas("Canvas");
        MakeEventSystem();
        new GameObject("Audio").AddComponent<AudioManager>();

        var menu = canvas.gameObject.AddComponent<MainMenu>();

        var title = MakeText(canvas.transform, "Title", GameInfo.Title.ToUpperInvariant(), 110, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.35f));
        Anchor(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(1400f, 160f));
        var subtitle = MakeText(canvas.transform, "Subtitle", GameInfo.Tagline, 30, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.6f));
        Anchor(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 210f), new Vector2(800f, 50f));

        var col = MakeButtonColumn(canvas.transform as RectTransform, new Vector2(0f, -10f));
        UnityEventTools.AddPersistentListener(MakeButton(col, "Jogar", "Jogar", new Color(0.2f, 0.45f, 0.3f), out _).onClick, menu.Play);
        var quit = MakeButton(col, "Sair", "Sair", new Color(0.35f, 0.2f, 0.22f), out _);
        UnityEventTools.AddPersistentListener(quit.onClick, menu.Quit);
        menu.quitButton = quit.gameObject;

        var howTo = MakeText(canvas.transform, "HowTo",
            "WASD / Setas: mover   •   Shift: dash   •   As armas atacam sozinhas   •   ESC: pausar\nColete gemas para subir de nível e sobreviva ao máximo de noites que conseguir!",
            26, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.75f));
        Anchor(howTo.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(1600f, 100f));

        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }

    // ===================================================================== Prefabs

    static Pickup MakePickup(string name, Sprite sprite, PickupType type, Color color, float scale, float value)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = -5;
        go.transform.localScale = Vector3.one * scale;
        var p = go.AddComponent<Pickup>();
        p.type = type;
        p.value = value;
        return SavePrefab<Pickup>(go, name);
    }

    static Enemy MakeEnemy(string name, Sprite sprite, Color color, float scale, float hp, float speed, float damage, float xp,
        Pickup gem, Pickup heal, Pickup magnet, Pickup gold, float goldChance, int goldMin, int goldMax)
    {
        var go = new GameObject(name);
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        go.AddComponent<CircleCollider2D>().radius = 0.42f;
        var e = go.AddComponent<Enemy>();
        e.maxHealth = hp;
        e.moveSpeed = speed;
        e.contactDamage = damage;
        e.xpValue = xp;
        e.xpGemPrefab = gem;
        e.healPrefab = heal;
        e.magnetPrefab = magnet;
        e.goldPrefab = gold;
        e.goldDropChance = goldChance;
        e.goldMin = goldMin;
        e.goldMax = goldMax;
        // Folha animada em Assets/Art/Resources/Enemies; com ela, o Enemy troca o sprite, tira a cor
        // e desenha em escala 1, mantendo o tamanho do colisor (ver EnemyArt).
        e.art = name.ToLowerInvariant();
        return SavePrefab<Enemy>(go, "Enemy_" + name);
    }

    // Planta animada (árvore/arbusto). Sem colisão para não prender os inimigos.
    static GameObject MakeDecoration(string name, Sprite[] frames)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = frames.Length > 0 ? frames[0] : null;
        sr.sortingOrder = 10; // mesma camada do jogador, ordenado pela altura
        sr.spriteSortPoint = SpriteSortPoint.Pivot;
        var loop = go.AddComponent<SpriteLoop>();
        loop.frames = frames;
        loop.frameTime = 0.12f;
        var asset = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/{name}.prefab");
        Object.DestroyImmediate(go);
        return asset;
    }

    static Projectile MakeProjectile(string name, Sprite sprite, Color color, Vector3 scale)
    {
        var go = new GameObject(name);
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = 15; // acima dos inimigos, que ficam na camada 10
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
        go.AddComponent<Projectile>();
        return SavePrefab<Projectile>(go, name);
    }

    static OrbitBlade MakeBlade(Sprite sprite)
    {
        var go = new GameObject("OrbitBlade");
        var visual = NewSprite("Visual", go.transform, sprite, new Color(0.85f, 0.95f, 1f), 15);
        visual.transform.localScale = new Vector3(0.7f, 0.3f, 1f);
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.32f;
        go.AddComponent<OrbitBlade>();
        return SavePrefab<OrbitBlade>(go, "OrbitBlade");
    }

    static T SavePrefab<T>(GameObject go, string name) where T : Component
    {
        var asset = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/{name}.prefab");
        Object.DestroyImmediate(go);
        return asset.GetComponent<T>();
    }

    static SpriteRenderer NewSprite(string name, Transform parent, Sprite sprite, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    // ===================================================================== UI helpers

    static Canvas MakeCanvas(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void MakeEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        // Usa o módulo do Input System novo se ele estiver instalado.
        var newModule = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (newModule != null) go.AddComponent(newModule);
        else go.AddComponent<StandaloneInputModule>();
    }

    static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform rt, float padding = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);
    }

    static void Anchor(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    static Text MakeText(Transform parent, string name, string content, int size, TextAnchor align, Color color)
    {
        var rt = NewUI(name, parent);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = content;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var shadow = rt.gameObject.AddComponent<Shadow>();
        shadow.effectDistance = new Vector2(2f, -2f);
        shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        return t;
    }

    static RectTransform MakeOverlay(string name, Transform canvas, float alpha)
    {
        var root = NewUI(name, canvas);
        Stretch(root);
        root.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, alpha);
        return root;
    }

    static RectTransform MakeButtonColumn(RectTransform parent, Vector2 position)
    {
        var col = NewUI("Buttons", parent);
        Anchor(col, new Vector2(0.5f, 0.5f), position, new Vector2(460f, 300f));
        var layout = col.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return col;
    }

    static Button MakeButton(Transform parent, string name, string label, Color color, out Text labelText)
    {
        var rt = NewUI(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sSquare;
        img.type = Image.Type.Sliced;
        img.color = color;
        var btn = rt.gameObject.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        colors.selectedColor = new Color(1.15f, 1.15f, 1.15f);
        colors.colorMultiplier = 1.5f;
        btn.colors = colors;
        rt.gameObject.AddComponent<LayoutElement>().preferredHeight = 80f;

        labelText = MakeText(rt, "Label", label, 36, TextAnchor.MiddleCenter, Color.white);
        Stretch(labelText.rectTransform);
        return btn;
    }

    // ===================================================================== Sprites placeholder

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    static Sprite MakeSprite(string name, int size, int ppu, Func<float, float, float, Color> shape, bool tiled = false, Color[] pixels = null)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        if (pixels != null) tex.SetPixels(pixels);
        else
        {
            float px = 2f / size;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, shape((x + 0.5f) * px - 1f, (y + 0.5f) * px - 1f, px));
        }
        tex.Apply();

        string path = $"{SpriteDir}/{name}.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = ppu;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.filterMode = tiled ? FilterMode.Point : FilterMode.Bilinear;
        imp.wrapMode = tiled ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        var settings = new TextureImporterSettings();
        imp.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        if (name == "Square") settings.spriteBorder = new Vector4(16, 16, 16, 16); // permite Image "Sliced" na UI
        imp.SetTextureSettings(settings);
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // Fatia uma spritesheet em grade (frames lidos da esquerda p/ direita, de cima p/ baixo)
    // e devolve os sprites em ordem. Os nomes são "<arquivo>_000", "<arquivo>_001"...
    static Sprite[] ImportSheet(string path, int frameW, int frameH, Vector2 pivot)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (!imp)
        {
            Debug.LogError($"[Survivors] Não achei {path}");
            return new Sprite[0];
        }
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = ArtPixelsPerUnit;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.filterMode = FilterMode.Point; // pixel art
        imp.wrapMode = TextureWrapMode.Clamp;
        var settings = new TextureImporterSettings();
        imp.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        imp.SetTextureSettings(settings);
        imp.SaveAndReimport();

        imp.GetSourceTextureWidthAndHeight(out int texW, out int texH);
        int cols = texW / frameW, rows = texH / frameH;
        string baseName = Path.GetFileNameWithoutExtension(path);

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dp = factory.GetSpriteEditorDataProviderFromObject(imp);
        dp.InitSpriteEditorDataProvider();
        // Reaproveita os IDs já existentes para não quebrar referências ao gerar de novo.
        var oldIds = dp.GetSpriteRects().GroupBy(r => r.name).ToDictionary(g => g.Key, g => g.First().spriteID);

        var rects = new SpriteRect[cols * rows];
        for (int i = 0; i < rects.Length; i++)
        {
            int col = i % cols, row = i / cols;
            string name = $"{baseName}_{i:000}";
            rects[i] = new SpriteRect
            {
                name = name,
                rect = new Rect(col * frameW, texH - (row + 1) * frameH, frameW, frameH),
                alignment = SpriteAlignment.Custom,
                pivot = pivot,
                spriteID = oldIds.TryGetValue(name, out var id) ? id : GUID.Generate(),
            };
        }
        dp.SetSpriteRects(rects);
        var names = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
        names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
        dp.Apply();
        imp.SaveAndReimport();

        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderBy(s => s.name, StringComparer.Ordinal).ToArray();
    }

    static Sprite MakeGroundSprite()
    {
        const int size = 32;
        var rng = new System.Random(42);
        var pixels = new Color[size * size];
        var baseColor = new Color(0.2f, 0.29f, 0.18f);
        for (int i = 0; i < pixels.Length; i++)
        {
            float n = (float)rng.NextDouble();
            var c = baseColor * (0.94f + 0.12f * (float)rng.NextDouble());
            if (n < 0.06f) c = new Color(0.15f, 0.23f, 0.13f);
            else if (n > 0.975f) c = new Color(0.28f, 0.38f, 0.22f);
            c.a = 1f;
            pixels[i] = c;
        }
        return MakeSprite("Ground", size, 16, null, true, pixels);
    }

    // Formas desenhadas em branco com contorno escuro, para poderem ser coloridas pelo SpriteRenderer.color.
    static class Shapes
    {
        static readonly Color OutlineColor = new Color(0.08f, 0.08f, 0.1f);

        // m: "distância" normalizada (<= 1 dentro da forma).
        static Color Fill(float m, float px, float outline = 0.14f)
        {
            float alpha = Mathf.Clamp01((1f - m) / (px * 1.5f));
            if (alpha <= 0f) return Color.clear;
            Color c = m > 1f - outline ? OutlineColor : Color.Lerp(Color.white, new Color(0.75f, 0.75f, 0.78f), m);
            c.a = alpha;
            return c;
        }

        public static Color Circle(float u, float v, float px) => Fill(Mathf.Sqrt(u * u + v * v) / 0.95f, px);

        public static Color Blob(float u, float v, float px)
        {
            float m = Mathf.Sqrt(u * u + v * v * 1.15f) / 0.95f;
            Color c = Fill(m, px);
            float eyeL = new Vector2(u + 0.3f, v - 0.12f).magnitude;
            float eyeR = new Vector2(u - 0.3f, v - 0.12f).magnitude;
            if (m < 0.86f && (eyeL < 0.14f || eyeR < 0.14f)) c = OutlineColor;
            return c;
        }

        public static Color Square(float u, float v, float px)
        {
            float m = Mathf.Pow(Mathf.Pow(Mathf.Abs(u), 6f) + Mathf.Pow(Mathf.Abs(v), 6f), 1f / 6f) / 0.95f;
            return Fill(m, px, 0.1f);
        }

        public static Color Diamond(float u, float v, float px) => Fill((Mathf.Abs(u) + Mathf.Abs(v)) / 0.95f, px, 0.16f);

        public static Color Cross(float u, float v, float px)
        {
            float au = Mathf.Abs(u), av = Mathf.Abs(v);
            float m = Mathf.Min(Mathf.Max(au / 0.35f, av / 0.9f), Mathf.Max(av / 0.35f, au / 0.9f));
            return Fill(m, px, 0.25f);
        }

        public static Color Ring(float u, float v, float px)
        {
            float d = Mathf.Sqrt(u * u + v * v);
            return Fill(Mathf.Abs(d - 0.7f) / 0.25f, px, 0.3f);
        }

        public static Color Glow(float u, float v, float px)
        {
            float d = Mathf.Sqrt(u * u + v * v);
            if (d >= 1f) return Color.clear;
            float a = Mathf.Pow(1f - d, 1.5f) * 0.6f + Mathf.Clamp01(1f - Mathf.Abs(d - 0.95f) / 0.05f) * 0.8f;
            return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
        }
    }
}
