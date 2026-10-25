using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Infrastructure.Data.Context;

namespace TriagemImagem.Infrastructure.Repositories;

public class PacienteRepository : IPacienteRepository
{
    private readonly AppDbContext _context;

    public PacienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Paciente?> ObterPorIdAsync(int id)
    {
        return await _context.Pacientes.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Paciente?> ObterPorCpfAsync(string cpf)
    {
        var limpo = Regex.Replace(cpf ?? string.Empty, @"[^\d]", "");
        return await _context.Pacientes
            .FirstOrDefaultAsync(p => p.DocumentoCpf == limpo || p.DocumentoCpf == cpf);
    }

    public async Task<IEnumerable<Paciente>> BuscarPorNomeAsync(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Enumerable.Empty<Paciente>();

        var termo = nome.Trim().ToLower();
        return await _context.Pacientes
            .AsNoTracking()
            .Where(p => p.NomeCompleto.ToLower().Contains(termo))
            .OrderBy(p => p.NomeCompleto)
            .Take(20)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Paciente paciente)
    {
        await _context.Pacientes.AddAsync(paciente);
    }

    public Task AtualizarAsync(Paciente paciente)
    {
        _context.Pacientes.Update(paciente);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAlteracoesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
