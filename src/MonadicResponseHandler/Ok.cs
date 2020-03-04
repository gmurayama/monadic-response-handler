namespace MonadicResponseHandler
{ 
    public struct Ok { }

    public struct Ok<T>
    {
        public Ok(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
