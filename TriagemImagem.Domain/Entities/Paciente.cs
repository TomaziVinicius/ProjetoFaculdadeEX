namespace TriagemImagem.Domain.Entities;

public class Paciente
{
    public int Id { get; set; }
    public string DocumentoCpf { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string Sexo { get; set; } = "M"; // M, F, O
    public decimal? PesoKg { get; set; }
    public int? AlturaCm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public int ObterIdade()
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - DataNascimento.Year;
        if (DataNascimento.Date > hoje.AddYears(-idade)) idade--;
        return idade;
    }
}
