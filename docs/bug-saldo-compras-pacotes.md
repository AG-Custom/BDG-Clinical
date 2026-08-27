# Bug: inconsistência no saldo das compras de pacotes

## 1. Resumo

O módulo de compras de pacotes pode apresentar os seguintes comportamentos incorretos:

- uma compra recém-registrada aparecer como **Sem saldo**, mesmo sem ter sido utilizada;
- uma compra cujo produto principal já foi totalmente consumido aparecer como **Saldo baixo**;
- o ajuste de saldo de uma compra interferir em compras futuras do mesmo pacote;
- a edição do pacote comercial alterar as quantidades de compras antigas.

A causa principal é que a compra não possui uma cópia própria dos produtos e das quantidades contratadas. Atualmente, todas as compras continuam utilizando os itens do pacote comercial compartilhado.

## 2. Severidade e impacto

**Severidade sugerida:** alta.

O problema afeta uma informação operacional e comercial importante: quanto o paciente ainda tem direito de utilizar.

Possíveis consequências:

- impedir uma aplicação que ainda deveria estar disponível;
- permitir interpretação incorreta do saldo do paciente;
- apresentar saldo incorreto logo após uma nova compra;
- alterar retroativamente contratos anteriores ao editar o pacote comercial;
- exigir correções manuais e dificultar a auditoria;
- gerar divergências entre o atendimento realizado e o saldo apresentado.

O saldo da compra é diferente do estoque físico da clínica. Este documento trata exclusivamente do direito de consumo adquirido pelo paciente.

## 3. Comportamento observado

### Cenário A — compra nova sem saldo

1. Um pacote comercial possui 5 unidades de um produto.
2. Uma compra antiga desse pacote recebe um ajuste manual indicando 5 unidades utilizadas.
3. Posteriormente, o mesmo pacote é vendido novamente para outro paciente.
4. A nova compra pode herdar as 5 unidades utilizadas do pacote anterior.
5. A tela apresenta 0 unidades restantes, embora a nova compra ainda não tenha aplicações.

### Cenário B — produto zerado apresentado como saldo baixo

Considere um pacote com dois produtos:

| Produto | Contratado | Utilizado | Restante |
|---|---:|---:|---:|
| GH | 5 un | 5 un | 0 un |
| Tirzepatida | 25 mg | 17,5 mg | 7,5 mg |

O frontend classifica esse pacote como **Saldo baixo**, porque um produto está zerado e outro ainda tem saldo.

Para o cliente que está considerando o GH como produto principal, a compra parece totalmente consumida. Entretanto, para a regra atual, o pacote ainda possui saldo parcial.

## 4. Causa raiz no backend

Hoje existem as seguintes estruturas principais:

```text
pacote
  └── item_pacote

compra_paciente
  └── referencia pacote_id
```

Não existe uma coleção de itens pertencente exclusivamente à compra.

O cálculo atual é equivalente a:

```text
quantidade utilizada da compra =
    quantidade_utilizada_base do item_pacote
    + soma das aplicações realizadas e não canceladas da compra

quantidade restante =
    quantidade_total do item_pacote
    - quantidade utilizada da compra
```

Os campos `quantidade_total` e `quantidade_utilizada_base` pertencem ao `item_pacote`, que é compartilhado por todas as compras vinculadas ao mesmo pacote.

### Consequência do ajuste manual

Ao ajustar o saldo de uma compra, a implementação atual modifica o item do pacote comercial. Existe uma proteção que permite o ajuste somente quando o pacote possui uma única compra naquele momento, mas isso não protege compras criadas posteriormente.

Exemplo:

```text
1. Pacote possui somente a Compra A.
2. Compra A é ajustada e altera o item_pacote.
3. Depois do ajuste, é criada a Compra B.
4. Compra B passa a utilizar o mesmo item_pacote alterado.
```

### Consequência da edição do pacote

Ao editar um pacote, seus itens são substituídos. Como as compras antigas consultam os itens atuais do pacote, uma alteração comercial pode modificar retroativamente:

- produtos contratados;
- quantidades contratadas;
- unidades de medida;
- cálculo do saldo das compras existentes.

## 5. Causa da classificação visual

O frontend classifica cada produto da seguinte maneira:

```text
restante <= 0                         => sem saldo
restante / contratado < 30%           => saldo baixo
demais casos                          => com saldo
```

Depois agrega os produtos do pacote:

```text
todos sem saldo                       => Sem saldo
algum baixo ou sem saldo              => Saldo baixo
todos com saldo                       => Com saldo
```

Essa agregação mistura duas situações diferentes:

- saldo realmente baixo;
- pacote parcialmente consumido, com um produto zerado e outro disponível.

## 6. Solução definitiva recomendada

Cada compra deve possuir uma fotografia dos itens contratados no momento da venda.

### Nova estrutura

Criar a tabela `item_compra_paciente`:

```text
item_compra_paciente
- id
- compra_paciente_id
- produto_id
- quantidade_contratada
- quantidade_utilizada_base
- unidade_medida
- criado_em
- atualizado_em
```

Relacionamentos:

```text
compra_paciente 1 ─── N item_compra_paciente
produto         1 ─── N item_compra_paciente
```

Restrições recomendadas:

- chave estrangeira de `compra_paciente_id` para `compra_paciente`;
- chave estrangeira de `produto_id` para `produto`;
- índice único em `(compra_paciente_id, produto_id)`;
- precisão decimal `(18,4)` para as quantidades;
- `quantidade_contratada > 0`;
- `quantidade_utilizada_base >= 0`.

### Novo fluxo de criação

Ao registrar uma compra:

1. validar paciente, unidade e pacote;
2. criar a compra;
3. copiar cada item do pacote para `item_compra_paciente`;
4. iniciar `quantidade_utilizada_base` com zero;
5. salvar compra e itens na mesma transação.

Depois desse momento, alterações no pacote comercial não devem modificar a compra.

### Novo cálculo

```text
quantidade utilizada =
    quantidade_utilizada_base do item da compra
    + soma das aplicações realizadas e não canceladas vinculadas à compra e ao produto

quantidade restante =
    máximo entre 0 e
    (quantidade_contratada - quantidade utilizada)
```

### Ajuste manual

O ajuste deve modificar somente `item_compra_paciente`.

Regras recomendadas:

- compra cancelada não pode ser ajustada;
- motivo obrigatório para auditoria;
- quantidade contratada deve ser maior que zero;
- quantidade utilizada não pode ser negativa;
- quantidade utilizada não pode superar a contratada;
- não permitir uma base que torne o consumo total negativo;
- registrar valores anteriores, valores novos, usuário, data e motivo;
- reabrir compra concluída quando o ajuste gerar saldo;
- concluir compra ativa quando todos os itens ficarem zerados.

### Validação na aplicação

Ao vincular uma compra a uma aplicação, o backend deve garantir que:

- a compra está ativa;
- a compra pertence ao paciente;
- o produto aplicado existe nos itens daquela compra;
- a quantidade solicitada é maior que zero;
- existe saldo suficiente daquele produto;
- o consumo e a atualização do status ocorram na mesma transação;
- consumos simultâneos não ultrapassem o saldo disponível.

## 7. Nova classificação visual sugerida

Separar o estado parcial do estado baixo:

| Condição | Classificação |
|---|---|
| Todos os produtos zerados | Sem saldo |
| Algum produto zerado e outro disponível | Saldo parcial |
| Nenhum zerado, mas algum abaixo de 30% | Saldo baixo |
| Todos com pelo menos 30% | Com saldo |

Além do indicador geral, a interface deve continuar apresentando o saldo individual de cada produto.

Para telas de aplicação, a decisão de permitir o uso deve considerar o saldo do produto aplicado, e não somente a classificação geral do pacote.

## 8. Migração das compras antigas

A alteração pode ser feita sem apagar ou recriar o banco. Será adicionada uma nova tabela e os dados existentes serão copiados e reconciliados.

### 8.1 Preparação

- gerar backup completo do banco;
- testar a restauração do backup;
- executar a migração primeiro em uma cópia do ambiente do cliente;
- impedir novas compras e aplicações durante a migração final;
- registrar contagens e saldos antes da execução.

### 8.2 Fontes para reconstrução

Para cada compra existente, consultar:

- pacote e itens atualmente vinculados;
- aplicações vinculadas à compra;
- quantidade utilizada em cada aplicação;
- aplicações canceladas;
- auditoria da criação da compra;
- auditoria dos ajustes manuais;
- observações ou registros de migração anteriores.

### 8.3 Estratégia de preenchimento

Para compras sem divergência:

1. copiar os produtos e quantidades contratadas para a nova tabela;
2. copiar a base de consumo aplicável àquela compra;
3. manter as aplicações como fonte do consumo operacional;
4. conferir se o saldo final novo é igual ao saldo esperado.

Para compras compartilhando pacotes que receberam ajustes, não se deve copiar cegamente a mesma `quantidade_utilizada_base` para todas elas. O valor correto deve ser reconstruído pelo histórico individual.

### 8.4 Relatório de reconciliação

Antes de liberar o ambiente, gerar um relatório como:

| Paciente | Compra | Pacote | Produto | Contratado | Aplicações válidas | Base reconstruída | Restante novo | Divergência |
|---|---|---|---|---:|---:|---:|---:|---|
| Paciente A | Compra A | Pacote GH | GH | 5 | 3 | 0 | 2 | Não |
| Paciente B | Compra B | Pacote GH | GH | 5 | 0 | 5 | 0 | Revisar |

Registros sem evidência suficiente devem ser apresentados para conferência manual, sem adivinhar o saldo.

### 8.5 Compatibilidade temporária

Durante a primeira implantação:

- manter `quantidade_utilizada_base` em `item_pacote`;
- não remover nenhuma coluna antiga;
- usar `item_compra_paciente` como fonte principal após o backfill;
- registrar e monitorar compras que não possuam itens migrados;
- remover a estrutura antiga somente em uma implantação posterior, depois da validação.

## 9. Estratégia de implantação

### Fase 1 — expansão

- criar a nova tabela e índices;
- adicionar entidade, configuração e repositório;
- implementar rotina de diagnóstico e reconciliação;
- não remover campos atuais.

### Fase 2 — migração

- pausar operações de compra e aplicação;
- executar o backfill;
- gerar relatório de divergências;
- corrigir ou confirmar registros pendentes;
- validar totais e saldos.

### Fase 3 — ativação

- publicar backend lendo os itens da compra;
- fazer novas compras criarem sua fotografia;
- publicar a nova classificação visual;
- acompanhar logs e divergências.

### Fase 4 — estabilização

- comparar saldos antes e depois;
- validar amostra com o cliente;
- manter backup e possibilidade de rollback;
- somente depois avaliar a remoção do campo antigo do pacote.

## 10. Impactos previstos

### Banco de dados

Impacto baixo a moderado:

- uma tabela adicional;
- aproximadamente uma linha para cada produto de cada compra;
- novos índices e chaves estrangeiras;
- nenhuma necessidade de apagar compras, aplicações ou pacotes.

### Backend

Impacto moderado:

- mudança na origem do cálculo do saldo;
- alteração na criação e ajuste da compra;
- atualização das validações de aplicação;
- atualização de consultas e mapeamentos;
- necessidade de proteção contra consumo concorrente.

### Frontend

Impacto baixo:

- o contrato atual da API pode ser preservado;
- alteração das classificações e dos rótulos;
- nenhuma reconstrução completa das telas é necessária.

### Operação

O maior risco está na reconstrução das compras antigas que já compartilham dados inconsistentes. A criação da tabela, isoladamente, apresenta baixo risco.

## 11. Critérios de aceite

- [ ] Uma compra nova inicia com todo o saldo contratado.
- [ ] Duas compras do mesmo pacote possuem saldos independentes.
- [ ] Ajustar uma compra não altera nenhuma outra compra.
- [ ] Editar o pacote comercial não modifica compras já registradas.
- [ ] Uma aplicação consome somente o produto e a compra selecionados.
- [ ] Não é possível usar uma compra para produto ausente em seus itens.
- [ ] Não é possível consumir quantidade superior ao saldo disponível.
- [ ] Cancelar uma aplicação devolve o saldo da compra correta.
- [ ] Uma compra é concluída quando todos os seus itens ficam sem saldo.
- [ ] Uma compra concluída é reaberta quando recupera saldo por cancelamento ou ajuste válido.
- [ ] Um produto zerado e outro disponível resultam em **Saldo parcial**.
- [ ] Todos os produtos zerados resultam em **Sem saldo**.
- [ ] O histórico identifica criação, aplicações, cancelamentos e ajustes.
- [ ] A migração apresenta relatório dos registros divergentes.
- [ ] Compras antigas validadas mantêm o saldo esperado após a implantação.

## 12. Testes mínimos necessários

### Backend

- criação de duas compras do mesmo pacote;
- aplicação parcial e total;
- pacote com vários produtos;
- tentativa de usar produto ausente na compra;
- ajuste de saldo individual;
- cancelamento de aplicação;
- conclusão e reabertura automática;
- edição do pacote depois da compra;
- duas aplicações concorrentes usando o último saldo;
- migração de compra sem aplicação;
- migração de compra com aplicação e cancelamento;
- migração de compra com ajuste manual.

### Frontend

- classificação com saldo integral;
- classificação abaixo de 30%;
- classificação parcialmente zerada;
- classificação totalmente zerada;
- apresentação individual dos produtos;
- atualização da listagem depois de aplicação, cancelamento e ajuste.

## 13. Rollback

Caso a validação identifique problema:

1. interromper temporariamente novas operações;
2. retornar a aplicação para a versão anterior;
3. manter a nova tabela sem utilização ou removê-la somente após confirmação;
4. restaurar o backup apenas se dados antigos tiverem sido modificados incorretamente;
5. preservar o relatório de reconciliação para uma nova tentativa.

Como a primeira alteração é aditiva e não remove a estrutura antiga, o rollback da aplicação é relativamente seguro.

## 14. Resultado esperado

Após a correção, o pacote será apenas um modelo comercial. Cada compra possuirá produtos, quantidades e saldo próprios, preservando o contrato realizado com o paciente e impedindo interferência entre compras antigas, novas ou pertencentes a pacientes diferentes.
