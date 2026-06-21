# Backend BGD Clinical — Guia para IA

Documentação de arquitetura e regras de operação do backend. Leia antes de implementar qualquer feature.

## Arquivos

| Arquivo | Conteúdo |
|---------|----------|
| [CHANGELOG.md](./CHANGELOG.md) | Histórico de mudanças nas regras de arquitetura |
| [rules/00-visao-geral.mdc](./rules/00-visao-geral.mdc) | Visão do projeto, camadas e contextos |
| [rules/01-camadas-e-dependencias.mdc](./rules/01-camadas-e-dependencias.mdc) | Direção de dependências e responsabilidades |
| [rules/02-checklist-nova-feature.mdc](./rules/02-checklist-nova-feature.mdc) | Passo a passo para criar feature |
| [rules/03-checklist-contexto.mdc](./rules/03-checklist-contexto.mdc) | Validação de fronteiras entre contextos |
| [rules/04-solid-clean-architecture.mdc](./rules/04-solid-clean-architecture.mdc) | SOLID e Clean Architecture |
| [rules/05-exemplo-identidade-login.mdc](./rules/05-exemplo-identidade-login.mdc) | Estrutura de referência do Login |
| [rules/06-multi-tenant-modulos-auditoria.mdc](./rules/06-multi-tenant-modulos-auditoria.mdc) | Tenant, licenças, permissões e auditoria |
| [rules/07-appsettings-e-configuracao.mdc](./rules/07-appsettings-e-configuracao.mdc) | AppSettings, Development local e variáveis de ambiente |

## Referência externa

- Domínio de dados: `100.Documentos/banco-de-dados.mkd`
- Código-fonte: `300.Codigo-Fonte/Backend/`

## Ao alterar arquitetura

1. Atualizar o arquivo de regra correspondente em `rules/`
2. Registrar a mudança em `CHANGELOG.md` com data e descrição
3. Manter contextos isolados — nunca adicionar código de feature na raiz das camadas
