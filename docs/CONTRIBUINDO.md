# Contribuindo

Projeto em equipe de 4 pessoas para a disciplina de Jogos Digitais do Insper, 2026.2.

## Equipe e responsabilidades

As responsabilidades ainda precisam ser confirmadas pela equipe. A coluna de histórico mostra o que está registrado no git, não uma divisão oficial.

| Integrante | GitHub | Responsabilidade | Histórico no repositório |
|---|---|---|---|
| Pedro Nadotti | Nadotti | **A definir** | Sprites do Aseprite e personagem animado; decoração; revisão e merge de PRs |
| Felipe Camelo | felipecameloss | **A definir** | Base do protótipo; dash e armas; dia e noite; ouro |
| Gustavo (nome completo a confirmar) | gustavolafz | **A definir** | Coleta diurna |
| Carlos (nome a confirmar) | carloslifranco | **A definir** | Sem commits até agora |

Toda frente da `RUBRICA.md` precisa de um responsável:

| Frente | Responsável |
|---|---|
| Conceito, narrativa e GDD no Milanote | **A definir** |
| Mecânicas e progressão | **A definir** |
| Balanceamento e testes | **A definir** |
| UI, HUD e menus | **A definir** |
| Arte e efeitos visuais | **A definir** |
| Som e música | **A definir** |
| Build WebGL, itch.io e Blackboard | **A definir** |

## Decisões de design

- O jogo segue `GDD.md` e `ESCOPO.md`. **Mudança de mecânica ou conteúdo é combinada com a equipe antes de ser implementada.**
- Mudou o design? Atualize `GDD.md` e o Milanote na mesma entrega.

## Fluxo de trabalho

1. Crie uma branch a partir de `main`: `feat/<assunto>` ou `fix/<assunto>`.
2. Faça commits pequenos, com mensagem em português e no imperativo ("Adiciona coleta de recursos").
3. Abra um Pull Request para `main`, explicando o que mudou e como testar.
4. Outra pessoa revisa e testa no Unity antes do merge.
5. Antes do merge, atualize os documentos afetados.

## Cuidados com o Unity

- Use a versão **6000.6.5f1**.
- Sempre versione os arquivos `.meta` junto com seus assets.
- Cenas (`.unity`) geram conflitos difíceis de resolver: combine quem está editando cada cena antes de começar.
- O menu **Survivors > Gerar sprites, prefabs e cenas** sobrescreve as cenas e `Assets/Generated`. Avise a equipe antes de usar. Veja `ARQUITETURA.md`.

## Antes de pedir merge

- [ ] Compila sem erros no Unity.
- [ ] Testado em Play, incluindo pausa, game over e reinício se a mudança os afetar.
- [ ] Sem texto, tecla ou função de teste acessível no build.
- [ ] `BALANCEAMENTO.md` atualizado se algum valor mudou.
- [ ] `ASSETS.md` e créditos do `README.md` atualizados se entrou asset novo.
- [ ] `GDD.md` atualizado se o design mudou.
