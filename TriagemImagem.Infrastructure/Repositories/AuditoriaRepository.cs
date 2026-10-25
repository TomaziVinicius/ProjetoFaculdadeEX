using Microsoft.EntityFrameworkCore;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Infrastructure.Data.Context;

namespace TriagemImagem.Infrastructure.Repositories;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly AppDbContext _context;

    public AuditoriaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarLogAsync(AuditoriaLog log)
    {
        await _context.AuditoriaLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditoriaLog>> ListarUltimosLogsAsync(int limite = 50)
    {
        return await _context.AuditoriaLogs
            .Include(a => a.Usuario)
            .AsNoTracking()
            .OrderByDescending(a => a.CriadoEm)
            .Take(limite)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditoriaLog>> ListarPorExameAsync(int exameId)
    {
        return await _context.AuditoriaLogs
            .Include(a => a.Usuario)
            .AsNoTracking()
            .Where(a => a.ExameId == exameId)
            .OrderByDescending(a => a.CriadoEm)
            .ToListAsync();
    }

    public async Task<int> SalvarAlteracoesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
