namespace Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

public sealed record QuerySort(
    string Field,
    bool Descending = false);