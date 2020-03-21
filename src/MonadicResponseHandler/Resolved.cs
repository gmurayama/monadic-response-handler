using System;
using System.Collections.Generic;
using System.Linq;

namespace MonadicResponseHandler
{
    /// <summary>
    /// Specifies the Resolved internal state based on it's stored value
    /// </summary>
    public enum ResolvedType
    {
        Ok,
        Err
    }

    /// <summary>
    /// Set of behaviors available in Err handling
    /// </summary>
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

        /// <summary>
        /// Resolved Value
        /// </summary>
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

        /// <summary>
        /// Evaluate if stored value is an Ok state
        /// </summary>
        public bool IsOk => Type == ResolvedType.Ok;

        /// <summary>
        /// Evaluate if stored value is an Err state
        /// </summary>
        public bool IsErr => Type == ResolvedType.Err;
    }

    public class Resolved : BaseResolved<Ok, Err>
    {
        public Resolved() : base(Ok()) { }

        public Resolved(Err err) : base(err) { }

        /// <summary>
        /// Execute one of the two functions provided based on the value stored and return a value of type T
        /// </summary>
        /// <typeparam name="T">The return type for both functions</typeparam>
        /// <param name="Ok">Function to be executed that returns a value of type T if the value stored is an Ok</param>
        /// <param name="Err">Function to be executed if the value stored is an Err that receives its stored value and returns a value of type T</param>
        /// <returns>A value of type T</returns>
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

        /// <summary>
        /// Execute a function that returns a Resolved value if the stored value is an Ok, otherwise execute a pre defined behavior
        /// </summary>
        /// <param name="Ok">Function to be executed that returns a Resolved if the value stored is an Ok</param>
        /// <param name="Err">A pre defined behaviors to occur if the stored value is an Err</param>
        /// <returns>Resolved value</returns>
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
                            throw new AggregateException("Resolved Value was an Err and expected an Ok value. The setted behavior was to throw an Exception.", ErrResult.Value);
                        default:
                            throw new InvalidOperationException($"Unexpected Behavior: {Err.GetType()} {Err}");
                    }
                default:
                    throw new InvalidOperationException("Resolved Value could not be casted to Ok or Err type.");
            }
        }

        /// <summary>
        /// Execute a function that returns a Resolved value if the stored value is an Ok, otherwise execute a pre defined behavior
        /// </summary>
        /// <typeparam name="T">Generic Type for OkType in Resolved</typeparam>
        /// <param name="Ok">Function to be executed that returns a Resolved&lt;T&gt; if the value stored is an Ok</param>
        /// <param name="Err">A pre defined behaviors to occur if the stored value is an Err</param>
        /// <returns>Resolved&lt;T&gt; value</returns>
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

        /// <summary>
        /// Execute one of the two functions provided based on the value stored
        /// </summary>
        /// <param name="Ok">Function to be executed if the value stored is an Ok</param>
        /// <param name="Err">Function to be executed if the value stored is an Err that receives its stored value as a parameter</param>
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

        /// <summary>
        /// Creates an Ok
        /// </summary>
        /// <returns>Ok struct</returns>
        public static Ok Ok() => new Ok();

        /// <summary>
        /// Creates an Ok<T>
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="value">Value of type T</param>
        /// <returns>Ok<T> struct</returns>
        public static Ok<T> Ok<T>(T value) => new Ok<T>(value);

        /// <summary>
        /// Creates an Err
        /// </summary>
        /// <param name="value">An IEnumerable of Exception</param>
        /// <returns>Err struct</returns>
        public static Err Err(IEnumerable<Exception> value) => new Err(value);

        /// <summary>
        /// Creates an Err receiving a single Exception instead of an IEnumerable<Exception>
        /// </summary>
        /// <param name="value">Exception object</param>
        /// <returns>Err struct</returns>
        public static Err ErrAsIEnumerable(Exception value) => new Err(new[] { value } as IEnumerable<Exception>);

        /// <summary>
        /// Creates an Err<T>
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="value">Value of type T</param>
        /// <returns>Err<T> struct</returns>
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

        /// <summary>
        /// Execute one of the two functions provided based on the value stored and return a value of type T
        /// </summary>
        /// <typeparam name="T">The return type for both functions</typeparam>
        /// <param name="Ok">Function to be executed if the value stored is an Ok that receives its stored vale and returns a value of type T</param>
        /// <param name="Err">Function to be executed if the value stored is an Err that receives its stored value and returns a value of type T</param>
        /// <returns>A value of type T</returns>
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

        /// <summary>
        /// Execute a function that returns a Resolved value if the stored value is an Ok, otherwise execute a pre defined behavior
        /// </summary>
        /// <param name="Ok">Function to be executed if the value stored is an Ok that receives its stored value and returns a Resolved</param>
        /// <param name="Err">A pre defined behaviors to occur if the stored value is an Err</param>
        /// <returns>Resolved value</returns>
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

        /// <summary>
        /// Execute a function that returns a Resolved&lt;OkType&gt; value if the stored value is an Ok, otherwise execute a pre defined behavior
        /// </summary>
        /// <param name="Ok">Function to be executed if the value stored is an Ok that receives its stored value and returns a Resolved&lt;OkType&gt;</param>
        /// <param name="Err">A pre defined behaviors to occur if the stored value is an Err</param>
        /// <returns>Resolved&lt;T&gt; value</returns>
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

        /// <summary>
        /// Execute a function that returns a Resolved&lt;T&gt; value if the stored value is an Ok, otherwise execute a pre defined behavior.
        /// </summary>
        /// <typeparam name="T">Generic Type for OkType in Resolved</typeparam>
        /// <param name="Ok">Function to be executed if the value stored is an Ok that receives its stored value and returns a Resolved&lt;T&gt;</param>
        /// <param name="Err">A pre defined behaviors to occur if the stored value is an Err</param>
        /// <returns>Resolved&lt;T&gt; value</returns>
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

        /// <summary>
        /// Execute one of the two functions provided based on the value stored
        /// </summary>
        /// <param name="Ok">Function to be executed if the value stored is an Ok that receives its stored value</param>
        /// <param name="Err">Function to be executed if the value stored is an Err that receives its stored value</param>
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

        /// <summary>
        /// Try to return the stored value in an Ok struct. If it is an Err, an Exception is throwed
        /// </summary>
        /// <returns>Value of type OkType</returns>
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

        /// <summary>
        /// Execute one of the two functions provided based on the value stored and return a value of type T
        /// </summary>
        /// <typeparam name="T">The return type for both functions</typeparam>
        /// <param name="Ok">Function to be executed if the value stored is an Ok that receives its stored vale and returns a value of type T</param>
        /// <param name="Err">Function to be executed if the value stored is an Err that receives its stored value and returns a value of type T</param>
        /// <returns>A value of type T</returns>
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

        /// <summary>
        /// Execute a function that returns a Resolved&lt;OkType, ErrType&gt; value if the stored value is an Ok, otherwise execute a pre defined behavior
        /// </summary>
        /// <param name="Ok">Function to be executed if the value stored is an Ok that receives its stored value and returns a Resolved&lt;OkType, ErrType&gt;</param>
        /// <param name="Err">A pre defined behavior to occur if the stored value is an Err</param>
        /// <returns>Resolved&lt;OkType, ErrType&gt; value</returns>
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

        /// <summary>
        /// Execute a function that returns a Resolved&lt;T, ErrType&gt; value if the stored value is an Ok, otherwise execute a pre defined behavior
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="Ok">Function to be executed if the value stored is an Ok that receives its stored value and returns a Resolved&lt;T, ErrType&gt;</param>
        /// <param name="Err">A pre defined behavior to occur if the stored value is an Err</param>
        /// <returns>Resolved&lt;T, ErrType&gt; value</returns>
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

        /// <summary>
        /// Execute one of the two functions provided based on the value stored
        /// </summary>
        /// <param name="Ok">Function to be executed if the value stored is an Ok that receives its stored value</param>
        /// <param name="Err">Function to be executed if the value stored is an Err that receives its stored value</param>
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

        /// <summary>
        /// Try to return the stored value in an Ok struct. If it is an Err, an Exception is throwed
        /// </summary>
        /// <returns>Value of type OkType</returns>
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
