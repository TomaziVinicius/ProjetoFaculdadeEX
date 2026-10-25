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
│   ├── wwwroot/                 # Frontend estático entregue pela API
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

---

## 2. 📋 Escopo da Etapa 1 (Setup Inicial)

Neste primeiro passo, foi estruturada a **fundação limpa do projeto** para início do histórico de versionamento:

- [x] Solução Visual Studio (`TriagemImagem.sln`) configurada com os 5 projetos em .NET 8.
- [x] Árvore completa de diretórios conforme o modelo da Onion Architecture.
- [x] Modelagem do banco de dados relacional documentada no arquivo [`schema.sql`](schema.sql).
- [x] Entidades e enums essenciais do domínio mapeados (`Usuario`, `Paciente`, `Exame`, `FormularioTriagem`, `ItemRespostaTriagem`, `AuditoriaLog`).
- [x] Carcaça visual inicial do Frontend (`frontend/`) com o visual, paleta de cores e modo escuro/claro baseados no `laudos-oit`.
- [x] Arquivo `.gitignore` para .NET e Web.

---

## 3. 🗺️ Roadmap de Desenvolvimento (Próximos Passos)

1. **Passo 2**: Implementação das regras do domínio (`TriagemImagem.Domain`):
   - Checklist de Aviação (validação binária com travas impeditivas).
   - Semáforo Clínico (regras de transição Verde, Amarelo e Vermelho).
2. **Passo 3**: Casos de uso e DTOs na aplicação (`TriagemImagem.Application`):
   - Calculadora de dose de contraste e estimativa de clearance renal.
3. **Passo 4**: Persistência de dados (`TriagemImagem.Infrastructure`):
   - Mapeamentos Fluent API e repositórios.
4. **Passo 5**: Endpoints e Controllers RESTful (`TriagemImagem.Api`):
   - Rotas de autenticação, atendimento, triagem, liberação e auditoria.
5. **Passo 6**: Interface ergonômica completa (`frontend/`):
   - Grade Worklist com busca rápida, atalhos de teclado (teclas 1, 2 e Tab) e Semáforo periférico.
6. **Passo 7**: Testes automatizados (`TriagemImagem.Tests`).

---

## 4. 🚀 Como Enviar para o GitHub

Para subir esta estrutura inicial para o seu repositório remoto:

```powershell
# 1. Vincular o repositório remoto do GitHub:
git remote add origin https://github.com/SEU_USUARIO/SEU_REPOSITORIO.git

# 2. Definir a branch principal e enviar:
git branch -M main
git push -u origin main
```
