using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Domain.Entities.Projects;
using Mebabl.Platform.Infrastructure.Tests.Support;
using Xunit;

namespace Mebabl.Platform.Infrastructure.Tests.Database.QueryEngine;

public sealed class CollectionConfigurationModelTests
{
    [Fact]
    public void Collection_UsesProductionTableAndRequiredColumnConstraints()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var entity = context.Model.FindEntityType(typeof(Collection));

        Assert.NotNull(entity);
        Assert.Equal("Collections", entity!.GetTableName());
        Assert.Equal<int?>(100, entity.FindProperty(nameof(Collection.Name))!.GetMaxLength());
        Assert.Equal<int?>(100, entity.FindProperty(nameof(Collection.Code))!.GetMaxLength());
        Assert.Equal<int?>(500, entity.FindProperty(nameof(Collection.Description))!.GetMaxLength());
        Assert.False((bool)entity.FindProperty(nameof(Collection.IsSystem))!.GetDefaultValue()!);
        Assert.True((bool)entity.FindProperty(nameof(Collection.IsActive))!.GetDefaultValue()!);
    }

    [Fact]
    public void Collection_UsesProjectScopedCodeAndNameIndexes()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var entity = context.Model.FindEntityType(typeof(Collection))!;
        var indexes = entity.GetIndexes().ToArray();

        var codeIndex = Assert.Single(
            indexes,
            index => index.GetDatabaseName() ==
                     "UX_Collections_ProjectId_Code_Active");

        Assert.True(codeIndex.IsUnique);
        Assert.Equal("\"IsDeleted\" = false", codeIndex.GetFilter());
        Assert.Equal(
            new[] { nameof(Collection.ProjectId), nameof(Collection.Code) },
            codeIndex.Properties.Select(property => property.Name));

        var nameIndex = Assert.Single(
            indexes,
            index => index.GetDatabaseName() ==
                     "IX_Collections_ProjectId_Name_Active");

        Assert.False(nameIndex.IsUnique);
        Assert.Equal("\"IsDeleted\" = false", nameIndex.GetFilter());
        Assert.Equal(
            new[] { nameof(Collection.ProjectId), nameof(Collection.Name) },
            nameIndex.Properties.Select(property => property.Name));
    }

    [Fact]
    public void Collection_UsesProjectForeignKeyAndCompositePrincipalKey()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var entity = context.Model.FindEntityType(typeof(Collection))!;

        var projectForeignKey = Assert.Single(
            entity.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType.ClrType ==
                          typeof(PlatformProject));

        Assert.Equal(
            new[] { nameof(Collection.ProjectId) },
            projectForeignKey.Properties.Select(property => property.Name));
        Assert.Equal(DeleteBehavior.Restrict, projectForeignKey.DeleteBehavior);

        // This alternate key is required by DocumentConfiguration's composite
        // relationship, preventing a document from crossing project boundaries.
        var projectScopedPrincipalKey = Assert.Single(
            entity.GetKeys(),
            key => key.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(Collection.ProjectId), nameof(Collection.Id) }));

        Assert.Equal(
            new[] { nameof(Collection.Id) },
            entity.FindPrimaryKey()!.Properties.Select(property => property.Name));
        Assert.Equal(
            new[] { nameof(Collection.ProjectId), nameof(Collection.Id) },
            projectScopedPrincipalKey.Properties.Select(property => property.Name));
    }
}
