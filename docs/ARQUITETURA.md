# Arquitetura

Unity 6 (`6000.6.5f1`), C#, jogo 2D top-down com alvo **WebGL**. Cenas: `Assets/Scenes/MainMenu.unity` e `Assets/Scenes/Game.unity`.

## Mapa do código

Tudo em `Assets/Scripts/`:

| Pasta | Responsabilidade | Principais classes |
|---|---|---|
| `Core/` | Estado da run, ciclo, input, câmera, menu, cenário | `GameManager`, `DayNightCycle`, `GameInput`, `CameraFollow`, `DecorationScatter`, `InfiniteBackground`, `SpriteLoop`, `MainMenu` |
| `Player/` | Movimento, atributos, XP, ouro, animação | `PlayerController`, `PlayerStats`, `PlayerExperience`, `PlayerWallet`, `CharacterAnimator`, `WorldHealthBar` |
| `Enemies/` | Comportamento e geração | `Enemy`, `EnemySpawner` |
| `Weapons/` | Base `Weapon`, 10 armas e seus projéteis/zonas | `WeaponHolder`, `Weapon`, `*Weapon` |
| `Upgrades/` | Opções de level up | `UpgradeManager` |
| `Pickups/` | XP, cura, ímã, ouro | `Pickup` |
| `Resources/` | Coleta diurna e inventário | `DayResourceSystem`, `ResourceInventory`, `ResourceHUD` |
| `Chests/` | Baús, chaves, recompensas | `ChestSystem`, `ChestOffer`, `ChestUI`, `ChestArt` |
| `UI/` | HUD e level up | `HUD`, `LevelUpPanel` |
| `FX/` | Partículas, popups, shake | `FxManager`, `FxParticle`, `FxFade`, `FloatingText` |
| `Audio/` | SFX e música | `AudioManager`, `SfxGenerator` |
| `Editor/` | Gerador do protótipo | `SurvivorsSetup` |

## Fluxo e estados

`GameManager` é a fonte do estado: `Playing`, `Paused`, `LevelUp`, `Chest`, `GameOver`.

- Pausa, level up, baú e game over usam `Time.timeScale = 0`.
- Sistemas de jogo verificam `GameManager.Instance.State == GameState.Playing` antes de agir.
- Entradas e saídas de estado passam sempre por métodos do `GameManager` (`Pause`, `EnterLevelUp`, `EnterChest`, `EndRun` etc.).
- Reiniciar recarrega a cena, então o estado da run não persiste. O projeto não usa `PlayerPrefs`.

```
MainMenu → Game (Playing) ⇄ Paused / LevelUp / Chest
                 └→ GameOver → Reiniciar | Menu
```

## Sistemas

**Dia e noite.** `DayNightCycle.periodDuration` é 180 s por período. O evento `OnPeriodChanged` avisa os interessados e `NightElapsed` acumula só o tempo de noite, que é o relógio da dificuldade. Ao amanhecer, `Enemy.VanishAll()` remove os inimigos restantes.

**Inimigos.** `EnemySpawner` só gera à noite e avalia curvas por `NightElapsed`. Armas encontram alvos pela lista estática `Enemy.Active`, com `Enemy.FindNearest` e `Enemy.DamageInRadius`, sem `Find`.

**Armas e upgrades.** `WeaponHolder` guarda as armas. Cada arma herda de `Weapon` e usa os multiplicadores de `PlayerStats`. `PlayerExperience` acumula level ups pendentes e `UpgradeManager` os consome quando o estado volta a `Playing`, para nenhum se perder durante uma pausa.

**Mundo procedural.** `DayResourceSystem`, `ChestSystem` e `DecorationScatter` dividem o mapa em células e usam sementes determinísticas por posição. Recursos também usam o dia; chaves usam a semente da run. Assim, sair e voltar a uma região reproduz o mesmo conteúdo, e o que foi consumido fica em conjuntos (`depleted`, `pickedKeys`).

**Baús.** Três baús fixos por run, a distâncias crescentes do início. `IsReserved` impede que árvores e rochas nasçam sobre início, baús e chaves. A chave só é gasta quando a recompensa é escolhida.

**Inicialização.** `GameManager.Start` adiciona `DayResourceSystem` e `ChestSystem` se faltarem na cena. `ChestUI` e `ResourceHUD` são criados por código pelos respectivos sistemas.

## Convenções

- Singletons de cena com `Instance`, limpos em `OnDestroy`.
- Comunicação entre sistemas por eventos C# (`OnChanged`, `OnPeriodChanged`, `OnLevelUp`, `OnGoldChanged`).
- Efeitos e som por chamadas estáticas: `FxManager.Burst/Popup/Shake`, `AudioManager.Play`.
- Input somente por `GameInput`, que abstrai teclado e gamepad e funciona com o Input Manager antigo.
- Valores ajustáveis em campos públicos com `[Tooltip]`. Valores de balanceamento são registrados em `BALANCEAMENTO.md`.
- Comentários e textos de jogo em português.

## Gerador `SurvivorsSetup`

O menu **Survivors > Gerar sprites, prefabs e cenas** recria `Assets/Generated` e **sobrescreve as duas cenas**. Alterações manuais em cenas ou prefabs gerados são perdidas ao rodá-lo. Valores definidos no gerador, como atributos de inimigos, também ficam salvos nos prefabs: para mudá-los, altere os dois ou confirme qual é usado.

## Como estender

- **Inimigo:** duplique um `Generated/Prefabs/Enemy_*`, ajuste e adicione em `GameSystems > EnemySpawner > Entries`.
- **Arma:** herde de `Weapon` (a `AuraWeapon` é a mais simples), crie o prefab e adicione em `GameSystems > UpgradeManager > Available Weapons`.
- **Upgrade de atributo:** uma linha `AddStat(...)` em `UpgradeManager.RollOptions`.
- **Recompensa de baú:** novo valor em `ChestReward`, com caso em `ChestOffer.Create` e `Apply`. O sorteio em `ChestSystem.Awake` usa `% 7` fixo: atualize junto.
- **Som:** arraste o `AudioClip` no `AudioManager`; sem clip, ele usa o som gerado.
