# Plano de implementação: base do acampamento

> **Para agentes:** use `superpowers:subagent-driven-development` ou `superpowers:executing-plans` para executar tarefa por tarefa. Os passos usam checkbox (`- [ ]`).

**Objetivo:** implementar o núcleo aprovado do jogo e concluir o acabamento e a entrega até 10/11/2026 às 23:50.

**Arquitetura:** novos sistemas criados por código em `Assets/Scripts/Camp/`, como `ChestSystem`. Monstros sem pathfinding atacam o que os bloqueia. A arte entra por `Resources.Load`, com placeholder gerado por código quando o arquivo não existe.

**Tecnologia:** Unity 6 (`6000.6.5f1`), C#, WebGL.

**Spec:** `docs/superpowers/specs/2026-11-05-base-do-acampamento-design.md`

## Restrições globais

- Prazo rígido: 10/11/2026 às 23:50.
- O projeto não tem testes automatizados: cada tarefa é verificada no Editor, e o que afeta desempenho, também no build WebGL.
- Todo input passa por `GameInput`; todo estado de run passa por `GameManager`.
- Não edite `Game.unity` nem rode **Survivors > Gerar sprites, prefabs e cenas** sem avisar a equipe.
- Textos, comentários e commits em português.
- Mudou valor de jogo: atualize `docs/BALANCEAMENTO.md`. Mudou design: `docs/GDD.md`. Entrou asset: `docs/ASSETS.md` e créditos do `README.md`.
- Mercador, compra de baús com ouro, baús renovados e cristal não voltam.

## Pontos de atenção na revisão

Casos que o spec implica e que cada tarefa dona verifica:

1. Jogador parado sobre um ponto ao construir: a construção é recusada com aviso, sem prender o jogador (Task 3).
2. Reiniciar a run: listas estáticas e estados voltam ao início (Task 2 e Task 8).
3. Vários alvos de F próximos: vale o mais próximo, e uma tecla só executa uma ação (Task 3).
4. Monstro preso em árvore ou baú a caminho da fogueira (Task 5).
5. NPC derrotado enquanto está a caminho ou com a janela da estação aberta (Task 10).

---

## Bloco: base

### Task 1: Integrar o código pendente na main

**Bloco:** base · **Alvo:** 06/11 · **Depende de:** nenhuma

Todas as tarefas dependem do código de baús e chaves, que está só em `feat/mudancas-dia`.

- [x] Revisar e commitar as mudanças pendentes (`Chests/`, remoção de `Trading/`, rochas, cenas).
- [x] Abrir PR para `main`, testar e fazer merge.
- [x] Avisar a equipe para criar branches a partir da `main` atualizada.

### Task 2: Fogueira, estruturas e derrota da base

**Bloco:** base · **Alvo:** 06/11 · **Depende de:** Task 1

**Arquivos:** criar `Assets/Scripts/Camp/Structure.cs`, `CampSystem.cs`, `CampArt.cs`, `CampHUD.cs`; alterar `Core/GameManager.cs`.

**Interfaces produzidas:**

- `enum StructureKind { Heart, Fence, Wall, Gate, Station }`
- `Structure`: `static List<Structure> Active`, `Kind`, `Health`, `MaxHealth`, `event Action<Structure> OnDestroyed`, `Init(StructureKind kind, float maxHealth)`, `TakeDamage(float amount)`, `RepairFull()`
- `CampSystem`: `static Instance`, `Vector3 Center`, `Structure Heart`, `string Prompt`, `bool IsReserved(Vector3 position, float radius)`
- `CampArt.Get(string name)`: tenta `Resources.Load<Sprite>("Camp/" + name)` e, sem arquivo, devolve placeholder gerado
- `GameManager.EndRun(string reason = null)`

- [x] Criar `Structure` com feedback ao levar dano: clarão, partículas e som.
- [x] Criar `CampSystem`, adicionado em `GameManager.Start` se faltar, com a fogueira de vida 300 no ponto inicial.
- [x] Fazer `IsReserved` proteger raio 7 em volta do centro; `DayResourceSystem` e chaves devem consultá-lo.
- [x] Destruir a fogueira chama `EndRun("A FOGUEIRA SE APAGOU NA NOITE N")`, com N real.
- [x] Criar `CampHUD` com a vida da fogueira e alerta abaixo de 30%.
- [x] Verificar no Editor: nada nasce na área reservada; dano de teste na fogueira até zerar mostra a derrota certa; Reiniciar volta com a vida cheia.

### Task 3: Pontos de construção, cercas e muros

**Bloco:** base · **Alvo:** 06/11 · **Depende de:** Task 2

**Arquivos:** criar `Camp/BuildSpot.cs`; alterar `Camp/CampSystem.cs`, `Chests/ChestSystem.cs`.

**Interfaces produzidas:** `BuildSpot`: `SpotKind`, `Structure Current`, `string ActionText`, `bool TryBuild(ResourceInventory bag)`; `CampSystem.NearestSpot`.

- [x] Em `ChestSystem`, mudar `firstChestDistance` para 9 e `firstKeyDistance` para 10 (em 08/11: 10 e 11).
- [x] Criar 16 pontos num círculo de raio 5,5 (em 08/11 virou quadrado, ver Task 12): 12 de muralha e 4 de portão, nos pontos cardeais. Criar 2 pontos de estação dentro do anel, bloqueados até a Task 9.
- [x] Pontos vazios ficam visíveis como marcas no chão.
- [x] Mostrar ação e custo no `Prompt`; F executa sem pausar, usando `ResourceInventory.TrySpend`. Vale o ponto mais próximo.
- [x] Custos e vidas: cerca 6 madeira, 60; muro 6 pedra, 150; portão 8 madeira, 100.
- [x] Recusar construção com o jogador ou um NPC sobre o ponto, com aviso.
- [x] Estrutura destruída devolve o ponto ao estado vazio, sem reembolso.
- [x] Verificar no Editor: construir sem materiais não cobra; cerca evolui para muro; construir parado sobre o ponto é recusado; baús e chaves ficam fora do anel.

### Task 4: Portões e dash

**Bloco:** base · **Alvo:** 07/11 · **Depende de:** Task 3

**Arquivos:** alterar `Camp/BuildSpot.cs`, `Player/PlayerController.cs`, `Enemies/EnemySpawner.cs`, `ProjectSettings/TagManager.asset`.

- [x] Portões usam `Physics2D.IgnoreCollision` com o colisor do jogador: ele passa, monstros não.
- [x] Criar a layer `Enemy` e aplicá-la ao instanciar monstros em `EnemySpawner.Spawn`.
- [x] Trocar o `isTrigger` do dash por ignorar colisão entre a layer do jogador e `Enemy`; o dash deve parar em estruturas, árvores e rochas.
- [x] Verificar no Editor: jogador passa pelos portões; monstro não passa; dash contra muro para; dash ainda atravessa monstros e dá invencibilidade.

### Task 5: Monstros atacam a base

**Bloco:** base · **Alvo:** 07/11 · **Depende de:** Task 2

**Arquivos:** alterar `Enemies/Enemy.cs`, `Enemies/EnemySpawner.cs`.

- [x] Alvo: jogador se estiver a até 7 unidades; senão a fogueira; sem fogueira, o jogador.
- [x] Ao colidir com `Structure`, causar `contactDamage` a cada 1 s, por monstro.
- [x] `relocateDistance` passa a usar a distância até o alvo atual.
- [x] Com o jogador a mais de 15 unidades da fogueira, metade dos monstros nasce a 16 unidades dela e metade em volta da câmera. Anéis e elites seguem a câmera.
- [x] Monstro parado por mais de 1 s contra árvore ou baú desvia lateralmente.
- [x] Verificar no Editor: sem muros, monstros chegam à fogueira; com muros, atacam e derrubam; longe da base, ela é atacada; ao amanhecer somem e os danos ficam.
- [ ] Verificar no build WebGL com muitos monstros atacando: desempenho aceitável. (Pendente: ainda não há build; fica para a Task 22.)

### Task 6: Reparo da base

**Bloco:** base · **Alvo:** 07/11 · **Depende de:** Task 3

**Arquivos:** alterar `Camp/BuildSpot.cs`, `Camp/CampSystem.cs`.

- [x] Perto de estrutura danificada, F restaura toda a vida.
- [x] Custo: 1 material a cada 10 de vida faltando, arredondado para cima. Muro usa pedra; as demais estruturas usam madeira.
- [x] Sem materiais suficientes, não repara nem cobra, e o aviso informa o custo.
- [x] Fogueira também é reparável, de dia ou de noite.
- [x] Verificar no Editor: custo correto; sem saldo, nada muda.

## Bloco: pessoas e progressão

### Task 7: Armas medievais e level up de atributos

**Bloco:** progressão · **Alvo:** 07/11 · **Depende de:** Task 1

**Arquivos:** alterar `Upgrades/UpgradeManager.cs`, `Editor/SurvivorsSetup.cs`, prefabs `W_MagicBolt`, `W_Whip`, `W_Boomerang`, `W_Knife`, `W_Bomb`, `Weapons/WeaponHolder.cs`.

**Interfaces produzidas:** `WeaponHolder.Unlock(Weapon prefab)`: adiciona a arma ou, se já existir, sobe um nível; `WeaponHolder.AvailablePrefabs`.

- [x] Renomear: Orbe Arcano → Besta (inicial), Chicote → Espada, Bumerangue → Machado de arremesso. Facas e Bombas continuam.
- [x] Mudar `UpgradeManager.RollOptions` para só oferecer atributos e o aviso "Duplicação" citar apenas armas existentes.
- [x] Tirar as outras 5 armas de `availableWeapons`.
- [ ] Verificar no Editor: run começa com Besta; level up só mostra atributos; "Lanche" aparece com tudo no máximo.

### Task 8: Pessoas perdidas e resgate

**Bloco:** progressão · **Alvo:** 08/11 · **Depende de:** Task 2

**Arquivos:** criar `Camp/LostPerson.cs`; alterar `Camp/CampSystem.cs`, `Camp/CampHUD.cs`.

**Interfaces produzidas:** `enum PersonRole { WeaponSmith, ArmorSmith }`, `enum PersonState { Lost, Returning, Resident }`; `LostPerson`: `Role`, `State`, `event Action<LostPerson> OnStateChanged`, `Rescue()`, `Defeat()`; `CampSystem.Residents`.

- [x] Criar 2 NPCs por run, em direções separadas, a 25–40 unidades da fogueira e validados contra colisões.
- [x] Usar `character_men` e `character_women` com cor diferente, por `CharacterAnimator`.
- [x] HUD indica a direção do perdido mais próximo durante o dia.
- [x] F resgata; o NPC anda sozinho até o acampamento sem bloquear e sem levar dano. Ele atravessa muros e portões.
- [ ] Verificar no Editor: dois NPCs alcançáveis em várias runs; resgate chega ao acampamento; Reiniciar volta ao estado inicial.

### Task 9: Estações de trabalho

**Bloco:** progressão · **Alvo:** 08/11 · **Depende de:** Task 3, Task 7, Task 8

**Arquivos:** criar `Camp/StationSystem.cs`, `Camp/StationUI.cs`; alterar `Core/GameManager.cs`, `Chests/ChestUI.cs`, `Camp/BuildSpot.cs`.

Relacionada à issue #5: quanto mais pessoas no acampamento, melhores as armas.

- [x] Renomear `GameState.Chest` para `Menu`, com `EnterMenu` e `ExitMenu`.
- [x] Ponto de estação libera após o resgate do NPC correspondente. Custo 30 madeira e 20 pedra; vida 120.
- [x] Criar `StationUI`, no modelo de `ChestUI`: 1, 2, 3 ou clique; Esc fecha.
- [x] Ferreiro de armas: Besta sempre; Espada e Facas com 1 residente; Machado e Bombas com 2. Custo 10 madeira e 10 pedra, +5 de cada por nível.
- [x] Ferreira de armaduras: até 5 níveis, +1 armadura e +10 de vida. Custo 15 pedra, +5 por nível.
- [x] Sem ofertas, mostrar "Nada mais a ensinar".
- [x] Estação destruída mantém as compras feitas.
- [ ] Verificar no Editor: comprar cobra uma vez; sem saldo não cobra; ofertas de 2 residentes só aparecem com 2; destruir e reconstruir mantém as compras.

### Task 10: NPC derrotado volta a ficar perdido

**Bloco:** progressão · **Alvo:** 08/11 · **Depende de:** Task 5, Task 9

**Arquivos:** alterar `Camp/LostPerson.cs`, `Enemies/Enemy.cs`, `Camp/StationSystem.cs`.

- [x] NPC residente tem 40 de vida e recebe dano por contato de monstros, que não o procuram.
- [x] Vida zerada chama `Defeat()`: aviso, efeito e nova posição a 25–40 unidades, validada contra colisões.
- [x] Estação fica construída e fechada, com o motivo no aviso. Ofertas que exigem 2 residentes somem.
- [x] Novo resgate restaura a vida e reabre a estação.
- [x] Derrota com janela aberta não ocorre, pois o jogo pausa. NPC em `Returning` não recebe dano.
- [ ] Verificar no Editor: derrotar NPC, achar pelo HUD, resgatar, estação reabre com compras mantidas.

### Task 11: Baús com armas e gear

**Bloco:** progressão · **Alvo:** 08/11 · **Depende de:** Task 7

**Arquivos:** alterar `Chests/ChestOffer.cs`, `Chests/ChestSystem.cs`.

- [x] Adicionar `ChestReward.Weapon`; `Apply` chama `WeaponHolder.Unlock`.
- [x] Trocar o `% 7` fixo por `Enum.GetValues(...).Length`.
- [x] Dar nomes medievais às recompensas atuais.
- [ ] Decidir com o responsável pelo ouro se Moeda da fortuna sai do sorteio. (Por ora, fica.)
- [ ] Verificar no Editor: arma nova entra; arma repetida sobe de nível; fechar e reabrir mantém as opções.

## Bloco: arte

### Task 12: Arte do acampamento

**Bloco:** arte · **Alvo:** 08/11 · **Depende de:** nenhuma

Exportar para `Assets/Art/Resources/Camp/` a 24 px por unidade. `CampArt` carrega sem mudança de código.

- [x] Fogueira animada: `heart.png`
- [x] Cerca, muro e portão de madeira e de pedra: `fence`, `wall`, `gate_wood`, `gate_stone`. Sem versão danificada: o dano escurece as peças.
- [x] Ferraria e armaria, vazias e com NPC: `station_weapon`, `station_armor` e `*_npc`
- [ ] Marca de ponto vazio: `spot`
- [x] Registrar em `docs/ASSETS.md`.

### Task 13: Arte dos monstros

**Bloco:** arte · **Alvo:** 09/11 · **Depende de:** nenhuma

- [ ] Substituir os placeholders de Slime, Bat e Golem por monstros medievais animados.
- [ ] Trocar nos prefabs `Enemy_*` sem rodar o gerador.
- [ ] Manter elite distinguível.
- [ ] Registrar em `docs/ASSETS.md`.

### Task 14: Arte de armas, itens e chão

**Bloco:** arte · **Alvo:** 09/11 · **Depende de:** Task 7

- [ ] Sprites das 5 armas e seus projéteis.
- [ ] XP, ouro, cura, ímã, baús e chaves.
- [ ] Chão com ambientação medieval.
- [ ] Registrar em `docs/ASSETS.md`.

## Bloco: som

### Task 15: Música

**Bloco:** som · **Alvo:** 08/11 · **Depende de:** nenhuma

**Arquivos:** alterar `Audio/AudioManager.cs`.

- [ ] Escolher ou compor faixas de dia, noite e menu, com licença registrada.
- [ ] `AudioManager` troca a faixa com fade em `OnPeriodChanged`.
- [ ] Verificar no build WebGL: áudio começa após o primeiro clique e faz loop sem corte.

### Task 16: Efeitos sonoros finais

**Bloco:** som · **Alvo:** 09/11 · **Depende de:** nenhuma

**Arquivos:** alterar `Audio/AudioManager.cs`.

- [ ] Substituir os 7 sons gerados por `SfxGenerator`.
- [ ] Adicionar sons de construção, reparo, estrutura destruída, resgate e alerta da fogueira.
- [ ] Registrar em `docs/ASSETS.md`.

## Bloco: acabamento

### Task 17: Nome, história e introdução

**Bloco:** acabamento · **Alvo:** 08/11 · **Depende de:** nenhuma

- [x] Definir nome do jogo, protagonista e origem dos monstros no `docs/GDD.md`. (*Brasa Viva*; Bento; maldição do Rei da Noite.)
- [x] Criar introdução curta ao iniciar a run, com opção de pular. (Seis cartões na primeira run; as opções reativam.)
- [x] Atualizar título do menu e `README.md`.

### Task 18: Tutorial no início da run

**Bloco:** acabamento · **Alvo:** 09/11 · **Depende de:** Task 3, Task 8

- [x] Guiar no primeiro dia: coletar, construir, procurar pessoas, preparar-se para a noite.
- [x] Mostrar as dicas como objetivos curtos, sem pausar o jogo.
- [x] Permitir pular.
- [x] Decidir se o primeiro dia dura menos que 180 s e registrar em `docs/BALANCEAMENTO.md`. (Mantido em 180 s.)

### Task 19: Menu de opções e identidade da UI

**Bloco:** acabamento · **Alvo:** 09/11 · **Depende de:** Task 15

- [x] Opções com volume de música e SFX, acessíveis no menu e na pausa, salvas em `PlayerPrefs`.
- [x] Tela de controles (dentro das opções).
- [x] Fonte pixelada (Pixel Operator, CC0) e painéis opacos com moldura em HUD, janelas e menus.
- [ ] Remover do HUD informações sem uso. (Pendente: o ouro ainda aparece; depende da decisão sobre o ouro.)

### Task 20: Dificuldade após a Noite 4

**Bloco:** acabamento · **Alvo:** 09/11 · **Depende de:** Task 5

Hoje as curvas param de subir em `t = 600` e o dano de contato é fixo.

- [x] Estender `spawnsPerSecond` e `healthMultiplier` além de `t = 600`.
- [x] Escalar o dano de contato com o tempo.
- [x] Definir se a run é infinita ou tem noite final, no `docs/GDD.md`.
- [x] Registrar em `docs/BALANCEAMENTO.md`.

### Task 21: Testes de jogo e balanceamento

**Bloco:** acabamento · **Alvo:** 09/11 · **Depende de:** Task 5, Task 9

- [ ] Fazer pelo menos 5 sessões, com pessoas de fora da equipe sempre que possível.
- [ ] Registrar em `docs/BALANCEAMENTO.md`.
- [ ] Ajustar custos, vidas e dificuldade conforme as metas.
- [ ] Definir o destino do ouro e dos "outros materiais" no `docs/GDD.md`.
- [ ] Corrigir ou registrar os bugs encontrados.

## Bloco: entrega

### Task 22: Primeiro build WebGL

**Bloco:** entrega · **Alvo:** 07/11 · **Depende de:** Task 1

Testar cedo para descobrir problemas de WebGL antes do prazo.

- [x] Gerar o build WebGL (08/11: 9,4 MB, `Survivors > Gerar build WebGL`). Publicar como rascunho no itch.io: pendente.
- [ ] Testar áudio, entrada, proporção da tela e desempenho no navegador.
- [x] Registrar o passo a passo no `README.md`.

### Task 23: Validar rubricas com o professor

**Bloco:** entrega · **Alvo:** 06/11 · **Depende de:** nenhuma

- [ ] Confirmar as 2 rubricas de peso 2 e as 2 desconsideradas.
- [ ] Anotar na planilha de acompanhamento e em `docs/RUBRICA.md`.
- [ ] Confirmar responsabilidades em `docs/CONTRIBUINDO.md`.

### Task 24: GDD no Milanote

**Bloco:** entrega · **Alvo:** 09/11 · **Depende de:** nenhuma

- [ ] Criar o GDD no Milanote a partir de `docs/GDD.md`, organizado por DDE.
- [ ] Manter o histórico de mudanças.
- [ ] Revisar na véspera da entrega para refletir o jogo final.

### Task 25: Créditos e licenças

**Bloco:** entrega · **Alvo:** 10/11 · **Depende de:** nenhuma

- [ ] Confirmar autoria dos sprites atuais em `docs/ASSETS.md`.
- [ ] Conferir a licença de todo asset de terceiros.
- [ ] Atualizar os créditos no `README.md`.

### Task 26: Entrega final

**Bloco:** entrega · **Alvo:** 10/11 · **Depende de:** Task 21, Task 22, Task 24, Task 25

- [ ] Revisar `docs/RUBRICA.md` frente a frente.
- [ ] Gerar o build WebGL final e testá-lo do início à derrota.
- [ ] Publicar na [jam do itch.io](https://itch.io/jam/projeto-jogo-2d-insper-20262), com descrição, controles, imagens e vídeo.
- [ ] Conferir o repositório no GitHub e o Milanote.
- [ ] Informar o projeto de forma completa no Blackboard antes das 23:50.
