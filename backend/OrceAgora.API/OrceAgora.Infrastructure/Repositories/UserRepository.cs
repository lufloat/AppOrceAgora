using Microsoft.EntityFrameworkCore;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;
using OrceAgora.Infrastructure.Data;

namespace OrceAgora.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(string email) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<bool> ExistsByEmailAsync(string email) =>
        db.Users.AnyAsync(u => u.Email == email);

    public async Task AddAsync(User user) => await db.Users.AddAsync(user);

    public Task UpdateAsync(User user)
    {
        db.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}