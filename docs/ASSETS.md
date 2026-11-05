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
| `player.png` | Jogador animado em 8 direções, 192 quadros de 32 px | Aseprite (`player.ase`) | Pedro Nadotti (autoral) |
| `character_women.png` | Sem uso no jogo; só é importada pelo gerador | Aseprite (`basic_sprites_character_women.ase`) | **A confirmar** |
| `tree.png` | Árvore coletável | Aseprite (`tree_topdown.ase`) | **A confirmar** |
| `bush_a.png`, `bush_b.png` | Decoração | Aseprite (`bush_*_topdown.ase`) | **A confirmar** |
| `rock_1_small` a `rock_5_cluster.png` | Rochas coletáveis | Aseprite (`rock_*.ase`) | **A confirmar** |
| `Resources/Camp/heart.png` | Fogueira animada, 16 quadros de 32 px | Aseprite (`campfire.ase`) | **A confirmar** |
| `Resources/Chests/chest.png` | Baú animado, 20 quadros de 32 px | Aseprite (`chest.ase`) | **A confirmar** |

### Placeholders por substituir

| Item | Hoje | Destino |
|---|---|---|
| Inimigos (Slime, Bat, Golem) | Formas geradas e coloridas | Sprites finais animados |
| Armas e projéteis | Formas geradas | Sprites finais ou efeitos |
| XP, ouro, cura, ímã | Formas geradas | Sprites finais |
| Chave | Pixel art gerada por código em `ChestArt` | Avaliar se mantém |
| Cerca, muro, portão, estações e marca de ponto | Gerados em `CampArt` | Sprites em `Assets/Art/Resources/Camp/` |
| Chão | Textura gerada em `Assets/Generated/Sprites` | Sprite ou tileset final |
| SFX | Sons 8-bit de `SfxGenerator` | `AudioClip` final no `AudioManager` |
| Música | **Nenhuma** (`music` vazio em `Game.unity`) | Trilha de dia e de noite |
| Fonte | LegacyRuntime | Fonte com identidade do jogo |

## Disponível, sem uso no jogo

`FREE_Adventurer 2D Pixel Art`, em `jogos/jogo assets/`. Licença do pacote: uso livre em jogos pessoais ou comerciais e modificação permitida; não pode ser revendido como asset nem virar NFT; crédito não obrigatório, mas apreciado. O arquivo de licença não informa o autor: se for usado, localize a página original para creditar.

## Créditos ao vivo

Mantenha esta lista igual à seção Créditos do `README.md`.

- Fonte: LegacyRuntime, fonte padrão do Unity.
- Código-base do protótipo: gerado com auxílio do Claude (Anthropic) e adaptado pela equipe.
