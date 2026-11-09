# CLAUDE.md

Jogo 2D em Unity 6 (`6000.6.5f1`), sobrevivência top-down medieval com defesa de acampamento, para o Projeto 2D de Jogos Digitais do Insper 2026.2. Equipe de 4 pessoas. Alvo: WebGL no itch.io. **Entrega: 10/11/2026 às 23:50.**

## Documentos do projeto

Consulte estes documentos antes de trabalhar e siga o que definem:

| Documento | Use para |
|---|---|
| `docs/GDD.md` | Saber o que o jogo é e como deve funcionar. Autoridade sobre design. |
| `docs/ESCOPO.md` | Saber o que entra, o que está fora e a prioridade. |
| `docs/RUBRICA.md` | Critérios de avaliação e entregáveis. Toda tarefa deve servir a uma frente. |
| `docs/BALANCEAMENTO.md` | Metas e valores de jogo. |
| `docs/ARQUITETURA.md` | Estrutura do código, convenções e como estender. |
| `docs/ASSETS.md` | Inventário, origem e licenças. |
| `docs/CONTRIBUINDO.md` | Equipe, responsabilidades e fluxo de trabalho. |

## Regras

- **Só implemente o que foi pedido ou está no GDD e no ESCOPO.** Não adicione mecânicas ou conteúdo por iniciativa própria; sugira primeiro. Funcionalidades fora do combinado já precisaram ser removidas deste projeto.
- **Não reintroduza** o que `docs/ESCOPO.md` lista como removido.
- **Se o pedido conflitar com um documento, aponte o conflito** antes de seguir.
- **Mantenha os documentos em dia, na mesma alteração:**
  - design mudou → `GDD.md`, incluindo o histórico de mudanças, e lembre que o Milanote precisa acompanhar;
  - valor de jogo mudou → `BALANCEAMENTO.md`;
  - estrutura de código mudou → `ARQUITETURA.md`;
  - asset entrou → `ASSETS.md` e créditos do `README.md`;
  - controles ou modo de jogar mudaram → `README.md`.
- Ao concluir algo relevante, diga qual frente da rubrica avançou e o que ainda falta nela.

## Trabalhando no Unity

- Não há testes automatizados de Play Mode: o usuário testa no Editor. Não diga que algo funciona em jogo sem esse teste; informe o que verificar.
- Não edite cenas (`.unity`) ou prefabs manualmente sem necessidade. Sempre inclua os `.meta`.
- O menu **Survivors > Gerar sprites, prefabs e cenas** sobrescreve as cenas e `Assets/Generated`. Se a mudança depender dele, avise.
- O alvo é WebGL: evite threads, acesso a arquivos e recursos indisponíveis no navegador.
- Todo input passa por `GameInput`; todo estado de run passa por `GameManager`.
- Textos de jogo, comentários e documentos em português.
