using Microsoft.EntityFrameworkCore;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Infrastructure.Data.Context;

namespace TriagemImagem.Infrastructure.Repositories;

public class TriagemRepository : ITriagemRepository
{
    private readonly AppDbContext _context;

    public TriagemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FormularioTriagem?> ObterPorIdAsync(int id)
    {
        return await _context.FormulariosTriagem
            .Include(f => f.Respostas)
            .Include(f => f.UsuarioResponsavel)
            .Include(f => f.Exame)
                .ThenInclude(e => e.Paciente)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<FormularioTriagem?> ObterPorExameIdAsync(int exameId)
    {
        return await _context.FormulariosTriagem
            .Include(f => f.Respostas)
            .Include(f => f.UsuarioResponsavel)
            .Include(f => f.Exame)
                .ThenInclude(e => e.Paciente)
            .FirstOrDefaultAsync(f => f.ExameId == exameId);
    }

    public async Task SalvarAsync(FormularioTriagem formulario)
    {
        if (formulario.Id == 0)
        {
            await _context.FormulariosTriagem.AddAsync(formulario);
        }
        else
        {
            _context.FormulariosTriagem.Update(formulario);
        }
    }

    public Task AtualizarAsync(FormularioTriagem formulario)
    {
        _context.FormulariosTriagem.Update(formulario);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAlteracoesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
