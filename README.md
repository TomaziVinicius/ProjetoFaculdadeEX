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

---

## 2. 📊 Status Atual do Projeto (O que já está 100% pronto)

| Componente | Detalhes Implementados | Status |
| :--- | :--- | :---: |
| **Banco de Dados MySQL** | Mapeamento relacional completo (`schema.sql`), Fluent API no EF Core (`AppDbContext`), Carga Inicial (`DbSeeder`). | ✅ Concluído |
| **Domínio Clínico** | Value Objects `Cpf` (validação matemática) e `DoseContraste` (cálculo por kg e tetos máximos para TC/RM), `SemaforoClinicoDomainService` (regras puras do Checklist de Aviação: Verde, Amarelo e Vermelho/Bloqueado). | ✅ Concluído |
| **Repositórios EF Core** | `ExameRepository`, `PacienteRepository`, `TriagemRepository`, `UsuarioRepository` e `AuditoriaRepository` implementados e registrados na DI. | ✅ Concluído |
| **Camada Application** | Todos os DTOs de entrada e saída, Validadores (`TriagemValidator`, `DoseContrasteValidator`), Serviços de Aplicação (`TriagemAppService`, `WorklistAppService`, `CalculoDoseAppService`, `AuthAppService`, `AuditoriaAppService`). | ✅ Concluído |
| **Testes Unitários** | Testes xUnit automatizados para Domínio e Aplicação em `TriagemImagem.Tests`. | ✅ Concluído |
| **Frontend Ergonômico** | Worklist com busca e filtros, Ficha de Triagem com Checklist de Aviação, Semáforo Dinâmico, Calculadora de Dosagem em tempo real e cliente HTTP `api.js`. | ✅ Concluído |

---

## 3. 🎯 Guia para o Desenvolvedor da API (O que precisa ser feito)

Se você assumiu o desenvolvimento da **camada de API (`TriagemImagem.Api`)**, o seu trabalho já está 90% adiantado! Toda a lógica de negócios, banco de dados, regras clínicas e validações **já estão injetadas e prontas para uso**.

Sua responsabilidade principal é implementar os **Controllers HTTP REST** e o **Tratamento de Erros/CORS** em `TriagemImagem.Api`.

### Passo A: Habilitar o CORS no `Program.cs`
Para que o navegador consiga fazer requisições do frontend para a API sem bloqueio de segurança:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// E antes de app.MapControllers():
app.UseCors("AllowAll");
```

---

### Passo B: Criar os Controllers em `TriagemImagem.Api/Controllers/`

Basta injetar as interfaces da camada `Application` (elas já estão registradas na Injeção de Dependências do .NET):

#### 1. `AuthController.cs` (`/api/v1/auth`)
* Injetar: `IAuthAppService`
* Endpoints:
  * `POST /api/v1/auth/login`: recebe `LoginRequestDto` e chama `_authService.AutenticarAsync(dto)`.

#### 2. `WorklistController.cs` (`/api/v1/worklist`)
* Injetar: `IWorklistAppService`
* Endpoints:
  * `GET /api/v1/worklist`: recebe query params de `FiltroWorklistDto` e chama `_worklistService.ObterWorklistAsync(filtro)`.
  * `GET /api/v1/worklist/{id}`: chama `_worklistService.ObterExamePorIdAsync(id)`.
  * `POST /api/v1/worklist/{id}/chamar`: recebe `{ usuarioId }` no corpo e chama `_worklistService.ChamarPacienteAsync(id, usuarioId)`.

#### 3. `TriagemController.cs` (`/api/v1/triagem`)
* Injetar: `ITriagemAppService` e `ICalculoDoseAppService`
* Endpoints:
  * `POST /api/v1/triagem`: recebe `SalvarTriagemRequestDto` e chama `_triagemService.SalvarTriagemAsync(dto)`.
  * `GET /api/v1/triagem/exame/{exameId}`: chama `_triagemService.ObterPorExameIdAsync(exameId)`.
  * `POST /api/v1/triagem/calcular-dose`: recebe `CalcularDoseRequestDto` e chama `_doseService.CalcularDose(dto)`.
  * `POST /api/v1/triagem/{id}/liberar`: recebe `{ usuarioId, observacao }` e chama `_triagemService.LiberarExameAsync(id, usuarioId, observacao)`.
  * `POST /api/v1/triagem/{id}/cancelar`: recebe `{ usuarioId, motivo }` e chama `_triagemService.CancelarExameAsync(id, usuarioId, motivo)`.

#### 4. `AuditoriaController.cs` (`/api/v1/auditoria`)
* Injetar: `IAuditoriaAppService`
* Endpoints:
  * `GET /api/v1/auditoria?limite=50`: chama `_auditoriaService.ObterUltimosLogsAsync(limite)`.

---

### Passo C: Criar o Middleware de Tratamento Global de Erros
Crie em `TriagemImagem.Api/Middlewares/ExceptionHandlingMiddleware.cs`:
* Captura `DomainException`: retorna **HTTP 400 Bad Request** com `{ "success": false, "message": ex.Message }`;
* Captura `Exception` genérica: loga o erro e retorna **HTTP 500 Internal Server Error** com `{ "success": false, "message": "Ocorreu um erro interno no servidor." }`.

---

## 4. 🚀 Como Rodar o Projeto

### Pré-requisitos:
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado;
* MySQL ou MariaDB rodando (porta 3306, usuário `root`, sem senha, ou configure sua senha no `appsettings.json`).

### Executando o Backend (API):
1. No terminal, na raiz do projeto:
   ```bash
   dotnet run --project TriagemImagem.Api
   ```
2. O Entity Framework criará automaticamente as tabelas e fará o seed inicial de dados se o banco estiver vazio.
3. Acesse a documentação interativa Swagger:
   * **URL:** `http://localhost:5000/swagger` ou `https://localhost:5001/swagger`

### Executando os Testes Automatizados:
```bash
dotnet test
```

### Executando o Frontend:
1. Abra o arquivo `frontend/index.html` diretamente no navegador (ou utilize a extensão *Live Server* do VS Code).
2. O frontend já conta com logins rápidos para teste:
   * **Biomédico:** `lucas.biomed` (senha: `123456`)
   * **Técnico:** `mariana.tec` (senha: `123456`)
   * **Admin:** `admin` (senha: `admin123`)
3. A interface conta com atalhos ergonômicos de teclado na triagem:
   * **[1]**: Seleciona resposta **NÃO** e avança para a próxima pergunta;
   * **[2]**: Seleciona resposta **SIM** e avança para a próxima pergunta;
   * **[Tab]**: Navega entre as perguntas do checklist.
