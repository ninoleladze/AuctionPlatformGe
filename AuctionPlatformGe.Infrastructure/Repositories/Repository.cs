using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatformGe.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id) => await _set.FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllAsync() => await _set.ToListAsync();

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);

    public void Update(T entity) => _context.Entry(entity).State = EntityState.Modified;

    public void Delete(T entity) => _set.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
