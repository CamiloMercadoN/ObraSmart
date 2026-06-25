
namespace ObraSmart.Application.Common
{
    // Clase base para operaciones que no devuelven datos
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public string? ErrorMessage { get; protected set; }
        public string? ErrorCode { get; protected set; }

        protected Result(bool isSuccess, string? errorMessage, string? errorCode)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        public static Result Success()
            => new Result(true, null, null);

        public static Result Failure(string message, string code = "ERROR")
            => new Result(false, message, code);
    }

    // Clase genérica que hereda de la base, para operaciones que sí devuelven datos
    public class Result<T> : Result
    {
        public T Data { get; private set; }

        private Result(bool isSuccess, T data, string? errorMessage, string? errorCode)
            : base(isSuccess, errorMessage, errorCode)
        {
            Data = data;
        }

        public static Result<T> Success(T data)
            => new Result<T>(true, data, null, null);

        // Se usa "new" para ocultar el método Failure de la clase base y devolver el tipo correcto
        public static new Result<T> Failure(string message, string code = "ERROR")
            => new Result<T>(false, default!, message, code);
    }
}
