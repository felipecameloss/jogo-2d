# Página do itch.io

Texto e configuração da página do jogo na [jam do Projeto 2D](https://itch.io/jam/projeto-jogo-2d-insper-20262). Quando o jogo mudar, atualize este arquivo e a página juntos.

## Configuração

| Campo | Valor |
|---|---|
| Title | Brasa Viva |
| Short description | Defenda a última brasa do vale. De dia, colete e construa; de noite, sobreviva aos monstros. |
| Kind of project | HTML |
| Upload | `Builds/jogo-2d-webgl.zip`, marcado como **This file will be played in the browser** |
| Viewport | 960 × 600, com **Fullscreen button** ligado |
| Genre | Survival (ou Action) |
| Tags | survival, roguelike, top-down, pixel-art, base-building, medieval, singleplayer |
| Input | Keyboard (gamepad parcial) |
| Language | Portuguese (Brazil) |

Gere o build antes de enviar (passo a passo no `README.md`, seção "Build WebGL"). O build de 08/11/2026 que está em `Builds/` é antigo: não tem a história, a música, os chefes nem a arte nova.

## Imagens

O itch.io pede uma capa (630 × 500) e aceita capturas de tela. Sugestões, tiradas do build final:

1. **Capa:** a fogueira acesa à noite, com o acampamento murado e monstros chegando.
2. Um cartão da introdução, com o título *Brasa Viva*.
3. O dia: Bento coletando madeira, com as missões e o minimapa visíveis.
4. A noite: horda contra os muros, números de dano e partículas.
5. A luta contra o Golem Ancião (Noite 4) ou o Rei da Noite (Noite 7).
6. A ferraria aberta, com as armas à venda.

Um GIF curto de 5 a 10 segundos de uma noite de defesa vale mais que várias capturas.

## Texto da página

Copie o bloco abaixo para o campo **Description**.

---

**O fogo é a última coisa que a escuridão não consegue tocar.**

O vale de Alvorada vivia em paz até o Rei da Noite despertar sob as ruínas. Sua maldição dá vida à escuridão a cada anoitecer: o lodo dos pântanos rasteja, os morcegos deixam as cavernas e as pedras antigas se erguem como golens. A aldeia caiu em uma só noite, e os sobreviventes se perderam na floresta.

Bento, um jovem lenhador, salvou a última brasa da forja e acendeu uma fogueira numa clareira. Só o fogo resiste à maldição, e é atrás dele que os monstros vêm.

**Mantenha a brasa viva.**

### Como é o jogo

*Brasa Viva* é um jogo de sobrevivência top-down com defesa de base.

- **De dia** não há monstros, e o primeiro dia é mais longo para você se preparar. Corte árvores e quebre rochas, construa cercas, muros e portões em volta da fogueira, procure chaves e abra baús.
- **Resgate** o ferreiro e a ferreira, perdidos pela floresta. Cada um abre uma estação no acampamento: a **ferraria** ensina e melhora armas, e a **armaria** dá armadura e vida.
- **De noite** os monstros vêm atrás da fogueira e destroem o que estiver no caminho. Suas armas atacam sozinhas: posicione-se, use o dash e escolha melhorias a cada nível.
- Na **quarta noite** desperta o Golem Ancião, e na **sétima** o próprio Rei da Noite vem apagar a brasa.
- A run não tem fim: cada noite é mais difícil que a anterior. Quantas noites você aguenta?

Se você morrer, ou se a fogueira se apagar, a run termina.

### Controles

| Ação | Teclado | Controle |
|---|---|---|
| Mover | WASD / setas | Analógico |
| Dash (atravessa monstros) | Shift esquerdo | — |
| Coletar madeira e pedra (segurar) | E | X (Xbox) / quadrado (PS) |
| Construir, reforçar, reparar, resgatar, abrir baú e estação | F | Y (Xbox) / triângulo (PS) |
| Escolher upgrade ou compra | 1 a 5 ou clique | — |
| Pausar | Esc / P | Start |
| Avançar a introdução | Espaço ou clique | A (Xbox) / X (PS) |
| Pular a introdução ou o tutorial | Enter | Select |

### Dicas

- O primeiro dia tem um tutorial nas missões, no canto esquerdo. Ele mostra a direção do baú mais próximo e de quem falta resgatar.
- Feche o acampamento antes da primeira noite: monstros que não encontram parede vão direto para a fogueira.
- **F** também repara estruturas danificadas e a própria fogueira, a qualquer hora. O dano da noite não some ao amanhecer.
- Cancelar a janela de um baú não gasta a chave.
- Se o ferreiro ou a ferreira forem derrotados, voltam a se perder na floresta. Resgate-os de novo para reabrir a estação; o que você comprou continua.
- Clique na tela do jogo antes de jogar, para o navegador enviar o teclado para ele. Use o botão de tela cheia para ver melhor.

### Equipe

Projeto 2D da disciplina de Jogos Digitais do Insper, 2026.2.

- Pedro Nadotti
- Felipe Camelo
- Gustavo
- Carlos

### Créditos

- Fonte Pixel Operator, de Jayvee Enaguas (HarvettFox96), CC0.
- Sprites do jogador e do acampamento feitos no Aseprite pela equipe; demais sprites, música e efeitos sonoros feitos pela equipe com auxílio do Claude (Anthropic).
- Código-fonte e créditos completos: [GitHub](https://github.com/felipecameloss/jogo-2d)

---

## Antes de publicar

- [ ] Completar os nomes do Gustavo e do Carlos na seção Equipe
- [ ] Conferir o link do GitHub
- [ ] Gerar o build atual e testar no navegador, inclusive em tela cheia
- [ ] Subir capa e capturas
- [ ] Inscrever o jogo na jam (o upload sozinho não inscreve)
- [ ] Deixar a página como **Public**
