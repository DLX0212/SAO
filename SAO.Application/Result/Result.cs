using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Application.Result
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? ErrorMessage { get; }

        protected Result(bool isSuccess, string? errorMessage)
        {
            if (isSuccess && !string.IsNullOrEmpty(errorMessage))
                throw new ArgumentException("Un resultado exitoso no puede tener mensaje de error");

            if (!isSuccess && string.IsNullOrEmpty(errorMessage))
                throw new ArgumentException("Un resultado fallido debe tener mensaje de error");

            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Crea un resultado exitoso
        /// </summary>
        public static Result Success()
        {
            return new Result(true, null);
        }

        /// <summary>
        /// Crea un resultado fallido con mensaje de error
        /// </summary>
        public static Result Failure(string errorMessage)
        {
            return new Result(false, errorMessage);
        }
    }

    /// <summary>
    /// Clase genérica que representa el resultado de una operación con valor de retorno
    /// </summary>
    public class Result<T> : Result
    {
        public T? Value { get; }

        protected Result(bool isSuccess, T? value, string? errorMessage)
            : base(isSuccess, errorMessage)
        {
            if (isSuccess && value == null)
                throw new ArgumentException("Un resultado exitoso debe tener un valor");

            Value = value;
        }

        /// <summary>
        /// Crea un resultado exitoso con valor
        /// </summary>
        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null);
        }

        /// <summary>
        /// Crea un resultado fallido con mensaje de error
        /// </summary>
        public new static Result<T> Failure(string errorMessage)
        {
            return new Result<T>(false, default, errorMessage);
        }
    }
}
