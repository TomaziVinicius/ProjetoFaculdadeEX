using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Exceptions;

namespace TriagemImagem.Domain.ValueObjects;

/// <summary>
/// Value Object que encapsula o cálculo e as diretrizes clínicas de segurança para dosagem
/// de meios de contraste em Tomografia Computadorizada (TC - Iodado) e Ressonância Magnética (RM - Gadolínio).
/// </summary>
public sealed class DoseContraste : IEquatable<DoseContraste>
{
    public const decimal TetoMaximoIodadoTcMl = 150.0m;
    public const decimal TetoMaximoGadolinioRmMl = 25.0m;

    public decimal PesoKg { get; }
    public ModalidadeExame Modalidade { get; }
    public decimal FatorMlPorKg { get; }
    public decimal VolumeCalculadoMl { get; }
    public decimal VolumeAjustadoMl { get; }
    public bool TetoMaximoAtingido { get; }
    public string? AlertaClinico { get; }

    private DoseContraste(
        decimal pesoKg, 
        ModalidadeExame modalidade, 
        decimal fatorMlPorKg, 
        decimal volumeCalculadoMl, 
        decimal volumeAjustadoMl, 
        bool tetoMaximoAtingido, 
        string? alertaClinico)
    {
        PesoKg = pesoKg;
        Modalidade = modalidade;
        FatorMlPorKg = fatorMlPorKg;
        VolumeCalculadoMl = volumeCalculadoMl;
        VolumeAjustadoMl = volumeAjustadoMl;
        TetoMaximoAtingido = tetoMaximoAtingido;
        AlertaClinico = alertaClinico;
    }

    /// <summary>
    /// Calcula a dosagem segura de contraste iodado para exames de Tomografia Computadorizada.
    /// Padrão: 1.5 mL/kg, com teto de segurança clínica de 150 mL.
    /// </summary>
    public static DoseContraste CalcularParaTC(decimal pesoKg, decimal fatorMlPorKg = 1.5m)
    {
        ValidarParametros(pesoKg, fatorMlPorKg);

        var calculoBruto = Math.Round(pesoKg * fatorMlPorKg, 1);
        var tetoAtingido = calculoBruto > TetoMaximoIodadoTcMl;
        var volumeAjustado = tetoAtingido ? TetoMaximoIodadoTcMl : calculoBruto;

        string? alerta = null;
        if (tetoAtingido)
        {
            alerta = $"Dose calculada ({calculoBruto:F1} mL) excedeu o teto seguro padrão de {TetoMaximoIodadoTcMl:F1} mL de contraste iodado para adultos. Dose ajustada para o teto.";
        }

        return new DoseContraste(pesoKg, ModalidadeExame.TC, fatorMlPorKg, calculoBruto, volumeAjustado, tetoAtingido, alerta);
    }

    /// <summary>
    /// Calcula a dosagem segura de contraste paramagnético (Gadolínio) para Ressonância Magnética.
    /// Padrão: 0.2 mL/kg, com teto de segurança clínica de 25 mL.
    /// </summary>
    public static DoseContraste CalcularParaRM(decimal pesoKg, decimal fatorMlPorKg = 0.2m)
    {
        ValidarParametros(pesoKg, fatorMlPorKg);

        var calculoBruto = Math.Round(pesoKg * fatorMlPorKg, 1);
        var tetoAtingido = calculoBruto > TetoMaximoGadolinioRmMl;
        var volumeAjustado = tetoAtingido ? TetoMaximoGadolinioRmMl : calculoBruto;

        string? alerta = null;
        if (tetoAtingido)
        {
            alerta = $"Dose calculada ({calculoBruto:F1} mL) excedeu o teto seguro padrão de {TetoMaximoGadolinioRmMl:F1} mL de gadolínio. Dose ajustada para o teto de segurança.";
        }

        return new DoseContraste(pesoKg, ModalidadeExame.RM, fatorMlPorKg, calculoBruto, volumeAjustado, tetoAtingido, alerta);
    }

    private static void ValidarParametros(decimal pesoKg, decimal fatorMlPorKg)
    {
        if (pesoKg <= 0 || pesoKg > 400)
            throw new DomainException($"Peso inválido: {pesoKg} kg. O peso do paciente deve ser entre 0.1 kg e 400 kg.");

        if (fatorMlPorKg <= 0 || fatorMlPorKg > 5.0m)
            throw new DomainException($"Fator de dosagem por kg inválido: {fatorMlPorKg} mL/kg. Deve estar entre 0.01 e 5.0 mL/kg.");
    }

    public bool Equals(DoseContraste? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return PesoKg == other.PesoKg &&
               Modalidade == other.Modalidade &&
               VolumeAjustadoMl == other.VolumeAjustadoMl;
    }

    public override bool Equals(object? obj) =>
        obj is DoseContraste other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(PesoKg, Modalidade, VolumeAjustadoMl);

    public override string ToString() =>
        $"{VolumeAjustadoMl:F1} mL ({Modalidade} - {FatorMlPorKg} mL/kg p/ {PesoKg:F1} kg)";
}
