using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PasswordStorageService.Data.Entities;

public class AccountEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string User { get; set; } = null!;

    public byte[] CredentialsHash { get; set; } = null!;
    
    public byte[] CredentialsSalt { get; set; } = null!;

    public string WebsiteNickName { get; set; } = null!;
    
    public Guid UserId { get; set; }

    public Uri WebsiteUrl { get; set; } = null!;
}

public class AccountEntityConfiguration : IEntityTypeConfiguration<AccountEntity>
{
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.Property(x => x.WebsiteUrl)
            .HasConversion(u => u.ToString(), u => new Uri(u));

        builder.HasIndex(x => x.UserId);
    }
}