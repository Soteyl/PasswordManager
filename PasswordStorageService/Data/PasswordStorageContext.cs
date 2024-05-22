using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
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

public class PasswordStorageContextFactory : IDesignTimeDbContextFactory<PasswordStorageContext>
{
    public PasswordStorageContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PasswordStorageContext>().UseNpgsql();
 
        return new PasswordStorageContext(optionsBuilder.Options);
    }
}