using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pandora.Domain.Entities;

namespace Pandora.Infrastructure.Database.Configuration;

public sealed class CredentialAccessSettingConfiguration : IEntityTypeConfiguration<CredentialAccessSetting>
{
    public void Configure(EntityTypeBuilder<CredentialAccessSetting> builder)
    {
        builder.ToTable(DatabaseConstants.CredentialAccessSettings.Table);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName(DatabaseConstants.CredentialAccessSettings.Id).ValueGeneratedNever();
        builder.Property(e => e.TtlMinutes).HasColumnName(DatabaseConstants.CredentialAccessSettings.TtlMinutes);
        builder.Property(e => e.UpdatedAt).HasColumnName(DatabaseConstants.CredentialAccessSettings.UpdatedAt);
        builder.Property(e => e.UpdatedBy).HasColumnName(DatabaseConstants.CredentialAccessSettings.UpdatedBy);
    }
}
