# Mapeamento de permissões — Telas × APIs (BGD Clinical Frontend)

Documento gerado a partir da análise do código em `src/router`, páginas, services, `MainLayout`, boot e `.cursor/docs/api-backend-contexto.md`.

**Contexto do backend (referência):**

- Autorização usa `[RequirePermission("chave")]` por endpoint.
- Admin tem bypass total.
- Funcionário precisa da chave exata OU de um filho hierárquico (ex.: `agenda.visualizar.equipe` satisfaz `agenda.visualizar`).
- Wildcards no cargo funcionam (ex.: `agenda.*`).
- Módulo licenciado também é checado (AGENDAMENTOS, PACIENTES, ESTOQUE, APLICACOES, FINANCEIRO, CORE).
- Envelope: `{ data, success, message }`.
- Erro de permissão: HTTP 403 com `message: "Usuário sem permissão para esta operação."`

**Legenda de permissão backend:** onde a doc HTTP não cita `[RequirePermission]`, infere-se pelo padrão `{domínio}.{ação}` do catálogo. Seções da doc que dizem “só autenticação” (appointments, operating-hours) podem divergir da implementação real.

---

## Chamadas globais (todas as telas autenticadas)

| Momento | Endpoint | Para quê |
|---------|----------|----------|
| Boot `main.ts` | `GET /api/auth/me` | Sincronizar usuário/permissões |
| `MainLayout` + `AppEmpresaSwitcher` onMounted | `GET /api/companies/current` | Marca/whitelabel |
| Idem | `GET /api/companies` | Seletor de clínica |

---

## Login — rota: `/login`

**Objetivo da tela:** Autenticar e escolher clínica quando o e-mail tem múltiplas empresas.

**Permissão mínima (guard):** `meta.publica` (sem permissão).

**Chamadas na montagem:**

| Ordem | Momento | Método | Endpoint | Para quê na UI | Obrigatória? | Se falhar 403, a tela quebra? |
|-------|---------|--------|----------|----------------|--------------|-------------------------------|
| — | — | — | — | Nenhuma API na montagem | — | — |

**Chamadas em ações:**

| Ação na UI | Método | Endpoint | Permissão backend | Pode funcionar sem outras telas? |
|------------|--------|----------|-------------------|----------------------------------|
| Continuar / selecionar clínica | POST | `/api/auth/login` | Pública | sim |
| Após login | GET | `/api/auth/me` | Autenticado | sim |

**Dependências cruzadas:** nenhuma.

**Permissão ideal sugerida:**

- Entrar: rotas auth públicas.
- Leituras auxiliares: —
- Mutações: —

**Problemas atuais:** nenhum relevante.

---

## Cadastro — rota: `/cadastro`

**Objetivo da tela:** Registrar clínica + administrador inicial.

**Permissão mínima (guard):** pública.

**Chamadas na montagem:** nenhuma.

**Chamadas em ações:**

| Ação na UI | Método | Endpoint | Permissão backend | Pode funcionar sem outras telas? |
|------------|--------|----------|-------------------|----------------------------------|
| Cadastrar | POST | `/api/auth/registrar` | Pública | sim |
| Após cadastro | GET | `/api/auth/me` | Autenticado | sim |

**Dependências cruzadas:** nenhuma.

**Permissão ideal:** manter rotas públicas.

**Problemas atuais:** nenhum.

---

## Primeiro acesso — rota: `/primeiro-acesso?token=...`

**Objetivo da tela:** Funcionário convidado define senha.

**Permissão mínima (guard):** pública.

**Chamadas na montagem:** nenhuma automática.

**Chamadas em ações:**

| Ação na UI | Método | Endpoint | Permissão backend | Pode funcionar sem outras telas? |
|------------|--------|----------|-------------------|----------------------------------|
| Validar e-mail | POST | `/api/auth/primeiro-acesso/validar-email` | Pública | sim |
| Concluir | POST | `/api/auth/primeiro-acesso/concluir` | Pública | sim |
| Após concluir | GET | `/api/auth/me` | Autenticado | sim |

**Dependências cruzadas:** nenhuma.

**Permissão ideal:** manter rotas públicas.

**Problemas atuais:** nenhum.

---

## Troca de clínica (overlay + header)

**Onde:** `AppEmpresaSwitcher`, `EmpresasListPage`, login multi-empresa.

**Chamadas:**

| Ação na UI | Método | Endpoint | Permissão backend (doc) | Pode funcionar sem outras telas? |
|------------|--------|----------|---------------------------|----------------------------------|
| Listar clínicas | GET | `/api/companies` | Qualquer autenticado | sim |
| Trocar clínica | POST | `/api/auth/switch-company` | Autenticado | sim |
| Após troca | — | reload + `GET /api/auth/me` | Autenticado | sim |

**Dependências cruzadas:** nenhuma de outro domínio CRUD.

**Permissão ideal:** qualquer autenticado para listar/trocar clínicas do mesmo e-mail.

**Problemas atuais:** `AppEmpresaSwitcher` e `MainLayout` duplicam `carregarEmpresas` + `carregarEmpresaAtual` (performance, não permissão).

---

## Dashboard / Agenda — rota: `/`

**Objetivo da tela:** Calendário de agendamentos (agenda do sistema; não há rota `/agenda` separada).

**Permissão mínima (guard):** `agenda.visualizar`

**Chamadas na montagem:**

| Ordem | Momento | Método | Endpoint | Para quê na UI | Obrigatória? | Se falhar 403, a tela quebra? |
|-------|---------|--------|----------|----------------|--------------|-------------------------------|
| 1 | onMounted | GET | `/api/units` | Filtro unidade (`{id,nome}`) | sim | parcial — filtros vazios |
| 2 | onMounted | GET | `/api/employees?unidadeId=` | Filtro profissional | não* | não — lista vazia |
| 3 | onMounted | GET | `/api/units/{id}/operating-hours` | Config calendário | sim | **sim** — agenda não libera |
| 4 | onMounted | GET | `/api/appointments` (hoje) | Chip contagem | não | não |
| 5 | `datesSet` calendário | GET | `/api/appointments` (período) | Eventos | sim | sim — calendário vazio |

\*Sem `unidadeId` no filtro inicial, lista todos os funcionários.

**Chamadas em ações (modais):**

| Ação na UI | Método | Endpoint | Permissão backend (catálogo) | Pode funcionar sem outras telas? |
|------------|--------|----------|------------------------------|----------------------------------|
| Novo/editar agendamento | POST/PUT | `/api/appointments` | `agendamento.criar` / `agendamento.editar` | não |
| Confirmar | PATCH | `/api/appointments/{id}/confirm` | `agendamento.confirmar` | sim |
| Concluir | PATCH | `/api/appointments/{id}/complete` | `agendamento.concluir` | não (cria aplicação/estoque) |
| Cancelar | PATCH | `/api/appointments/{id}/cancel` | `agendamento.cancelar` | sim |
| Falta | PATCH | `/api/appointments/{id}/no-show` | `agendamento.registrar_falta` | sim |
| Abrir form (dialog) | GET | `/api/units`, `/api/procedures`, `/api/patients`, `/api/employees` | `unidade.*`, `procedimento.*`, `paciente.*`, `funcionario.*` | não |
| Detalhe (procedimento) | GET | `/api/procedures/{id}` | `procedimento.visualizar` | sim |

**Dependências cruzadas:**

| Endpoint | Domínio | Só leitura? | Sem permissão desse domínio deveria usar a tela? |
|----------|---------|-------------|--------------------------------------------------|
| `GET /api/units` | unidade | sim (`{id,nome,ativo}`) | **sim** — filtro essencial |
| `GET /api/employees` | funcionário | sim | **sim** — filtro profissional |
| `GET /api/units/{id}/operating-hours` | unidade/horário | sim | **sim** — sem isso agenda bloqueia |
| `GET /api/patients` | paciente | sim (dropdown) | **sim** para criar agendamento |
| `GET /api/procedures` | procedimento | sim | **sim** se tipo=Aplicação |
| `GET /api/procedures/{id}` | procedimento | sim | sim (detalhe conclusão) |

**Permissão ideal sugerida:**

- Entrar: `agenda.visualizar` OU `agenda.visualizar.propria` OU `agenda.visualizar.equipe`
- Leituras auxiliares: `GET /api/units` com `unidade.visualizar` **OU** `agenda.visualizar*`; `GET /api/employees` com `funcionario.visualizar` **OU** `agenda.visualizar*`; `GET operating-hours` com `unidade.visualizar` **OU** `agenda.visualizar*`
- Mutações: manter `agendamento.*` específicas

**Problemas atuais:**

- **Bug UI:** botões Novo/Confirmar/Editar/Cancelar na agenda **não** usam `usePermissao(agendamento.*)`.
- **Bug guard:** link “Configurar horários” aponta para rota com guard `unidade.editar`, mas usuário só com `agenda.visualizar` pode precisar **ver** horários.
- Doc API diz appointments sem permissão granular — diverge do catálogo informado.

---

## Criar/editar agendamento (modal na dashboard)

**Rota frontend:** modal em `/` (não é rota própria).

**Objetivo:** Formulário de criação/edição de agendamento.

**Permissão mínima:** herda `agenda.visualizar` (não exige `agendamento.criar` para abrir modal).

**Montagem ao abrir dialog:** `GET /api/units`, `GET /api/procedures`; se unidade selecionada: `GET /api/patients`, `GET /api/employees`; garantias: `GET /api/patients/{id}`, `GET /api/employees/{id}`.

**Submit:** `POST` ou `PUT /api/appointments`.

Ver seção Dashboard para dependências cruzadas e problemas.

---

## Pacientes — lista `/pacientes`

**Objetivo da tela:** Listar e filtrar pacientes por unidade.

**Permissão mínima (guard):** `paciente.visualizar`

**Chamadas na montagem:**

| Ordem | Momento | Método | Endpoint | Para quê na UI | Obrigatória? | Se falhar 403, a tela quebra? |
|-------|---------|--------|----------|----------------|--------------|-------------------------------|
| 1 | onMounted | GET | `/api/units?includeInactive=true` | Filtro unidade | sim | parcial |
| 2 | onMounted | GET | `/api/patients` | Tabela | sim | **sim** |

**Chamadas em ações:**

| Ação na UI | Método | Endpoint | Permissão backend | Pode funcionar sem outras telas? |
|------------|--------|----------|-------------------|----------------------------------|
| Desativar | DELETE | `/api/patients/{id}` | `paciente.excluir` | sim |
| Reativar | PATCH | `/api/patients/{id}/reactivate` | `paciente.excluir` | sim |

**Dependências cruzadas:**

| Endpoint | Domínio | Só leitura? | Sem permissão deveria usar a tela? |
|----------|---------|-------------|-------------------------------------|
| `GET /api/units` | unidade | sim | **sim** (filtro); alternativa: aceitar `paciente.visualizar` |

**Permissão ideal:** `GET /api/units` com `unidade.visualizar` OU `paciente.visualizar`.

**Problemas atuais:** filtro de unidade pode falhar sem `unidade.visualizar`.

---

## Paciente cadastro `/pacientes/novo` | `/pacientes/:id/editar`

**Objetivo da tela:** Cadastrar ou editar paciente.

**Permissão mínima (guard):** `paciente.criar` | `paciente.editar`

**Chamadas na montagem:**

| Ordem | Momento | Método | Endpoint | Para quê | Obrigatória? | 403 quebra? |
|-------|---------|--------|----------|----------|--------------|-------------|
| 1 | onMounted | GET | `/api/units` | Dropdown unidade | sim | parcial |
| 2 | onMounted (edição) | GET | `/api/patients/{id}` | Formulário | sim | sim |
| 3 | sob demanda | GET | `/api/units/{id}` | Garantir unidade na lista | não | não |

**Chamadas em ações:** `POST` / `PUT /api/patients`.

**Dependências cruzadas:** `GET /api/units` — só `{id,nome}`; alternativa `paciente.criar`.

**Problemas atuais:** nenhum crítico além de cruzada com unidades.

---

## Sintomas — `/sintomas`, `/sintomas/novo`, `/sintomas/:id/editar`

**Objetivo da tela:** CRUD de sintomas.

**Permissão mínima (guard):** `sintoma.visualizar` | `sintoma.criar` | `sintoma.editar`

**Montagem:** `GET /api/symptoms` (lista); `GET /api/symptoms/{id}` (form edição).

**Ações:** `POST`, `PUT`, `DELETE`, `PATCH .../reactivate` em `/api/symptoms`.

**Dependências cruzadas:** nenhuma.

**Permissão ideal:** `sintoma.*` por operação.

**Problemas atuais:** nenhum.

---

## Funcionários — `/funcionarios`

**Objetivo da tela:** Listar colaboradores com filtros por unidade e cargo.

**Permissão mínima (guard):** `funcionario.visualizar`

**Chamadas na montagem:**

| Ordem | Momento | Método | Endpoint | Para quê | Obrigatória? | 403 quebra? |
|-------|---------|--------|----------|----------|--------------|-------------|
| 1 | onMounted | GET | `/api/units?includeInactive=true` | Filtro | sim | parcial |
| 2 | onMounted | GET | `/api/positions?includeInactive=true` | Filtro cargo | sim | parcial |
| 3 | onMounted | GET | `/api/employees` | Lista | sim | **sim** |

**Ações:** desativar `DELETE`, reativar `PATCH .../reactivate`.

**Dependências cruzadas:** unidades + cargos para filtros (`{id,nome}`).

**Permissão ideal:** `GET /api/units` e `GET /api/positions` com `funcionario.visualizar` como alternativa.

**Problemas atuais:**

- **Crítico:** doc API diz `POST/PUT /api/employees` retorna **403 se não Admin**, mas front usa `funcionario.criar/editar`.

---

## Funcionário form `/funcionarios/novo` | `/funcionarios/:id/editar`

**Objetivo:** Cadastro/edição de colaborador.

**Guard:** `funcionario.criar` | `funcionario.editar`

**Montagem:** `GET /api/units`, `GET /api/positions`, `GET /api/employees/{id}` (edição), `GET /api/positions/{id}`.

**Submit:** `POST` / `PUT /api/employees`.

**Dependências cruzadas:** unidades + cargos obrigatórios para vínculo.

**Problemas:** mesmo desalinhamento Admin vs `funcionario.*` no backend.

---

## Permissões do funcionário `/funcionarios/:id/permissoes`

**Objetivo:** Overrides Allow/Deny de permissões individuais.

**Guard:** `funcionario.editar`

**Montagem:**

| Endpoint | Para quê |
|----------|----------|
| `GET /api/employees/{id}` | Nome/contexto |
| `GET /api/employees/{id}/permissions` | Allows/denies |
| `GET /api/permissions/map` | Árvore (via `PermissaoArvoreEditor`) |

**Submit:** `PUT /api/employees/{id}/permissions`; se próprio usuário → `GET /api/auth/me`.

**Problemas:** permissão de `GET /api/permissions/map` não documentada no catálogo.

---

## Cargos — `/cargos`, `/cargos/novo`, `/cargos/:id/editar`

**Objetivo:** CRUD de cargos e permissões do cargo.

**Guard:** `cargo.visualizar` | `cargo.criar` | `cargo.editar` (**sugestão:** `cargo.*` não está no catálogo de referência do backend; front já usa `cargo.*`).

**Montagem (form):** `GET /api/positions/{id}`, `GET /api/positions/{id}/permissions`, `GET /api/permissions/map`.

**Submit:** `POST/PUT /api/positions` + `PUT /api/positions/{id}/permissions`.

**Dependências cruzadas:** apenas mapa de permissões.

**Problemas:** chaves `cargo.*` podem não existir no backend.

---

## Unidades — `/unidades`, `/unidades/nova`, `/unidades/:id/editar`

**Objetivo:** CRUD de unidades da clínica.

**Guard:** `unidade.visualizar` | `unidade.criar` | `unidade.editar`

**Montagem:** `GET /api/units` (lista); `GET /api/units/{id}` (form).

**Ações:** `POST`, `PUT`, `DELETE`, `PATCH .../reactivate`; na criação também `POST /api/units/{id}/operating-hours` (horários pendentes).

**Dependências cruzadas:** nenhuma de outro domínio.

**Problemas:** nenhum crítico.

---

## Horário funcionamento — `/unidades/:id/horario-funcionamento`

**Objetivo:** Configurar dias/horários de atendimento da unidade.

**Guard:** `unidade.editar` (leitura também exige editar hoje).

**Montagem:** `GET /api/units/{id}`, `GET /api/units/{id}/operating-hours`.

**Ações:** `POST/PUT` operating-hours, `PATCH /api/unit-operating-hours/{id}/active`.

**Dependências cruzadas:** nenhuma.

**Permissão ideal:** leitura com `unidade.visualizar` OU `agenda.visualizar*`; mutação com `unidade.editar`.

**Problemas:** usuário da agenda precisa ver horários mas guard exige `unidade.editar`.

---

## Empresas — `/empresas`

**Objetivo:** Listar clínicas do e-mail e trocar contexto.

**Guard:** `empresa.visualizar`

**Montagem:** `GET /api/companies`.

**Ações:** trocar → `POST /api/auth/switch-company`; configurar → navega para `/empresas/atual`.

**Problemas:** catálogo Core cita `empresa.editar`; front separa `empresa.visualizar`.

---

## Nova clínica — `/empresas/nova`

**Objetivo:** Criar nova clínica vinculada ao e-mail.

**Guard:** `empresa.criar`

**Submit:** `POST /api/companies` (+ opcional `POST /api/companies/current/logo`).

**Problemas:** doc diz criação para qualquer autenticado; logo exige Admin.

---

## Config clínica — `/empresas/atual`

**Objetivo:** Editar dados e logo da clínica atual.

**Guard:** `empresa.editar`

**Montagem:** `GET /api/companies/current` (via `empresaStore`).

**Ações:** `PUT /api/companies/current`, `POST /api/companies/current/logo`.

**Problemas:** doc exige **Admin** em PUT/logo; guard usa `empresa.editar` para não-admin.

---

## Produtos — `/produtos`, `/produtos/novo`, `/produtos/:id/editar`

**Objetivo:** Catálogo de produtos.

**Guard:** `produto.visualizar` | `produto.criar` | `produto.editar`

**Lista:** `GET /api/product-types` (filtro) + `GET /api/products`.

**Form:** `GET /api/product-types`, `GET /api/measurement-units` (autocomplete), `GET /api/products/{id}`.

**Dependências cruzadas:** tipos de produto + unidades de medida (dropdown mínimo).

**Permissão ideal:** `GET /api/product-types` e `GET /api/measurement-units` com `produto.criar` como alternativa.

---

## Tipos de produto — `/tipos-produto`

**Objetivo:** CRUD de tipos de produto.

**Guard:** `tipo_produto.*`

**API:** `/api/product-types` — isolado.

---

## Unidades de medida — `/unidades-medida`

**Objetivo:** CRUD de unidades de medida.

**Guard:** `unidade_medida.*`

**API:** `/api/measurement-units` — isolado; usado como autocomplete em produtos.

---

## Fornecedores — `/fornecedores`

**Objetivo:** CRUD de fornecedores.

**Guard:** `fornecedor.*`

**API:** `/api/suppliers` — isolado.

---

## Pedidos fornecedor — `/pedidos-fornecedor`

**Objetivo:** Listar pedidos de compra.

**Guard:** `pedido.visualizar`

**Montagem:** `GET /api/suppliers?limit=50`, `GET /api/units?includeInactive=true`, `GET /api/supplier-orders`.

**Ações:** cancelar `PATCH .../cancel`; receber `PATCH .../receive` (`pedido.aprovar` no front).

**Dependências cruzadas:** fornecedor + unidade para filtros.

**Permissão ideal:** leituras auxiliares com `pedido.visualizar` como alternativa.

---

## Pedido form `/pedidos-fornecedor/novo` | `/pedidos-fornecedor/:id/editar`

**Objetivo:** Criar/editar pedido com itens e anexos.

**Guard:** `pedido.criar` | `pedido.editar`

**Montagem:** `GET /api/units`, `GET /api/products`, `GET /api/suppliers`, `GET /api/supplier-orders/{id}` (edição), garantias por id.

**Submit:** `POST/PUT /api/supplier-orders`; anexos `POST/DELETE .../attachments`.

**Dependências cruzadas:** fornecedor + unidade + produto — leitura; sem `produto.visualizar` o form quebra.

**Permissão ideal:** `GET /api/products` com `pedido.criar` como alternativa.

---

## Saldos estoque — `/saldos-estoque`

**Objetivo:** Saldo atual por unidade e produto.

**Guard:** `estoque.visualizar`

**Montagem:** `GET /api/units`, `GET /api/products`, `GET /api/stock-balances`.

**Dependências cruzadas:** unidade + produto para filtros (`{id,nome}`).

**Permissão ideal:** filtros com `estoque.visualizar` como alternativa única.

---

## Movimentações histórico — `/movimentacoes-estoque`

**Objetivo:** Extrato de movimentações.

**Guard:** `estoque.visualizar`

**Montagem:** `GET /api/units`, `GET /api/products`, `GET /api/stock-movements`.

**Ações:** navegação para pedido/aplicação (sem API extra).

---

## Movimentação entrada/saída — `/movimentacoes-estoque/entrada` | `/saida`

**Objetivo:** Registrar entrada ou perda manual.

**Guard:** `estoque.movimentar`

**Montagem:** `GET /api/units`, `GET /api/products`, `GET /api/stock-balances` (saída), `GET /api/stock-movements` (histórico lateral).

**Submit:** `POST /api/stock-movements/adjustment` ou `/loss`.

---

## Aplicações paciente — `/aplicacoes-paciente`

**Objetivo:** Listar aplicações realizadas.

**Guard:** `aplicacao.visualizar`

**Montagem:** `GET /api/units`, `/api/products`, `/api/procedures`, `/api/positions`; condicional `GET /api/patients`, `GET /api/employees`; `GET /api/patient-applications`.

**Ações:** cancelar `POST /api/patient-applications/{id}/cancel`.

**Problemas:** botão “Nova aplicação” usa `useAplicador()` (`isAdmin || flagAplicador`), **não** `aplicacao.criar`.

---

## Aplicação form `/aplicacoes-paciente/nova` | `/:id/editar`

**Objetivo:** Registrar ou editar aplicação (maior cruzamento de domínios).

**Guard rota:** `aplicacao.criar` | `aplicacao.editar` — UI usa `podeGerenciarAplicacoes` (aplicador/admin).

**Montagem:** paralelo `GET /api/units`, `/api/products`, `/api/procedures`, `/api/employees`, `/api/symptoms`, `/api/positions`; condicionais pacientes, saldos, etc.; edição `GET /api/patient-applications/{id}`.

**Submit:** `POST/PUT /api/patient-applications`; cancelar `POST .../cancel`.

**Dependências cruzadas:** paciente, funcionário, procedimento, produto, sintoma, saldo estoque, cargo.

**Permissão ideal:** leituras auxiliares com `aplicacao.criar` OU `aplicacao.editar` como alternativa.

---

## Procedimentos — `/procedimentos`, form

**Objetivo:** Kits de insumos para aplicações.

**Guard:** `procedimento.*`

**Lista:** `GET /api/procedures`.

**Form:** `GET /api/products` (+ por id), `GET /api/procedures/{id}`.

**Dependências cruzadas:** produtos para itens do kit.

**Permissão ideal:** `GET /api/products` com `procedimento.criar` como alternativa.

---

## 404 — `/:pathMatch(.*)*`

Sem API. Rota pública.

---

## Financeiro

Constante `financeiro.visualizar` em `src/constants/permissoes.ts` — **sem rota/tela** no frontend.

---

# Matriz consolidada (para o backend)

| Tela | Endpoint | Método | Permissão backend hoje | Leitura auxiliar? | Permissão alternativa sugerida | Prioridade |
|------|----------|--------|------------------------|-------------------|-------------------------------|------------|
| Global (layout) | `/api/auth/me` | GET | autenticado | não | — | P0 |
| Global | `/api/companies/current` | GET | autenticado | sim (marca) | qualquer autenticado | P1 |
| Global | `/api/companies` | GET | autenticado | sim (switcher) | qualquer autenticado | P1 |
| Dashboard | `/api/appointments` | GET | `agenda.visualizar*` | não | — | P0 |
| Dashboard | `/api/units` | GET | `unidade.visualizar` | sim | `agenda.visualizar*` | P0 |
| Dashboard | `/api/employees` | GET | `funcionario.visualizar` | sim | `agenda.visualizar*` | P1 |
| Dashboard | `/api/units/{id}/operating-hours` | GET | desconhecida / auth | sim | `agenda.visualizar*` OU `unidade.visualizar` | P0 |
| Dashboard | `/api/appointments` | POST | `agendamento.criar` | não | — | P0 |
| Dashboard | `/api/appointments/{id}` | PUT | `agendamento.editar` | não | — | P1 |
| Dashboard | `/api/appointments/{id}/confirm` | PATCH | `agendamento.confirmar` | não | — | P1 |
| Dashboard | `/api/appointments/{id}/complete` | PATCH | `agendamento.concluir` | não | — | P1 |
| Dashboard | `/api/appointments/{id}/cancel` | PATCH | `agendamento.cancelar` | não | — | P1 |
| Dashboard | `/api/appointments/{id}/no-show` | PATCH | `agendamento.registrar_falta` | não | — | P2 |
| Dashboard (modal) | `/api/patients` | GET | `paciente.visualizar` | sim | `agendamento.criar` | P0 |
| Dashboard (modal) | `/api/procedures` | GET | `procedimento.visualizar` | sim | `agendamento.criar` | P1 |
| Pacientes lista | `/api/patients` | GET | `paciente.visualizar` | não | — | P0 |
| Pacientes lista | `/api/units` | GET | `unidade.visualizar` | sim | `paciente.visualizar` | P1 |
| Paciente form | `/api/patients` | POST/PUT | `paciente.criar/editar` | não | — | P0 |
| Paciente form | `/api/units` | GET | `unidade.visualizar` | sim | `paciente.criar` | P1 |
| Funcionários lista | `/api/employees` | GET | `funcionario.visualizar` | não | — | P0 |
| Funcionários lista | `/api/units` | GET | `unidade.visualizar` | sim | `funcionario.visualizar` | P1 |
| Funcionários lista | `/api/positions` | GET | `cargo.visualizar` (sugestão) | sim | `funcionario.visualizar` | P1 |
| Funcionário form | `/api/employees` | POST/PUT | **Admin?** / `funcionario.criar` | não | alinhar doc × código | P0 |
| Func. permissões | `/api/employees/{id}/permissions` | GET/PUT | desconhecida | não | `funcionario.editar` | P1 |
| Func./Cargo editor | `/api/permissions/map` | GET | desconhecida | sim | `funcionario.editar` OU `cargo.editar` | P1 |
| Cargos | `/api/positions` | GET | `cargo.visualizar` (sugestão) | não | — | P0 |
| Cargos form | `/api/positions/{id}/permissions` | PUT | desconhecida | não | `cargo.editar` | P1 |
| Unidades | `/api/units` | GET | `unidade.visualizar` | não | — | P0 |
| Horário unidade | `/api/units/{id}/operating-hours` | GET | desconhecida | não | `agenda.visualizar*` OU `unidade.visualizar` | P0 |
| Horário unidade | `/api/units/{id}/operating-hours` | POST/PUT | `unidade.editar` | não | — | P1 |
| Empresas config | `/api/companies/current` | PUT | Admin | não | `empresa.editar` se não-admin | P0 |
| Empresas config | `/api/companies/current/logo` | POST | Admin | não | `empresa.editar` | P1 |
| Produtos lista | `/api/products` | GET | `produto.visualizar` | não | — | P0 |
| Produtos lista | `/api/product-types` | GET | `tipo_produto.visualizar` | sim | `produto.visualizar` | P1 |
| Produto form | `/api/measurement-units` | GET | `unidade_medida.visualizar` | sim | `produto.criar` | P1 |
| Procedimento form | `/api/products` | GET | `produto.visualizar` | sim | `procedimento.criar` | P1 |
| Pedidos lista | `/api/supplier-orders` | GET | `pedido.visualizar` | não | — | P0 |
| Pedidos lista | `/api/suppliers` | GET | `fornecedor.visualizar` | sim | `pedido.visualizar` | P1 |
| Pedidos lista | `/api/units` | GET | `unidade.visualizar` | sim | `pedido.visualizar` | P1 |
| Pedido form | `/api/products` | GET | `produto.visualizar` | sim | `pedido.criar` | P0 |
| Pedido form | `/api/supplier-orders/{id}/receive` | PATCH | `pedido.aprovar` | não | — | P1 |
| Saldos | `/api/stock-balances` | GET | `estoque.visualizar` | não | — | P0 |
| Saldos | `/api/units`, `/api/products` | GET | vários | sim | `estoque.visualizar` | P1 |
| Movimentações | `/api/stock-movements` | GET | `estoque.visualizar` | não | — | P0 |
| Mov. manual | `/api/stock-movements/adjustment` | POST | `estoque.movimentar` | não | — | P0 |
| Mov. manual | `/api/stock-movements/loss` | POST | `estoque.movimentar` | não | — | P0 |
| Aplicações lista | `/api/patient-applications` | GET | `aplicacao.visualizar` | não | — | P0 |
| Aplicações lista | `/api/units`,`/products`,`/procedures`,`/positions` | GET | vários | sim | `aplicacao.visualizar` | P1 |
| Aplicações lista | `/api/patients`,`/employees` | GET | paciente/funcionário | sim | `aplicacao.visualizar` | P1 |
| Aplicação form | `/api/patient-applications` | POST | `aplicacao.criar` | não | — | P0 |
| Aplicação form | `/api/stock-balances` | GET | `estoque.visualizar` | sim | `aplicacao.criar` | P2 |
| Aplicação form | `/api/symptoms` | GET | `sintoma.visualizar` | sim | `aplicacao.criar` | P2 |
| Sintomas/Tipos/U.M./Fornec. | respectivos `/api/*` | CRUD | `domínio.*` | não | — | P0 |

**Prioridade:** P0 = tela não abre sem isso; P1 = funcionalidade parcial quebra; P2 = nice to have.

---

# TOP 10 desalinhamentos mais críticos

1. **Funcionários CRUD:** front permite `funcionario.criar/editar`; doc API exige **Admin** em POST/PUT — funcionário com permissão granular recebe 403 ao salvar (**bug**).

2. **Agenda × unidades/horários:** guard `agenda.visualizar` abre dashboard, mas `GET /api/units` e `operating-hours` podem exigir `unidade.visualizar/editar` — agenda fica bloqueada ou sem filtros (**bug** se backend já restringe).

3. **Link “Configurar horários”** na agenda aponta para rota com guard `unidade.editar` — recepcionista só com agenda não entra (**bug de rota**).

4. **Aplicações:** rota exige `aplicacao.criar`, mas UI de criar/editar usa `flagAplicador`/`isAdmin`, não `aplicacao.*` — comportamento inconsistente (**bug**).

5. **Empresa config:** guard `empresa.editar` vs backend “somente Admin” em PUT/logo — não-admin com chave no cargo falha ao salvar (**bug** ou decisão de produto não refletida no front).

6. **Agenda — ações de agendamento:** nenhum `usePermissao(agendamento.*)` nos botões/modais; usuário vê ações que o backend nega (**bug UX**).

7. **Pedidos / Aplicações / Procedimentos:** montagem exige listagens de produto + unidade + fornecedor; usuário com permissão só do domínio principal pode abrir tela mas formulários/filtros falham em 403 (**decisão de produto**).

8. **Cargos (`cargo.*`):** front inteiro usa chaves `cargo.*` ausentes do catálogo de referência — risco de backend não ter as mesmas chaves (**desalinhamento de catálogo**).

9. **`empresa.visualizar` vs catálogo Core (`empresa.editar`):** front separa visualizar/criar/editar; catálogo só cita `empresa.editar`.

10. **Doc API desatualizada:** appointments e operating-hours documentados como “só Bearer”, enquanto o modelo real usa `RequirePermission` (**risco de contrato**).

---

## Observações finais

- Não existe tela de **detalhe** de paciente — só lista + form.
- **Módulo licenciado** não é checado no front além das chaves em `GET /me`; validação é 403 na API.
- `src/utils/checar-permissao.ts` trata hierarquia **filho satisfaz pai** (`agenda.visualizar.equipe` → guard `agenda.visualizar`), alinhado ao backend — desde que as chaves efetivas venham resolvidas em `/me`.
- Guards de rota: `src/router/routes.ts`; constantes: `src/constants/permissoes.ts`.
