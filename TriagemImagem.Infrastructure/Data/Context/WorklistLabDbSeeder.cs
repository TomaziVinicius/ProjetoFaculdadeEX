using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;

namespace TriagemImagem.Infrastructure.Data.Context;

/// <summary>
/// Carga inicial do Banco de Dados 2 (Laboratório / HIS-RIS Worklist)
/// </summary>
public static class WorklistLabDbSeeder
{
    public static void Seed(WorklistLabDbContext context)
    {
        if (context.Pacientes.Any()) return;

        // 1. Pacientes cadastrados no Laboratório
        var p1 = new WorklistPacienteLab
        {
            DocumentoCpf = "78912345688",
            NomeCompleto = "Fernanda Lima Rocha",
            DataNascimento = new DateTime(1995, 2, 19),
            Sexo = "F",
            PesoKg = 58.00m,
            AlturaCm = 165,
            Telefone = "(44) 99881-2233",
            CriadoEm = DateTime.UtcNow
        };

        var p2 = new WorklistPacienteLab
        {
            DocumentoCpf = "45678912344",
            NomeCompleto = "Carlos Eduardo Mendes",
            DataNascimento = new DateTime(1988, 11, 5),
            Sexo = "M",
            PesoKg = 82.00m,
            AlturaCm = 180,
            Telefone = "(44) 99772-3344",
            CriadoEm = DateTime.UtcNow
        };

        var p3 = new WorklistPacienteLab
        {
            DocumentoCpf = "98765432100",
            NomeCompleto = "Maria Aparecida Souza",
            DataNascimento = new DateTime(1962, 9, 28),
            Sexo = "F",
            PesoKg = 64.00m,
            AlturaCm = 160,
            Telefone = "(44) 99663-4455",
            CriadoEm = DateTime.UtcNow
        };

        var p4 = new WorklistPacienteLab
        {
            DocumentoCpf = "12345678901",
            NomeCompleto = "João Carlos de Oliveira",
            DataNascimento = new DateTime(1975, 4, 12),
            Sexo = "M",
            PesoKg = 78.50m,
            AlturaCm = 175,
            Telefone = "(44) 99554-5566",
            CriadoEm = DateTime.UtcNow
        };

        var p5 = new WorklistPacienteLab
        {
            DocumentoCpf = "32165498722",
            NomeCompleto = "Lucas Ferreira da Silva",
            DataNascimento = new DateTime(1990, 7, 22),
            Sexo = "M",
            PesoKg = 74.00m,
            AlturaCm = 172,
            Telefone = "(44) 99445-6677",
            CriadoEm = DateTime.UtcNow
        };

        context.Pacientes.AddRange(p1, p2, p3, p4, p5);
        context.SaveChanges();

        // 2. Pedidos de Exames Agendados na Worklist
        var pedidos = new List<WorklistPedidoLab>
        {
            // Formulário Especializado 1: QUESTIONÁRIO DE RM DA COLUNA
            new WorklistPedidoLab
            {
                PacienteId = p1.Id,
                Atendimento = 104520,
                Modalidade = ModalidadeExame.RM,
                CodigoProcedimento = "41101022",
                DescricaoProcedimento = "Ressonância Magnética de Coluna Lombar",
                TipoFormulario = "RM_COLUNA",
                SegmentoLado = "Lombar",
                PreparoAdequado = true,
                MotivoExame = "Lombociatalgia crônica refratária com irradiação para membro inferior esquerdo. Suspeita de hérnia discal.",
                MedicoSolicitante = "Dr. Marcos Valério",
                CrmMedico = "CRM/PR 24150",
                Convenio = "Unimed",
                Sala = "RM-01",
                HorarioAgendado = DateTime.Today.AddHours(8).AddMinutes(0),
                Status = "AGENDADO",
                CriadoEm = DateTime.UtcNow
            },

            // Formulário Especializado 2: QUESTIONÁRIO DE RM DE BRAÇO / ANTEBRAÇO
            new WorklistPedidoLab
            {
                PacienteId = p2.Id,
                Atendimento = 104521,
                Modalidade = ModalidadeExame.RM,
                CodigoProcedimento = "41101030",
                DescricaoProcedimento = "Ressonância Magnética de Braço e Antebraço",
                TipoFormulario = "RM_BRACO_ANTEBRACO",
                SegmentoLado = "Direito",
                PreparoAdequado = true,
                MotivoExame = "Queda com trauma esportivo em membro superior direito. Dor persistente e perda de força de preensão manual.",
                MedicoSolicitante = "Dra. Camila Toledo",
                CrmMedico = "CRM/PR 31200",
                Convenio = "Bradesco Saúde",
                Sala = "RM-02",
                HorarioAgendado = DateTime.Today.AddHours(8).AddMinutes(45),
                Status = "AGENDADO",
                CriadoEm = DateTime.UtcNow
            },

            // Formulário Especializado 3: QUESTIONÁRIO DE RM DE BACIA / QUADRIL
            new WorklistPedidoLab
            {
                PacienteId = p3.Id,
                Atendimento = 104522,
                Modalidade = ModalidadeExame.RM,
                CodigoProcedimento = "41101049",
                DescricaoProcedimento = "Ressonância Magnética de Bacia e Articulação do Quadril",
                TipoFormulario = "RM_BACIA_QUADRIL",
                SegmentoLado = "Esquerdo",
                PreparoAdequado = true,
                MotivoExame = "Dor profunda articular em quadril esquerdo. Paciente em uso crônico de corticoide. Investigar necrose avascular.",
                MedicoSolicitante = "Dr. Roberto Silveira",
                CrmMedico = "CRM/PR 19850",
                Convenio = "Paraná Clínicas",
                Sala = "RM-01",
                HorarioAgendado = DateTime.Today.AddHours(9).AddMinutes(30),
                Status = "AGENDADO",
                CriadoEm = DateTime.UtcNow
            },

            // Exame TC: QUESTIONÁRIO DE TOMOGRAFIA DE TÓRAX
            new WorklistPedidoLab
            {
                PacienteId = p4.Id,
                Atendimento = 104523,
                Modalidade = ModalidadeExame.TC,
                CodigoProcedimento = "41001010",
                DescricaoProcedimento = "Tomografia Computadorizada de Tórax com Contraste",
                TipoFormulario = "TC_TORAX",
                SegmentoLado = "Tórax",
                PreparoAdequado = true,
                MotivoExame = "Nódulo pulmonar em lobo superior direito visualizado em radiografia prévia. Protocolo com contraste iodado.",
                MedicoSolicitante = "Dra. Beatriz Fontana",
                CrmMedico = "CRM/PR 28900",
                Convenio = "Unimed",
                Sala = "TC-01",
                HorarioAgendado = DateTime.Today.AddHours(10).AddMinutes(15),
                Status = "AGENDADO",
                CriadoEm = DateTime.UtcNow
            },

            // Exame TC: QUESTIONÁRIO DE TOMOGRAFIA DE ABDÔMEN TOTAL
            new WorklistPedidoLab
            {
                PacienteId = p5.Id,
                Atendimento = 104524,
                Modalidade = ModalidadeExame.TC,
                CodigoProcedimento = "41001028",
                DescricaoProcedimento = "Tomografia Computadorizada de Abdômen Total com Contraste",
                TipoFormulario = "TC_ABDOMEN",
                SegmentoLado = "Abdômen Total",
                PreparoAdequado = true,
                MotivoExame = "Dor aguda intensa em fossa ilíaca direita há 18 horas. Descartar apendicite aguda.",
                MedicoSolicitante = "Dr. Henrique Rossi",
                CrmMedico = "CRM/PR 35400",
                Convenio = "Particular",
                Sala = "TC-02",
                HorarioAgendado = DateTime.Today.AddHours(11).AddMinutes(0),
                Status = "AGENDADO",
                CriadoEm = DateTime.UtcNow
            }
        };

        context.Pedidos.AddRange(pedidos);
        context.SaveChanges();
    }
}
