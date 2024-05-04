using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PasswordManager.TelegramClient.Data.Entities;

namespace PasswordManager.TelegramClient.Data;

public class TelegramClientContext: DbContext
{
    public DbSet<TelegramUserDataEntity> TelegramUserData { get; set; }
    
    public TelegramClientContext(DbContextOptions<TelegramClientContext> options): base(options)
    { }
}

public class TelegramClientContextFactory : IDesignTimeDbContextFactory<TelegramClientContext>
{
    public TelegramClientContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TelegramClientContext>().UseNpgsql();
 
         return new TelegramClientContext(optionsBuilder.Options);
     }
 }