# Game Design Document

> **Rascunho em construção.** Registra o jogo como ele é em 05/11/2026 e as decisões ainda abertas. A rubrica exige o GDD no **Milanote**: este arquivo é a base de trabalho e cada mudança deve ser levada para lá.

Organizado pelo framework **DDE** (Design, Dynamics, Experience), como pede a rubrica.

| Item | Valor |
|---|---|
| Nome | **A definir** (provisório: Survivors 2D) |
| Gênero | Sobrevivência e defesa de base com ondas de monstros, top-down 2D |
| Plataforma | Navegador (WebGL), teclado; suporte parcial a gamepad |
| Motor | Unity 6 |
| Equipe | 4 pessoas (ver `CONTRIBUINDO.md`) |
| Público | **A definir** |

## Decisões tomadas

- **Tema (05/11/2026):** medieval.
- **Fantasia central (05/11/2026):** começar sozinho num acampamento e transformá-lo numa base capaz de resistir às noites.
- **Progressão da base (05/11/2026):** resgatar pessoas perdidas pelo mapa e construir estações de trabalho para elas.

- **Derrota (05/11/2026):** a run termina se o jogador morrer ou se o coração da base, uma estrutura central do acampamento, for destruído. Cercas e muros podem cair sem encerrar a run.
- **Progressão (05/11/2026):** XP e level up dão apenas atributos e passivas. Armas e armaduras vêm de baús e estações.
- **Combate (05/11/2026):** armas automáticas e sem magia. Serão 4 a 6 armas medievais, escolhidas entre as atuais e adaptadas.
- **Construção (05/11/2026):** em pontos fixos do acampamento. O jogador se aproxima, aperta F e paga os materiais.
- **Forma do acampamento (08/11/2026):** anel quadrado, com um portão no meio de cada lado. Portões de madeira podem ser reforçados com pedra.

- **Coração da base (05/11/2026):** uma fogueira no centro do acampamento.
- **Monstros (05/11/2026):** vão até a fogueira, atacam o que os bloqueia e perseguem o jogador a até 7 unidades. Os danos persistem entre noites.
- **Pessoas (05/11/2026):** dois NPCs, ferreiro de armas e ferreira de armaduras. Nunca morrem: derrotados, voltam a ficar perdidos longe do acampamento e precisam de novo resgate; a estação fecha até lá.
- **Armas (05/11/2026):** Besta (inicial), Espada, Facas, Machado de arremesso e Bombas. Quanto mais pessoas no acampamento, melhores as armas à venda.
- **Baús (05/11/2026):** mantêm chaves e oferecem armas e gear.
- **Tutorial (08/11/2026):** objetivos curtos no primeiro dia, sem pausar: coletar, construir, resgatar e abrir um baú. Termina ao anoitecer com um aviso para defender a fogueira. Pode ser pulado e só aparece na primeira run; as opções o reativam.
- **Duração da run (08/11/2026):** infinita. A dificuldade cresce sem limite a partir da Noite 4, e o objetivo é sobreviver a mais noites.

Detalhes em `docs/superpowers/specs/2026-11-05-base-do-acampamento-design.md`. A ideia pode ser refinada durante o desenvolvimento: registre cada mudança no histórico ao final.

## Decisões abertas

1. Ouro continua no jogo? Com qual uso?
2. Quais são os "outros materiais" além de madeira e pedra?
3. Nome do jogo, protagonista e origem dos monstros
4. Qual o incentivo para jogar de novo

## 1. Design

### 1.1 Blueprint: conceito e mundo

Mundo medieval. O protagonista começa sozinho num acampamento e tem uma missão: usar o dia para reunir recursos, porque à noite os monstros chegam e ele precisa defender seu espaço. Cada noite é mais difícil que a anterior, e sozinho ele não aguenta por muito tempo. Por isso, procura pessoas perdidas pelo mapa, resgata-as para o acampamento e constrói estações de trabalho para elas. Com o tempo, o acampamento vira uma comunidade, e os monstros passam a ameaçar a base.

Nome, protagonista e origem dos monstros: **a definir**.

### 1.2 Mechanics: regras do jogo

**Planejado.** Sistemas da visão do jogo que ainda não existem no código:

- **Mais estações (se sobrar tempo):** reparador e construtor operados por NPCs.

**Implementado hoje.** O restante desta seção descreve o jogo atual.

**Laço principal.** De dia, explorar, coletar, resgatar pessoas e construir; de noite, defender o acampamento; a cada noite, enfrentar uma horda mais forte. Os inimigos somem ao amanhecer, e os danos na base ficam.

**Controles**

| Ação | Teclado | Gamepad |
|---|---|---|
| Mover | WASD / setas | Analógico |
| Dash | Left Shift | Indisponível |
| Coletar (segurar) | E | Oeste (X no Xbox) |
| Interagir: construir, reparar, resgatar, abrir baú e estação | F | Norte (Y no Xbox) |
| Escolher opção | 1 a 5 ou clique | Não testado |
| Pausar | Esc / P | Indisponível |

**Sistemas**

- **Combate:** as armas atacam sozinhas, só à noite; o jogador decide posição e dash, que dá invencibilidade.
- **Progressão:** inimigos soltam gemas de XP; cada nível oferece 3 opções entre armas novas, melhorias de arma e atributos.
- **Coleta:** de dia, segurar E junto a árvores e rochas rende madeira e pedra. Pontos ricos rendem o dobro e tudo renova ao amanhecer. **Os materiais ainda não têm uso.**
- **Baús:** três por run, perto do início, cada vez mais distantes e mais fortes. Cada abertura consome uma chave encontrada no mapa, e o jogador escolhe 1 de 3 bônus permanentes.
- **Ouro:** inimigos soltam ouro, mas **ainda não há onde gastá-lo**.
- **Acampamento:** o jogador começa ao lado da fogueira, cercada por um anel quadrado de 12 pontos de muralha (4 cantos e 2 trechos por lado) e 4 de portão, um no meio de cada lado. Se ela for destruída, a run termina.
- **Construção e reparo:** com F, em pontos fixos, pagando madeira e pedra. Cercas viram muros, portões de madeira viram portões de pedra e estruturas danificadas podem ser reparadas. Portões bloqueiam monstros e deixam o jogador passar; eles se abrem quando o jogador chega perto, para o lado oposto ao dele.
- **Armas:** cinco armas medievais e automáticas, que só atacam à noite (de dia não há monstros): Besta (inicial), Espada, Facas, Machado de arremesso e Bombas. Vêm de baús e da ferraria; o level up não dá armas.
- **Level up:** 3 opções entre 15 atributos e passivas.
- **Baús com equipamento:** cada baú oferece 1 arma e 2 equipamentos (amuleto, pedra de amolar, cota de malha, luvas, alforje, ervas, moeda); a força cresce com a distância do baú.
- **Pessoas perdidas:** o ferreiro e a ferreira começam perdidos, em direções opostas, longe do acampamento. De dia, o HUD aponta o mais próximo. Com F, a pessoa é resgatada e anda sozinha até o acampamento.
- **Estações:** cada residente libera a construção da sua estação. A ferraria ensina e melhora armas (Besta sempre; Espada e Facas com 1 residente; Machado e Bombas com os 2). A armaria dá até 5 níveis de armadura e vida. Compras são pagas com madeira e pedra e continuam valendo se a estação cair.
- **Residentes em perigo:** monstros não procuram os residentes, mas ferem quem estiver encostado. Derrotado, o residente volta a ficar perdido em outro ponto do mapa; a estação fica fechada, e as armas que dependem dos dois residentes somem da ferraria até o novo resgate.
- **Inimigos:** vão até a fogueira, atacam o que os bloqueia e perseguem o jogador se ele estiver perto. Longe da base, o jogador é caçado enquanto ela é atacada. Causam dano por contato. Variam entre Slime (básico), Bat (rápido) e Golem (resistente), além de elites, anéis que cercam o jogador e chefes.
- **Chefes:** na Noite 4 vem o Golem Ancião; na Noite 7, o Rei da Noite. Nessas noites não aparecem outros monstros: o relógio para à meia-noite até o chefe morrer, e então amanhece em poucos segundos. O chefe persegue só o jogador, investe depois de um aviso em linha vermelha e dispara anéis de tiros. O Rei da Noite também solta espirais e leques mirados, e entra em fúria com metade da vida. Derrotado, solta muito XP e ouro, cura e ímã.
- **Derrota:** vida do jogador zerada ou fogueira destruída encerra a run e mostra as estatísticas.

Valores detalhados em `BALANCEAMENTO.md`.

### 1.3 Interface

- **HUD:** painéis opacos com moldura. Canto superior esquerdo: nível, abates e barra de XP rotulada; abaixo, materiais (madeira, pedra, chaves, ouro) e **missões**. Topo: relógio, período e vida da fogueira; em noite de chefe, abaixo deles, nome e barra de vida do chefe. Canto superior direito: minimapa. Embaixo: avisos de interação, barra de coleta (só perto de árvore ou rocha) e dash.
- **Missões:** lista no canto esquerdo com o passo do tutorial, as pessoas a resgatar (distância e direção), estações a construir, o baú mais próximo e a meta do período (coletar de dia, defender a fogueira à noite).
- **Minimapa:** radar centrado no jogador, com alcance fixo, que mostra a fogueira e os baús fechados; o que está além do alcance fica preso na borda, indicando a direção. Chaves e inimigos ficam de fora para manter a exploração.
- **Menus:** principal, pausa, opções (volume de música e efeitos, controles), level up, baú, ferraria, armaria e game over.
- **Identidade visual:** fonte pixelada (Pixel Operator) em toda a interface, sem suavização; painéis e botões opacos em tons de madeira com moldura.
- **Arte dos monstros:** pixel art no estilo do jogador (sem contorno preto, paleta terrosa, luz de cima à esquerda), com olhos que brilham para ler bem à noite. Slime verde com um osso engolido; Morcego roxo de olhos âmbar; Golem de pedra com musgo. O Golem Ancião é um golem maior de basalto, com cristais âmbar nas costas e uma runa de lava no peito; o Rei da Noite é um rei espectral que flutua, com capa roxa rasgada, coroa, lua no peito e espadão violeta. As cores seguem as dos tiros de cada chefe (laranja e roxo).
- **Feedback no mundo:** monstros avermelham e crescem um pouco ao levar golpe; números de dano; tela tremendo e jogador piscando ao sofrer dano; recursos esgotados acinzentados; chaves pulsando; escuridão à noite com luz em volta do jogador.
- **Som:** SFX gerados por código, ainda placeholder. **Sem música.**

## 2. Dynamics

Comportamentos esperados quando as regras se combinam:

- **Kiting:** com armas automáticas, a habilidade está em se mover, abrir caminho com dash e agrupar inimigos.
- **Montagem de build:** o jogador combina atributos sorteados no level up com as armas e equipamentos que escolhe em baús e na ferraria.
- **Risco e recompensa no mapa:** buscar chaves e baús mais distantes.
- **Tensão crescente:** noites cada vez mais cheias, alternadas com a calma do dia.
- **Gestão do tempo do dia:** o dia é curto para tudo; o jogador escolhe entre coletar, explorar em busca de pessoas e construir.
- **Prioridades da base:** reparar o que foi danificado ou investir em novas estações e defesas.
- **Crescimento da comunidade:** cada resgate abre novas opções e aumenta o que há a perder.

## 3. Experience

Sair da solidão e da vulnerabilidade para o orgulho de ver o acampamento crescer, com o medo constante de perder o que foi construído. O dia traz urgência e planejamento; a noite traz tensão e defesa. Os critérios de sucesso são as metas de `BALANCEAMENTO.md`.

## Histórico de mudanças

Registre aqui as mudanças de trajetória, como pede a rubrica.

| Data | Mudança | Motivo |
|---|---|---|
| 09/11/2026 | Arte dos monstros e chefes no jogo (sprites em `Assets/Art/Resources/Enemies/`), substituindo as formas coloridas. Monstros passam a ser desenhados pela altura, atrás ou na frente de árvores, muros e jogador. | Rubrica pede sprites medievais de inimigos (item 11 do `ESCOPO.md`) |
| 09/11/2026 | Chefes na Noite 4 (Golem Ancião) e na Noite 7 (Rei da Noite): a noite só acaba quando o chefe morre. A confirmar pela equipe no PR da branch `feature/inimigos`. | Variar a run infinita com lutas diferentes das hordas |
| 08/11/2026 | As armas só atacam à noite. | De dia não há monstros; os ataques só faziam barulho e poluíam a tela durante a coleta |
| 08/11/2026 | Fonte trocada de Jersey 10 para Pixel Operator, maior (32/64) e com letras inequívocas. | A Jersey 10 estava difícil de ler; a Pixelify Sans confundia B/G, 5/S e C/O |
| 08/11/2026 | UI refeita: fonte pixelada, painéis opacos, missões no canto esquerdo (com o tutorial e as direções de pessoas e baús), XP rotulada junto do nível, minimapa sozinho no canto direito. | Pedido da equipe: textos borrados, minimapa sobreposto e caixa de coleta sempre visível |
| 08/11/2026 | Tutorial do primeiro dia e tela de opções com volume e controles; fonte medieval na interface. | Ensinar o laço do jogo e dar identidade à UI |
| 08/11/2026 | Run infinita confirmada; a dificuldade (quantidade, vida e dano dos monstros) continua crescendo depois da Noite 4. | A partir da Noite 4 a run estabilizava |
| 08/11/2026 | Implementados resgate do ferreiro e da ferreira, ferraria e armaria, armas medievais (5) e baús com 1 arma e 2 equipamentos. Level up só dá atributos. | Núcleo aprovado em 05/11 |
| 08/11/2026 | Anel do acampamento passa de círculo a quadrado; portão de madeira pode ser reforçado com pedra; portões abrem ao ver o jogador. | A arte final de cercas, muros e portões é top-down, em peças horizontais, verticais e de canto, e veio com portão de pedra |
| 05/11/2026 | Minimapa de orientação na HUD, com fogueira e baús fechados, sem chaves nem inimigos. | Evitar que o jogador se perca no mapa infinito sem tirar a busca por chaves |
| 05/11/2026 | Level up deixa de dar armas; armas ficam medievais, sem magia, e serão de 4 a 6 em vez de 10. | Dar papel próprio às estações e reduzir o trabalho de arte |
| 05/11/2026 | Conceito definido: defesa de um acampamento medieval, com construção, resgate de pessoas e estações de trabalho. | Dar propósito ao dia e aos recursos, e identidade ao jogo |
| 04/11/2026 | Removidos mercador, compra de baús com ouro, renovação diária de baús e cristal. Baús passam a ser 3 por run, abertos com chaves encontradas no mapa. | Funcionalidades fora do que foi combinado para o jogo |
