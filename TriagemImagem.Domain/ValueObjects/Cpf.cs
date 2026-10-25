using System.Text.RegularExpressions;
using TriagemImagem.Domain.Exceptions;

namespace TriagemImagem.Domain.ValueObjects;

/// <summary>
/// Value Object que representa e valida o Cadastro de Pessoa Física (CPF).
/// Garante que nenhuma entidade exista com CPF em formato inválido ou com dígitos verificadores incorretos.
/// </summary>
public sealed class Cpf : IEquatable<Cpf>
{
    public string Valor { get; }

    public string ValorFormatado =>
        Valor.Length == 11
            ? $"{Valor[..3]}.{Valor.Substring(3, 3)}.{Valor.Substring(6, 3)}-{Valor.Substring(9, 2)}"
            : Valor;

    public Cpf(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("O número de CPF é obrigatório.");

        var apenasNumeros = Regex.Replace(valor, @"[^\d]", "");

        if (!Validar(apenasNumeros))
            throw new DomainException($"O CPF informado '{valor}' é inválido.");

        Valor = apenasNumeros;
    }

    public static bool Validar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return false;

        var digitos = Regex.Replace(valor, @"[^\d]", "");

        if (digitos.Length != 11)
            return false;

        // Elimina CPFs conhecidos com todos os números iguais (ex: 00000000000, 11111111111)
        if (new string(digitos[0], 11) == digitos)
            return false;

        // Cálculo do primeiro dígito verificador
        var multiplicador1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var soma = 0;
        for (var i = 0; i < 9; i++)
            soma += int.Parse(digitos[i].ToString()) * multiplicador1[i];

        var resto = soma % 11;
        var primeiroDigito = resto < 2 ? 0 : 11 - resto;

        if (int.Parse(digitos[9].ToString()) != primeiroDigito)
            return false;

        // Cálculo do segundo dígito verificador
        var multiplicador2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        soma = 0;
        for (var i = 0; i < 10; i++)
            soma += int.Parse(digitos[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        var segundoDigito = resto < 2 ? 0 : 11 - resto;

        return int.Parse(digitos[10].ToString()) == segundoDigito;
    }

    public static implicit operator string(Cpf cpf) => cpf.Valor;
    public static explicit operator Cpf(string valor) => new(valor);

    public bool Equals(Cpf? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Valor == other.Valor;
    }

    public override bool Equals(object? obj) =>
        obj is Cpf other && Equals(other);

    public override int GetHashCode() =>
        Valor.GetHashCode();

    public override string ToString() =>
        ValorFormatado;
}
