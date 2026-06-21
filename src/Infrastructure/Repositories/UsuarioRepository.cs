using Fiap.TechChallenge.OficinaMecanica.Application.Abstractions;
using Fiap.TechChallenge.OficinaMecanica.Application.Behaviors;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly OficinaDbContext _context;

    public UsuarioRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorUsuarioAsync(string usuarioLogin, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioLogin == usuarioLogin, cancellationToken);
    }
}
