using System;

namespace Net.Shared.Abstractions.Models.Data;

public sealed record PaginatedResult<T> where T : new()
{
    public T[] Items { get; init; } = Array.Empty<T>();
    public int Page { get; set; }
    public int Limit { get; set; }
}
