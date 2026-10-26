using Microsoft.EntityFrameworkCore;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Enums;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Infrastructure.Data.Context;

namespace TriagemImagem.Infrastructure.Repositories;

public class ExameRepository : IExameRepository
{
    private readonly AppDbContext _context;

    public ExameRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Exame?> ObterPorIdAsync(int id)
    {
        return await _context.Exames
            .Include(e => e.Paciente)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Exame>> ListarDoDiaAsync(
        DateTime data, 
        ModalidadeExame? modalidade = null, 
        StatusExame? status = null)
    {
        var query = _context.Exames
            .Include(e => e.Paciente)
            .AsNoTracking()
            .Where(e => e.HorarioAgendado.Date == data.Date);

        if (modalidade.HasValue)
        {
            query = query.Where(e => e.Modalidade == modalidade.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        return await query
            .OrderBy(e => e.HorarioAgendado)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exame>> ListarTodosAsync(ModalidadeExame? modalidade = null, StatusExame? status = null)
    {
        var query = _context.Exames
            .Include(e => e.Paciente)
            .AsNoTracking()
            .AsQueryable();

        if (modalidade.HasValue)
        {
            query = query.Where(e => e.Modalidade == modalidade.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        return await query
            .OrderBy(e => e.HorarioAgendado)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exame>> BuscarAsync(string? termoBusca, DateTime? data = null)
    {
        var query = _context.Exames
            .Include(e => e.Paciente)
            .AsNoTracking()
            .AsQueryable();

        if (data.HasValue)
        {
            query = query.Where(e => e.HorarioAgendado.Date == data.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(termoBusca))
        {
            var termo = termoBusca.Trim().ToLower();
            query = query.Where(e =>
                e.Paciente.NomeCompleto.ToLower().Contains(termo) ||
                e.Paciente.DocumentoCpf.Contains(termo) ||
                e.DescricaoExame.ToLower().Contains(termo) ||
                e.CodigoProcedimento.ToLower().Contains(termo) ||
                e.Atendimento.ToString().Contains(termo));
        }

        return await query
            .OrderBy(e => e.HorarioAgendado)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Exame exame)
    {
        await _context.Exames.AddAsync(exame);
    }

    public Task AtualizarAsync(Exame exame)
    {
        _context.Exames.Update(exame);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAlteracoesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
