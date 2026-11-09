# Survivors 2D (nome provisório)

Roguelike de sobrevivência top-down 2D feito em Unity 6 (6000.6.5f1) para o Projeto 2D de Jogos Digitais do Insper 2026.2. De dia você explora e coleta; de noite, sobrevive às ondas de inimigos.

Inspirações de gênero: Vampire Survivors e MegaBonk.

## Como jogar

| Ação | Teclado | Controle |
|---|---|---|
| Mover | WASD / Setas | Analógico |
| Dash (invencível, atravessa monstros) | Left Shift | Indisponível |
| Coletar materiais (segurar) | E | Botão oeste (X no Xbox, quadrado no PlayStation) |
| Construir, reparar, resgatar, abrir baú e estação | F | Botão norte (Y no Xbox, triângulo no PlayStation) |
| Escolher upgrade, recompensa ou compra | 1 a 5 ou clique | Não testado |
| Pausar | Esc / P | Start |

- As armas atacam sozinhas, só à noite. Você começa com a Besta; Espada, Facas, Machado de arremesso e Bombas vêm de baús e da ferraria.
- Colete as gemas que os inimigos derrubam para subir de nível e escolher atributos.
- Cada dia e cada noite duram 3 minutos. De dia não aparecem inimigos. De noite eles vêm, e os que sobrarem somem ao amanhecer. O relógio e o período ficam no topo da tela.
- Objetivo: sobreviver ao máximo de noites. A run não tem fim.

### Coleta de materiais

- Segure **E** perto de uma árvore ou rocha. Madeira rende 5 e pedra rende 3. Pontos ricos são maiores e rendem o dobro.
- Soltar o botão, sair do alcance, dar dash, pausar ou anoitecer cancela a coleta.
- Pontos esgotados ficam menores e acinzentados, e se renovam ao amanhecer.
- Árvores e rochas têm colisão na base.

### Minimapa

- O radar no canto da tela fica centrado em você (branco) e mostra a fogueira (laranja) e os baús fechados (losangos dourados).
- O que está fora do alcance fica na borda, mais apagado, apontando a direção. Chaves não aparecem: é preciso procurá-las.

### Baús e chaves

- Três baús ficam perto do início da run, cada um mais distante e mais forte. Eles não voltam depois de abertos.
- Cada baú precisa de uma chave. A primeira nasce perto do início; as demais ficam espalhadas pelo mapa e são coletadas ao passar por cima.
- Perto do baú, aperte **F** e escolha **uma** de três recompensas: uma arma e dois equipamentos. A chave só é gasta ao confirmar a escolha.
- **Esc / P** ou Voltar fecha a janela sem gastar a chave. As opções continuam as mesmas ao reabrir.
- As missões, no canto esquerdo, indicam a distância e a direção do baú fechado mais próximo.

### Acampamento

- Você começa ao lado da fogueira, no centro do acampamento. **Se ela se apagar, a run termina**; a barra fica no topo da tela.
- Perto de uma marca no chão, aperte **F** para construir cerca ou portão. Depois, F reforça com pedra: a cerca vira muro e o portão de madeira vira portão de pedra. O custo aparece na tela.
- F também repara estruturas danificadas, inclusive a fogueira, a qualquer hora. Estruturas escurecem conforme levam dano.
- Portões bloqueiam monstros e se abrem para você passar. O dash atravessa monstros, mas não construções, árvores ou rochas.
- Monstros vão até a fogueira e destroem o que estiver no caminho. Longe da base, você é caçado enquanto ela é atacada. Os danos continuam após o amanhecer.
- Estruturas destruídas deixam o ponto vazio, sem devolver materiais.

### Pessoas perdidas e estações

- O ferreiro e a ferreira estão perdidos longe do acampamento. As missões, no canto esquerdo, mostram a distância e a direção de cada um.
- Perto da pessoa, aperte **F** para resgatá-la. Ela anda sozinha até o acampamento.
- Cada residente libera um ponto de estação: **ferraria** (ferreiro) e **armaria** (ferreira), por 30 madeira e 20 pedra.
- Perto da estação, **F** abre as compras: a ferraria ensina e melhora armas; a armaria dá armadura e vida. Pague com madeira e pedra (1 a 5 ou clique; Esc fecha).
- Monstros encostados ferem os residentes. Derrotado, o residente se perde de novo e a estação fecha até o novo resgate. O que você comprou continua.

### Ouro

Inimigos têm chance de dropar ouro, e o valor cresce a cada noite. O ouro ainda não tem uso no jogo (ver `docs/GDD.md`).

### Tutorial

Na primeira run, o topo das **missões** (canto esquerdo) mostra um objetivo por vez durante o primeiro dia: coletar, construir, resgatar e abrir um baú. **Enter** pula. Para ver de novo, use **Opções > Tutorial na próxima run**.

As missões também mostram, o tempo todo, quem falta resgatar e o baú mais próximo, com distância e direção.

### Opções

No menu principal ou na pausa, **Opções** ajusta o volume da música e dos efeitos (fica salvo para a próxima vez) e mostra os controles.

### Atalho de teste

**N** (só no Editor): pula para o próximo período. Em noite de chefe, mata o chefe na hora.

## Documentação

| Documento | Conteúdo |
|---|---|
| [GDD](docs/GDD.md) | Design do jogo. Publicado também no Milanote. |
| [Escopo](docs/ESCOPO.md) | O que entra e o que fica fora |
| [Rubrica](docs/RUBRICA.md) | Critérios de avaliação e entregáveis |
| [Balanceamento](docs/BALANCEAMENTO.md) | Metas e valores de jogo |
| [Arquitetura](docs/ARQUITETURA.md) | Estrutura do código e como estender |
| [Assets](docs/ASSETS.md) | Inventário, origem e licenças |
| [Contribuindo](docs/CONTRIBUINDO.md) | Equipe, responsabilidades e fluxo de trabalho |

## Estrutura do projeto

```
Assets/
  Art/Sprites/       sprites finais exportados do Aseprite
  Generated/         sprites e prefabs gerados pelo menu Survivors
  Scenes/            MainMenu e Game
  Scripts/
    Core/            GameManager, dia e noite, câmera, input, menu, decoração
    Player/          movimento, atributos, XP, ouro, animação, barra de vida
    Enemies/         Enemy (perseguição, dano, drops) e EnemySpawner (ondas, anéis, elites)
    Weapons/         Weapon (base) + 5 armas no jogo: Besta, Espada, Facas, Machado de arremesso
                     e Bombas (outras 5 fora do jogo)
    Resources/       coleta diurna, geração de recursos e inventário de materiais
    Chests/          baús, chaves, recompensas e interface
    Camp/            fogueira, construções, pessoas perdidas, estações e HUD do acampamento
    Upgrades/        UpgradeManager (opções de level up)
    Pickups/         gemas de XP, cura, ímã e ouro
    UI/              HUD e tela de level up
    FX/              partículas, números de dano, screen shake
    Audio/           AudioManager + sons placeholder gerados por código
    Editor/          SurvivorsSetup (gera sprites, prefabs e cenas), WebGLBuild e CampDebug
```

### Regenerar o protótipo

Menu **Survivors > Gerar sprites, prefabs e cenas**. **Atenção:** isso sobrescreve `Assets/Generated` e as duas cenas. Veja [Arquitetura](docs/ARQUITETURA.md), que também explica como adicionar inimigos, armas, upgrades e sons.

## Build WebGL (entrega no itch.io)

1. No Editor, menu **Survivors > Gerar build WebGL**. Ou, com o Unity fechado:
   `Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod WebGLBuild.Build -logFile build.log`
2. O build sai em `Builds/WebGL` (fora do git). Compacte o **conteúdo** da pasta, com o `index.html` na raiz do .zip:
   `cd Builds/WebGL && zip -r ../jogo-2d-webgl.zip .`
3. No itch.io: Kind of project **HTML**, envie o .zip e marque **This file will be played in the browser**. Tamanho sugerido da janela: 960 × 600.

O build usa Gzip com *decompression fallback* ligado, porque o itch.io não envia o cabeçalho de compressão dos arquivos `.unityweb`.

## Créditos

- Equipe: ver [Contribuindo](docs/CONTRIBUINDO.md).
- Código-base do protótipo gerado com auxílio do Claude (Anthropic) e adaptado pela equipe.
- Fonte: Pixel Operator, de Jayvee Enaguas (HarvettFox96), sob CC0 1.0 ([licença](docs/licencas/PixelOperator-CC0.txt)).
- Jogador: sprites feitos no Aseprite por Pedro Nadotti.
- Cercas, muros, portões e estações do acampamento: sprites feitos no Aseprite por Pedro Nadotti.
- Árvore, arbustos, rochas, fogueira e baú: feitos no Aseprite (autoria a confirmar em [Assets](docs/ASSETS.md)).
- Inimigos e chefes: sprites gerados por código com auxílio do Claude (Anthropic), no estilo do jogador de Pedro Nadotti.
- Armas, equipamentos, drops, ícones de recurso e chão: sprites gerados por código com auxílio do Claude (Anthropic), no mesmo estilo.
- Demais sprites e sons: placeholders gerados por código.
