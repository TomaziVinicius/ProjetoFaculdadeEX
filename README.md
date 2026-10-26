# Sistema de Triagem para Diagnóstico por Imagem (TC & RM)

Aplicação web e API desenvolvida sob o padrão **Onion Architecture (Arquitetura em Cebola)** em **C# e .NET 8**, projetada para a triagem ergonômica e ágil de exames de Tomografia Computadorizada (TC) e Ressonância Magnética (RM), com interface inspirada no posto clínico do `Laudos-OIT`.

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
│   └── Program.cs               # Configuração do pipeline HTTP, CORS e Swagger
│
├── TriagemImagem.Application
│   ├── UseCases/                # Casos de uso da aplicação (fluxos clínicos)
│   ├── DTOs/                    # Objetos de transferência de dados (request/response)
│   ├── Interfaces/              # Contratos dos serviços da aplicação
│   ├── Services/                # Serviços de aplicação (Triagem, Worklist, Dose, Auth, Auditoria)
│   ├── Validators/              # Validações de entrada e consistência de regras
│   └── Extensions/              # Extension methods de registro no container de DI
│
├── TriagemImagem.Domain
│   ├── Entities/                # Entidades de negócio (Usuario, Paciente, Exame, etc.)
│   ├── ValueObjects/            # Objetos de valor imutáveis (Cpf, DoseContraste)
│   ├── Aggregates/              # Agregados de domínio
│   ├── Services/                # Serviços de domínio puro (SemaforoClinico e Checklist Aviação)
│   ├── Events/                  # Eventos de domínio clínico
│   ├── Repositories/            # Interfaces dos repositórios de persistência
│   ├── Exceptions/              # Exceções de negócio (DomainException)
│   └── Enums/                   # Enums (PerfilUsuario, ModalidadeExame, StatusExame, StatusRisco)
│
├── TriagemImagem.Infrastructure
│   ├── Data/
│   │   ├── Context/             # DbContext do Entity Framework Core (AppDbContext)
│   │   ├── Configurations/      # Mapeamentos fluentes (Fluent API)
│   │   └── Migrations/          # Migrações do banco de dados relacional
│   │
│   ├── Repositories/            # Implementações EF Core (Exame, Paciente, Triagem, Usuario, Auditoria)
│   ├── Extensions/              # Injeção de dependências dos repositórios e serviços de domínio
│   └── Services/                # Serviços de infraestrutura externa
│
├── TriagemImagem.Tests
│   ├── Domain/                  # Testes unitários do domínio (CpfTests, DoseContrasteTests, SemaforoClinicoTests)
│   ├── Application/             # Testes dos casos de uso (CalculoDoseAppServiceTests, TriagemAppServiceTests)
│   └── Infrastructure/          # Testes de persistência
│
└── frontend/                    # 🎨 Layout do Posto de Triagem (HTML5 / Vanilla CSS / JS)
    ├── css/style.css            # Design System Glassmorphism Dark/Light
    ├── js/app.js                # Lógica da interface, atalhos de teclado [1=NÃO, 2=SIM, Tab] e calculadora
    ├── js/api.js                # Cliente HTTP mapeado 1-to-1 com os DTOs do backend
    └── index.html               # Estrutura visual da estação de trabalho
```
