# Changelog — Arquitetura Backend BGD Clinical

Registro de mudanças nas regras de arquitetura e no modo de operação da IA para o backend.

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/).

---

## [Unreleased]

---

## [1.1.0] - 2026-06-21

### Added

- Regra `07-appsettings-e-configuracao.mdc` com padrão de configuração por host (WebApi vs Jobs.Web).
- Definição de o que vai em `appsettings.json`, `appsettings.Development.json`, `.example` e variáveis de ambiente.
- Checklist para adicionar novas configurações sem misturar hosts ou expor segredos.

---

## [1.0.0] - 2026-06-21

### Added

- Estrutura inicial de regras em `.cursor/rules/` para o backend BGD Clinical.
- Definição de camadas: Domain, Application, Infra.Data, Infra.ExternalApis, Infra.Jobs, WebApi, Jobs.Web.
- Contextos delimitados alinhados ao documento de banco de dados e aos módulos contratáveis.
- Checklist para criação de nova feature.
- Checklist para validação de contexto antes de implementar.
- Regras de SOLID e Clean Architecture adaptadas ao padrão do projeto.
- Regras de multi-tenant (`empresa_id` obrigatório em toda operação de negócio).
- Exemplo estrutural do contexto Identidade (Login, Logout, Permissões).
- Convenções de nomenclatura, Result pattern, tratamento de exceções e registro de DI.

### Contexto do projeto

- SaaS white label multi-tenant para clínicas.
- Módulos contratáveis: ESTOQUE, APLICACOES, AGENDAMENTOS, FINANCEIRO, RELATORIOS.
- Stack: .NET 10, EF Core + SQL Server, arquitetura em camadas com separação por contextos.
