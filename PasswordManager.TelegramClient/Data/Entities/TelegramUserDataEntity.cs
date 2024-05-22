using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.TelegramClient.Resources;

namespace PasswordManager.TelegramClient.Data.Entities;

public class TelegramUserDataEntity
{
    public long TelegramUserId { get; set; }

    public Guid InternalId { get; set; } = Guid.NewGuid();
    
    public string? MasterPasswordHash { get; set; } 
    
    public Locale Locale { get; set; }
    
    public Guid? FormId { get; set; }
    
    public TelegramUserRequestFormEntity? RequestForm { get; set; }
}

public class TelegramUserDataEntityConfiguration : IEntityTypeConfiguration<TelegramUserDataEntity>
{
    public void Configure(EntityTypeBuilder<TelegramUserDataEntity> builder)
    {
        builder.HasKey(x => x.TelegramUserId);

        builder.HasOne(x => x.RequestForm)
            .WithOne(x => x.User)
            .HasForeignKey<TelegramUserDataEntity>(x => x.FormId);
    }
}