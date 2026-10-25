using Microsoft.EntityFrameworkCore;
using TriagemImagem.Domain.Entities;
using TriagemImagem.Domain.Repositories;
using TriagemImagem.Infrastructure.Data.Context;

namespace TriagemImagem.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> ObterPorLoginAsync(string login)
    {
        var loginNormalizado = login?.Trim().ToLower() ?? string.Empty;
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Login.ToLower() == loginNormalizado);
    }

    public async Task<IEnumerable<Usuario>> ListarAtivosAsync()
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.StatusUsuario)
            .OrderBy(u => u.Nome)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
    }

    public Task AtualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAlteracoesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
