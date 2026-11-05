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
| Arma inicial | Orbe Arcano |

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

Dano e recarga base; todas vão até o nível 5. O jogador tem no máximo 6 armas.

| Arma | Dano | Recarga (s) |
|---|---|---|
| Orbe Arcano | 10 | 0,9 |
| Lâminas Orbitais | 8 | contínua |
| Aura Sagrada | 5 | 0,6 |
| Facas | 7 | 0,6 |
| Raio Celeste | 14 | 1,8 |
| Bumerangue | 9 | 1,6 |
| Frasco Venenoso | 4 | 2,5 |
| Chicote | 13 | 1,3 |
| Nova Gélida | 8 | 4,0 |
| Bombas | 30 | 3,0 |

### XP e level up

XP para o próximo nível: `5 + 8 × (nível − 1)`. São 333 XP até o nível 10 e 1.463 até o nível 20. Cada nível oferece 3 opções entre armas e 15 atributos.

| Upgrade | Efeito por nível | Máx. |
|---|---|---|
| Coração Robusto | +20 vida | 5 |
| Botas Leves | +10% velocidade | 5 |
| Força Bruta | +12% dano | 5 |
| Mente Ágil | −8% recarga | 5 |
| Expansão | +12% área | 5 |
| Ímã | +30% alcance de coleta | 5 |
| Regeneração | +0,5 vida/s | 5 |
| Duplicação | +1 projétil | 2 |
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
| Estação | 30 madeira e 20 pedra | 120 |

Anel com 12 pontos de muralha e 4 de portão, em raio 5,5. Área livre de recursos em raio 7.

- **Reparo:** 1 material a cada 10 de vida faltando, arredondado para cima. Muro usa pedra; as demais, madeira.
- **Dano de monstros em estruturas:** 50% do dano de contato, a cada 2 s, por monstro.
- **Alvo:** o jogador a até 7 unidades; senão, a fogueira.
- **Jogador a mais de 15 unidades da fogueira:** metade dos monstros nasce a 16 unidades dela; o resto caça o jogador.

### Recursos, ouro e baús

- **Coleta:** 1,1 s segurando E, alcance 1,9. Madeira rende 5, pedra 3. Pontos ricos (1 em 8) rendem o dobro.
- **Distribuição:** células de 6,5 unidades; 25% têm árvore, 30% têm pedra, 45% ficam vazias. Tudo renova ao amanhecer.
- **Drops por inimigo:** ouro 20% (1–2, com +20% por noite após a primeira), cura 1%, ímã 0,4%.
- **Baús:** 3 por run, a 9; 12,5 e 16 unidades do centro do acampamento. Cada um oferece 3 recompensas de força ×1, ×2 e ×3, conforme o baú, e custa 1 chave.
- **Chaves:** uma garantida a 10 unidades do acampamento; demais em 20% das células de 14 unidades, somente enquanto faltarem chaves para os baús fechados.

| Recompensa | Por ponto de força |
|---|---|
| Coração de âmbar | +15 vida máxima e cura |
| Pólvora solar | +10% dano |
| Escamas do guardião | +1 armadura |
| Engrenagem de vento | −5% recarga |
| Bússola magnética | +1 alcance de coleta |
| Musgo da aurora | +0,3 vida/s |
| Moeda da fortuna | +15% ouro |

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
| 05/11/2026 | Dano em estruturas | 100% a cada 1 s → 50% a cada 2 s | Em teste automático, 15 Slimes apagariam a fogueira em segundos. Com anel de cercas e sem jogador, ela resistiu 75 s na Noite 1 |
| 05/11/2026 | Distância do primeiro baú | 5 → 9 | Abrir espaço para o acampamento |
| 05/11/2026 | Distância da primeira chave | 7 → 10 | Abrir espaço para o acampamento |
