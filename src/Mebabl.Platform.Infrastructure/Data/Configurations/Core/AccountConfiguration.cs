
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Infrastructure.Data.Configurations.Core;

public sealed class AccountConfiguration
    : IEntityTypeConfiguration<Account>
{
    public void Configure(
        EntityTypeBuilder<Account> builder)
    {
        // يعرّف بنية الحساب الأساسية دون تكرار علاقات الملف الشخصي.
        builder.ToTable("Accounts");

        builder.HasKey(x => x.Id);
    }
}
