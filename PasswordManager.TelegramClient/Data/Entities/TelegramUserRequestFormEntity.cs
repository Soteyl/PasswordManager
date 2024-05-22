using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace PasswordManager.TelegramClient.Data.Entities;

public class TelegramUserRequestFormEntity
{
    public Guid FormId { get; set; } = Guid.NewGuid();
    
    public string FormType { get; set; }
    
    public int CurrentStep { get; set; }
    
    public Dictionary<string, string>? Data { get; set; }
    
    public long UserId { get; set; }
    
    public TelegramUserDataEntity User { get; set; }
}

public class TelegramUserFormEntityConfiguration: IEntityTypeConfiguration<TelegramUserRequestFormEntity>
{
    public void Configure(EntityTypeBuilder<TelegramUserRequestFormEntity> builder)
    {
        builder.HasKey(x => x.FormId);
        
        builder.HasOne(x => x.User)
            .WithOne(x => x.RequestForm)
            .HasForeignKey<TelegramUserRequestFormEntity>(x => x.UserId);
        
        builder.Property(x => x.Data)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));
    }
}