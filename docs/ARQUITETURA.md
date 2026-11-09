# Arquitetura

Unity 6 (`6000.6.5f1`), C#, jogo 2D top-down com alvo **WebGL**. Cenas: `Assets/Scenes/MainMenu.unity` e `Assets/Scenes/Game.unity`.

## Mapa do código

Tudo em `Assets/Scripts/`:

| Pasta | Responsabilidade | Principais classes |
|---|---|---|
| `Core/` | Estado da run, ciclo, input, câmera, menu, cenário | `GameManager`, `DayNightCycle`, `GameInput`, `CameraFollow`, `DecorationScatter`, `InfiniteBackground`, `SpriteLoop`, `MainMenu`, `GameSettings` |
| `Player/` | Movimento, atributos, XP, ouro, animação | `PlayerController`, `PlayerStats`, `PlayerExperience`, `PlayerWallet`, `CharacterAnimator`, `WorldHealthBar` |
| `Enemies/` | Comportamento, geração, chefes e arte | `Enemy`, `EnemySpawner`, `Boss`, `EnemyBullet`, `EnemyArt` |
| `Weapons/` | Base `Weapon`, 10 armas e seus projéteis/zonas | `WeaponHolder`, `Weapon`, `*Weapon` |
| `Upgrades/` | Opções de level up e catálogo de armas | `UpgradeManager` |
| `Pickups/` | XP, cura, ímã, ouro | `Pickup` |
| `Resources/` | Coleta diurna e inventário | `DayResourceSystem`, `ResourceInventory`, `ResourceHUD` |
| `Chests/` | Baús, chaves, recompensas | `ChestSystem`, `ChestOffer`, `ChestUI`, `ChestArt` |
| `Camp/` | Acampamento: estruturas, pontos de construção, HUD e arte | `CampSystem`, `BuildSpot`, `Structure`, `GateVisual`, `CampHUD`, `CampArt` |
| `UI/` | HUD, minimapa, level up, opções e estilo | `HUD`, `Minimap`, `LevelUpPanel`, `OptionsMenu`, `UIStyle`, `Tutorial`, `QuestLog`, `PromptBox` |
| `FX/` | Partículas, popups, shake | `FxManager`, `FxParticle`, `FxFade`, `FloatingText` |
| `Audio/` | SFX e música | `AudioManager`, `SfxGenerator` |
| `Editor/` | Gerador do protótipo, build WebGL e atalhos de teste | `SurvivorsSetup`, `WebGLBuild`, `CampDebug` |

## Fluxo e estados

`GameManager` é a fonte do estado: `Playing`, `Paused`, `LevelUp`, `Menu`, `GameOver`. `Menu` é qualquer janela de jogo (baú, ferraria, armaria): `EnterMenu(onClose)` recebe quem fecha a janela quando o jogador aperta Esc / P / Start.

- Pausa, level up, janelas de baú e estação e game over usam `Time.timeScale = 0`.
- Sistemas de jogo verificam `GameManager.Instance.State == GameState.Playing` antes de agir.
- Entradas e saídas de estado passam sempre por métodos do `GameManager` (`Pause`, `EnterLevelUp`, `EnterMenu`, `EndRun` etc.).
- Reiniciar recarrega a cena, então o estado da run não persiste. O projeto não usa `PlayerPrefs`.

```
MainMenu → Game (Playing) ⇄ Paused / LevelUp / Menu
                 └→ GameOver → Reiniciar | Menu
```

## Sistemas

**Dia e noite.** `DayNightCycle.periodDuration` é 180 s por período. O evento `OnPeriodChanged` avisa os interessados e `NightElapsed` acumula só o tempo de noite, que é o relógio da dificuldade. Ao amanhecer, `Enemy.VanishAll()` remove os inimigos restantes. `HoldNight` prende o relógio à meia-noite (noite de chefe) e `FinishNightIn(segundos)` acelera o resto da noite até amanhecer.

**Inimigos.** `EnemySpawner` só gera à noite e avalia curvas por `NightElapsed`. Monstros não usam pathfinding: `Enemy` reavalia o alvo a cada 0,25 s, anda reto, ataca a `Structure` que o bloqueia e desvia de lado se ficar parado. `HuntsPlayer` marca quem nasceu para caçar o jogador longe da base. O contato não usa `OnCollisionStay2D`, que o Unity chamaria a cada passo para todo par de inimigos encostados: `OnCollisionEnter2D`/`Exit2D` registram o jogador e as construções tocados, e `FixedUpdate` aplica o dano. Monstros ficam na layer `Enemy`, que o dash ignora por `Collider2D.excludeLayers`. Armas encontram alvos pela lista estática `Enemy.Active`, com `Enemy.FindNearest` e `Enemy.DamageInRadius`, sem `Find`.

**Arte dos inimigos.** O campo `Enemy.art` nomeia uma folha em `Assets/Art/Resources/Enemies/` (tira de 8 quadros quadrados olhando para a direita, importada como sprite único). `EnemyArt` fatia a folha em tempo de execução, como `CampArt`, e guarda por folha o pivô e o tempo de quadro. O pivô fica no centro do corpo, como nas formas antigas, para colisor, mira, tiros e números de dano não mudarem de lugar. Com arte, `Enemy.ApplyArt` troca o sprite, tira a cor do prefab (que passa a colorir só as partículas da morte), põe o inimigo na camada 10 ordenada pelo pivô, junto do jogador, das árvores e dos muros, e desenha em escala 1 sem mudar o tamanho do colisor no mundo: o raio compensa a escala que o prefab tinha. O golpe avermelha em vez de clarear, e o lento também desacelera a animação. Sem a folha, o inimigo usa o sprite e a cor do prefab. Projéteis (`Projectile`, `Knife`, `OrbitBlade`, bumerangue) ficam na camada 15 para passar por cima dos monstros.

**Chefes.** Nas noites de chefe, `EnemySpawner.UpdateBossNight` liga `DayNightCycle.HoldNight`, suspende spawns, anéis e elites e cria o chefe: um `Enemy` comum transformado por `Enemy.InitBoss` (vida, colisor, arte, drops, `HuntsPlayer`). O colisor cresce como antes (×2,2 e ×3); a arte troca para `midBossArt` ou `bigBossArt`, desenhada em `bossArtScale` (1,5) e com o componente `Boss` adicionado. `Boss` roda os padrões de ataque em corrotinas; durante a investida, liga `Enemy.externalMovement` para controlar a velocidade sozinho. Os tiros são `EnemyBullet`, sem física: checam a distância até o jogador e atravessam construções. `Enemy.Died` só dispara em morte por dano (não ao sumir no amanhecer); `Boss.Spawned` e `Boss.Defeated` avisam o `HUD` (aviso e barra de vida) e o spawner, que chama `FinishNightIn`. No Editor, N mata o chefe na hora.

**Arte de armas e itens.** O gerador põe nos prefabs os sprites de `Assets/Art/Resources/` (pickups, virote, faca, machado, bomba) em escala 1; os raios de colisão dos projéteis guardam as áreas de acerto antigas. `Weapon.icon` é o ícone do baú e da ferraria; `ChestOffer.Icon` usa esse ícone ou `ChestOffer.GearIcon` (também usado pela armaria); `StationOffer.Icon` leva o da compra. `ChestUI` e `StationUI` mostram o ícone com `UIStyle.Icon`/`SetIcon`, e `ResourceHUD` põe os de `Hud/` ao lado de madeira e pedra. Explosão e golpe da espada são tiras (`BombWeapon.explosionFrames`, `WhipWeapon.slashFrames`) tocadas por `FxManager.Animate`, que espelha com escala negativa em x; sem quadros, voltam ao `Flash` antigo. O golpe é esticado para cobrir a área de acerto da espada. A chave vem de `Resources/Pickups/chave` a 20 px por unidade (o tamanho antigo), em cache e fora de `ownedSprites`.

**Desempenho dos efeitos.** `FxManager` reaproveita partículas, números de dano e flashes em pilhas em vez de criar e destruir objetos, com teto de `maxPopups` e `maxParticles`; cada efeito chama `FxManager.Release` ao terminar. `Pickup.TryMergeXP` soma o XP numa gema existente quando já há 250 no chão.

**Armas e upgrades.** `WeaponHolder` guarda as armas. Cada arma herda de `Weapon` e usa os multiplicadores de `PlayerStats`. `PlayerExperience` acumula level ups pendentes e `UpgradeManager` os consome quando o estado volta a `Playing`, para nenhum se perder durante uma pausa. O level up só oferece atributos. `UpgradeManager.availableWeapons` é o catálogo de armas do jogo, lido por `UpgradeManager.Catalog()` pela ferraria e pelos baús; `WeaponHolder.Unlock` adiciona a arma ou sobe um nível.

**Mundo procedural.** `DayResourceSystem`, `ChestSystem` e `DecorationScatter` dividem o mapa em células e usam sementes determinísticas por posição. Recursos também usam o dia; chaves usam a semente da run. Assim, sair e voltar a uma região reproduz o mesmo conteúdo, e o que foi consumido fica em conjuntos (`depleted`, `pickedKeys`).

**Baús.** Três baús fixos por run, a distâncias crescentes do início. `IsReserved` impede que árvores e rochas nasçam sobre início, baús e chaves. A chave só é gasta quando a recompensa é escolhida. Ao abrir, o jogo pausa e a animação roda em tempo não escalado; `ChestUI` só mostra a janela depois dela (`ShowOffers`), porque o véu da janela cobriria o baú. Desistir toca a animação ao contrário. A arte vem de `Resources/Chests/chest.png`, e não pode ir para `ownedSprites`, que destrói a textura.

**Acampamento.** `CampSystem` cria por código a fogueira, o anel quadrado de 16 pontos e 2 pontos de estação. O anel é uma grade de nós a cada `CampArt.Cell` (32 px); cada `BuildSpot` recebe suas peças (`SpotPiece`: posição e quadro da folha) e seus retângulos de colisão, e desenha uma peça por nó. `BuildSpot` conhece a próxima ação e seu custo; `Structure` guarda vida e destruição e escurece todas as peças com o dano. `GateVisual` anima o portão conforme o lado do jogador, sem mexer na colisão. Destruir a fogueira chama `GameManager.EndRun(motivo)`. Com ação disponível, o F é do acampamento e `ChestSystem` não abre baú. `IsReserved` mantém a área livre de recursos e decoração.

**Pessoas e estações.** `CampSystem` cria no primeiro `Update` as duas `LostPerson` (ferreiro com os quadros do jogador tingidos; ferreira com `Resources/Characters/character_women`), em pontos validados contra colisões e baús. F resgata (`Rescue`); a pessoa anda até `Home` sem colisor e vira `Resident`. Residentes checam monstros encostados em `Enemy.Active`; derrotados, `CampSystem.DefeatPerson` os devolve ao mapa. `StationSystem` sincroniza a cada frame o bloqueio dos pontos de estação, a arte da barraca (com ou sem NPC, `BuildSpot.ShowResident`) e a visibilidade do NPC, monta as ofertas (`StationOffer`) e abre `StationUI` via `GameManager.EnterMenu`. Compras ficam no `WeaponHolder` e em `PlayerStats`, por isso sobrevivem à estação.

**Inicialização.** `GameManager.Awake` chama `CampSystem.Ensure`, e `ChestSystem.Awake` também, pois baús são medidos a partir do centro do acampamento. `GameManager.Start` adiciona `DayResourceSystem` e `ChestSystem` se faltarem na cena. `GameManager.Start` também adiciona `Minimap`. `ChestUI` e `ResourceHUD` são criados por código pelos respectivos sistemas.

## Convenções

- Singletons de cena com `Instance`, limpos em `OnDestroy`.
- Comunicação entre sistemas por eventos C# (`OnChanged`, `OnPeriodChanged`, `OnLevelUp`, `OnGoldChanged`, `Boss.Spawned`, `Boss.Defeated`, `Enemy.Died`).
- Efeitos e som por chamadas estáticas: `FxManager.Burst/Popup/Shake/Flash/Animate`, `AudioManager.Play`.
- Input somente por `GameInput`, que abstrai teclado e gamepad e funciona com o Input Manager antigo.
- Valores ajustáveis em campos públicos com `[Tooltip]`. Valores de balanceamento são registrados em `BALANCEAMENTO.md`.
- Comentários e textos de jogo em português.

## Gerador `SurvivorsSetup`

Durante o Play, **Survivors > Testes** adiciona materiais e causa dano na fogueira. O menu **Survivors > Gerar sprites, prefabs e cenas** recria `Assets/Generated` e **sobrescreve as duas cenas**. Alterações manuais em cenas ou prefabs gerados são perdidas ao rodá-lo. Valores definidos no gerador, como atributos de inimigos, também ficam salvos nos prefabs: para mudá-los, altere os dois ou confirme qual é usado.

## Como estender

- **Inimigo:** duplique um `Generated/Prefabs/Enemy_*`, ajuste e adicione em `GameSystems > EnemySpawner > Entries`. Para a arte, ponha a folha em `Assets/Art/Resources/Enemies/` (importada como as outras: sprite único, filtro *Point*, sem compressão), preencha `art` no prefab e registre pivô e tempo de quadro em `EnemyArt`.
- **Arma:** herde de `Weapon` (a `AuraWeapon` é a mais simples), crie o prefab e adicione em `GameSystems > UpgradeManager > Available Weapons`.
- **Upgrade de atributo:** uma linha `AddStat(...)` em `UpgradeManager.RollOptions`.
- **Recompensa de baú:** novo valor em `ChestReward`, com caso em `ChestOffer.Create` e `Apply`. O sorteio em `ChestSystem.Awake` usa `% 7` fixo: atualize junto.
- **Arte do acampamento:** fica em `Assets/Art/Resources/Camp/`, sem compressão e importada como sprite único; `CampArt` usa placeholder se faltar. A fogueira é uma tira de quadros quadrados (`CampArt.Frames`). Cerca, muro, portões e estações são folhas em grade fatiadas por `CampArt.Sheet`: `fence` e `wall` têm 13 quadros de 48 × 64 na ordem descrita em `CampArt`; `gate_wood` e `gate_stone`, 4 linhas de 6 quadros, uma por sentido de abertura; `station_*_npc`, 8 quadros de 64 × 68. O pivô das peças é o pé do poste, que fica sobre o nó da grade.
- **Som:** arraste o `AudioClip` no `AudioManager`; sem clip, ele usa o som gerado.

**Opções e estilo.** `GameSettings` guarda os volumes em `PlayerPrefs`, salvos ao fechar as opções; `AudioManager` escuta `GameSettings.Changed`. `OptionsMenu.Open` cria a tela por cima do menu ou da pausa; enquanto `OptionsMenu.BlocksInput`, o `GameManager` ignora Esc para não despausar junto. O botão Opções é clonado em tempo de execução (`UIStyle.CloneButton`), sem editar as cenas.

**Interface.** `UIStyle` concentra a identidade: fonte Pixel Operator, regular e negrito (`Resources/Fonts`, importadas em *Hinted Raster*); a fonte é desenhada numa grade de 16 px, então o texto usa 32 (`Body`) e 64 (`Title`, sempre em negrito), nítidos em 1920×1080 e em 960×540. `UIStyle.Bold` aplica o negrito; `ColumnWidth` é a largura da coluna esquerda para cair no grid da fonte, paleta, `Panel` (moldura + miolo opaco), `Button`, `Label` e `Canvas` (1920×1080 de referência, *pixel perfect*). `UIStyle.ApplyToScene` reestiliza textos, botões e canvases das cenas geradas. `HUD.Layout` reorganiza em painéis os textos da cena. `QuestLog` monta as missões a cada frame a partir de `Tutorial.Current`, `CampSystem.Persons`, estações livres e `ChestSystem.TryGetClosest`. `PromptBox` é a caixa de aviso de interação que some sem texto.

**Tutorial.** `GameManager.Start` adiciona `Tutorial` se `Tutorial.Seen` for falso. Cada passo é um texto e uma condição verificada a cada frame (materiais coletados, estrutura construída, pessoa resgatada, baú aberto). Ao anoitecer, mostra o aviso da noite e termina; Enter (`GameInput.SkipTutorialPressed`) pula. `Seen` fica em `PlayerPrefs`.
