# Assets

Inventário dos assets do jogo. A rubrica exige referências e créditos no `README.md`: ao adicionar um asset, registre aqui e atualize os créditos de lá.

## Regras

- Registre origem, autor e licença **antes** de usar um asset de terceiros.
- Use apenas licenças que permitam uso no projeto e publicação no itch.io.
- Pixel art do projeto usa 24 pixels por unidade.
- Arte final fica em `Assets/Art/`; `Assets/Generated/` é sobrescrita pelo gerador (ver `ARQUITETURA.md`).
- Os arquivos-fonte `.ase` ficam fora do repositório, em `jogos/jogo assets/`.

## Inventário

### Sprites finais (`Assets/Art/Sprites/`)

| Arquivo | Uso | Origem | Autoria e licença |
|---|---|---|---|
| `player.png` | Jogador animado em 8 direções, 192 quadros de 32 px; tingido, também é o ferreiro (NPC) | Aseprite (`player.ase`) | Pedro Nadotti (autoral) |
| `Resources/Characters/character_women.png` | Ferreira (NPC), 192 quadros de 32 px no mesmo formato do jogador | Aseprite (`basic_sprites_character_women.ase`) | Equipe, com auxílio do Claude (Anthropic) (autoral) |
| `tree.png` | Árvore coletável | Aseprite (`tree_topdown.ase`) | Equipe, com auxílio do Claude (Anthropic) (autoral) |
| `bush_a.png`, `bush_b.png` | Decoração | Aseprite (`bush_*_topdown.ase`) | Equipe, com auxílio do Claude (Anthropic) (autoral) |
| `rock_1_small` a `rock_5_cluster.png` | Rochas coletáveis | Aseprite (`rock_*.ase`) | Equipe, com auxílio do Claude (Anthropic) (autoral) |
| `Resources/Camp/heart.png` | Fogueira animada, 16 quadros de 32 px | Aseprite (`campfire.ase`) | Equipe, com auxílio do Claude (Anthropic) (autoral) |
| `Resources/Camp/fence.png`, `wall.png` | Cerca e muro do anel: 13 peças de 48 × 64 (retas, diagonais, cantos e poste) | Aseprite (`fence.ase`, `wall.ase`) | Pedro Nadotti (autoral) |
| `Resources/Camp/gate_wood.png`, `gate_stone.png` | Portões de madeira e de pedra abrindo nos 4 sentidos, 24 quadros de 48 × 64 | Aseprite (`gate_wood.ase`, `gate_stone.ase`) | Pedro Nadotti (autoral) |
| `Resources/Camp/station_weapon_npc.png`, `station_armor_npc.png` | Ferraria e armaria com o NPC trabalhando, 8 quadros de 64 × 68 | Aseprite (`station_*_npc.ase`) | Pedro Nadotti (autoral) |
| `Resources/Camp/station_weapon.png`, `station_armor.png` | Estações vazias e fechadas, enquanto o NPC está perdido | Aseprite (`station_weapons.ase`, `station_armor.ase`) | Pedro Nadotti (autoral) |
| `Resources/Fonts/PixelOperator.ttf`, `PixelOperator-Bold.ttf` | Fonte pixelada de toda a interface e dos números de dano. Importadas em *Hinted Raster* (sem suavização); use 32 ou 64 | [Pixel Operator](https://www.dafont.com/pixel-operator.font) | Jayvee Enaguas (HarvettFox96), CC0 1.0 (domínio público). Licença em `docs/licencas/PixelOperator-CC0.txt` |
| `Resources/Chests/chest.png` | Baú animado, 20 quadros de 32 px | Aseprite (`chest.ase`) | Equipe, com auxílio do Claude (Anthropic) (autoral) |
| `Resources/Enemies/slime.png`, `bat.png` | Slime (salto) e Morcego (voo), 8 quadros de 32 × 32 | Gerados por código (`tools/sprites-inimigos/gerar.py`) no estilo do `player.png` | Equipe, com auxílio do Claude (Anthropic) (autoral) |
| `Resources/Enemies/golem.png` | Golem (passo pesado), 8 quadros de 48 × 48 | Idem | Idem |
| `Resources/Enemies/golem_anciao.png` | Golem Ancião, chefe da Noite 4 (passo, runa pulsando), 8 quadros de 64 × 64, desenhado a 1,5× | Idem | Idem |
| `Resources/Enemies/rei_da_noite.png` | Rei da Noite, chefe da Noite 7 (flutuando, capa ao vento), 8 quadros de 80 × 80, desenhado a 1,5× | Idem | Idem |
| `Resources/Weapons/icone_besta.png`, `icone_espada`, `icone_facas`, `icone_machado`, `icone_bombas` | Ícones das 5 armas (baú e ferraria), 24 × 24 | Gerados por código (`tools/sprites-itens/gerar.py`) no estilo do `player.png` | Equipe, com auxílio do Claude (Anthropic) (autoral) |
| `Resources/Weapons/virote.png`, `faca.png`, `machado.png` | Projéteis da Besta, das Facas e do Machado, 16 × 16, apontando para a direita (o machado é girado pelo jogo) | Idem | Idem |
| `Resources/Weapons/bomba.png` | Bomba no chão com pavio piscando, 2 quadros de 16 × 16 | Idem | Idem |
| `Resources/Weapons/explosao.png` | Explosão, 7 quadros de 48 × 48 | Idem | Idem |
| `Resources/Weapons/golpe_espada.png` | Golpe em arco da Espada, de cima para baixo olhando para a direita, 5 quadros de 48 × 48 | Idem | Idem |
| `Resources/Gear/*.png` | Ícones dos 7 equipamentos dos baús (amuleto de carvalho, pedra de amolar, cota de malha, luvas de couro, alforje, ervas do curandeiro, moeda da fortuna), 24 × 24 | Idem | Idem |
| `Resources/Pickups/gema_xp.png`, `moeda.png`, `cura.png`, `ima.png`, `chave.png` | Drops no chão, 16 × 16 | Idem | Idem |
| `Resources/Hud/madeira.png`, `pedra.png` | Ícones de recurso no HUD e na loja, 16 × 16 | Idem | Idem |
| `Sprites/chao.png` | Ladrilho contínuo de grama, 48 × 48 (2 unidades, igual ao `tileSize` do `InfiniteBackground`), importado com *Repeat* | Idem | Idem |

Autoria confirmada por Pedro Nadotti em 09/11/2026: ferreira, árvore, arbustos, rochas, fogueira e baú foram feitos pela equipe com auxílio do Claude (Anthropic); jogador, cercas, muros, portões e estações são de Pedro Nadotti.

### Formas geradas (`Assets/Generated/Sprites/`)

Formas simples (`Blob`, `Circle`, `Cross`, `Diamond`, `Glow`, `Pixel`, `Ring`, `Square`) desenhadas por código em `SurvivorsSetup` a cada execução do gerador: corpo provisório dos inimigos sem folha, partículas, brilhos e as armas mágicas fora do escopo. Fazem parte do código-base (equipe, com auxílio do Claude).

As folhas de `Resources/Enemies/` são tiras de 8 quadros olhando para a direita (o jogo vira com `flipX`), com a sombra desenhada no chão. São importadas como sprite único (filtro *Point*, sem compressão, 24 px por unidade) e fatiadas por `EnemyArt`, que guarda o pivô de cada uma: o centro do corpo, a 12 px da base no Slime, 19 no Morcego, 23 no Golem, 29 no Golem Ancião e 40 no Rei da Noite. A fonte é o script `tools/sprites-inimigos/gerar.py` (Python com Pillow): mude cores ou formas lá e rode de novo, em vez de editar os PNGs.

Armas, equipamentos, drops, ícones do HUD e chão vêm de `tools/sprites-itens/gerar.py`, que usa o mesmo motor (`px.py`) e a paleta do baú e da ferraria. As tiras (`bomba`, `explosao`, `golpe_espada`) têm quadros quadrados lado a lado; o gerador de cenas as fatia (`ImportSheet`, que passa o `.meta` para *Multiple*), com o pivô do golpe na mão, a 12 px da esquerda. Os demais são sprite único: pickups, projéteis e machado vão para os prefabs pelo gerador, em escala 1; ícones de armas, equipamentos e recursos são carregados em jogo por `Resources`. Rodar o script de novo mantém os `.meta` existentes (e os GUIDs).

### Áudio (`Assets/Art/Resources/Audio/`)

Gerado pelo script `tools/sons/gerar.py` (Python com numpy e soundfile), por síntese: nenhum sample de terceiros. Para ajustar, mude o script e rode de novo. Para trocar por um arquivo de outra fonte, registre aqui origem e licença e salve com o mesmo nome. Os WAV são importados com as opções padrão do Unity, que os comprime no build.

| Arquivo | Uso | Autoria e licença |
|---|---|---|
| `musica_dia.wav` | Dia: alaúde em arpejos e flauta doce, ré maior/mixolídio, 80 BPM, loop de 48 s | Equipe, com auxílio do Claude (Anthropic) (autoral) |
| `musica_noite.wav` | Noite: ostinato grave, cordas em trêmulo e taikos, ré menor, 96 BPM, loop de 40 s | Idem |
| `musica_chefe.wav` | Noite de chefe, a partir do aparecimento: ostinato no frígio, tambores de guerra, metais e coro, 112 BPM, loop de 34 s | Idem |
| `sfx_passo_1` a `_4.wav` | Passo do jogador, sorteado a cada passada | Idem |
| `sfx_machado_1` a `_3.wav` | Golpes ao coletar madeira | Idem |
| `sfx_picareta_1` a `_3.wav` | Golpes ao coletar pedra | Idem |
| `sfx_chave.wav` | Chave coletada | Idem |

### Placeholders por substituir

| Item | Hoje | Destino |
|---|---|---|
| Armas mágicas antigas | Lâminas Orbitais, Aura, Raio, Frasco e Nova seguem com formas geradas, mas não entram no jogo | Nenhum: fora do escopo |
| Gemas de XP valiosas | A gema azul é tingida de verde (5+) ou vermelho (20+) e tende a ficar escura (conferir no Editor) | Variações de cor no `gerar.py`, se a equipe quiser |
| Marca de ponto de construção | Gerada em `CampArt` | `spot.png` em `Assets/Art/Resources/Camp/` |
| SFX de tiro, acerto, morte, coleta, nível, dano e seleção | Sons 8-bit de `SfxGenerator` | WAV em `Resources/Audio` com o nome que o `AudioManager` procura (ex.: `sfx_tiro.wav`) |
| Música do menu | Nenhuma | Opcional: `music` no `AudioManager` do menu |

## Disponível, sem uso no jogo

`FREE_Adventurer 2D Pixel Art`, em `jogos/jogo assets/`. Licença do pacote: uso livre em jogos pessoais ou comerciais e modificação permitida; não pode ser revendido como asset nem virar NFT; crédito não obrigatório, mas apreciado. O arquivo de licença não informa o autor: se for usado, localize a página original para creditar.

## Créditos ao vivo

Mantenha esta lista igual à seção Créditos do `README.md` (links a partir da raiz do repositório, com `/`, para funcionarem nos dois arquivos).

- Equipe: ver [`docs/CONTRIBUINDO.md`](/docs/CONTRIBUINDO.md).
- Código-base do protótipo: gerado com auxílio do Claude (Anthropic) e adaptado pela equipe.
- Fonte: Pixel Operator, de Jayvee Enaguas (HarvettFox96), CC0 1.0 ([licença](/docs/licencas/PixelOperator-CC0.txt)).
- Jogador, cercas, muros, portões e estações do acampamento: sprites feitos no Aseprite por Pedro Nadotti.
- Ferreira, árvore, arbustos, rochas, fogueira e baú: sprites feitos pela equipe com auxílio do Claude (Anthropic).
- Inimigos e chefes: sprites gerados por código com auxílio do Claude (Anthropic), no estilo do jogador de Pedro Nadotti.
- Armas, equipamentos, drops, ícones de recurso e chão: sprites gerados por código com auxílio do Claude (Anthropic), no mesmo estilo.
- Músicas (dia, noite e chefe) e efeitos de passo, machado, picareta e chave: compostos e sintetizados por código com auxílio do Claude (Anthropic), sem samples de terceiros (`tools/sons/gerar.py`).
- Demais sprites e sons: placeholders gerados por código.
