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

- **Coração da base (05/11/2026):** uma fogueira no centro do acampamento.
- **Monstros (05/11/2026):** vão até a fogueira, atacam o que os bloqueia e perseguem o jogador a até 7 unidades. Os danos persistem entre noites.
- **Pessoas (05/11/2026):** dois NPCs, ferreiro de armas e ferreira de armaduras. Nunca morrem: derrotados, voltam a ficar perdidos longe do acampamento e precisam de novo resgate; a estação fecha até lá.
- **Armas (05/11/2026):** Besta (inicial), Espada, Facas, Machado de arremesso e Bombas. Quanto mais pessoas no acampamento, melhores as armas à venda.
- **Baús (05/11/2026):** mantêm chaves e oferecem armas e gear.

Detalhes em `docs/superpowers/specs/2026-11-05-base-do-acampamento-design.md`. A ideia pode ser refinada durante o desenvolvimento: registre cada mudança no histórico ao final.

## Decisões abertas

1. Ouro continua no jogo? Com qual uso?
2. Quais são os "outros materiais" além de madeira e pedra?
3. A run é infinita ou existe uma noite final?
4. Nome do jogo, protagonista e origem dos monstros
5. Como ensinar o jogo e qual o incentivo para jogar de novo

## 1. Design

### 1.1 Blueprint: conceito e mundo

Mundo medieval. O protagonista começa sozinho num acampamento e tem uma missão: usar o dia para reunir recursos, porque à noite os monstros chegam e ele precisa defender seu espaço. Cada noite é mais difícil que a anterior, e sozinho ele não aguenta por muito tempo. Por isso, procura pessoas perdidas pelo mapa, resgata-as para o acampamento e constrói estações de trabalho para elas. Com o tempo, o acampamento vira uma comunidade, e os monstros passam a ameaçar a base.

Nome, protagonista e origem dos monstros: **a definir**.

### 1.2 Mechanics: regras do jogo

**Planejado.** Sistemas da visão do jogo que ainda não existem no código:

- **Acampamento:** ponto de partida e espaço a defender.
- **Construção:** cercas e muros feitos com madeira, pedra e outros materiais.
- **Resgate:** pessoas perdidas pelo mapa podem ser levadas ao acampamento.
- **Estações de trabalho:** cada pessoa resgatada opera uma estação (ferreiro de armadura, ferreiro de armas, reparador, construtor etc.) que libera itens melhores.
- **Destruição da base:** com o tempo, os monstros conseguem destruir o acampamento.
- **Baús com equipamento:** contêm armas básicas e gear para as primeiras noites.

**Implementado hoje.** O restante desta seção descreve o jogo atual.

**Laço principal.** Planejado: de dia, explorar, coletar, resgatar e construir; de noite, defender o acampamento; a cada noite, enfrentar uma horda mais forte. Hoje: coleta de dia, sobrevivência à noite e inimigos que somem ao amanhecer.

**Controles**

| Ação | Teclado | Gamepad |
|---|---|---|
| Mover | WASD / setas | Analógico |
| Dash | Left Shift | Indisponível |
| Coletar (segurar) | E | Oeste (X no Xbox) |
| Interagir com baú | F | Norte (Y no Xbox) |
| Escolher opção | 1, 2, 3 ou clique | Não testado |
| Pausar | Esc / P | Indisponível |

**Sistemas**

- **Combate:** as armas atacam sozinhas; o jogador decide posição e dash, que dá invencibilidade.
- **Progressão:** inimigos soltam gemas de XP; cada nível oferece 3 opções entre armas novas, melhorias de arma e atributos.
- **Coleta:** de dia, segurar E junto a árvores e rochas rende madeira e pedra. Pontos ricos rendem o dobro e tudo renova ao amanhecer. **Os materiais ainda não têm uso.**
- **Baús:** três por run, perto do início, cada vez mais distantes e mais fortes. Cada abertura consome uma chave encontrada no mapa, e o jogador escolhe 1 de 3 bônus permanentes.
- **Ouro:** inimigos soltam ouro, mas **ainda não há onde gastá-lo**.
- **Inimigos:** perseguem e causam dano por contato. Variam entre Slime (básico), Bat (rápido) e Golem (resistente), além de elites e anéis que cercam o jogador.
- **Derrota:** vida zerada encerra a run e mostra as estatísticas.

Valores detalhados em `BALANCEAMENTO.md`.

### 1.3 Interface

- **HUD:** vida, XP, tempo, relógio do dia, ouro, materiais, chaves, direção do baú mais próximo e avisos de interação.
- **Menus:** principal, pausa, level up, baú e game over. Ainda não há opções.
- **Feedback no mundo:** clarão e escala ao acertar; números de dano; tela tremendo e jogador piscando ao sofrer dano; recursos esgotados acinzentados; chaves pulsando; escuridão à noite com luz em volta do jogador.
- **Som:** SFX gerados por código, ainda placeholder. **Sem música.**

## 2. Dynamics

Comportamentos esperados quando as regras se combinam:

- **Kiting:** com armas automáticas, a habilidade está em se mover, abrir caminho com dash e agrupar inimigos.
- **Montagem de build:** o jogador combina armas e atributos a partir de opções sorteadas, adaptando-se ao que surge.
- **Risco e recompensa no mapa:** buscar chaves e baús mais distantes.
- **Tensão crescente:** noites cada vez mais cheias, alternadas com a calma do dia.
- **Gestão do tempo do dia (planejado):** o dia é curto para tudo; o jogador escolhe entre coletar, explorar em busca de pessoas e construir.
- **Prioridades da base (planejado):** reparar o que foi danificado ou investir em novas estações e defesas.
- **Crescimento da comunidade (planejado):** cada resgate abre novas opções e aumenta o que há a perder.

## 3. Experience

Sair da solidão e da vulnerabilidade para o orgulho de ver o acampamento crescer, com o medo constante de perder o que foi construído. O dia traz urgência e planejamento; a noite traz tensão e defesa. Os critérios de sucesso são as metas de `BALANCEAMENTO.md`.

## Histórico de mudanças

Registre aqui as mudanças de trajetória, como pede a rubrica.

| Data | Mudança | Motivo |
|---|---|---|
| 05/11/2026 | Level up deixa de dar armas; armas ficam medievais, sem magia, e serão de 4 a 6 em vez de 10. | Dar papel próprio às estações e reduzir o trabalho de arte |
| 05/11/2026 | Conceito definido: defesa de um acampamento medieval, com construção, resgate de pessoas e estações de trabalho. | Dar propósito ao dia e aos recursos, e identidade ao jogo |
| 04/11/2026 | Removidos mercador, compra de baús com ouro, renovação diária de baús e cristal. Baús passam a ser 3 por run, abertos com chaves encontradas no mapa. | Funcionalidades fora do que foi combinado para o jogo |
