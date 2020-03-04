using System;
using System.Collections.Generic;

namespace MonadicResponseHandler
{
    public struct Err
    {
        public Err(IEnumerable<Exception> value)
        {
            Value = value;
        }

        public IEnumerable<Exception> Value { get; }
    }

    public struct Err<T>
    {
        public Err(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}