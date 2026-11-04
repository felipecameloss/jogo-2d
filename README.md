# Survivors 2D (nome provisório)

Roguelike top-down 2D no estilo "survivors" (inspirado em Vampire Survivors e MegaBonk), feito em Unity 6 (6000.6.5f1) para o Projeto 2D de Jogos Digitais — Insper 2026.1.

## Como jogar

- **WASD / Setas**: mover (gamepad também funciona se o Input System estiver instalado)
- **E (segurar)**: coletar materiais durante o dia
- **F**: interagir com baús e mercador
- **Left Shift**: dash (fica invencível e atravessa inimigos durante o dash; recarrega sozinho)
- As armas atacam sozinhas
- Colete as gemas que os inimigos derrubam para subir de nível e escolher upgrades (clique ou teclas **1, 2, 3**)
- **ESC / P**: pausar
- Dia e noite: cada período dura 3 minutos. De dia não aparecem inimigos; de noite eles vêm (e os que sobrarem somem quando amanhece). O relógio e o período aparecem no topo da tela.
- Inimigos têm chance de dropar ouro (moedas douradas), e o valor cresce a cada noite. Use o ouro para abrir baús e comprar melhorias com o mercador diurno
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
    Resources/       coleta diurna, geração de recursos e inventário de materiais
    Trading/         baús, mercador, compras e interface
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


## Coleta de materiais durante o dia

- Explore e segure **E** perto de uma árvore, rocha ou cristal. No controle, use o botão oeste (X no Xbox / quadrado no PlayStation; no Input Manager antigo, botão 2).
- Madeira rende 5, pedra 3 e cristal 2. Pontos maiores são ricos e rendem o dobro; cristais levam mais tempo para extrair.
- O painel **Materiais da vila** acompanha os saldos. A barra mostra o progresso da coleta.
- Soltar o botão, sair do alcance, dar dash, pausar ou anoitecer cancela a coleta em andamento. Armas continuam automáticas.
- Os pontos esgotados ficam menores e acinzentados; voltar à mesma região no mesmo dia não repõe seus recursos.
- Ao amanhecer, os pontos se renovam. O mapa mantém apenas 49 pontos ativos ao redor do jogador.
- Materiais acumulam durante a run; reiniciar a partida zera o inventário. Construções e estações de trabalho serão implementadas em suas tasks próprias.
- Integração: `ResourceInventory.Count`, `Add`, `CanAfford(wood, stone, crystal)`, `TrySpend(wood, stone, crystal)` e evento `OnChanged`. Gastos compostos são atômicos e custos negativos são rejeitados.
- Nenhuma regeneração de cena é necessária. Para testar o ciclo no Editor, **N** alterna dia/noite.


## Baús e mercador

- A cada amanhecer surgem um mercador e três baús perto da posição atual do jogador. O indicador no canto superior direito aponta o mercador.
- Chegue perto e pressione **F** (Y no Xbox / triângulo no PlayStation; botão 3 no Input Manager antigo). O jogo pausa durante a escolha.
- Escolha com **1, 2, 3**, clique ou navegação de UI pelo controle. **Esc / P / Start** ou o botão Voltar fecha a janela sem cobrar.
- O mercador trabalha de dia, vende uma unidade por oferta e renova o estoque no amanhecer. A sopa só pode ser comprada com vida abaixo do máximo.
- Cada baú oferece três recompensas fixas: você recebe UMA e paga somente ao confirmar. Fechar/reabrir não muda as opções. Baús permanecem disponíveis à noite.
- Os três níveis de baú custam 15/30/45 ouro no primeiro dia; poder e preços aumentam conforme o nível do baú, e preços crescem nos dias seguintes.
- Melhorias afetam vida, dano, armadura, recarga, alcance de coleta de drops, regeneração ou ganho de ouro. Todos os benefícios duram até o fim da run.
- Saldo insuficiente, compra repetida e compra de cura com vida cheia não descontam ouro. Compras usam `PlayerWallet.TrySpend`.
- Para testar no Editor: durante o Play, **Survivors > Testes > Adicionar 100 de ouro**. Esse menu não é incluído no build. **N** alterna dia/noite.

### Roteiro rápido de teste

1. Pare o Play, espere a recompilação e abra `Assets/Scenes/MainMenu` ou `Game`. Dê Play novamente; não é necessário regenerar as cenas.
2. Durante o dia, teste E em um recurso e confira o saldo de materiais.
3. Use **Survivors > Testes > Adicionar 100 de ouro** e siga o indicador até o mercador. Aperte F, compre uma melhoria e confira preço, saldo e item esgotado.
4. Abra um baú, feche com Esc e reabra: as opções e o saldo devem continuar iguais. Escolha uma recompensa: o valor deve ser descontado uma única vez e o baú deve ficar esgotado.
5. Teste sem ouro suficiente e tente comprar sopa com vida cheia: nenhuma dessas tentativas pode descontar ouro.
6. Use N para anoitecer: a coleta e o mercador ficam indisponíveis, mas baús fechados continuam utilizáveis. Use N de novo: recursos, baús e estoque se renovam; saldos e melhorias permanecem.
7. Reinicie a partida: ouro, materiais e melhorias devem voltar ao estado inicial.
