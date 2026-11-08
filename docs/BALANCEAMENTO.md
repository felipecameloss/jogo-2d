# Balanceamento

Define as metas de equilíbrio do jogo e registra os valores em uso. Atende à frente **Balanceamento** da `RUBRICA.md` e sustenta **Engajamento** e **Progressão**.

**Regra:** ao mudar um valor de jogo, atualize este arquivo na mesma alteração, com o motivo em "Registro de mudanças".

## Metas

Metas propostas, a confirmar com testes de jogo:

| Meta | Critério verificável |
|---|---|
| Início justo | Quem nunca jogou passa da Noite 1 na maioria das tentativas |
| Curva crescente | Cada noite é visivelmente mais difícil que a anterior; não há pico súbito |
| Sem estagnação | A dificuldade continua subindo enquanto o jogador ganha poder |
| Dia com valor | Coletar de dia muda o resultado da noite seguinte |
| Escolhas reais | Nenhuma opção de upgrade é sempre a melhor ou sempre inútil |
| Morte justa | O jogador entende por que morreu |

## Valores atuais

Fonte: código e `Assets/Scenes/Game.unity`, em 05/11/2026.

### Jogador

| Atributo | Valor |
|---|---|
| Vida | 100 |
| Velocidade | 5 |
| Invencibilidade após dano | 0,5 s |
| Raio de coleta de drops | 2 |
| Dash | velocidade 17 · 0,16 s · 1 carga · recarga 1,2 s · invencível |
| Crítico | 0% · dano ×2 |
| Arma inicial | Besta |

### Ritmo e dificuldade

Cada dia ou noite dura 180 s. A dificuldade depende de `t`, o total de segundos de noite já passados. Durante o dia, nada avança.

| Noite | `t` (s) | Inimigos/s | Multiplicador de vida |
|---|---|---|---|
| 1 | 0–180 | 1,0 → 2,8 | 1,0 → 1,9 |
| 2 | 180–360 | 2,8 → 4,6 | 1,9 → 2,8 |
| 3 | 360–540 | 4,6 → 6,4 | 2,8 → 3,7 |
| 4 | 540–720 | 6,4 → 7,0 | 3,7 → 4,0 |
| 5+ | 720+ | 7,0 | 4,0 |

As duas curvas chegam ao teto em `t = 600`, 60 s depois do início da Noite 4.

Eventos: anel de 24 inimigos a cada 60 s de `t`; elite a cada 90 s de `t`; limite de 350 inimigos simultâneos, exceto em anéis e elites.

### Inimigos

| Inimigo | Vida | Velocidade | Dano | XP | Aparece em `t` | Peso |
|---|---|---|---|---|---|---|
| Slime | 10 | 1,8 | 6 | 1 | 0 | 3 |
| Bat | 6 | 3,4 | 4 | 1 | 45 | 2 |
| Golem | 60 | 1,2 | 12 | 5 | 150 | 1 |

Elite: vida ×12, dano ×1,5, XP ×10, ouro garantido com faixa maior, cura garantida, resistência a empurrão de pelo menos 0,8.

O dano de contato não cresce com o tempo.

### Armas

Dano e recarga base; todas vão até o nível 5. As cinco armas vêm de baús e da ferraria. Lâminas Orbitais, Aura Sagrada, Raio Celeste, Frasco Venenoso e Nova Gélida ainda existem como prefabs, mas saíram do jogo.

| Arma | Dano | Recarga (s) | Ferraria libera com |
|---|---|---|---|
| Besta (inicial) | 10 | 0,9 | sempre |
| Espada | 13 | 1,3 | 1 residente |
| Facas | 7 | 0,6 | 1 residente |
| Machado de arremesso | 9 | 1,6 | 2 residentes |
| Bombas | 30 | 3,0 | 2 residentes |

### XP e level up

XP para o próximo nível: `5 + 8 × (nível − 1)`. São 333 XP até o nível 10 e 1.463 até o nível 20. Cada nível oferece 3 opções entre 15 atributos; armas não aparecem no level up.

| Upgrade | Efeito por nível | Máx. |
|---|---|---|
| Coração Robusto | +20 vida | 5 |
| Botas Leves | +10% velocidade | 5 |
| Força Bruta | +12% dano | 5 |
| Mente Ágil | −8% recarga | 5 |
| Expansão | +12% área | 5 |
| Ímã | +30% alcance de coleta | 5 |
| Regeneração | +0,5 vida/s | 5 |
| Duplicação | +1 virote, faca, machado e bomba | 2 |
| Sabedoria | +15% XP | 5 |
| Armadura | −1 dano por golpe, mínimo 1 | 5 |
| Olho Afiado | +8% crítico | 5 |
| Ampulheta | +15% duração | 5 |
| Presas | +4% de chance de curar 2 ao matar | 5 |
| Passo Rápido | −15% recarga do dash | 5 |
| Fôlego Extra | +1 carga de dash | 2 |

### Acampamento

| Estrutura | Custo | Vida |
|---|---|---|
| Fogueira | Inicial | 300 |
| Cerca | 6 madeira | 60 |
| Muro (melhoria da cerca) | 6 pedra | 150 |
| Portão | 8 madeira | 100 |
| Portão de pedra (melhoria do portão) | 8 pedra | 200 |
| Estação | 30 madeira e 20 pedra | 120 |

Anel quadrado de 8 × 8 células de 1,33 unidade (32 px), com lado de 10,7 unidades: 4 cantos, 8 trechos de 3 peças e 4 portões. Área livre de recursos em raio 8.

- **Reparo:** 1 material a cada 10 de vida faltando, arredondado para cima. Muro e portão de pedra usam pedra; as demais, madeira.
- **Dano de monstros em estruturas:** 50% do dano de contato, a cada 2 s, por monstro.
- **Alvo:** o jogador a até 7 unidades; senão, a fogueira.
- **Jogador a mais de 15 unidades da fogueira:** metade dos monstros nasce a 16 unidades dela; o resto caça o jogador.

### Recursos, ouro e baús

- **Coleta:** 1,1 s segurando E, alcance 1,9. Madeira rende 5, pedra 3. Pontos ricos (1 em 8) rendem o dobro.
- **Distribuição:** células de 6,5 unidades; 25% têm árvore, 30% têm pedra, 45% ficam vazias. Tudo renova ao amanhecer.
- **Drops por inimigo:** ouro 20% (1–2, com +20% por noite após a primeira), cura 1%, ímã 0,4%.
- **Baús:** 3 por run, a 10; 13,5 e 17 unidades do centro do acampamento. Cada um oferece 3 recompensas de força ×1, ×2 e ×3, conforme o baú, e custa 1 chave.
- **Chaves:** uma garantida a 11 unidades do acampamento; demais em 20% das células de 14 unidades, somente enquanto faltarem chaves para os baús fechados.

Cada baú oferece 1 arma (diferente em cada baú, sem a Besta) e 2 equipamentos.

| Recompensa | Por ponto de força |
|---|---|
| Arma | aprende a arma; cada ponto além do primeiro sobe 1 nível. Se já tiver, +1 nível por ponto |
| Amuleto de carvalho | +15 vida máxima e cura |
| Pedra de amolar | +10% dano |
| Cota de malha | +1 armadura |
| Luvas de couro | −5% recarga |
| Alforje | +1 alcance de coleta |
| Ervas do curandeiro | +0,3 vida/s |
| Moeda da fortuna | +15% ouro (fica até decidirem o destino do ouro) |

### Pessoas e estações

- **Pessoas perdidas:** 2 por run (ferreiro e ferreira), em direções opostas (180° ± 40°), a 25–40 unidades da fogueira, em ponto sem colisão. Andam até o acampamento a 3,5 unidades/s.
- **Residentes:** 40 de vida. Monstros encostados (raio 0,6) causam 50% do dano de contato a cada 2 s. Derrotado, volta a ficar perdido a 25–40 unidades; o novo resgate restaura a vida.
- **Ferraria:** aprender ou subir uma arma para o nível N custa `10 + 5 × (N − 1)` de madeira e o mesmo de pedra.
- **Armaria:** 5 níveis; cada um dá +1 de armadura e +10 de vida máxima, e custa `15 + 5 × (nível atual)` de pedra (15, 20, 25, 30, 35).

## Problemas conhecidos

Identificados por leitura do código; ainda não medidos em jogo.

| # | Problema | Efeito | Direção |
|---|---|---|---|
| 1 | **Ouro não tem uso:** nada chama `PlayerWallet.TrySpend` | Drop sem valor; Moeda da fortuna é recompensa inútil | Definir uso no GDD ou remover |
| 2 | **Madeira e pedra não têm uso:** nada chama `ResourceInventory.TrySpend` | Viola "Dia com valor"; o dia inteiro perde sentido | Custos de cercas, muros e estações |
| 3 | **Dificuldade para de subir** em `t = 600`, enquanto o jogador segue evoluindo | Viola "Sem estagnação"; a run pode ficar trivial | Ampliar a curva ou criar novas ameaças |
| 4 | **Dano de contato fixo** enquanto a armadura se acumula | Armadura pode reduzir golpes de Slime e Bat ao mínimo de 1 | Testar e escalar o dano |
| 5 | **Baús só no início:** depois de abertos, não há mais recompensas no mapa | Exploração perde objetivo | Avaliar no GDD |
| 6 | **Primeiro dia longo:** 3 min sem ameaça antes de qualquer combate | Risco de início lento e de perder testadores | Encurtar o dia ou dar objetivo |

## Roteiro de teste

Registre aqui cada sessão: data, quem jogou, versão, noite alcançada, causa da morte, armas usadas e observações. Priorize pessoas que não participaram do desenvolvimento.

| Data | Jogador | Versão | Noite | Morte | Observações |
|---|---|---|---|---|---|
| — | — | — | — | — | — |

## Registro de mudanças

| Data | Valor | De → Para | Motivo |
|---|---|---|---|
| 08/11/2026 | Portão de pedra | — → 8 pedra, 200 de vida | Nova melhoria do portão, no mesmo ritmo do muro (cerca a muro: +90 de vida por 6 pedra) |
| 08/11/2026 | Área livre de recursos | raio 7 → 8 | Os cantos do anel quadrado ficam a 7,5 do centro |
| 08/11/2026 | Distância do primeiro baú | 9 → 10 | Não encostar nos cantos do anel quadrado |
| 08/11/2026 | Distância da primeira chave | 10 → 11 | Acompanhar o primeiro baú |
| 05/11/2026 | Dano em estruturas | 100% a cada 1 s → 50% a cada 2 s | Em teste automático, 15 Slimes apagariam a fogueira em segundos. Com anel de cercas e sem jogador, ela resistiu 75 s na Noite 1 |
| 05/11/2026 | Distância do primeiro baú | 5 → 9 | Abrir espaço para o acampamento |
| 05/11/2026 | Distância da primeira chave | 7 → 10 | Abrir espaço para o acampamento |
