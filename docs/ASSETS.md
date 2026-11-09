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
| `Resources/Characters/character_women.png` | Ferreira (NPC), 192 quadros de 32 px no mesmo formato do jogador | Aseprite (`basic_sprites_character_women.ase`) | **A confirmar** |
| `tree.png` | Árvore coletável | Aseprite (`tree_topdown.ase`) | **A confirmar** |
| `bush_a.png`, `bush_b.png` | Decoração | Aseprite (`bush_*_topdown.ase`) | **A confirmar** |
| `rock_1_small` a `rock_5_cluster.png` | Rochas coletáveis | Aseprite (`rock_*.ase`) | **A confirmar** |
| `Resources/Camp/heart.png` | Fogueira animada, 16 quadros de 32 px | Aseprite (`campfire.ase`) | **A confirmar** |
| `Resources/Camp/fence.png`, `wall.png` | Cerca e muro do anel: 13 peças de 48 × 64 (retas, diagonais, cantos e poste) | Aseprite (`fence.ase`, `wall.ase`) | Pedro Nadotti (autoral) |
| `Resources/Camp/gate_wood.png`, `gate_stone.png` | Portões de madeira e de pedra abrindo nos 4 sentidos, 24 quadros de 48 × 64 | Aseprite (`gate_wood.ase`, `gate_stone.ase`) | Pedro Nadotti (autoral) |
| `Resources/Camp/station_weapon_npc.png`, `station_armor_npc.png` | Ferraria e armaria com o NPC trabalhando, 8 quadros de 64 × 68 | Aseprite (`station_*_npc.ase`) | Pedro Nadotti (autoral) |
| `Resources/Camp/station_weapon.png`, `station_armor.png` | Estações vazias e fechadas, enquanto o NPC está perdido | Aseprite (`station_weapons.ase`, `station_armor.ase`) | Pedro Nadotti (autoral) |
| `Resources/Fonts/PixelOperator.ttf`, `PixelOperator-Bold.ttf` | Fonte pixelada de toda a interface e dos números de dano. Importadas em *Hinted Raster* (sem suavização); use 32 ou 64 | [Pixel Operator](https://www.dafont.com/pixel-operator.font) | Jayvee Enaguas (HarvettFox96), CC0 1.0 (domínio público). Licença em `docs/licencas/PixelOperator-CC0.txt` |
| `Resources/Chests/chest.png` | Baú animado, 20 quadros de 32 px | Aseprite (`chest.ase`) | **A confirmar** |
| `Resources/Enemies/slime.png`, `bat.png` | Slime (salto) e Morcego (voo), 8 quadros de 32 × 32. **Ainda não ligados ao jogo** | Gerados por código (`tools/sprites-inimigos/gerar.py`) no estilo do `player.png` | Equipe, com auxílio do Claude (Anthropic) (autoral) |
| `Resources/Enemies/golem.png` | Golem (passo pesado), 8 quadros de 48 × 48. **Ainda não ligado ao jogo** | Idem | Idem |
| `Resources/Enemies/golem_anciao.png` | Golem Ancião, chefe da Noite 4 (passo, runa pulsando), 8 quadros de 64 × 64. **Ainda não ligado ao jogo** | Idem | Idem |
| `Resources/Enemies/rei_da_noite.png` | Rei da Noite, chefe da Noite 7 (flutuando, capa ao vento), 8 quadros de 80 × 80. **Ainda não ligado ao jogo** | Idem | Idem |

As folhas de `Resources/Enemies/` são tiras de 8 quadros olhando para a direita (o jogo vira com `flipX`), com a sombra desenhada no chão. O centro da sombra fica a 4,5 px da base do quadro (5,5 px no Rei da Noite): use esse ponto como pivô. A fonte é o script `tools/sprites-inimigos/gerar.py` (Python com Pillow): mude cores ou formas lá e rode de novo, em vez de editar os PNGs.

### Placeholders por substituir

| Item | Hoje | Destino |
|---|---|---|
| Inimigos (Slime, Bat, Golem) e chefes | Formas geradas e coloridas; o chefe é o Golem ampliado e tingido | Sprites finais prontos em `Resources/Enemies/`; falta ligar no código |
| Armas e projéteis | Formas geradas | Sprites finais ou efeitos |
| XP, ouro, cura, ímã | Formas geradas | Sprites finais |
| Chave | Pixel art gerada por código em `ChestArt` | Avaliar se mantém |
| Marca de ponto de construção | Gerada em `CampArt` | `spot.png` em `Assets/Art/Resources/Camp/` |
| Chão | Textura gerada em `Assets/Generated/Sprites` | Sprite ou tileset final |
| SFX | Sons 8-bit de `SfxGenerator` | `AudioClip` final no `AudioManager` |
| Música | **Nenhuma** (`music` vazio em `Game.unity`) | Trilha de dia e de noite |

## Disponível, sem uso no jogo

`FREE_Adventurer 2D Pixel Art`, em `jogos/jogo assets/`. Licença do pacote: uso livre em jogos pessoais ou comerciais e modificação permitida; não pode ser revendido como asset nem virar NFT; crédito não obrigatório, mas apreciado. O arquivo de licença não informa o autor: se for usado, localize a página original para creditar.

## Créditos ao vivo

Mantenha esta lista igual à seção Créditos do `README.md`.

- Fonte: Pixel Operator, de Jayvee Enaguas (HarvettFox96), CC0 1.0 (`docs/licencas/PixelOperator-CC0.txt`).
- Código-base do protótipo: gerado com auxílio do Claude (Anthropic) e adaptado pela equipe.
- Inimigos e chefes: sprites gerados por código com auxílio do Claude (Anthropic), no estilo do jogador de Pedro Nadotti.
