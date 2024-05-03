using Microsoft.EntityFrameworkCore;
using PasswordStorageService.Data.Entities;

namespace PasswordStorageService.Data;

public class PasswordStorageContext: DbContext
{
    public DbSet<AccountEntity> Accounts { get; set; }

    public PasswordStorageContext(DbContextOptions<PasswordStorageContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountEntityConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}