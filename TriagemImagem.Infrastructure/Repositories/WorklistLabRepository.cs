using Microsoft.EntityFrameworkCore;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Infrastructure.Data.Context;

namespace TriagemImagem.Infrastructure.Repositories;

public class WorklistLabRepository : IWorklistLabRepository
{
    private readonly WorklistLabDbContext _context;

    public WorklistLabRepository(WorklistLabDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorklistPedidoLab>> ListarPedidosAsync()
    {
        return await _context.Pedidos
            .Include(p => p.Paciente)
            .OrderBy(p => p.HorarioAgendado)
            .ToListAsync();
    }

    public async Task<WorklistPedidoLab?> ObterPorAtendimentoAsync(int atendimento)
    {
        return await _context.Pedidos
            .Include(p => p.Paciente)
            .FirstOrDefaultAsync(p => p.Atendimento == atendimento);
    }

    public async Task AtualizarStatusAsync(int atendimento, string status)
    {
        var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.Atendimento == atendimento);
        if (pedido != null)
        {
            pedido.Status = status;
            await _context.SaveChangesAsync();
        }
    }
}
