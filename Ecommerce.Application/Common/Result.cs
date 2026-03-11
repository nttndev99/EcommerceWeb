using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? Error { get; private set; }
        public IEnumerable<string> Errors { get; private set; } = Enumerable.Empty<string>();

        private Result() { }

        public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
        public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
        public static Result<T> Failure(IEnumerable<string> errors) => new() { IsSuccess = false, Errors = errors };
    }

    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string? Error { get; private set; }
        public IEnumerable<string> Errors { get; private set; } = Enumerable.Empty<string>();

        private Result() { }

        public static Result Success() => new() { IsSuccess = true };
        public static Result Failure(string error) => new() { IsSuccess = false, Error = error };
        public static Result Failure(IEnumerable<string> errors) => new() { IsSuccess = false, Errors = errors };
    }
}