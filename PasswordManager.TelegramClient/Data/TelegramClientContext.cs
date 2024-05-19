using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PasswordManager.TelegramClient.Data.Entities;

namespace PasswordManager.TelegramClient.Data;

public class TelegramClientContext: DbContext
{
    public DbSet<TelegramUserDataEntity> Users { get; set; }
    
    public DbSet<TelegramUserRequestFormEntity> RequestForms { get; set; }
    
    public TelegramClientContext(DbContextOptions<TelegramClientContext> options): base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TelegramClientContext).Assembly);
    }
}

public class TelegramClientContextFactory : IDesignTimeDbContextFactory<TelegramClientContext>
{
    public TelegramClientContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TelegramClientContext>().UseNpgsql();
 
         return new TelegramClientContext(optionsBuilder.Options);
     }
 }