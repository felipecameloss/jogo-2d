# Rubrica do Projeto 2D — Insper 2026.2

Fonte: enunciado oficial da disciplina. Este arquivo é a referência de avaliação do projeto: toda decisão de escopo e toda tarefa devem apontar para uma ou mais frentes abaixo.

## Entrega

- **Tipo:** multi developer (4 pessoas). Jogo 2D novo, original, completo e bem acabado.
- **Prazo rígido:** 10/11/2026 às 23:50. Há punição por dia de atraso.
- **Acompanhamento:** semanal, com Sprints definidas em aula.
- **Conteúdo:** não pode ser ofensivo; ideias de risco devem ser discutidas com o professor.

| Onde | O que entregar |
|---|---|
| Blackboard | Informar o projeto de forma completa. Sem isso, **o trabalho é considerado não entregue**. |
| [Itch.io (jam)](https://itch.io/jam/projeto-jogo-2d-insper-20262) | Build **WebGL** na jam, com descrição, como jogar (teclas, segredos) e imagens e/ou vídeos. |
| GitHub | Código-fonte, com referências e créditos dos assets no `README.md`. |
| Milanote | GDD atualizado, como documento vivo que registra mudanças de trajetória. O jogo precisa poder ser entendido pelo GDD. |

## Cálculo da nota

Conceitos por frente: **A** 8,5–10 · **B** 6–8,5 · **C** 4–6 · **D** 0–4.

Cada grupo tem 2 rubricas de maior importância (peso 2) e 2 de menor importância (desconsideradas), definidas conforme o tipo de jogo e validadas com o professor na planilha de acompanhamento.

| Nota final | Condições |
|---|---|
| **A+ (10)** | 7 rubricas A, incluindo as de maior importância; no máximo 1 C; nenhum D. |
| **A (8,5–9,5)** | 5 rubricas A, incluindo as de maior importância; no máximo 2 C; no máximo 1 D. |
| **Demais casos** | Média ponderada: `(2 × importantes + 8 rubricas) / 12`, com nota máxima 8,5. |

As rubricas desconsideradas não contam nessas condições. **Gerenciamento de Estados e Erros** e **GDD** não podem ser excluídas nem dobradas.

### Pesos do grupo

| Papel | Rubricas | Status |
|---|---|---|
| Maior importância (peso 2) | — | **Pendente: validar com o professor** |
| Menor importância (excluída) | — | **Pendente: validar com o professor** |

## Situação atual do projeto

Avaliação revista a partir do código em 10/11/2026, dia da entrega, ainda sem teste de jogo registrado. Serve para priorizar trabalho; não é uma previsão de nota. Atualize a cada entrega importante.

| # | Frente | Situação observada | Lacuna principal |
|---|---|---|---|
| 1 | Blueprint | Nome (*Brasa Viva*), protagonista, origem dos monstros, chefes e introdução em seis cartões; arte própria de jogador, acampamento, monstros, armas e itens | Levar a história ao Milanote e à página do itch.io |
| 2 | Mechanics | Combate automático com 5 armas, level up, dash, coleta, construção e reforço, reparo, resgate, ferraria e armaria, baús, chefes nas Noites 4 e 7; ouro paga as estações | Confirmar preços em teste de jogo |
| 3 | Feedback | Flash de dano, números, partículas, shake, estruturas que escurecem com o dano, barra da fogueira, SFX | 7 efeitos ainda 8-bit; sem som de construir, reparar e resgatar |
| 4 | UI/HUD e menus | Fonte pixelada, painéis opacos, missões com distância e direção, minimapa, opções com volume e controles | Testar leitura e cliques no build WebGL |
| 5 | Engajamento | Dia com objetivos (coletar, construir, resgatar, baús) e noite de defesa da base; história e chefes dão marcos | Confirmar ritmo em teste de jogo |
| 6 | Level design | Tutorial no primeiro dia; dificuldade cresce sem limite após a Noite 4; chefes nas Noites 4 e 7 | Confirmar a curva em teste de jogo |
| 7 | Som e música | Música de dia, noite e chefe com fade; passos, coleta e chave com som próprio | Trocar os 7 efeitos 8-bit restantes; sons de construção, reparo e resgate |
| 8 | VFX | Partículas, shake, noite com luz em volta do jogador, golpes e explosões animados | Coesão e polimento |
| 9 | Replayability | Armas, equipamentos e upgrades sorteados; run infinita | Sem recorde nem progresso entre runs |
| 10 | Balanceamento | Valores documentados; uma sessão de teste registrada (10/11/2026), que alongou o primeiro dia | Registrar mais sessões, de preferência com quem não é da equipe |
| 11 | Estados e erros | Pausa, game over e reinício; baú não gasta chave ao cancelar; residentes derrotados voltam a ficar perdidos | Testar saídas e casos-limite no build WebGL |
| 12 | GDD | `GDD.md` organizado em DDE e com histórico até 09/11/2026 | Publicar no Milanote |

## Critérios por frente

### 1. Blueprint: Conceito e Narrativa

Avalia a força e a originalidade do conceito central, incluindo ambientação, personagens e a coerência e o engajamento da narrativa ou lore.

- **A:** alto nível de cuidado e criatividade; coeso, com elementos originais bem explorados e profundidade notável nos detalhes, enriquecendo a experiência dentro do escopo.
- **B:** bem elaborado, com boa coesão, elementos originais e nível considerável de detalhe e criatividade para o escopo.
- **C:** funcional, mas carece de profundidade, originalidade ou coesão; ideias básicas com desenvolvimento limitado.
- **D:** genérico, pouco desenvolvido, inconsistente ou clichê, sem elementos que se destaquem.

### 2. Mechanics: Mecânicas do jogo

Avalia design e implementação das regras e sistemas: ações principais, progressão, interação com mundo e inimigos, e como tudo se combina na jogabilidade central.

- **A:** alto grau de polimento; coesas, com profundidade ou variação interessante, combinadas de forma inteligente numa jogabilidade sólida e envolvente.
- **B:** bem implementadas, estáveis, interessantes, com bom nível de desafio e variação.
- **C:** funcionais, mas com pouca variação ou profundidade; podem ficar repetitivas; pequenos bugs ou falta de polimento.
- **D:** muito simples, pouco funcionais, com bugs significativos ou sem desafios claros.

### 3. Feedback e Clareza de Interação (Interface)

Avalia a clareza com que o jogo comunica informações por elementos visuais e sonoros integrados ao mundo, reduzindo a dependência de UI/HUD: o que é interativo, feedback de ações (ataques, dano) e estado de inimigos e objetos.

- **A:** feedback visual e sonoro integrado muito claro e eficaz na maioria das situações; o jogador entende interações, estados e resultados por pistas no ambiente.
- **B:** comunica a maioria das interações e estados importantes por feedback integrado ao mundo (animações de dano, sons de interação, mudanças visuais em objetos).
- **C:** feedback básico, mas inconsistente, pouco claro ou limitado a poucas situações.
- **D:** mínimo, confuso ou ausente; depende quase totalmente de UI/HUD.

### 4. Interface de Usuário (UI/HUD) e Menus

Avalia qualidade, funcionalidade e estética do HUD, menus (principal, pausa, opções etc.) e outros elementos informativos: clareza, usabilidade, apelo visual e recursos esperados.

- **A:** muito bem desenhados: claros, intuitivos, responsivos, com estética coesa e agradável; funcionalidades relevantes organizadas e polidas, com assets de boa qualidade.
- **B:** claros, fáceis de usar, visualmente agradáveis, com a maioria das funcionalidades esperadas e boa organização.
- **C:** funcionais, mas poderiam ser mais intuitivos ou bem acabados; básicos, com pouco polimento.
- **D:** confusos, difíceis de usar, visualmente pobres ou sem funções essenciais (sair, controles básicos); assets ruins ou inconsistentes.

### 5. Engajamento e Imersão

Avalia a capacidade de capturar e manter o interesse: como desafio, narrativa, feedback, atmosfera e ritmo criam flow, motivam a continuidade e conectam o jogador.

- **A:** experiência muito envolvente e coesa, com desafios bem ajustados, ritmo adequado, feedback claro e objetivos motivadores trabalhando juntos.
- **B:** consistentemente engajador; bom equilíbrio entre desafio e habilidade, objetivos claros e feedback que funciona.
- **C:** momentos de engajamento, mas inconsistente; partes tediosas, confusas ou mal balanceadas; imersão fraca ou quebrada.
- **D:** falha em prender a atenção; confuso, frustrante, monótono ou sem objetivos claros.

### 6. Level Design e Progressão

Avalia estrutura e fluxo das áreas: como introduzem mecânicas, ensinam a jogar, apresentam desafios crescentes e gerenciam ritmo e dificuldade.

- **A:** muito bem executado; guia com eficácia, introduz complexidade de forma gradual e clara, com desafios variados e dificuldade bem ajustada.
- **B:** bem pensado; curva de aprendizado razoável, mecânicas compreensíveis, desafios crescentes e variados, ritmo majoritariamente bem gerenciado.
- **C:** funcional, mas progressão irregular, lenta ou pouco interessante; desafios repetitivos ou mal posicionados; ritmo inconsistente.
- **D:** confuso, muito repetitivo ou mal balanceado; falha em guiar; progressão inexistente ou frustrante.

### 7. Design de Som e Música

Avalia qualidade técnica e adesão de SFX e música à ambientação: adequação ao tema, atmosfera, feedback sonoro, variedade e polimento.

- **A:** alto nível de cuidado; SFX e música de boa qualidade, bem integrados, reforçam a atmosfera e dão feedback claro; implementação polida.
- **B:** boa qualidade, bem escolhidos ou criados, encaixam no tema; implementação técnica correta.
- **C:** funcional, mas genérico, mediano ou inconsistente; contribuição limitada.
- **D:** baixa qualidade, ausente em momentos-chave, repetitivo, mal implementado ou inadequado.

### 8. Efeitos Visuais e Polimento Gráfico

Avalia elementos além dos sprites básicos: partículas, iluminação, shaders, pós-processamento e outras técnicas que dão vida, atmosfera e feedback.

- **A:** usados de forma muito eficaz e integrada; contribuem significativamente para atmosfera, feedback e polimento.
- **B:** bem utilizados, com camada visível de polimento, coerentes com o estilo.
- **C:** básicos, limitados ou genéricos; contribuem pouco.
- **D:** inexistentes, muito básicos, mal implementados ou destoantes; cena estática ou inacabada.

### 9. Replayability (Rejogabilidade)

- **A:** excelente, com ampla gama de opções, caminhos, desafios e incentivos que mantêm o jogador interessado em explorar todas as possibilidades.
- **B:** boa, com várias opções, caminhos, desafios ou incentivos que motivam a revisitar e tentar abordagens diferentes.
- **C:** razoável, com alguma variação, mas faltam profundidade ou incentivos significativos.
- **D:** muito limitada, sem variação, desafios adicionais ou incentivos.

### 10. Balanceamento

Avalia o ajuste fino para uma experiência justa, desafiadora na medida certa e recompensadora: economia, curva de dificuldade, poder de habilidades e itens, inimigos e progressão.

- **A:** muito cuidado; dificuldade bem ajustada, sistemas justos e interligados, desafiador na medida certa e recompensador.
- **B:** bom na maior parte; dificuldade geralmente justa e progressiva, recursos e habilidades úteis.
- **C:** mediano; há desequilíbrios notáveis (partes muito fáceis ou difíceis, itens inúteis ou superpoderosos).
- **D:** claramente inadequado; injusto ou trivial; recursos, inimigos ou progressão desajustados.

### 11. Gerenciamento de Estados e Erros *(não pode ser excluída nem dobrada)*

Avalia robustez em situações inesperadas: prevenção de dead ends, clareza e justiça na recuperação após falha, tratamento de erros que interrompam a experiência.

- **A:** muito robusto; falhas tratadas de forma clara e justa, dead ends prevenidos ativamente e experiência raramente interrompida.
- **B:** previne a maioria dos dead ends; recuperação justa e clara; bugs que impedem progresso são raros.
- **C:** trata o mínimo, de forma básica ou inconveniente; alguns bugs menores.
- **D:** dead ends frequentes; recuperação punitiva, confusa ou inexistente; bugs que impedem progresso.

### 12. GDD, utilizando o DDE *(não pode ser excluída nem dobrada)*

Avalia qualidade, organização e completude do documento: se foi útil durante o desenvolvimento, reflete o jogo final e documenta decisões, mecânicas, sistemas e conteúdo.

- **A:** muito bem escrito, completo para o escopo, organizado e atualizado; reflete o produto final e mostra ter sido útil.
- **B:** detalhado o suficiente, organizado e razoavelmente atualizado; bom guia de referência.
- **C:** razoável, com lacunas significativas, informações vagas ou parcialmente desatualizado.
- **D:** muito básico, incompleto, desatualizado, desorganizado ou não reflete o jogo.
