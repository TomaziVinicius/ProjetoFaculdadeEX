# Sistema de Triagem para Diagnóstico por Imagem (TC & RM)

Aplicação web e API desenvolvida sob o padrão **Onion Architecture (Arquitetura em Cebola)** em **C# e .NET**, projetada para a triagem ergonômica e ágil de exames de Tomografia Computadorizada (TC) e Ressonância Magnética (RM), com interface inspirada no posto clínico do `Laudos-OIT`.

---

## 1. 🏗️ Arquitetura da Solução (Onion Architecture)

A organização das camadas e pastas do projeto segue rigorosamente o padrão arquitetural em cebola:

```text
TriagemImagem.sln
│
├── TriagemImagem.Api
│   ├── Controllers/             # Endpoints HTTP/RESTful (/api/v1/...)
│   ├── Middlewares/             # Tratamento global de erros e auditoria
│   ├── Extensions/              # Injeção de dependências e extensões de configuração
│   └── Program.cs               # Configuração do pipeline HTTP e inicialização
│
├── TriagemImagem.Application
│   ├── UseCases/                # Casos de uso da aplicação (fluxos clínicos)
│   ├── DTOs/                    # Objetos de transferência de dados (request/response)
│   ├── Interfaces/              # Contratos dos serviços da aplicação
│   ├── Services/                # Serviços de aplicação (cálculo de contraste e dose)
│   └── Validators/              # Validações de entrada e consistência
│
├── TriagemImagem.Domain
│   ├── Entities/                # Entidades de negócio (Usuario, Paciente, Exame, etc.)
│   ├── ValueObjects/            # Objetos de valor imutáveis
│   ├── Aggregates/              # Agregados de domínio
│   ├── Services/                # Serviços de domínio puro (Semáforo e Checklist)
│   ├── Events/                  # Eventos de domínio clínico
│   ├── Repositories/            # Interfaces dos repositórios de persistência
│   ├── Exceptions/              # Exceções de negócio (DomainException)
│   └── Enums/                   # Enums (PerfilUsuario, ModalidadeExame, StatusExame, StatusRisco)
│
├── TriagemImagem.Infrastructure
│   ├── Data/
│   │   ├── Context/             # DbContext do Entity Framework Core
│   │   ├── Configurations/      # Mapeamentos fluentes (Fluent API)
│   │   └── Migrations/          # Migrações do banco de dados relacional
│   │
│   ├── Repositories/            # Implementações concretas de acesso a dados
│   └── Services/                # Serviços de infraestrutura externa
│
├── TriagemImagem.Tests
│   ├── Domain/                  # Testes unitários do domínio
│   ├── Application/             # Testes dos casos de uso e serviços
│   └── Infrastructure/          # Testes de integração e persistência
│
└── frontend/                    # 🎨 Layout do Frontend (HTML5 / Vanilla CSS / JS)
    ├── css/style.css            # Design System Glassmorphism (inspirado no Laudos-OIT)
    ├── js/app.js                # Lógica inicial da interface e modo escuro/claro
    └── index.html               # Estrutura inicial do posto de triagem
```

