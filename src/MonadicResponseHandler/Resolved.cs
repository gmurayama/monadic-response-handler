using System;
using System.Collections.Generic;
using System.Linq;

namespace MonadicResponseHandler
{
    public enum ResolvedType
    {
        Ok,
        Err
    }

    public enum Behavior
    {
        Forward,
        ThrowEx
    }

    public abstract class BaseResolved<OkType, ErrType>
    {
        public BaseResolved(OkType value)
        {
            OkResult = value;
            Type = ResolvedType.Ok;
        }

        public BaseResolved(ErrType value)
        {
            ErrResult = value;
            Type = ResolvedType.Err;
        }

        public object Value
        {
            get
            {
                switch (Type)
                {
                    case ResolvedType.Ok:
                        return OkResult;
                    case ResolvedType.Err:
                        return ErrResult;
                    default:
                        throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
                }
            }
        }

        protected OkType OkResult { get; }

        protected ErrType ErrResult { get; }

        protected ResolvedType Type { get; }

        public bool IsOk => Type == ResolvedType.Ok;

        public bool IsErr => Type == ResolvedType.Err;
    }

    public class Resolved : BaseResolved<Ok, Err>
    {
        public Resolved() : base(Ok()) { }

        public Resolved(Err err) : base(err) { }

        public T Match<T>(Func<T> Ok, Func<IEnumerable<Exception>, T> Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    return Ok();
                case ResolvedType.Err:
                    if (ErrResult.Value == null)
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(ErrResult));
                    else
                        return Err(ErrResult.Value);
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public Resolved Match(Func<Resolved> Ok, Behavior Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    return Ok();
                case ResolvedType.Err:
                    switch (Err)
                    {
                        case Behavior.Forward:
                            return ErrResult;
                        case Behavior.ThrowEx:
                            throw new InvalidOperationException("Resolved Value was an Err and expected an Ok value. The setted behavior was to throw an Exception.");
                        default:
                            throw new InvalidOperationException($"Unexpected Behavior: {Err.GetType()} {Err}");
                    }
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public Resolved<T> Match<T>(Func<Resolved<T>> Ok, Behavior Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    return Ok();
                case ResolvedType.Err:
                    switch (Err)
                    {
                        case Behavior.Forward:
                            return ErrResult;
                        case Behavior.ThrowEx:
                            throw new InvalidOperationException("Resolved Value was an Err and expected an Ok value. The setted behavior was to throw an Exception.");
                        default:
                            throw new InvalidOperationException($"Unexpected Behavior: {Err.GetType()} {Err}");
                    }
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public void Match(Action Ok, Action<IEnumerable<Exception>> Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    Ok();
                    break;
                case ResolvedType.Err:
                    if (ErrResult.Value == null)
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(ErrResult));
                    else
                        Err(ErrResult.Value);
                    break;
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public static Ok Ok() => new Ok();

        public static Ok<T> Ok<T>(T value) => new Ok<T>(value);

        public static Err Err(IEnumerable<Exception> value) => new Err(value);

        public static Err ErrAsIEnumerable(Exception value) => new Err(new[] { value } as IEnumerable<Exception>);

        public static Err<T> Err<T>(T value) => new Err<T>(value);

        public static implicit operator Resolved(Ok value)
        {
            return new Resolved();
        }

        public static implicit operator Resolved(Err err)
        {
            return new Resolved(err);
        }
    }

    public class Resolved<OkType> : BaseResolved<Ok<OkType>, Err>
    {
        public Resolved(Ok<OkType> ok) : base(ok) { }

        public Resolved(Err err) : base(err) { }

        public T Match<T>(Func<OkType, T> Ok, Func<IEnumerable<Exception>, T> Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    if (OkResult.Value == null)
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(OkResult));
                    else
                        return Ok(OkResult.Value);
                case ResolvedType.Err:
                    if (ErrResult.Value == null)
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(ErrResult));
                    else
                        return Err(ErrResult.Value);
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public Resolved Match(Func<OkType, Resolved> Ok, Behavior Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    return Ok(OkResult.Value);
                case ResolvedType.Err:
                    switch (Err)
                    {
                        case Behavior.Forward:
                            return ErrResult;
                        case Behavior.ThrowEx:
                            throw new InvalidOperationException("Resolved Value was an Err and expected an Ok value. The setted behavior was to throw an Exception.");
                        default:
                            throw new InvalidOperationException($"Unexpected Behavior: {Err.GetType()} {Err}");
                    }
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public Resolved<OkType> Match(Func<OkType, Resolved<OkType>> Ok, Behavior Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    return Ok(OkResult.Value);
                case ResolvedType.Err:
                    switch (Err)
                    {
                        case Behavior.Forward:
                            return ErrResult;
                        case Behavior.ThrowEx:
                            throw new InvalidOperationException("Resolved Value was an Err and expected an Ok value. The setted behavior was to throw an Exception.");
                        default:
                            throw new InvalidOperationException($"Unexpected Behavior: {Err.GetType()} {Err}");
                    }
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public Resolved<T> Match<T>(Func<OkType, Resolved<T>> Ok, Behavior Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    return Ok(OkResult.Value);
                case ResolvedType.Err:
                    switch (Err)
                    {
                        case Behavior.Forward:
                            return ErrResult;
                        case Behavior.ThrowEx:
                            throw new InvalidOperationException("Resolved Value was an Err and expected an Ok value. The setted behavior was to throw an Exception.");
                        default:
                            throw new InvalidOperationException($"Unexpected Behavior: {Err.GetType()} {Err}");
                    }
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public void Match(Action<OkType> Ok, Action<IEnumerable<Exception>> Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    if (OkResult.Value == null)
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(OkResult));
                    else
                        Ok(OkResult.Value);
                    break;
                case ResolvedType.Err:
                    if (ErrResult.Value == null)
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(ErrResult));
                    else
                        Err(ErrResult.Value);
                    break;
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public OkType Unwrap()
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    return OkResult.Value;
                case ResolvedType.Err:
                    throw new InvalidOperationException(
                        $"Invalid attempt to unwrap object of type {ErrResult.GetType()}",
                        new AggregateException(ErrResult.Value)
                    );
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public static implicit operator Resolved<OkType>(Ok<OkType> ok)
        {
            return new Resolved<OkType>(ok);
        }

        public static implicit operator Resolved<OkType>(Err err)
        {
            return new Resolved<OkType>(err);
        }
    }

    public class Resolved<OkType, ErrType> : BaseResolved<Ok<OkType>, Err<ErrType>>
    {
        public Resolved(Ok<OkType> ok) : base(ok) { }

        public Resolved(Err<ErrType> err) : base(err) { }

        public T Match<T>(Func<OkType, T> Ok, Func<ErrType, T> Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    if (OkResult.Value == null)
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(OkResult));
                    else
                        return Ok(OkResult.Value);
                case ResolvedType.Err:
                    if (ErrResult.Value == null)
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(ErrResult));
                    else
                        return Err(ErrResult.Value);
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public Resolved<OkType, ErrType> Match(Func<OkType, Resolved<OkType, ErrType>> Ok, Behavior Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    return Ok(OkResult.Value);
                case ResolvedType.Err:
                    switch (Err)
                    {
                        case Behavior.Forward:
                            return ErrResult;
                        case Behavior.ThrowEx:
                            throw new InvalidOperationException("Resolved Value was an Err and expected an Ok value. The setted behavior was to throw an Exception.");
                        default:
                            throw new InvalidOperationException($"Unexpected Behavior: {Err.GetType()} {Err}");
                    }
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public Resolved<T, ErrType> Match<T>(Func<OkType, Resolved<T, ErrType>> Ok, Behavior Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    return Ok(OkResult.Value);
                case ResolvedType.Err:
                    switch (Err)
                    {
                        case Behavior.Forward:
                            return ErrResult;
                        case Behavior.ThrowEx:
                            throw new InvalidOperationException("Resolved Value was an Err and expected an Ok value. The setted behavior was to throw an Exception.");
                        default:
                            throw new InvalidOperationException($"Unexpected Behavior: {Err.GetType()} {Err}");
                    }
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public void Match(Action<OkType> Ok, Action<ErrType> Err)
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    if (OkResult.Value == null)
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(OkResult));
                    else
                        Ok(OkResult.Value);
                    break;
                case ResolvedType.Err:
                    if (ErrResult.Value == null)
                        throw new ArgumentNullException(
                            message: "Resolved Value is null. It must be either Ok or Err.",
                            paramName: nameof(ErrResult));
                    else
                        Err(ErrResult.Value);
                    break;
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public OkType Unwrap()
        {
            switch (Type)
            {
                case ResolvedType.Ok:
                    return OkResult.Value;
                case ResolvedType.Err:
                    if (typeof(ErrType).IsGenericTypeDefinition &&
                        typeof(ErrType).GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                        typeof(ErrType).GetGenericArguments().Single().IsSubclassOf(typeof(Exception)))
                        throw new InvalidOperationException(
                            $"Invalid attempt to unwrap object of type {ErrResult.GetType()}",
                            new AggregateException(ErrResult.Value as IEnumerable<Exception>)
                        );
                    else
                        throw new InvalidOperationException(
                            $"Invalid attempt to unwrap object of type {typeof(ErrType)}"
                        );
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        public static implicit operator Resolved<OkType, ErrType>(Ok<OkType> ok)
        {
            return new Resolved<OkType, ErrType>(ok);
        }

        public static implicit operator Resolved<OkType, ErrType>(Err<ErrType> err)
        {
            return new Resolved<OkType, ErrType>(err);
        }
    }
}
