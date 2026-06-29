# BGD Clinical — Cargos e Permissões (API)

Documento de contrato HTTP para **cargos** (`/api/positions`) e **mapa de permissões** (`/api/permissions`). Complementa o [api-rotas-frontend.md](./api-rotas-frontend.md).

**Base URL (desenvolvimento):** `http://localhost:5111` ou `https://localhost:7013`  
**Formato:** JSON (`Content-Type: application/json`)  
**Nomenclatura JSON:** camelCase (padrão ASP.NET Core)

Todas as rotas abaixo exigem **Bearer token**, exceto quando indicado.

```http
Authorization: Bearer {token}
```

Envelope padrão: `ApiResponse<T>` — ver seção 2 em [api-rotas-frontend.md](./api-rotas-frontend.md).

---

## 1. Modelo de permissões

### Como funciona

| Camada | Descrição |
|--------|-----------|
| **Catálogo** (`permissao_sistema`) | Lista global de chaves (`agendamento.criar`, `paciente.visualizar`, …). Seed no startup da API. |
| **Cargo** (`cargo` + `cargo_permissao_item`) | Cada cargo da empresa possui um conjunto de `permissionKeys` (suporta wildcards: `agenda.*`, `produto.*`). |
| **Vínculo** (`funcionario_vinculo.cargo_id`) | Funcionário herda as permissões do cargo do vínculo ativo na empresa. |
| **Override** (`usuario_permissao`) | Allow/Deny individual por usuário (exceções ao cargo). |
| **Resolução** | `efetivo = cargo + allows − denies` (wildcards e `implies` resolvidos no servidor). |

### Regras importantes

- **Admin** (`tipo_usuario = Admin`): bypass — todas as chaves do catálogo em `GET /api/auth/me` e nas checagens da API.
- **JWT não carrega permissões** — use `GET /api/auth/me` → campo `permissions[]` para montar o menu e guards no front.
- **Permissões do funcionário vêm do `cargoId`** no cadastro/edição — não existe mais `perfilId`.
- **Cargos** são criados manualmente pelo admin (`POST /api/positions`) com as permissões desejadas (`PUT /api/positions/{id}/permissions`).
- O mapa (`GET /api/permissions/map`) retorna **todo o catálogo** de permissões do sistema (para configurar cargos). A licença de módulo restringe o **uso** na API, não a listagem do mapa.

---

## 2. Sessão — permissões do usuário logado

Documentado em [api-rotas-frontend.md](./api-rotas-frontend.md) (`GET /api/auth/me`). Campos relevantes:

| Campo | Descrição |
|-------|-----------|
| `isAdmin` | `true` = Admin; `false` = Funcionário |
| `flagAplicador` | `true` se Admin; se Funcionário, `true` quando o **cargo** do vínculo ativo tem `flagAplicador = true` |
| `permissions` | Chaves efetivas resolvidas no servidor. **Não** vão no JWT. |

```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nome": "Maria Silva",
    "email": "maria@clinica.com",
    "isAdmin": false,
    "flagAplicador": false,
    "permissions": ["agenda.visualizar", "agendamento.criar", "paciente.visualizar"]
  },
  "success": true,
  "message": null
}
```

---

## 3. Mapa de permissões — `/api/permissions`

### GET `/api/permissions/map`

Árvore completa do catálogo para montar UI de seleção (checkboxes, árvore hierárquica).

Retorna **todas** as permissões cadastradas em `permissao_sistema`, independente de licença de módulo da empresa. Use `moduleCode` para agrupar ou sinalizar na UI quais módulos a clínica contratou (consulta separada de licenças, se necessário).

**Response 200** — array de nós em `data`:

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `key` | `string` | Chave única (`agendamento.criar`) |
| `description` | `string` | Texto para exibição |
| `category` | `string` | Agrupamento na UI (Agenda, Pacientes, Sintomas, Produto, Tipo de produto, Unidade de medida, Fornecedor, Pedido, Estoque, Aplicações, Procedimentos, Financeiro, Funcionários, Unidades, Empresa) |
| `moduleCode` | `string` | Código do módulo (`AGENDAMENTOS`, `ESTOQUE`, `CORE`, …) |
| `order` | `number` | Ordem na UI |
| `parent` | `string \| null` | Chave pai (hierarquia visual) |
| `children` | `array` | Filhos na árvore |

**Exemplo (trecho):**

```json
{
  "data": [
    {
      "key": "agenda.visualizar",
      "description": "Visualizar agenda",
      "category": "Agenda",
      "moduleCode": "AGENDAMENTOS",
      "order": 10,
      "parent": null,
      "children": [
        {
          "key": "agenda.visualizar.propria",
          "description": "Visualizar agenda própria",
          "category": "Agenda",
          "moduleCode": "AGENDAMENTOS",
          "order": 11,
          "parent": "agenda.visualizar",
          "children": []
        }
      ]
    }
  ],
  "success": true,
  "message": null
}
```

---

## 4. Cargos — `/api/positions`

Cadastro de cargos vinculados aos funcionários (ex.: Médico, Enfermeiro, Recepcionista). A flag `flagAplicador` define se funcionários com aquele cargo podem realizar **aplicações**. Os dados são filtrados pela empresa do token.

### GET `/api/positions`

Lista cargos da empresa logada.

**Query params**

| Param | Tipo | Default | Descrição |
|-------|------|---------|-----------|
| `includeInactive` | `boolean` | `false` | Incluir cargos desativados |

**Exemplo:** `GET /api/positions?includeInactive=false`

**Response 200**

```json
{
  "data": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "nome": "Médico",
      "flagAplicador": true,
      "ativo": true,
      "criadoEm": "2026-06-25T12:00:00Z",
      "atualizadoEm": null
    }
  ],
  "success": true,
  "message": null
}
```

---

### GET `/api/positions/{id}`

**Response 200** — um `PositionDto` em `data`.

**Response 404**

```json
{
  "data": null,
  "success": false,
  "message": "Cargo não encontrado."
}
```

---

### POST `/api/positions`

**Request**

```json
{
  "nome": "Enfermeiro",
  "flagAplicador": true
}
```

| Campo | Obrigatório | Descrição |
|-------|-------------|-----------|
| `nome` | Sim | Único por empresa |
| `flagAplicador` | Não | Default `false`. Indica se funcionários com este cargo podem realizar aplicações |

**Response 201** — `Location: /api/positions/{id}`

**Response 400** — nome duplicado:

```json
{
  "data": null,
  "success": false,
  "message": "Já existe um cargo com este nome."
}
```

---

### PUT `/api/positions/{id}`

**Request** — mesmo body do POST (`nome`, `flagAplicador`).

**Response 200** — `PositionDto` atualizado.  
**Response 404** — cargo não encontrado.  
**Response 400** — cargo inativo ou nome duplicado.

---

### DELETE `/api/positions/{id}`

Desativa o cargo. Funcionários que já possuem o cargo vinculado mantêm a referência histórica; novos vínculos devem usar apenas cargos ativos.

**Response 200** — `PositionDto` com `ativo: false`.

---

### PATCH `/api/positions/{id}/reactivate`

Reativa um cargo inativo. Sem body.

**Response 200** — `PositionDto` com `ativo: true`.

---

## 5. Permissões do cargo — `/api/positions/{id}/permissions`

### GET `/api/positions/{id}/permissions`

Retorna as chaves configuradas no cargo.

**Response 200**

```json
{
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "nome": "Recepcionista",
    "permissionKeys": ["agenda.visualizar", "agendamento.criar", "paciente.visualizar"]
  },
  "success": true,
  "message": null
}
```

**Response 404** — cargo não encontrado.

---

### PUT `/api/positions/{id}/permissions`

Atualiza permissões do cargo. Funcionários com esse cargo têm cache de permissões invalidado automaticamente.

**Request**

```json
{
  "permissionKeys": [
    "agenda.visualizar",
    "agendamento.criar",
    "paciente.visualizar"
  ]
}
```

| Campo | Obrigatório | Descrição |
|-------|-------------|-----------|
| `permissionKeys` | Sim | Array de chaves do catálogo; wildcards permitidos (`agenda.*`) |

**Response 200** — `PositionPermissionsDto` (mesmo formato do GET).

**Response 404** — cargo não encontrado.

---

## 6. Permissões do funcionário — `/api/employees/{id}/permissions`

Rotas no controller de funcionários; documentadas aqui por serem parte do modelo de permissões.

**Requer `tipo_usuario = Admin`.**

### GET `/api/employees/{id}/permissions`

Retorna cargo do vínculo, permissões do cargo, overrides e permissões efetivas.

**Response 200**

```json
{
  "data": {
    "employeeId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "cargoId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "cargoNome": "Recepcionista",
    "cargoPermissionKeys": ["agenda.visualizar", "agendamento.criar"],
    "allows": [],
    "denies": ["agendamento.cancelar"],
    "effectivePermissions": ["agenda.visualizar", "agendamento.criar"]
  },
  "success": true,
  "message": null
}
```

| Campo | Descrição |
|-------|-----------|
| `cargoId` / `cargoNome` | Cargo do vínculo ativo na empresa |
| `cargoPermissionKeys` | Grants do cargo (antes de overrides) |
| `allows` | Overrides Allow no usuário |
| `denies` | Overrides Deny no usuário |
| `effectivePermissions` | Resultado final resolvido no servidor |

**Response 404** — funcionário não encontrado.

---

### PUT `/api/employees/{id}/permissions`

Atualiza **apenas overrides** individuais. Permissões base vêm do cargo (`cargoId` no `POST`/`PUT` do funcionário).

**Request**

```json
{
  "allows": ["financeiro.visualizar"],
  "denies": ["agendamento.cancelar"]
}
```

| Campo | Obrigatório | Descrição |
|-------|-------------|-----------|
| `allows` | Sim | Chaves com efeito Allow |
| `denies` | Sim | Chaves com efeito Deny (prevalece sobre grant do cargo) |

**Response 200** — mesmo formato do GET.

**Response 400** — funcionário sem usuário de acesso.

---

## 7. Integração com cadastro de funcionário

No `POST` / `PUT` `/api/employees`, informe `cargoId` para definir cargo e permissões base:

```json
{
  "nome": "Maria Silva",
  "emailLogin": "maria@clinica.com",
  "linkToEmpresa": false,
  "unidadeIds": ["a1b2c3d4-e5f6-7890-abcd-ef1234567890"],
  "cargoId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "isAdmin": false
}
```

| Campo | Efeito em permissões |
|-------|----------------------|
| `cargoId` | Herda `permissionKeys` do cargo nos vínculos |
| `isAdmin: true` | Ignora cargo para autorização (bypass Admin) |
| Overrides | Somente via `PUT /api/employees/{id}/permissions` |

Detalhes do CRUD de funcionários: [api-rotas-frontend.md § 8](./api-rotas-frontend.md).

---

## 8. Tipos TypeScript (referência)

```typescript
interface Position {
  id: string;
  nome: string;
  flagAplicador: boolean;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

interface CreatePositionRequest {
  nome: string;
  flagAplicador?: boolean;
}

interface UpdatePositionRequest {
  nome: string;
  flagAplicador: boolean;
}

interface PositionPermissions {
  id: string;
  nome: string;
  permissionKeys: string[];
}

interface UpdatePositionPermissionsRequest {
  permissionKeys: string[];
}

interface PermissionMapNode {
  key: string;
  description: string;
  category: string;
  moduleCode: string;
  order: number;
  parent: string | null;
  children: PermissionMapNode[];
}

interface EmployeePermissions {
  employeeId: string;
  usuarioId: string | null;
  cargoId: string | null;
  cargoNome: string | null;
  cargoPermissionKeys: string[];
  allows: string[];
  denies: string[];
  effectivePermissions: string[];
}

interface UpdateEmployeePermissionsRequest {
  allows: string[];
  denies: string[];
}

interface AuthenticatedUser {
  id: string;
  nome: string;
  email: string;
  isAdmin: boolean;
  flagAplicador: boolean;
  permissions: string[];
}
```

---

## 9. Fluxo sugerido no frontend

```text
1. Carregar sessão        → GET  /api/auth/me                    → permissions[] para menu e guards
2. Tela de cargos         → GET  /api/positions                   → listar cargos
3. Editar permissões      → GET  /api/permissions/map             → árvore para UI
                            GET  /api/positions/{id}/permissions  → valores atuais
                            PUT  /api/positions/{id}/permissions  → salvar
4. Cadastrar funcionário  → GET  /api/positions                   → select de cargo
                            POST /api/employees { cargoId }       → permissões via cargo
5. Exceções por pessoa    → GET  /api/employees/{id}/permissions
                            PUT  /api/employees/{id}/permissions  → allows/denies
6. Trocar cargo           → PUT  /api/employees/{id} { cargoId } → invalida cache no servidor
```

### Dicas de UI

- Use `GET /api/permissions/map` uma vez e cache local; use `permissionKeys` do cargo para marcar checkboxes.
- Wildcards no cargo (`agenda.*`) expandem na resolução — `effectivePermissions` e `permissions` em `/me` trazem chaves concretas.
- Deny sempre vence grant (cargo ou allow individual).
- Admin não precisa de cargo para acessar rotas — mas ainda pode ter `cargoId` no vínculo para `flagAplicador`.

---

## 10. Códigos de módulo (referência)

| `moduleCode` | Módulo |
|--------------|--------|
| `CORE` | Funcionário, unidade, empresa |
| `AGENDAMENTOS` | Agenda e agendamentos |
| `PACIENTES` | Pacientes e sintomas (categorias `Pacientes` e `Sintomas`) |
| `ESTOQUE` | Produtos, tipos, unidades de medida, fornecedores, pedidos e movimentação (categorias separadas na UI) |
| `APLICACOES` | Aplicações e procedimentos (categorias `Aplicações` e `Procedimentos`) |
| `FINANCEIRO` | Financeiro |

### Categorias do mapa (`category`)

Cada permissão tem `category` para agrupar na UI de cargos. O `moduleCode` continua indicando o módulo licenciado; a categoria é apenas visual:

| `category` | Chaves (prefixo) |
|------------|------------------|
| Agenda | `agenda.*`, `agendamento.*` |
| Pacientes | `paciente.*` |
| Sintomas | `sintoma.*` |
| Produto | `produto.*` |
| Tipo de produto | `tipo_produto.*` |
| Unidade de medida | `unidade_medida.*` |
| Fornecedor | `fornecedor.*` |
| Pedido | `pedido.*` |
| Estoque | `estoque.*` |
| Aplicações | `aplicacao.*` |
| Procedimentos | `procedimento.*` |
| Financeiro | `financeiro.*` |
| Funcionários | `funcionario.*` |
| Unidades | `unidade.*` |
| Empresa | `empresa.*` |

O campo `moduleCode` em cada nó do mapa indica a qual módulo a permissão pertence. A licença do módulo controla acesso às **rotas** da API (`[RequirePermission]`), não a visibilidade no mapa.

### Chaves auxiliares (cadastros de apoio)

| Prefixo | Uso na API |
|---------|------------|
| `sintoma.*` | `GET/POST/PUT/DELETE /api/symptoms` |
| `procedimento.*` | `GET/POST/PUT/PATCH /api/procedures` |
| `tipo_produto.*` | `GET/POST/PUT/DELETE /api/product-types` |
| `unidade_medida.*` | `GET/POST/PUT/DELETE /api/measurement-units` |

Cargos e permissões de funcionário usam `funcionario.visualizar` / `funcionario.editar` (módulo `CORE`). O mapa (`GET /api/permissions/map`) permanece acessível a qualquer usuário autenticado.
