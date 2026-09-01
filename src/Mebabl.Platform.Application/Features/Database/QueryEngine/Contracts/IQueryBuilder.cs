using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

public interface IQueryBuilder
{
    IQueryable<Document> Apply(
        IQueryable<Document> query,
        QueryRequest request);

}