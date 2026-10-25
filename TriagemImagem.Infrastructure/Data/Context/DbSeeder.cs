using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Infrastructure.Data.Context;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        // Se já possui usuários cadastrados, não reinicializa
        if (context.Usuarios.Any()) return;

        // 1. Usuários Operacionais e Administrador
        var admin = new Usuario
        {
            Nome = "Administrador do Sistema",
            Login = "admin",
            SenhaUsuario = "123456",
            Perfil = PerfilUsuario.ADMIN,
            StatusUsuario = true,
            CriadoEm = DateTime.UtcNow
        };

        var biomedico = new Usuario
        {
            Nome = "Lucas Silva (Biomédico)",
            Login = "lucas.biomed",
            SenhaUsuario = "123456",
            Perfil = PerfilUsuario.BIOMEDICO,
            StatusUsuario = true,
            CriadoEm = DateTime.UtcNow
        };

        var tecnico = new Usuario
        {
            Nome = "Mariana Santos (Técnica)",
            Login = "mariana.tec",
            SenhaUsuario = "123456",
            Perfil = PerfilUsuario.TECNICO,
            StatusUsuario = true,
            CriadoEm = DateTime.UtcNow
        };

        context.Usuarios.AddRange(admin, biomedico, tecnico);
        context.SaveChanges();

        // 2. Pacientes
        var p1 = new Paciente
        {
            DocumentoCpf = "12345678901",
            NomeCompleto = "João Carlos de Oliveira",
            DataNascimento = new DateTime(1975, 4, 12),
            Sexo = "M",
            PesoKg = 78.50m,
            AlturaCm = 175,
            CriadoEm = DateTime.UtcNow
        };

        var p2 = new Paciente
        {
            DocumentoCpf = "98765432100",
            NomeCompleto = "Maria Aparecida Souza",
            DataNascimento = new DateTime(1962, 9, 28),
            Sexo = "F",
            PesoKg = 64.00m,
            AlturaCm = 160,
            CriadoEm = DateTime.UtcNow
        };

        var p3 = new Paciente
        {
            DocumentoCpf = "45678912344",
            NomeCompleto = "Carlos Eduardo Mendes",
            DataNascimento = new DateTime(1988, 11, 5),
            Sexo = "M",
            PesoKg = 82.00m,
            AlturaCm = 180,
            CriadoEm = DateTime.UtcNow
        };

        var p4 = new Paciente
        {
            DocumentoCpf = "78912345688",
            NomeCompleto = "Fernanda Lima Rocha",
            DataNascimento = new DateTime(1995, 2, 19),
            Sexo = "F",
            PesoKg = 58.00m,
            AlturaCm = 165,
            CriadoEm = DateTime.UtcNow
        };

        context.Pacientes.AddRange(p1, p2, p3, p4);
        context.SaveChanges();

        // 3. Exames Agendados do Dia
        var e1 = new Exame
        {
            PacienteId = p1.Id,
            Atendimento = 104520,
            CodigoProcedimento = "41001010",
            SequenciaInterna = 1,
            DescricaoExame = "Tomografia Computadorizada de Tórax com Contraste",
            Modalidade = ModalidadeExame.TC,
            Sala = "TC-01",
            HorarioAgendado = DateTime.Today.AddHours(8).AddMinutes(30),
            Status = StatusExame.AGUARDANDO_TRIAGEM,
            CriadoEm = DateTime.UtcNow
        };

        var e2 = new Exame
        {
            PacienteId = p2.Id,
            Atendimento = 104521,
            CodigoProcedimento = "41101014",
            SequenciaInterna = 1,
            DescricaoExame = "Ressonância Magnética de Crânio com Contraste",
            Modalidade = ModalidadeExame.RM,
            Sala = "RM-01",
            HorarioAgendado = DateTime.Today.AddHours(9).AddMinutes(0),
            Status = StatusExame.AGUARDANDO_TRIAGEM,
            CriadoEm = DateTime.UtcNow
        };

        var e3 = new Exame
        {
            PacienteId = p3.Id,
            Atendimento = 104522,
            CodigoProcedimento = "41001028",
            SequenciaInterna = 1,
            DescricaoExame = "Tomografia Computadorizada de Abdômen Total com Contraste",
            Modalidade = ModalidadeExame.TC,
            Sala = "TC-02",
            HorarioAgendado = DateTime.Today.AddHours(9).AddMinutes(40),
            Status = StatusExame.AGUARDANDO_TRIAGEM,
            CriadoEm = DateTime.UtcNow
        };

        var e4 = new Exame
        {
            PacienteId = p4.Id,
            Atendimento = 104523,
            CodigoProcedimento = "41101022",
            SequenciaInterna = 1,
            DescricaoExame = "Ressonância Magnética de Coluna Lombar",
            Modalidade = ModalidadeExame.RM,
            Sala = "RM-02",
            HorarioAgendado = DateTime.Today.AddHours(10).AddMinutes(15),
            Status = StatusExame.AGUARDANDO_TRIAGEM,
            CriadoEm = DateTime.UtcNow
        };

        context.Exames.AddRange(e1, e2, e3, e4);
        context.SaveChanges();
    }
}
