namespace TriagemImagem.Domain.Exceptions;

/// <summary>
/// Exceção padrão disparada por violações de regras clínicas no núcleo do domínio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
