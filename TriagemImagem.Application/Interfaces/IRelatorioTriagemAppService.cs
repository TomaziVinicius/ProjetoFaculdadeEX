using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Services;

namespace TriagemImagem.Application.Interfaces;

public interface IRelatorioTriagemAppService
{
    Task<string> GerarEArmazenarRelatorioAsync(Exame exame, FormularioTriagem formulario, ResultadoSemaforoClinico avaliacao, string nomeUsuario);
}
