# Design: base do acampamento

Data: 05/11/2026. Status: **aprovado**. Prazo do jogo: 10/11/2026 às 23:50.

## Objetivo

Transformar o protótipo de sobrevivência no conceito do `docs/GDD.md`: o jogador coleta de dia, reforça o acampamento, resgata pessoas e defende a base à noite. Cobre os 7 itens do núcleo aprovado em `docs/ESCOPO.md`.

Frentes da rubrica atendidas: Blueprint, Mechanics, Engajamento, Level Design, Balanceamento, Estados e Erros.

## Decisões já tomadas

- A run termina se o jogador morrer ou se o coração da base for destruído.
- XP dá apenas atributos e passivas; armas e armaduras vêm de baús e estações.
- Armas automáticas, sem magia, de 4 a 6.
- Construção em pontos fixos, com F, pagando materiais.

## Padrões assumidos nesta proposta

Decisões abertas do GDD que este design precisa fixar. Mude qualquer uma na revisão.

| Assunto | Proposta |
|---|---|
| Coração da base | Fogueira |
| Alvo dos monstros | Vão até a fogueira; perseguem o jogador se ele estiver a até 7 unidades |
| Danos | Persistem entre noites |
| NPCs | Dois: ferreiro de armas e ferreira de armaduras |
| Derrota de NPC | NPCs não morrem. Com a vida zerada, voltam a ficar perdidos longe do acampamento e precisam de novo resgate |
| Baús | Mantêm chaves; passam a oferecer armas e gear |
| Ouro | Sem mudança neste trabalho |
| Armas | 5: Besta, Espada, Machado de arremesso, Facas e Bombas |

## Abordagem

**Escolhida:** sistemas criados por código, como `ChestSystem` e `DayResourceSystem`, com monstros sem pathfinding. Eles andam em linha reta e atacam o que os bloqueia.

Motivos:

- Evita editar `Game.unity`, que gera conflitos numa equipe de 4 pessoas.
- Pontos fixos e linha reta dispensam navegação, o maior risco técnico para o prazo.
- Segue os padrões existentes do projeto.

**Descartadas:** A* ou NavMesh 2D, pelo custo e pelo risco no WebGL; montar o acampamento manualmente na cena, por conflitos e por depender do gerador.

## Layout do acampamento

Centrado no ponto inicial do jogador:

```
          portão
      ▪ ▪   ▪   ▪ ▪
    ▪               ▪
   ▪    E1     E2    ▪
portão    FOGUEIRA    portão
   ▪                 ▪
    ▪               ▪
      ▪ ▪   ▪   ▪ ▪
          portão
```

- **Fogueira:** centro.
- **Anel:** 16 pontos num círculo de raio 5,5. 12 de muralha e 4 de portão, nos pontos cardeais.
- **Estações:** 2 pontos fixos dentro do anel.
- **Área reservada:** raio 7 sem árvores, rochas, chaves ou baús.

O anel entra em conflito com a posição atual de baús e chaves, então os valores mudam:

| Campo em `ChestSystem` | Hoje | Novo |
|---|---|---|
| `firstChestDistance` | 5 | 9 |
| `firstKeyDistance` | 7 | 10 |

## Componentes

Nova pasta `Assets/Scripts/Camp/`.

| Classe | Responsabilidade |
|---|---|
| `Structure` | Vida, dano, reparo, destruição. Lista estática `Active`, como `Enemy.Active`. |
| `BuildSpot` | Um ponto fixo: tipo, nível atual, custo do próximo passo, estrutura presente. |
| `CampSystem` | Singleton. Cria o layout, escolhe o ponto mais próximo, trata F, expõe `Prompt`, `Heart` e `IsReserved`. |
| `LostPerson` | NPC: estados perdido, indo ao acampamento e residente; resgate e nova perda. |
| `StationSystem` | Regras e ofertas das estações. |
| `StationUI` | Janela de compra, no mesmo modelo de `ChestUI`. |
| `CampHUD` | Vida da fogueira e avisos. |

`GameManager.Start` adiciona `CampSystem` e `StationSystem` se faltarem, como já faz com `ChestSystem`.

## Comportamento

### Construção e reparo

Perto de um ponto vazio ou danificado, um aviso mostra a ação e o custo. F executa na hora, sem abrir menu nem pausar.

| Estrutura | Custo | Vida |
|---|---|---|
| Cerca (nível 1) | 6 madeira | 60 |
| Muro (nível 2, melhoria da cerca) | 6 pedra | 150 |
| Portão | 8 madeira | 100 |
| Estação | 30 madeira e 20 pedra | 120 |
| Fogueira | Já existe no início | 300 |

- **Reparo:** F restaura toda a vida; custa 1 material a cada 10 de vida faltando, arredondado para cima. Muro usa pedra; as demais estruturas usam madeira. Funciona de dia e de noite.
- **Destruição:** o ponto volta a ficar vazio, sem reembolso. Uma estação destruída precisa ser reconstruída, mas as compras já feitas permanecem.
- **Portões:** bloqueiam monstros e deixam o jogador passar, com `Physics2D.IgnoreCollision` entre portão e jogador. Assim o jogador nunca fica preso.

Valores iniciais, a ajustar em teste e registrar em `docs/BALANCEAMENTO.md`.

### Monstros

Mudanças em `Enemy`:

- **Alvo:** o jogador, se estiver a até 7 unidades; senão, a fogueira. Sem fogueira no jogo, sempre o jogador.
- **Ataque:** ao colidir com uma `Structure`, causa `contactDamage` a cada 1 s.
- **Reposicionamento:** `relocateDistance` passa a usar a distância até o alvo atual.

Mudanças em `EnemySpawner`:

- Com o jogador a menos de 15 unidades da fogueira, nada muda: inimigos nascem fora da tela.
- Com o jogador mais longe, metade nasce em volta da fogueira, a 16 unidades dela, e metade em volta da câmera. Sair da base tem risco.
- Anéis e elites seguem a câmera.

Ao amanhecer, `Enemy.VanishAll` continua funcionando. Os danos ficam.

### Derrota

`GameManager.EndRun` recebe um motivo opcional. A destruição da fogueira mostra "A FOGUEIRA SE APAGOU NA NOITE N". Uma fogueira com pouca vida gera aviso na tela e som.

### Resgate e estações

- Dois `LostPerson` aparecem por run, em direções sorteadas e separadas, a 25–40 unidades da fogueira, fora de colisões.
- Usam as spritesheets `character_men` e `character_women` com outra cor.
- O HUD indica a direção do NPC mais próximo durante o dia.
- F perto do NPC o resgata. Ele vai sozinho ao acampamento, sem bloquear e sem receber dano no caminho.
- No acampamento, o NPC tem 40 de vida e recebe dano por contato de monstros. Monstros não o procuram: o perigo é a base ser invadida.
- Com a vida zerada, o NPC não morre: mostra um aviso, some com um efeito e reaparece perdido a 25–40 unidades da fogueira, fora de colisões. O HUD volta a indicar sua direção.
- Enquanto o NPC estiver fora, sua estação fica construída, mas fechada. As compras já feitas permanecem. O novo resgate restaura sua vida e reabre a estação.
- O ponto da estação só aceita construção depois do resgate correspondente.
- F perto de uma estação pronta abre `StationUI`. O jogo pausa pelo estado `Chest`, que será renomeado para `Menu` neste trabalho, pois deixa de ser exclusivo de baús.

| Estação | Oferta | Custo inicial |
|---|---|---|
| Ferreiro de armas | Nova arma ou nível de arma. Espada e Facas exigem 1 residente; Machado e Bombas exigem 2 | 10 madeira e 10 pedra, +5 de cada por nível |
| Ferreira de armaduras | Até 5 níveis: +1 armadura e +10 de vida | 15 pedra, +5 por nível |

### Armas e level up

- `UpgradeManager` deixa de oferecer armas e níveis de arma, mantendo os atributos.
- Armas adaptadas das atuais, com nome e visual novos:

| Nova | Base | Observação |
|---|---|---|
| Besta | `MagicBoltWeapon` | Arma inicial |
| Espada | `WhipWeapon` | |
| Machado de arremesso | `BoomerangWeapon` | |
| Facas | `KnifeWeapon` | |
| Bombas | `BombWeapon` | |

As demais 5 saem do jogo. Removemos os prefabs e as classes depois que o novo fluxo estiver testado.

### Baús

`ChestReward` ganha a opção de arma: os baús sorteiam entre armas básicas e os bônus atuais, tratados como gear e com nomes medievais. Se o jogador já tiver a arma, ela sobe um nível. Chaves e três opções continuam iguais.

## Dead ends e casos-limite

| Situação | Tratamento |
|---|---|
| Jogador cercado por muros | Portões sempre passam |
| Dash contra um muro | O dash passa a ignorar só inimigos, pela nova layer `Enemy`, e para em estruturas |
| Sem materiais para reparar | Recursos renovam a cada amanhecer |
| NPC preso ou fora de alcance | Posição validada a cada vez que ele fica perdido; NPC nunca morre, então a estação sempre pode ser recuperada |
| Monstro preso em árvore ou baú | Colisores arredondados deslizam; validar em teste. Se falhar, desviar lateralmente ao ficar parado |
| Fogueira destruída durante um menu | Derrota vale ao fechar o menu; monstros não agem com `timeScale = 0` |
| Todas as ofertas compradas | Estação informa "Nada mais a ensinar" |

## Riscos conhecidos

- **Dash:** hoje ele torna o colisor do jogador um trigger e atravessaria muros. A troca por layer exige criar `Enemy` e aplicá-la aos prefabs.
- **Desempenho:** até 350 inimigos colidindo com estruturas no WebGL. Testar no build, não só no Editor.
- **Escuridão:** à noite, apenas o jogador tem luz. A base pode ficar difícil de ver. Uma luz na fogueira fica para o acabamento.
- **Código pendente:** o trabalho atual de baús e remoção do mercador ainda não foi commitado. Commitar antes de começar.

## Testes

Sem testes automatizados de Play Mode: a equipe testa no Editor e no build WebGL.

1. Construir cada estrutura, com e sem materiais.
2. Evoluir cerca para muro; deixar destruir; reconstruir.
3. Passar pelos portões e confirmar que monstros não passam.
4. Deixar o anel cair e a fogueira ser destruída: derrota correta.
5. Sair da base à noite e confirmar o ataque à base.
6. Resgatar os dois NPCs, construir estações e comprar tudo.
   Deixar monstros derrotarem um NPC: ele reaparece perdido, a estação fecha e reabre após novo resgate.
7. Abrir baú com arma nova e com arma repetida.
8. Reiniciar e confirmar que tudo volta ao estado inicial.
9. Repetir no build WebGL com muitos inimigos.

## Divisão sugerida do trabalho

Blocos com poucas dependências entre si:

| Bloco | Conteúdo | Sugestão |
|---|---|---|
| Base | `Structure`, `CampSystem`, construção, reparo, IA, derrota | Pedro |
| Pessoas e progressão | NPCs, estações, level up, armas, baús | 1 integrante |
| Arte | Fogueira, muros, portões, estações, monstros, armas | 1 integrante |
| Som e acabamento | Música, SFX, tutorial, UI, itch.io, Milanote | 1 integrante |

## Fora deste design

Reparador e construtor como NPCs, mais monstros, construção livre, luz da fogueira, tutorial, nome e história do jogo.
