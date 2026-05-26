namespace AE.Market.Domain.Common
{
    public class Result 
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public IEnumerable<Error>? Errors { get; }
        public Error Error { get; }

        protected Result(bool isSuccess, Error error, IEnumerable<Error>? errors = null)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("A success result can not have an error.");
            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException("A failure result must have an error");
            IsSuccess = isSuccess;
            Error = error;
            Errors = errors;
        }

        // Factory Methodes for clean instantiation
        public static Result Success() => new(true, Error.None);
                            
        public static Result Fail(Error error) => new(false, error);
        public static Result Fail(Error error,IEnumerable<Error> errors) => new(false, error,errors);



    }
    public class Result<T> : Result
    {
        private readonly T? _value;
        public T Value => IsSuccess ?
            _value!: throw new InvalidOperationException("Cannot access the value of failure result.");
        protected Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
        {
            _value = value;
        }
        protected Result(T? value, bool isSuccess, Error error, IEnumerable<Error>? errors) : base(isSuccess, error,errors)
        {
            _value = value;
        }
        // Generic versions for when you need to return a value (e.g., returning a new User)
        public static Result<T> Success(T value) => new(value, true, Error.None);
        public static new Result<T> Fail(Error error, IEnumerable<Error> errors) => new(default, false, error, errors);
        public static new Result<T> Fail(Error error) => new(default, false, error);
    }

}
