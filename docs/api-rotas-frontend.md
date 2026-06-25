# BGD Clinical — Referência de API para o Frontend

Documento de contrato HTTP entre o backend (`BGD.CLINICAL.WebApi`) e o frontend.

**Base URL (desenvolvimento):** `http://localhost:5111` ou `https://localhost:7013`  
**Swagger:** `/swagger`  
**Formato:** JSON (`Content-Type: application/json`)  
**Nomenclatura JSON:** camelCase (padrão ASP.NET Core)

---

## 1. Autenticação

### Header em rotas protegidas

```http
Authorization: Bearer {token}
```

O token JWT é retornado em `POST /api/auth/login`, `POST /api/auth/registrar` e `POST /api/auth/primeiro-acesso/concluir`.  
Ele contém o tenant (`empresa_id`) e o usuário logado — **não envie `empresa_id` no body** das rotas de negócio.

### Rotas públicas (sem token)

| Método | Rota |
|--------|------|
| POST | `/api/auth/registrar` |
| POST | `/api/auth/login` |
| POST | `/api/auth/primeiro-acesso/validar-email` |
| POST | `/api/auth/primeiro-acesso/concluir` |
| GET | `/api/health` |

### Rotas protegidas (exigem token)

Todas as demais rotas documentadas abaixo, incluindo `GET /api/auth/me`.

---

## 2. Envelope padrão de resposta

A maioria dos endpoints retorna o envelope `ApiResponse<T>`:

```json
{
  "data": { },
  "success": true,
  "message": null
}
```

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `data` | `T` | Payload da operação. Em erro de validação/negócio costuma ser `null`. |
| `success` | `boolean` | `true` em sucesso, `false` em falha tratada pelo controller. |
| `message` | `string \| null` | Mensagem de erro em português quando `success = false`. |

### Erros tratados pelo controller

Exemplo (400):

```json
{
  "data": null,
  "success": false,
  "message": "Informe o nome da unidade."
}
```

### Erros não tratados / exceção de domínio

Formato **RFC 7807** (`application/problem+json`):

```json
{
  "status": 400,
  "title": "BadRequest",
  "detail": "Mensagem da regra de domínio."
}
```

### Códigos HTTP usados

| Código | Quando |
|--------|--------|
| 200 | Sucesso (GET, PUT, DELETE lógico) |
| 201 | Criado (POST) — header `Location` aponta para o recurso |
| 400 | Validação / regra de negócio |
| 401 | Não autenticado ou credenciais inválidas |
| 404 | Recurso não encontrado no tenant |
| 409 | Múltiplas contas com o mesmo e-mail no login |
| 500 | Erro inesperado |

---

## 3. Auth — `/api/auth`

### POST `/api/auth/registrar`

Cadastro inicial de clínica + usuário administrador. **Público.**

**Request**

```json
{
  "nomeEmpresa": "Clínica Exemplo",
  "nome": "João Admin",
  "email": "admin@clinica.com",
  "senha": "senha1234",
  "cnpj": "12.345.678/0001-90",
  "telefone": "11999999999",
  "corPrincipal": "#2563EB"
}
```

| Campo | Obrigatório | Regras |
|-------|-------------|--------|
| `nomeEmpresa` | Sim | Nome da clínica |
| `nome` | Sim | Nome do responsável |
| `email` | Sim | E-mail válido; único globalmente entre contas ativas |
| `senha` | Sim | Mínimo 8 caracteres |
| `cnpj` | Não | Opcional; único entre empresas se informado |
| `telefone` | Não | Telefone de contato da clínica |
| `corPrincipal` | Não | Cor hex (`#RGB` ou `#RRGGBB`) para white label |

**Response 200**

```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "usuario": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "João Admin",
      "email": "admin@clinica.com",
      "isAdmin": true
    }
  },
  "success": true,
  "message": null
}
```

**Response 400** — e-mail já cadastrado, validação, etc.

```json
{
  "data": null,
  "success": false,
  "message": "Já existe uma conta com este e-mail."
}
```

---

### POST `/api/auth/login`

**Público.**

**Request**

```json
{
  "email": "admin@clinica.com",
  "senha": "senha1234",
  "empresaId": null
}
```

| Campo | Obrigatório | Regras |
|-------|-------------|--------|
| `email` | Sim | E-mail de login |
| `senha` | Sim | Senha |
| `empresaId` | Não | Quando o mesmo e-mail tem várias clínicas, informe o `empresaId` escolhido no seletor |

**Response 200** — uma única clínica (login direto):

```json
{
  "data": {
    "requiresCompanySelection": false,
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "usuario": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "João Admin",
      "email": "admin@clinica.com",
      "isAdmin": true
    },
    "companies": null
  },
  "success": true,
  "message": null
}
```

**Response 200** — múltiplas clínicas (exibir seletor; reenviar login com `empresaId`):

```json
{
  "data": {
    "requiresCompanySelection": true,
    "token": null,
    "usuario": null,
    "companies": [
      {
        "empresaId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "nome": "Clínica Centro",
        "logo": null,
        "corPrincipal": "#2563EB",
        "ativo": true,
        "isCurrent": false
      },
      {
        "empresaId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
        "usuarioId": "4fb96f75-6828-5673-c4gd-3d074g77bgb7",
        "nome": "Clínica Norte",
        "logo": "https://cdn.exemplo.com/logo.png",
        "corPrincipal": "#1D4ED8",
        "ativo": true,
        "isCurrent": false
      }
    ]
  },
  "success": true,
  "message": null
}
```

**Response 401**

```json
{
  "data": null,
  "success": false,
  "message": "Credenciais inválidas"
}
```

**Response 401** — funcionário ainda não definiu senha (primeiro acesso pendente):

```json
{
  "data": null,
  "success": false,
  "message": "É necessário definir a senha no primeiro acesso."
}
```

> O funcionário deve usar o **link enviado por e-mail** no cadastro (`POST /api/employees`). O front monta a tela em `/primeiro-acesso?token=...` (URL configurável no backend).

---

### POST `/api/auth/switch-company`

Troca o contexto para outra clínica do **mesmo e-mail** (usuário já autenticado). Retorna novo JWT.

**Request**

```json
{
  "empresaId": "b2c3d4e5-f6a7-8901-bcde-f12345678901"
}
```

**Response 200** — mesmo formato de `AuthResponse` (token + usuario).

**Response 400** — sem acesso à clínica:

```json
{
  "data": null,
  "success": false,
  "message": "Você não tem acesso a esta clínica."
}
```

---

### POST `/api/auth/primeiro-acesso/validar-email`

Valida o token do link e confirma que o e-mail informado corresponde ao convite. **Público.**

Use na tela de primeiro acesso: o usuário abre o link (`?token=...`), digita o e-mail e o front chama este endpoint antes de exibir o campo de senha.

**Request**

```json
{
  "token": "token-recebido-na-url",
  "email": "maria@clinica.com"
}
```

| Campo | Obrigatório | Regras |
|-------|-------------|--------|
| `token` | Sim | Valor do query param `token` na URL do convite |
| `email` | Sim | Deve ser o mesmo `emailLogin` cadastrado pelo admin |

**Response 200**

```json
{
  "data": {
    "nome": "Maria Silva",
    "email": "maria@clinica.com"
  },
  "success": true,
  "message": null
}
```

**Response 400** — token inválido/expirado, e-mail não confere ou convite já usado:

```json
{
  "data": null,
  "success": false,
  "message": "Convite inválido ou expirado."
}
```

---

### POST `/api/auth/primeiro-acesso/concluir`

Define a senha e conclui o primeiro acesso. Retorna JWT (mesmo formato do login). **Público.**

**Request**

```json
{
  "token": "token-recebido-na-url",
  "email": "maria@clinica.com",
  "senha": "senha1234"
}
```

| Campo | Obrigatório | Regras |
|-------|-------------|--------|
| `token` | Sim | Mesmo token da URL |
| `email` | Sim | Mesmo e-mail validado na etapa anterior |
| `senha` | Sim | Mínimo 8 caracteres |

**Response 200** — mesmo formato de `AuthResponse` do login/registrar.

```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "usuario": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "Maria Silva",
      "email": "maria@clinica.com",
      "isAdmin": false
    }
  },
  "success": true,
  "message": null
}
```

**Response 400** — convite inválido, senha curta, etc.

```json
{
  "data": null,
  "success": false,
  "message": "Convite inválido ou expirado."
}
```

---

### GET `/api/auth/me`

Retorna o usuário autenticado a partir do token. **Requer Bearer token.**

| Campo | Descrição |
|-------|-----------|
| `isAdmin` | `true` = perfil Admin; `false` = perfil Funcionario |

**Response 200**

```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nome": "João Admin",
    "email": "admin@clinica.com",
    "isAdmin": true
  },
  "success": true,
  "message": null
}
```

**Response 401**

```json
{
  "data": null,
  "success": false,
  "message": "Usuário não autenticado."
}
```

---

## 4. Health — `/api/health`

### GET `/api/health`

**Público.** Não usa o envelope `ApiResponse`.

**Response 200**

```json
{
  "status": "Healthy",
  "service": "BGD.CLINICAL.WebApi"
}
```

---

## 5. Empresa — `/api/companies`

Rotas para **listar**, **criar** e gerenciar clínicas do usuário logado. Um mesmo e-mail pode ter acesso a várias empresas (cada uma com um registro `Usuario`).

| Método | Rota | Auth |
|--------|------|------|
| GET | `/api/companies` | Qualquer usuário autenticado — lista todas as clínicas do e-mail |
| POST | `/api/companies` | Qualquer usuário autenticado — cria clínica e vincula o e-mail atual como Admin |
| GET | `/api/companies/current` | Qualquer usuário autenticado — dados da clínica do token |
| PUT | `/api/companies/current` | Somente Admin (`tipo_usuario = Admin`) |
| PATCH | `/api/companies/{empresaId}/reactivate` | Somente Admin — reativa clínica inativa |
| POST | `/api/companies/current/logo` | Somente Admin — upload multipart |

A logo é armazenada no **Cloudflare R2**; o campo `logo` na resposta contém a URL pública para o frontend exibir a imagem.

### GET `/api/companies`

Lista todas as clínicas às quais o e-mail logado tem acesso.

**Response 200**

```json
{
  "data": [
    {
      "empresaId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "usuarioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "Clínica Centro",
      "logo": null,
      "corPrincipal": "#2563EB",
      "ativo": true,
      "isCurrent": true
    }
  ],
  "success": true,
  "message": null
}
```

---

### POST `/api/companies`

Cria uma nova clínica e vincula o **mesmo e-mail e senha** do usuário logado como Admin. Retorna JWT já no contexto da nova clínica.

**Request**

```json
{
  "nome": "Clínica Norte",
  "cnpj": "98.765.432/0001-10",
  "telefone": "11988887777",
  "email": "contato@clinicanorte.com",
  "corPrincipal": "#1D4ED8"
}
```

| Campo | Obrigatório | Regras |
|-------|-------------|--------|
| `nome` | Sim | Nome da clínica |
| `cnpj` | Não | Único entre empresas se informado |
| `telefone` | Não | — |
| `email` | Não | E-mail de contato da clínica; padrão = e-mail do usuário logado |
| `corPrincipal` | Não | Hex `#RGB` ou `#RRGGBB` |

**Response 200** — `AuthResponse` (novo token + usuario na nova clínica).

---

### GET `/api/companies/current`

Retorna os dados da clínica vinculada ao token.

**Response 200**

```json
{
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "nome": "Clínica Exemplo",
    "cnpj": "12.345.678/0001-90",
    "telefone": "11999999999",
    "email": "contato@clinica.com",
    "logo": null,
    "corPrincipal": "#2563EB",
    "ativo": true,
    "criadoEm": "2026-06-24T12:00:00Z",
    "atualizadoEm": null
  },
  "success": true,
  "message": null
}
```

---

### PUT `/api/companies/current`

Atualiza dados da empresa. **Somente Admin.**

**Request**

```json
{
  "nome": "Clínica Exemplo LTDA",
  "cnpj": "12.345.678/0001-90",
  "telefone": "11988887777",
  "email": "contato@clinica.com",
  "corPrincipal": "#1D4ED8",
  "logo": null,
  "ativo": true
}
```

| Campo | Obrigatório | Regras |
|-------|-------------|--------|
| `nome` | Sim | Nome da clínica |
| `cnpj` | Não | Único entre empresas se informado |
| `telefone` | Não | — |
| `email` | Não | E-mail válido se informado |
| `corPrincipal` | Não | Hex `#RGB` ou `#RRGGBB` |
| `logo` | Não | URL da imagem; `null` no JSON mantém o valor atual |
| `ativo` | Sim | `false` desativa a clínica; `true` em clínica inativa **reativa** (somente Admin) |

**Response 200** — `CompanyDto` atualizado em `data`.

**Response 400** — validação, CNPJ duplicado ou usuário sem permissão de Admin:

```json
{
  "data": null,
  "success": false,
  "message": "Somente administradores podem reativar a clínica."
}
```

---

### PATCH `/api/companies/{empresaId}/reactivate`

Reativa uma clínica **inativa** do mesmo e-mail. **Somente Admin.** Útil quando você está logado em outra clínica e quer reativar uma da lista (`GET /api/companies`). Sem body.

**Response 200** — `CompanyDto` com `ativo: true`.

**Response 400** — clínica já ativa ou sem permissão:

```json
{
  "data": null,
  "success": false,
  "message": "Esta clínica já está ativa."
}
```

---

### POST `/api/companies/current/logo`

Envia a logo da empresa para o Cloudflare R2. **Somente Admin.**

**Content-Type:** `multipart/form-data`

| Campo | Tipo | Obrigatório | Regras |
|-------|------|-------------|--------|
| `file` | arquivo | Sim | PNG, JPEG ou WebP; máximo 2 MB |

**Exemplo (JavaScript)**

```javascript
const formData = new FormData();
formData.append("file", arquivoSelecionado);

await fetch("/api/companies/current/logo", {
  method: "POST",
  headers: { Authorization: `Bearer ${token}` },
  body: formData,
});
```

**Response 200** — `CompanyDto` com `logo` preenchido:

```json
{
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "nome": "Clínica Exemplo",
    "cnpj": "12.345.678/0001-90",
    "telefone": "11999999999",
    "email": "contato@clinica.com",
    "logo": "https://pub-7e257d5d40d64b209cf998d85ebf78ed.r2.dev/companies/a1b2c3d4-e5f6-7890-abcd-ef1234567890/logo.png",
    "corPrincipal": "#2563EB",
    "ativo": true,
    "criadoEm": "2026-06-24T12:00:00Z",
    "atualizadoEm": "2026-06-24T15:30:00Z"
  },
  "success": true,
  "message": null
}
```

**Response 400** — formato inválido, arquivo grande ou storage não configurado:

```json
{
  "data": null,
  "success": false,
  "message": "Formato não suportado. Envie PNG, JPEG ou WebP."
}
```

---

## 6. Unidades — `/api/units`

Todas as rotas exigem **Bearer token**. Os dados são sempre filtrados pela empresa do token (multi-tenant).

### GET `/api/units`

Lista unidades da empresa logada.

**Query params**

| Param | Tipo | Default | Descrição |
|-------|------|---------|-----------|
| `includeInactive` | `boolean` | `false` | Incluir unidades desativadas |

**Exemplo:** `GET /api/units?includeInactive=false`

**Response 200**

```json
{
  "data": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "nome": "Unidade Centro",
      "endereco": "Rua das Flores, 100",
      "ativo": true,
      "criadoEm": "2026-06-24T12:00:00Z",
      "atualizadoEm": null
    }
  ],
  "success": true,
  "message": null
}
```

---

### GET `/api/units/{id}`

**Response 200** — um `UnitDto` em `data`.

```json
{
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "nome": "Unidade Centro",
    "endereco": "Rua das Flores, 100",
    "ativo": true,
    "criadoEm": "2026-06-24T12:00:00Z",
    "atualizadoEm": null
  },
  "success": true,
  "message": null
}
```

**Response 404**

```json
{
  "data": null,
  "success": false,
  "message": "Unidade não encontrada."
}
```

---

### POST `/api/units`

**Request**

```json
{
  "nome": "Unidade Centro",
  "endereco": "Rua das Flores, 100"
}
```

| Campo | Obrigatório |
|-------|-------------|
| `nome` | Sim |
| `endereco` | Não |

**Response 201** — `Location: /api/units/{id}`

```json
{
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "nome": "Unidade Centro",
    "endereco": "Rua das Flores, 100",
    "ativo": true,
    "criadoEm": "2026-06-24T12:00:00Z",
    "atualizadoEm": null
  },
  "success": true,
  "message": null
}
```

**Response 400** — nome duplicado na empresa:

```json
{
  "data": null,
  "success": false,
  "message": "Já existe uma unidade com este nome."
}
```

---

### PUT `/api/units/{id}`

**Request** — mesmo body do POST.

```json
{
  "nome": "Unidade Centro Atualizada",
  "endereco": "Av. Principal, 200"
}
```

**Response 200** — `UnitDto` atualizado em `data`.  
**Response 404** — unidade não encontrada.  
**Response 400** — unidade inativa ou nome duplicado.

---

### DELETE `/api/units/{id}`

Desativa a unidade (soft delete). Não remove do banco.

**Response 200**

```json
{
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "nome": "Unidade Centro",
    "endereco": "Rua das Flores, 100",
    "ativo": false,
    "criadoEm": "2026-06-24T12:00:00Z",
    "atualizadoEm": "2026-06-24T15:30:00Z"
  },
  "success": true,
  "message": null
}
```

---

### PATCH `/api/units/{id}/reactivate`

Reativa uma unidade inativa. Sem body.

**Response 200**

```json
{
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "nome": "Unidade Centro",
    "endereco": "Rua das Flores, 100",
    "ativo": true,
    "criadoEm": "2026-06-24T12:00:00Z",
    "atualizadoEm": "2026-06-24T16:00:00Z"
  },
  "success": true,
  "message": null
}
```

**Response 404** — unidade não encontrada.

**Response 400** — exemplos:

```json
{ "data": null, "success": false, "message": "Unidade já está ativa." }
```

```json
{ "data": null, "success": false, "message": "Já existe uma unidade com este nome." }
```

---

## 7. Cargos — `/api/positions`

Cadastro de cargos vinculados aos funcionários (ex.: Médico, Enfermeiro, Recepcionista). Todas as rotas exigem **Bearer token**. Os dados são filtrados pela empresa do token.

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
  "nome": "Enfermeiro"
}
```

| Campo | Obrigatório |
|-------|-------------|
| `nome` | Sim |

**Response 201** — `Location: /api/positions/{id}`

**Response 400** — nome duplicado na empresa:

```json
{
  "data": null,
  "success": false,
  "message": "Já existe um cargo com este nome."
}
```

---

### PUT `/api/positions/{id}`

**Request** — mesmo body do POST.

**Response 200** — `PositionDto` atualizado em `data`.  
**Response 404** — cargo não encontrado.  
**Response 400** — cargo inativo ou nome duplicado.

---

### DELETE `/api/positions/{id}`

Desativa o cargo (soft delete). Funcionários que já possuem o cargo vinculado mantêm a referência histórica; novos vínculos devem usar apenas cargos ativos.

**Response 200** — `PositionDto` com `ativo: false`.

---

### PATCH `/api/positions/{id}/reactivate`

Reativa um cargo inativo. Sem body.

**Response 200** — `PositionDto` com `ativo: true`.

---

## 8. Funcionários — `/api/employees`

Cadastro de colaboradores com acesso à plataforma. Todas as rotas exigem **Bearer token**.

**Criar e editar** (`POST`, `PUT`) exigem usuário com `tipo_usuario = Admin` (claim JWT `tipo_usuario: Admin`). Demais rotas ficam disponíveis para qualquer usuário autenticado da empresa.

### Conceitos importantes para o front

| Conceito | Descrição |
|----------|-----------|
| `linkToEmpresa` | `true` = funcionário atua em **todas** as unidades da empresa logada |
| `unidadeIds` | Quando `linkToEmpresa = false`, lista de unidades específicas (ao menos uma) |
| `emailLogin` | E-mail usado para login na plataforma |
| `isAdmin` | `true` = usuário da plataforma com perfil **Admin**; `false` = **Funcionario** |
| `pendentePrimeiroAcesso` | `true` = usuário criado sem senha; deve passar pelo fluxo de primeiro acesso |
| `links` | Vínculos do funcionário com empresa/unidade na resposta |

**Não enviar senha no cadastro** — a senha é definida no primeiro acesso via link enviado por e-mail para `emailLogin`.

Ao criar o funcionário, o backend:
1. Cria o usuário com `pendentePrimeiroAcesso = true`
2. Gera um convite com token único (validade padrão: 7 dias)
3. Envia e-mail com link no formato `{FrontendBaseUrl}/primeiro-acesso?token=...`

Se o envio do e-mail falhar, a API retorna **400** (funcionário e convite já persistidos — o admin pode reenviar no futuro).

---

### GET `/api/employees`

**Query params**

| Param | Tipo | Default |
|-------|------|---------|
| `includeInactive` | `boolean` | `false` |

**Response 200**

```json
{
  "data": [
    {
      "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
      "nome": "Maria Silva",
      "telefone": "11999998888",
      "email": "maria@clinica.com",
      "emailLogin": "maria@clinica.com",
      "pendentePrimeiroAcesso": true,
      "isAdmin": false,
      "ativo": true,
      "links": [
        {
          "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
          "empresaId": null,
          "unidadeId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
          "cargoId": null,
          "flagAplicador": true,
          "ativo": true
        }
      ],
      "criadoEm": "2026-06-24T12:00:00Z",
      "atualizadoEm": null
    }
  ],
  "success": true,
  "message": null
}
```

---

### GET `/api/employees/{id}`

**Response 200** — um `EmployeeDto` em `data` (mesma estrutura do item da lista).

**Response 404**

```json
{
  "data": null,
  "success": false,
  "message": "Funcionário não encontrado."
}
```

---

### POST `/api/employees`

**Requer perfil Admin.**

#### Exemplo A — vínculo por unidade(s)

```json
{
  "nome": "Maria Silva",
  "telefone": "11999998888",
  "email": "maria@clinica.com",
  "emailLogin": "maria@clinica.com",
  "linkToEmpresa": false,
  "unidadeIds": [
    "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
  ],
  "cargoId": null,
  "flagAplicador": true,
  "isAdmin": false
}
```

#### Exemplo B — vínculo em toda a empresa

```json
{
  "nome": "Carlos Souza",
  "telefone": null,
  "email": null,
  "emailLogin": "carlos@clinica.com",
  "linkToEmpresa": true,
  "unidadeIds": null,
  "cargoId": null,
  "flagAplicador": false,
  "isAdmin": true
}
```

| Campo | Obrigatório | Regras |
|-------|-------------|--------|
| `nome` | Sim | |
| `telefone` | Não | |
| `email` | Não | Contato; pode ser diferente do `emailLogin` |
| `emailLogin` | Sim | E-mail válido; único por empresa |
| `linkToEmpresa` | Sim | `true` ou `false` |
| `unidadeIds` | Condicional | Obrigatório (≥1) quando `linkToEmpresa = false` |
| `cargoId` | Não | Deve existir na empresa, se informado |
| `flagAplicador` | Sim | Indica se pode realizar aplicações |
| `isAdmin` | Não | Default `false`. `true` cria usuário com `tipo_usuario = Admin` |

**Response 201**

```json
{
  "data": {
    "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "nome": "Maria Silva",
    "telefone": "11999998888",
    "email": "maria@clinica.com",
    "emailLogin": "maria@clinica.com",
    "pendentePrimeiroAcesso": true,
    "isAdmin": false,
    "ativo": true,
    "links": [
      {
        "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
        "empresaId": null,
        "unidadeId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "cargoId": null,
        "flagAplicador": true,
        "ativo": true
      }
    ],
    "criadoEm": "2026-06-24T12:00:00Z",
    "atualizadoEm": null
  },
  "success": true,
  "message": null
}
```

**Response 403** — usuário autenticado não é Admin.

**Response 400** — exemplos de mensagens:

```json
{ "data": null, "success": false, "message": "Já existe um usuário com este e-mail nesta empresa." }
```

```json
{ "data": null, "success": false, "message": "Informe ao menos uma unidade ou vincule o funcionário à empresa." }
```

```json
{ "data": null, "success": false, "message": "Uma ou mais unidades não pertencem à empresa." }
```

---

### PUT `/api/employees/{id}`

**Requer perfil Admin.**

Atualiza dados pessoais, perfil de acesso (`isAdmin`) e **substitui** os vínculos do funcionário na empresa logada.

**Request** — sem `emailLogin` (login não é alterado por esta rota):

```json
{
  "nome": "Maria Silva Santos",
  "telefone": "11988887777",
  "email": "maria.santos@clinica.com",
  "linkToEmpresa": false,
  "unidadeIds": [
    "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "d4e5f6a7-b8c9-0123-defa-234567890123"
  ],
  "cargoId": null,
  "flagAplicador": true,
  "isAdmin": false
}
```

**Response 200** — `EmployeeDto` atualizado.  
**Response 403** — usuário autenticado não é Admin.  
**Response 404** — funcionário não encontrado.  
**Response 400** — funcionário inativo ou validação de vínculos.

---

### DELETE `/api/employees/{id}`

Desativa funcionário, vínculos na empresa e usuário de acesso.

**Response 200**

```json
{
  "data": {
    "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "nome": "Maria Silva",
    "telefone": "11999998888",
    "email": "maria@clinica.com",
    "emailLogin": "maria@clinica.com",
    "pendentePrimeiroAcesso": true,
    "ativo": false,
    "links": [
      {
        "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
        "empresaId": null,
        "unidadeId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "cargoId": null,
        "flagAplicador": true,
        "ativo": false
      }
    ],
    "criadoEm": "2026-06-24T12:00:00Z",
    "atualizadoEm": "2026-06-24T16:00:00Z"
  },
  "success": true,
  "message": null
}
```

---

### PATCH `/api/employees/{id}/reactivate`

Reativa funcionário inativo, os vínculos inativos na empresa logada e o usuário de acesso. Sem body.

**Response 200**

```json
{
  "data": {
    "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "nome": "Maria Silva",
    "telefone": "11999998888",
    "email": "maria@clinica.com",
    "emailLogin": "maria@clinica.com",
    "pendentePrimeiroAcesso": true,
    "ativo": true,
    "links": [
      {
        "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
        "empresaId": null,
        "unidadeId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "cargoId": null,
        "flagAplicador": true,
        "ativo": true
      }
    ],
    "criadoEm": "2026-06-24T12:00:00Z",
    "atualizadoEm": "2026-06-24T17:00:00Z"
  },
  "success": true,
  "message": null
}
```

**Response 404** — funcionário não encontrado.

**Response 400** — exemplos:

```json
{ "data": null, "success": false, "message": "Funcionário já está ativo." }
```

```json
{ "data": null, "success": false, "message": "Não há vínculos inativos para reativar nesta empresa." }
```

> Para listar inativos antes de reativar: `GET /api/employees?includeInactive=true`

---

## 9. Pacientes — `/api/patients`

Cadastro de pacientes da clínica. Todas as rotas exigem **Bearer token**. Os dados são filtrados pela empresa do token; a unidade é informada no body e deve pertencer à empresa logada.

### GET `/api/patients`

Lista pacientes da empresa logada.

**Query params**

| Param | Tipo | Default | Descrição |
|-------|------|---------|-----------|
| `unidadeId` | `uuid` | — | Filtrar por unidade |
| `includeInactive` | `boolean` | `false` | Incluir pacientes desativados |

**Exemplo:** `GET /api/patients?unidadeId=a1b2c3d4-e5f6-7890-abcd-ef1234567890&includeInactive=false`

**Response 200**

```json
{
  "data": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "unidadeId": "d4e5f6a7-b8c9-0123-defa-234567890123",
      "nome": "Maria Silva",
      "cpf": "12345678900",
      "telefone": "(11) 99999-0000",
      "email": "maria@email.com",
      "dataNascimento": "1990-05-15",
      "observacao": null,
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

### GET `/api/patients/{id}`

**Response 200** — um `PatientDto` em `data`.

**Response 404**

```json
{
  "data": null,
  "success": false,
  "message": "Paciente não encontrado."
}
```

---

### POST `/api/patients`

**Request**

```json
{
  "unidadeId": "d4e5f6a7-b8c9-0123-defa-234567890123",
  "nome": "Maria Silva",
  "cpf": "123.456.789-00",
  "telefone": "(11) 99999-0000",
  "email": "maria@email.com",
  "dataNascimento": "1990-05-15",
  "observacao": null
}
```

| Campo | Obrigatório | Descrição |
|-------|-------------|-----------|
| `unidadeId` | Sim | Unidade ativa da empresa logada |
| `nome` | Sim | Nome completo |
| `cpf` | Não | Se informado, único na empresa (11 dígitos) |
| `telefone` | Não | |
| `email` | Não | Formato válido se informado |
| `dataNascimento` | Não | Formato `YYYY-MM-DD` |
| `observacao` | Não | |

`empresaId` **não** vai no body — vem do JWT.

**Response 201** — `Location: /api/patients/{id}`

**Response 400** — exemplos:

```json
{ "data": null, "success": false, "message": "Já existe um paciente com este CPF nesta empresa." }
```

```json
{ "data": null, "success": false, "message": "Unidade não encontrada ou inativa." }
```

---

### PUT `/api/patients/{id}`

**Request** — mesmo body do POST.

**Response 200** — `PatientDto` atualizado.  
**Response 404** — paciente não encontrado.  
**Response 400** — paciente inativo, CPF duplicado ou unidade inválida.

---

### DELETE `/api/patients/{id}`

Desativa o paciente (soft delete).

**Response 200** — `PatientDto` com `ativo: false`.

---

### PATCH `/api/patients/{id}/reactivate`

Reativa um paciente inativo. Sem body.

**Response 200** — `PatientDto` com `ativo: true`.

**Response 400** — exemplos:

```json
{ "data": null, "success": false, "message": "Paciente já está ativo." }
```

```json
{ "data": null, "success": false, "message": "A unidade vinculada ao paciente está inativa." }
```

---

## 10. Tipos de Produto — `/api/product-types`

Cadastro dos tipos de produto da clínica (ex.: Medicamento, Insumo, Implante, Material descartável, Equipamento, Outro). Todas as rotas exigem **Bearer token**.

### GET `/api/product-types`

Lista tipos da empresa logada.

**Query params:** `includeInactive` (boolean, default `false`)

**Response 200**

```json
{
  "data": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "nome": "Medicamento",
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

### GET `/api/product-types/{id}`

**Response 404:** `"Tipo de produto não encontrado."`

---

### POST `/api/product-types`

**Request**

```json
{ "nome": "Medicamento" }
```

**Response 400:** `"Já existe um tipo de produto com este nome."`

---

### PUT `/api/product-types/{id}`

**Request** — mesmo body do POST.

---

### DELETE `/api/product-types/{id}`

Desativa o tipo (soft delete). Produtos já vinculados mantêm a referência; novos produtos exigem tipo ativo.

---

### PATCH `/api/product-types/{id}/reactivate`

Reativa um tipo inativo.

---

## 11. Unidades de Medida — `/api/measurement-units`

Cadastro das unidades de medida utilizadas pela clínica (ex.: mg, g, ml, L, un, caixa). Todas as rotas exigem **Bearer token**.

### GET `/api/measurement-units`

Lista unidades da empresa logada ou busca por termo (autocomplete / filtro remoto).

**Query params**

| Param | Tipo | Default | Descrição |
|-------|------|---------|-----------|
| `includeInactive` | `boolean` | `false` | Incluir inativas |
| `tipo` | `string` | — | Filtrar por tipo: `Massa`, `Volume`, `Unidade`, `Embalagem`, `Outro` |
| `search` | `string` | — | Busca parcial em `nome` e `sigla` (mín. 2 caracteres) |
| `limit` | `integer` | `20` com `search`; sem default na listagem completa | Máximo de itens retornados (máx. `50`) |

**Modos de uso**

| Cenário | Exemplo |
|---------|---------|
| Listagem completa | `GET /api/measurement-units` |
| Select ao abrir (primeiros itens) | `GET /api/measurement-units?limit=20` |
| Autocomplete digitando | `GET /api/measurement-units?search=cent&limit=20` |
| Listagem + filtro de tipo | `GET /api/measurement-units?tipo=Volume&search=ml&limit=20` |

**Response 400** — exemplos:

```json
{ "data": null, "success": false, "message": "Informe ao menos 2 caracteres para buscar." }
```

```json
{ "data": null, "success": false, "message": "O limite máximo permitido é 50." }
```

> No frontend: debounce (~300 ms), mínimo 2 caracteres e `AbortController` para cancelar requisições anteriores.

**Response 200**

```json
{
  "data": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "nome": "Miligrama",
      "sigla": "mg",
      "tipo": "Massa",
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

### GET `/api/measurement-units/{id}`

**Response 404:** `"Unidade de medida não encontrada."`

---

### POST `/api/measurement-units`

**Request**

```json
{
  "nome": "Miligrama",
  "sigla": "mg",
  "tipo": "Massa"
}
```

| Campo | Obrigatório | Descrição |
|-------|-------------|-----------|
| `nome` | Sim | Único na empresa (ex.: Miligrama) |
| `sigla` | Sim | Único na empresa (ex.: mg, ml, un) |
| `tipo` | Sim | `Massa`, `Volume`, `Unidade`, `Embalagem` ou `Outro` |

**Response 400** — exemplos:

```json
{ "data": null, "success": false, "message": "Já existe uma unidade de medida com esta sigla." }
```

---

### PUT `/api/measurement-units/{id}`

**Request** — mesmo body do POST. Não permite editar unidade inativa.

---

### DELETE `/api/measurement-units/{id}`

Desativa a unidade (soft delete). Falha se houver produtos ativos vinculados.

---

### PATCH `/api/measurement-units/{id}/reactivate`

Reativa uma unidade inativa.

---

## 12. Produtos — `/api/products`

Cadastro de produtos físicos para controle de estoque. Todas as rotas exigem **Bearer token**.

> Cadastre os tipos em `/api/product-types` e as unidades em `/api/measurement-units` antes de criar produtos.

### GET `/api/products`

**Query params**

| Param | Tipo | Default | Descrição |
|-------|------|---------|-----------|
| `tipoProdutoId` | `uuid` | — | Filtrar por tipo |
| `includeInactive` | `boolean` | `false` | Incluir inativos |

**Response 200**

```json
{
  "data": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "tipoProdutoId": "d4e5f6a7-b8c9-0123-defa-234567890123",
      "tipoProdutoNome": "Medicamento",
      "unidadeMedidaId": "e5f6a7b8-c9d0-1234-efab-345678901234",
      "unidadeMedidaNome": "Miligrama",
      "unidadeMedidaSigla": "mg",
      "nome": "Tirzepatida",
      "estoqueMinimo": 10,
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

### GET `/api/products/{id}`

**Response 404:** `"Produto não encontrado."`

---

### POST `/api/products`

**Request**

```json
{
  "tipoProdutoId": "d4e5f6a7-b8c9-0123-defa-234567890123",
  "unidadeMedidaId": "e5f6a7b8-c9d0-1234-efab-345678901234",
  "nome": "Tirzepatida",
  "estoqueMinimo": 10
}
```

| Campo | Obrigatório | Descrição |
|-------|-------------|-----------|
| `tipoProdutoId` | Sim | Tipo ativo da empresa |
| `unidadeMedidaId` | Sim | Unidade de medida ativa da empresa |
| `nome` | Sim | Único na empresa |
| `estoqueMinimo` | Sim | `>= 0` |

**Response 400** — exemplos:

```json
{ "data": null, "success": false, "message": "Tipo de produto não encontrado." }
```

```json
{ "data": null, "success": false, "message": "Unidade de medida não encontrada." }
```

```json
{ "data": null, "success": false, "message": "Já existe um produto com este nome." }
```

---

### PUT `/api/products/{id}`

**Request** — mesmo body do POST.

---

### DELETE `/api/products/{id}`

Desativa o produto (soft delete).

---

### PATCH `/api/products/{id}/reactivate`

Reativa um produto inativo. Falha se o tipo ou a unidade de medida vinculados estiverem inativos.

---

## 13. Tipos TypeScript (referência)

```typescript
interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string | null;
}

interface AuthResponse {
  token: string;
  usuario: AuthenticatedUser;
}

interface LoginResponse {
  requiresCompanySelection: boolean;
  token: string | null;
  usuario: AuthenticatedUser | null;
  companies: UserCompany[] | null;
}

interface UserCompany {
  empresaId: string;
  usuarioId: string;
  nome: string;
  logo: string | null;
  corPrincipal: string | null;
  ativo: boolean;
  isCurrent: boolean;
}

interface CreateCompanyRequest {
  nome: string;
  cnpj?: string | null;
  telefone?: string | null;
  email?: string | null;
  corPrincipal?: string | null;
}

interface SwitchCompanyRequest {
  empresaId: string;
}

interface AuthenticatedUser {
  id: string;
  nome: string;
  email: string;
  isAdmin: boolean;
}

interface ValidateFirstAccessEmailRequest {
  token: string;
  email: string;
}

interface ValidateFirstAccessEmailResponse {
  nome: string;
  email: string;
}

interface CompleteFirstAccessRequest {
  token: string;
  email: string;
  senha: string;
}

interface Unit {
  id: string;
  nome: string;
  endereco: string | null;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

interface Company {
  id: string;
  nome: string;
  cnpj: string | null;
  telefone: string | null;
  email: string | null;
  logo: string | null;
  corPrincipal: string | null;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

interface Position {
  id: string;
  nome: string;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

interface ProductType {
  id: string;
  nome: string;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

interface MeasurementUnit {
  id: string;
  nome: string;
  sigla: string;
  tipo: 'Massa' | 'Volume' | 'Unidade' | 'Embalagem' | 'Outro';
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

interface Product {
  id: string;
  tipoProdutoId: string;
  tipoProdutoNome: string;
  unidadeMedidaId: string;
  unidadeMedidaNome: string;
  unidadeMedidaSigla: string;
  nome: string;
  estoqueMinimo: number;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

interface Patient {
  id: string;
  unidadeId: string;
  nome: string;
  cpf: string | null;
  telefone: string | null;
  email: string | null;
  dataNascimento: string | null;
  observacao: string | null;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

interface EmployeeLink {
  id: string;
  empresaId: string | null;
  unidadeId: string | null;
  cargoId: string | null;
  flagAplicador: boolean;
  ativo: boolean;
}

interface Employee {
  id: string;
  nome: string;
  telefone: string | null;
  email: string | null;
  emailLogin: string;
  pendentePrimeiroAcesso: boolean;
  isAdmin: boolean;
  ativo: boolean;
  links: EmployeeLink[];
  criadoEm: string;
  atualizadoEm: string | null;
}
```

---

## 14. Fluxo sugerido no frontend

```text
1. Registrar clínica  → POST /api/auth/registrar  → guardar token (primeira clínica)
2. Ou login           → POST /api/auth/login       → se requiresCompanySelection, exibir seletor e relogar com empresaId
3. Carregar sessão    → GET  /api/auth/me          → header Authorization
4. Listar clínicas    → GET  /api/companies        → menu de troca de contexto
5. Nova clínica       → POST /api/companies         → retorna token na nova clínica
6. Trocar clínica     → POST /api/auth/switch-company → novo token
7. Dados da clínica   → GET  /api/companies/current (branding white label)
8. Upload da logo     → POST /api/companies/current/logo (somente Admin, multipart)
9. Editar clínica     → PUT  /api/companies/current (somente Admin)
10. CRUD unidades     → /api/units/*
11. CRUD cargos       → /api/positions/* (popular select antes de cadastrar funcionário)
12. CRUD funcionários → POST/PUT /api/employees (somente Admin) → e-mail de convite no create
13. CRUD pacientes    → /api/patients/*
14. CRUD tipos produto → /api/product-types/*
15. CRUD unidades medida → /api/measurement-units/* (antes de cadastrar produtos)
16. CRUD produtos     → /api/products/*
17. Funcionário abre link → /primeiro-acesso?token=...
   a. Digita e-mail   → POST /api/auth/primeiro-acesso/validar-email
   b. Define senha    → POST /api/auth/primeiro-acesso/concluir → guardar token
18. Login funcionário → se message = "É necessário definir a senha no primeiro acesso."
                        → orientar a usar o link do e-mail (ou solicitar reenvio ao admin)
```

---

## 15. Rotas ainda não disponíveis

| Recurso | Status |
|---------|--------|
| Reenvio de convite de primeiro acesso | Não implementado |
| Permissões por módulo | Não implementado |

---

*Última atualização: junho/2026 — alinhado ao backend BGD Clinical (Companies + Units + Positions + Employees + Patients + ProductTypes + MeasurementUnits + Products + Auth + Primeiro acesso).*
