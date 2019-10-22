namespace MonadicResponseHandler
{ 
    public struct Ok : ResolvedType { }

    public struct Ok<T> : ResolvedType
    {
        public Ok(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
