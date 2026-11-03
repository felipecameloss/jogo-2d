# Survivors 2D (nome provisório)

Roguelike top-down 2D no estilo "survivors" (inspirado em Vampire Survivors e MegaBonk), feito em Unity 6 (6000.6.5f1) para o Projeto 2D de Jogos Digitais — Insper 2026.1.

## Como jogar

- **WASD / Setas**: mover (gamepad também funciona se o Input System estiver instalado)
- **Left Shift**: dash (fica invencível e atravessa inimigos durante o dash; recarrega sozinho)
- As armas atacam sozinhas
- Colete as gemas que os inimigos derrubam para subir de nível e escolher upgrades (clique ou teclas **1, 2, 3**)
- **ESC / P**: pausar
- Dia e noite: cada período dura 3 minutos. De dia não aparecem inimigos; de noite eles vêm (e os que sobrarem somem quando amanhece). O relógio e o período aparecem no topo da tela.
- **N** (só no Editor): pula para o próximo período, para testar sem esperar
- Objetivo: não tem fim. Sobreviva ao máximo de noites que conseguir

## Estrutura do projeto

```
Assets/
  Scenes/            MainMenu e Game
  Scripts/
    Core/            GameManager (estados, tempo, pausa, game over), câmera, input, menu
    Player/          movimento, atributos, XP, barra de vida
    Enemies/         Enemy (IA de perseguição, dano, drops) e EnemySpawner (ondas, anel, elite)
    Weapons/         Weapon (base) + 10 armas: Orbe Arcano, Lâminas Orbitais, Aura Sagrada, Facas,
                     Raio Celeste, Bumerangue, Frasco Venenoso, Chicote, Nova Gélida, Bombas
    Upgrades/        UpgradeManager (opções de level up)
    Pickups/         gemas de XP, cura, ímã
    UI/              HUD e tela de level up
    FX/              partículas, números de dano, screen shake
    Audio/           AudioManager + sons placeholder gerados por código
    Editor/          SurvivorsSetup (gera sprites, prefabs e cenas)
  Generated/         sprites e prefabs placeholder (gerados pelo menu Survivors)
```

### Regenerar o protótipo

Menu **Survivors > Gerar sprites, prefabs e cenas**. Atenção: isso sobrescreve `Assets/Generated` e as duas cenas.
Quando vocês começarem a editar as cenas/prefabs à mão, parem de usar esse menu (ou apaguem o script).

### Como adicionar coisas

- **Novo inimigo**: duplique um prefab em `Generated/Prefabs/Enemy_*`, ajuste os valores e adicione em `GameSystems > EnemySpawner > Entries` com o tempo em que ele começa a aparecer.
- **Nova arma**: crie uma classe que herda de `Weapon` (veja `AuraWeapon.cs`, a mais simples), faça um prefab com ela e adicione em `GameSystems > UpgradeManager > Available Weapons`.
- **Novo upgrade de atributo**: uma linha `AddStat(...)` em `UpgradeManager.RollOptions`.
- **Sons**: arraste AudioClips nos campos do `AudioManager` (em `GameSystems`) para substituir os sons gerados.

## Build WebGL (entrega no itch.io)

File > Build Profiles > Web > Switch Platform > Build. Suba o .zip da pasta gerada no itch.io marcando "This file will be played in the browser".

## Créditos

- Código base do protótipo gerado com auxílio do Claude (Anthropic) e adaptado pela equipe.
- Fonte: LegacyRuntime (fonte padrão do Unity).
- Arte e sons atuais: placeholders gerados por código.
- (adicione aqui os assets usados, com autor e licença)
