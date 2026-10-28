namespace TriagemImagem.Tests.Api;

public static class TestDataFactory
{
    public static object LoginInvalido() => new { login = "teste.api.invalido", senha = "senha-incorreta" };
    public static object CalculoDoseTcValido() => new { modalidade = 1, pesoKg = 80m, fatorMlPorKg = 1.5m };
    public static object CalculoDosePesoInvalido() => new { modalidade = 1, pesoKg = -1m, fatorMlPorKg = 1.5m };
    public static object TriagemSemRespostas() => new { exameId = 1, usuarioResponsavelId = 1, respostas = Array.Empty<object>() };
    public static object TriagemParaExameInexistente() => new { exameId = 999999, usuarioResponsavelId = 1, pesoKg = 70m, respostas = new[] { new { codigoPergunta = "TESTE-01", perguntaEnunciado = "Entrada de teste", respostaValor = "NAO", impeditivo = false } } };
}
