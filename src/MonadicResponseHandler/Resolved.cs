using System;
using System.Collections.Generic;

namespace MonadicResponseHandler
{
    public interface ResolvedType { }

    public abstract class BaseResolved
    {
        public BaseResolved(ResolvedType value)
        {
            Value = value;
        }

        public ResolvedType Value { get; }

        public bool IsOk => Value.GetType() == typeof(Ok) || Value.GetType().IsGenericType && Value.GetType().GetGenericTypeDefinition() == typeof(Ok<>);

        public bool IsErr => Value.GetType() == typeof(Err) || Value.GetType().IsGenericType && Value.GetType().GetGenericTypeDefinition() == typeof(Err<>);
    }

    public class Resolved : BaseResolved
    {
        public Resolved(ResolvedType value) : base(value) { }

        public T Match<T>(Func<T> Ok, Func<IEnumerable<Exception>, T> Err)
        {
            switch (Value)
            {
                case Ok ok:
                    return Ok();
                case Err err:
                    return Err(err.Value);
                default:
                    if (Value is null)
                    {
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(Value));
                    }
                    else
                    {
                        throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
                    }
            }
        }

        public Resolved<T> Match<T>(Func<Resolved<T>> Ok, Func<IEnumerable<Exception>, Resolved<T>> Err)
        {
            return Match<Resolved<T>>(Ok, Err);
        }

        public void Match(Action Ok, Action<IEnumerable<Exception>> Err)
        {
            switch (Value)
            {
                case Ok ok:
                    Ok();
                    break;
                case Err err:
                    Err(err.Value);
                    break;
                default:
                    if (Value is null)
                    {
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(Value));
                    }
                    else
                    {
                        throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
                    }
            }
        }

        public static Ok Ok() => new Ok();
        
        public static Ok<T> Ok<T>(T value) => new Ok<T>(value);

        public static Err Err(IEnumerable<Exception> value) => new Err(value);

        public static Err<T> Err<T>(T value) => new Err<T>(value);

        public static implicit operator Resolved(Ok value)
        {
            return new Resolved(value);
        }

        public static implicit operator Resolved(Err value)
        {
            return new Resolved(value);
        }
    }

    public class Resolved<OkType> : BaseResolved
    {
        public Resolved(ResolvedType value) : base(value) { }

        public T Match<T>(Func<OkType, T> Ok, Func<IEnumerable<Exception>, T> Err)
        {
            switch (Value)
            {
                case Ok<OkType> ok:
                    return Ok(ok.Value);
                case Err err:
                    return Err(err.Value);
                default:
                    if (Value is null)
                    {
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(Value));
                    }
                    else
                    {
                        throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
                    }
            }
        }

        public Resolved<T> Match<T>(Func<OkType, Resolved<T>> Ok, Func<IEnumerable<Exception>, Resolved<T>> Err)
        {
            return Match<Resolved<T>>(Ok, Err);
        }

        public void Match(Action<OkType> Ok, Action<IEnumerable<Exception>> Err)
        {
            switch (Value)
            {
                case Ok<OkType> ok:
                    Ok(ok.Value);
                    break;
                case Err err:
                    Err(err.Value);
                    break;
                default:
                    if (Value is null)
                    {
                        throw new ArgumentNullException(
                            message: $"Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(Value));
                    }
                    else
                    {
                        throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
                    }
            }
        }

        public static implicit operator Resolved<OkType>(Ok<OkType> value)
        {
            return new Resolved<OkType>(value);
        }

        public static implicit operator Resolved<OkType>(Err value)
        {
            return new Resolved<OkType>(value);
        }
    }
}
