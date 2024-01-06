using System;
using System.Collections.Generic;
using System.Linq;

namespace Net.Shared.Abstractions.Models.Data;

public sealed record Result<T>
{
    public T[] Data { get; }
    public string[] Errors { get; }

    public Result(T data)
    {
        Data = new[] { data };
        Errors = Array.Empty<string>();
    }

    public Result(IEnumerable<T> data)
    {
        Data = data.ToArray();
        Errors = Array.Empty<string>();
    }

    public Result(Exception exception)
    {
        Data = Array.Empty<T>();
        Errors = exception.InnerException is null
            ? new string[] { exception.Message }
            : new string[] { exception.Message, exception.InnerException.Message };
    }

    public Result(string error)
    {
        Data = Array.Empty<T>();
        Errors = new[] { error };
    }

    public Result(IEnumerable<string> errors)
    {
        Data = Array.Empty<T>();
        Errors = errors.ToArray();
    }

    public Result(IEnumerable<T> data, IEnumerable<string> errors)
    {
        Data = data.ToArray();
        Errors = errors.ToArray();
    }

    public string GetError() => string.Join(", ", Errors);
}
