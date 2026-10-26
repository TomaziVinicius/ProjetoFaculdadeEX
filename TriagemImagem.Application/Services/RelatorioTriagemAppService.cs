using System.Text;
using TriagemImagem.Application.Interfaces;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Services;

namespace TriagemImagem.Application.Services;

public class RelatorioTriagemAppService : IRelatorioTriagemAppService
{
    public async Task<string> GerarEArmazenarRelatorioAsync(
        Exame exame, 
        FormularioTriagem formulario, 
        ResultadoSemaforoClinico avaliacao, 
        string nomeUsuario)
    {
        // 1. Determina o diretório de destino na pasta Documentos do usuário
        var pastaDocumentos = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "TriagensDiagnosticoImagem");

        if (!Directory.Exists(pastaDocumentos))
        {
            Directory.CreateDirectory(pastaDocumentos);
        }

        // 2. Determina o título específico com base no tipo de questionário da folha real
        var tipoForm = exame.TipoFormulario ?? (exame.Modalidade == ModalidadeExame.RM ? "RM_COLUNA" : "TC_TORAX");
        var tituloQuestionario = tipoForm switch
        {
            "RM_COLUNA" => "QUESTIONÁRIO PARA RESSONÂNCIA DA COLUNA",
            "RM_BRACO_ANTEBRACO" => "QUESTIONÁRIO PARA RESSONÂNCIA DE BRAÇO/ANTEBRAÇO",
            "RM_BACIA_QUADRIL" => "QUESTIONÁRIO PARA RESSONÂNCIA DE BACIA/QUADRIL",
            "TC_TORAX" => "QUESTIONÁRIO DE SEGURANÇA PARA TOMOGRAFIA DE TÓRAX",
            "TC_ABDOMEN" => "QUESTIONÁRIO DE SEGURANÇA PARA TOMOGRAFIA DE ABDÔMEN",
            _ => "QUESTIONÁRIO DE TRIAGEM CLÍNICA PARA DIAGNÓSTICO POR IMAGEM"
        };

        var statusCor = avaliacao.StatusRisco switch
        {
            StatusRiscoSemaforo.VERDE => "#10b981",
            StatusRiscoSemaforo.AMARELO => "#f59e0b",
            StatusRiscoSemaforo.VERMELHO => "#ef4444",
            _ => "#6b7280"
        };

        var statusTexto = avaliacao.StatusRisco switch
        {
            StatusRiscoSemaforo.VERDE => "VERDE - LIBERADO PARA EXAME",
            StatusRiscoSemaforo.AMARELO => "AMARELO - ATENÇÃO / PROTOCOLO ESPECIAL",
            StatusRiscoSemaforo.VERMELHO => "VERMELHO - CONTRAINDICAÇÃO ABSOLUTA / BLOQUEADO",
            _ => "NÃO AVALIADO"
        };

        // 3. Monta o HTML timbrado
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"pt-BR\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine($"  <title>{tituloQuestionario} - Atendimento {exame.Atendimento}</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    * { box-sizing: border-box; margin: 0; padding: 0; }");
        sb.AppendLine("    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #1e293b; background: #fff; padding: 24px; line-height: 1.5; font-size: 13px; }");
        sb.AppendLine("    .page-container { max-width: 820px; margin: 0 auto; border: 1px solid #cbd5e1; padding: 32px; border-radius: 8px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); }");
        sb.AppendLine("    .header { display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #0284c7; padding-bottom: 16px; margin-bottom: 20px; }");
        sb.AppendLine("    .header-logo-text { font-size: 18px; font-weight: 800; color: #0369a1; text-transform: uppercase; letter-spacing: 0.5px; }");
        sb.AppendLine("    .header-contact { text-align: right; font-size: 11px; color: #64748b; }");
        sb.AppendLine("    .doc-title { text-align: center; font-size: 16px; font-weight: 700; color: #0f172a; margin-bottom: 20px; text-transform: uppercase; background: #f8fafc; padding: 10px; border: 1px solid #e2e8f0; border-radius: 4px; }");
        sb.AppendLine("    .section-box { border: 1px solid #e2e8f0; border-radius: 6px; padding: 14px; margin-bottom: 16px; background: #fdfdfd; }");
        sb.AppendLine("    .section-title { font-size: 12px; font-weight: 700; color: #0369a1; text-transform: uppercase; margin-bottom: 10px; border-bottom: 1px solid #e2e8f0; padding-bottom: 4px; }");
        sb.AppendLine("    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }");
        sb.AppendLine("    .grid-3 { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 10px; }");
        sb.AppendLine("    .info-label { font-size: 11px; color: #64748b; text-transform: uppercase; font-weight: 600; }");
        sb.AppendLine("    .info-value { font-size: 13px; font-weight: 600; color: #0f172a; }");
        sb.AppendLine("    .table-questions { width: 100%; border-collapse: collapse; margin-top: 10px; }");
        sb.AppendLine("    .table-questions th, .table-questions td { border: 1px solid #cbd5e1; padding: 8px 10px; text-align: left; }");
        sb.AppendLine("    .table-questions th { background: #f1f5f9; font-size: 11px; text-transform: uppercase; color: #334155; }");
        sb.AppendLine("    .badge-sim { display: inline-block; padding: 2px 8px; border-radius: 4px; font-weight: 700; font-size: 11px; background: #fee2e2; color: #991b1b; }");
        sb.AppendLine("    .badge-nao { display: inline-block; padding: 2px 8px; border-radius: 4px; font-weight: 700; font-size: 11px; background: #dcfce7; color: #166534; }");
        sb.AppendLine("    .semaforo-banner { border-left: 6px solid " + statusCor + "; background: #f8fafc; padding: 14px; border-radius: 4px; margin-bottom: 16px; display: flex; align-items: center; justify-content: space-between; }");
        sb.AppendLine("    .footer-signature { margin-top: 32px; border-top: 1px dashed #94a3b8; padding-top: 20px; display: flex; justify-content: space-between; align-items: flex-end; }");
        sb.AppendLine("    .sign-box { text-align: center; width: 260px; border-top: 1px solid #334155; padding-top: 6px; font-size: 11px; }");
        sb.AppendLine("    @media print {");
        sb.AppendLine("      body { padding: 0; background: #fff; }");
        sb.AppendLine("      .page-container { border: none; box-shadow: none; padding: 0; max-width: 100%; }");
        sb.AppendLine("    }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <div class=\"page-container\">");
        sb.AppendLine("    <div class=\"header\">");
        sb.AppendLine("      <div>");
        sb.AppendLine("        <div class=\"header-logo-text\">Centro de Diagnóstico por Imagem - UNICIVE</div>");
        sb.AppendLine("        <div style=\"font-size: 12px; color: #475569;\">Projeto Acadêmico Faculdade UNICIVE - Serviço de TC &amp; RM</div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("      <div class=\"header-contact\">");
        sb.AppendLine("        <div>Sistema de Triagem Clínica</div>");
        sb.AppendLine("        <div>Ambiente Acadêmico de Demonstração</div>");
        sb.AppendLine("        <div>Checklist de Segurança Radiológica</div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("    </div>");

        sb.AppendLine($"    <div class=\"doc-title\">{tituloQuestionario}</div>");

        // Identificação do Paciente e Exame
        sb.AppendLine("    <div class=\"section-box\">");
        sb.AppendLine("      <div class=\"section-title\">Identificação do Paciente e Atendimento</div>");
        sb.AppendLine("      <div class=\"grid-3\" style=\"margin-bottom: 10px;\">");
        sb.AppendLine($"        <div><div class=\"info-label\">Paciente</div><div class=\"info-value\">{exame.Paciente.NomeCompleto}</div></div>");
        sb.AppendLine($"        <div><div class=\"info-label\">Atendimento</div><div class=\"info-value\">#{exame.Atendimento}</div></div>");
        sb.AppendLine($"        <div><div class=\"info-label\">CPF</div><div class=\"info-value\">{exame.Paciente.DocumentoCpf}</div></div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("      <div class=\"grid-3\">");
        sb.AppendLine($"        <div><div class=\"info-label\">Idade / Sexo / Peso</div><div class=\"info-value\">{exame.Paciente.ObterIdade()} anos | {exame.Paciente.Sexo} | {(formulario.Exame?.Paciente?.PesoKg ?? exame.Paciente.PesoKg)} kg</div></div>");
        sb.AppendLine($"        <div><div class=\"info-label\">Procedimento</div><div class=\"info-value\">{exame.DescricaoExame}</div></div>");
        sb.AppendLine($"        <div><div class=\"info-label\">Modalidade / Sala</div><div class=\"info-value\">{exame.Modalidade} - Sala {exame.Sala}</div></div>");
        sb.AppendLine("      </div>");
        if (!string.IsNullOrWhiteSpace(exame.SegmentoLado) || !string.IsNullOrWhiteSpace(exame.MotivoExame))
        {
            sb.AppendLine("      <div class=\"grid-2\" style=\"margin-top: 10px; border-top: 1px dotted #e2e8f0; padding-top: 8px;\">");
            sb.AppendLine($"        <div><div class=\"info-label\">Segmento / Lado Examinado</div><div class=\"info-value\">{exame.SegmentoLado ?? "Geral"}</div></div>");
            sb.AppendLine($"        <div><div class=\"info-label\">Motivo Clínico do Exame</div><div class=\"info-value\">{exame.MotivoExame ?? "Investigação diagnóstica"}</div></div>");
            sb.AppendLine("      </div>");
        }
        sb.AppendLine("    </div>");

        // Semáforo Clínico
        sb.AppendLine("    <div class=\"semaforo-banner\">");
        sb.AppendLine("      <div>");
        sb.AppendLine("        <div class=\"info-label\">Avaliação de Risco Clínico (Semáforo da Aviação)</div>");
        sb.AppendLine($"        <div style=\"font-size: 15px; font-weight: 800; color: {statusCor};\">{statusTexto}</div>");
        if (avaliacao.MotivosContraindicacao.Any())
        {
            sb.AppendLine($"        <div style=\"font-size: 11px; color: #991b1b; margin-top: 4px;\"><strong>Impedimentos:</strong> {string.Join(" | ", avaliacao.MotivosContraindicacao)}</div>");
        }
        else if (avaliacao.AlertasAtencao.Any())
        {
            sb.AppendLine($"        <div style=\"font-size: 11px; color: #b45309; margin-top: 4px;\"><strong>Alertas:</strong> {string.Join(" | ", avaliacao.AlertasAtencao)}</div>");
        }
        sb.AppendLine("      </div>");
        sb.AppendLine($"      <div style=\"text-align: right;\"><div class=\"info-label\">Status do Exame</div><div class=\"info-value\">{exame.Status}</div></div>");
        sb.AppendLine("    </div>");

        // Respostas do Questionário
        sb.AppendLine("    <div class=\"section-box\">");
        sb.AppendLine($"      <div class=\"section-title\">Itens do Questionário de Segurança ({formulario.Respostas.Count} Questões)</div>");
        sb.AppendLine("      <table class=\"table-questions\">");
        sb.AppendLine("        <thead>");
        sb.AppendLine("          <tr>");
        sb.AppendLine("            <th style=\"width: 60px;\">Item</th>");
        sb.AppendLine("            <th>Enunciado Clínico</th>");
        sb.AppendLine("            <th style=\"width: 90px; text-align: center;\">Resposta</th>");
        sb.AppendLine("            <th style=\"width: 90px; text-align: center;\">Classificação</th>");
        sb.AppendLine("          </tr>");
        sb.AppendLine("        </thead>");
        sb.AppendLine("        <tbody>");

        int index = 1;
        foreach (var r in formulario.Respostas)
        {
            var isSim = r.RespostaValor.Equals("SIM", StringComparison.OrdinalIgnoreCase);
            var badgeClass = isSim ? "badge-sim" : "badge-nao";
            var badgeTexto = isSim ? "SIM" : "NÃO";
            var tipoRisco = r.Impeditivo ? "<span style=\"color: #dc2626; font-weight: 700;\">Impeditivo</span>" : "<span style=\"color: #64748b;\">Monitoramento</span>";

            sb.AppendLine("          <tr>");
            sb.AppendLine($"            <td style=\"font-weight: 700; color: #0284c7;\">Q{index++}</td>");
            sb.AppendLine($"            <td>{r.PerguntaEnunciado}</td>");
            sb.AppendLine($"            <td style=\"text-align: center;\"><span class=\"{badgeClass}\">{badgeTexto}</span></td>");
            sb.AppendLine($"            <td style=\"text-align: center; font-size: 11px;\">{tipoRisco}</td>");
            sb.AppendLine("          </tr>");
        }
        sb.AppendLine("        </tbody>");
        sb.AppendLine("      </table>");
        sb.AppendLine("    </div>");

        // Contraste e Observações
        if (formulario.VolumeContrasteMl.HasValue || !string.IsNullOrWhiteSpace(formulario.ObservacoesClinicas))
        {
            sb.AppendLine("    <div class=\"section-box\">");
            sb.AppendLine("      <div class=\"section-title\">Administração de Contraste e Observações</div>");
            sb.AppendLine("      <div class=\"grid-3\">");
            sb.AppendLine($"        <div><div class=\"info-label\">Volume de Contraste</div><div class=\"info-value\">{(formulario.VolumeContrasteMl.HasValue ? $"{formulario.VolumeContrasteMl.Value:F1} mL" : "Não Administrado")}</div></div>");
            sb.AppendLine($"        <div><div class=\"info-label\">Lote do Meio de Contraste</div><div class=\"info-value\">{formulario.LoteContraste ?? "N/A"}</div></div>");
            sb.AppendLine($"        <div><div class=\"info-label\">Data/Hora da Triagem</div><div class=\"info-value\">{DateTime.Now:dd/MM/yyyy HH:mm}</div></div>");
            sb.AppendLine("      </div>");
            if (!string.IsNullOrWhiteSpace(formulario.ObservacoesClinicas))
            {
                sb.AppendLine("      <div style=\"margin-top: 10px; border-top: 1px dotted #e2e8f0; padding-top: 8px;\">");
                sb.AppendLine($"        <div class=\"info-label\">Observações Clínicas / Recomendações</div><div style=\"font-size: 12px; color: #334155; margin-top: 2px;\">{formulario.ObservacoesClinicas}</div>");
                sb.AppendLine("      </div>");
            }
            sb.AppendLine("    </div>");
        }

        // Rodapé com Assinatura e Carimbo
        sb.AppendLine("    <div class=\"footer-signature\">");
        sb.AppendLine("      <div>");
        sb.AppendLine($"        <div style=\"font-size: 11px; color: #64748b;\">Arquivo Clínico Armazenado em Documentos</div>");
        sb.AppendLine($"        <div style=\"font-family: monospace; font-size: 10px; color: #475569;\">ID Transação: {Guid.NewGuid():D}</div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("      <div class=\"sign-box\">");
        sb.AppendLine($"        <div style=\"font-weight: 700; color: #0f172a;\">{nomeUsuario}</div>");
        sb.AppendLine("        <div>Equipe de Biomedicina / Triagem</div>");
        sb.AppendLine("        <div style=\"color: #64748b;\">Assinatura Digital Registrada</div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("    </div>");

        sb.AppendLine("  </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        // 4. Salva o arquivo no disco do usuário
        var nomeArquivo = $"Ficha_Triagem_{exame.Atendimento}_{tipoForm}_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        var caminhoCompleto = Path.Combine(pastaDocumentos, nomeArquivo);

        await File.WriteAllTextAsync(caminhoCompleto, sb.ToString(), Encoding.UTF8);

        return caminhoCompleto;
    }
}
