# Escopo

**Entrega: 10/11/2026 às 23:50.** Situação revista no dia da entrega: núcleo e acabamento implementados, faltam os entregáveis externos e o teste de jogo. Prioridade: um jogo completo e bem acabado, em vez de muitos sistemas pela metade.

> O núcleo está aprovado. Detalhes de cada item dependem das decisões abertas do `GDD.md`.

## Entregáveis obrigatórios

Sem negociação; ver `RUBRICA.md`.

- [ ] Build WebGL publicado no itch.io e inscrito na jam
- [ ] Página do itch.io com descrição, controles e imagens ou vídeo
- [x] Repositório no GitHub com créditos dos assets no `README.md`
- [ ] GDD atualizado no Milanote
- [ ] Projeto informado por completo no Blackboard
- [ ] Rubricas de maior e menor importância validadas com o professor

## Dentro do escopo

### Já existe

- Movimento, dash e combate automático
- 5 armas medievais no jogo (outras 5 no código, fora do jogo), 15 upgrades de atributo e level up
- 3 inimigos, elites e anéis de inimigos
- Chefes nas Noites 4 e 7, com padrões de ataque e barra de vida (proposto em 09/11/2026 por Carlos; entrou no PR #42)
- Dia e noite, com iluminação noturna
- Coleta de madeira e pedra
- 3 baús com chaves espalhadas
- Fogueira, construção de cercas, muros e portões, e reparo
- Monstros que atacam e destroem a base
- Mapa infinito com decoração
- Menu principal, pausa, game over, reinício
- Música de dia, noite e chefe; sons de passo, coleta de madeira e pedra e chave (09/11/2026)
- Nome (*Brasa Viva*), história e introdução em seis cartões na primeira run, com opção de pular (09/11/2026)
- HUD
- Minimapa de orientação, sem chaves nem inimigos (pedido em 05/11/2026; nada saiu do escopo por ele)

### Núcleo aprovado para a entrega

Versão mínima do conceito do `GDD.md`, em ordem de prioridade. Aprovada em 05/11/2026:

1. [x] Acampamento no ponto inicial, com vida e representação visual
2. [x] Construção de cercas e muros com madeira e pedra
3. [x] Monstros que atacam e destroem construções
4. [x] Pessoas perdidas pelo mapa e resgate
5. [x] Duas estações de trabalho: ferreiro de armas e ferreiro de armadura
6. [x] Reparo da base
7. [x] Baús com armas básicas e gear

### Acabamento obrigatório pela rubrica

8. [x] Nome, história e introdução curta
9. [x] Tutorial ou introdução guiada das mecânicas
10. [ ] Música de dia e de noite, e SFX finais: música pronta; faltam 7 efeitos 8-bit e os sons de construir, reparar e resgatar
11. [x] Sprites medievais de inimigos, armas e pickups
12. [x] Dificuldade que continue crescendo após a Noite 4
13. [x] Tela de opções com volume e identidade visual da UI
14. [ ] Teste de jogo e ajuste, com registro em `BALANCEAMENTO.md`: uma sessão registrada em 10/11/2026; faltam testes com quem não é da equipe

### Se sobrar tempo

- Estações de reparador e construtor operadas por NPCs
- Mais tipos de monstros, muros e itens

## Fora do escopo

Removidos por decisão do projeto. Não reintroduzir sem combinar antes:

- Mercador
- Compra de baús com ouro
- Baús renovados a cada amanhecer
- Cristal como recurso

Também fora:

- Multiplayer
- Save de run em andamento
- Plataformas além de WebGL como alvo de entrega

## Regra para novas ideias

Antes de entrar no escopo, uma ideia precisa melhorar uma frente da `RUBRICA.md`, caber no prazo e ter responsável. Se uma entra, avalie o que sai.
