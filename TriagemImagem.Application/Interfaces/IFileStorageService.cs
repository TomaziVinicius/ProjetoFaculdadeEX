namespace TriagemImagem.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Salva o conteúdo do relatório/ficha clínica no diretório configurado (ex: \\fsa\anexos\IMI\Anexos_Formularios_PROJ).
    /// Suporta gravação direta via sistema de arquivos (Windows) e gravação gerenciada via protocolo SMB2/3 (Linux/Rede).
    /// </summary>
    /// <param name="nomeArquivo">Nome do arquivo gerado (ex: Ficha_Triagem_2051062_RM_COLUNA_20261030_101454.html)</param>
    /// <param name="conteudoHtml">Conteúdo HTML timbrado do relatório</param>
    /// <returns>Caminho canônico do arquivo armazenado</returns>
    Task<string> SalvarArquivoAsync(string nomeArquivo, string conteudoHtml);
}
