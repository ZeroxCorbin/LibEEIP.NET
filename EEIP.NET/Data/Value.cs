namespace Sres.Net.EEIP.Data
{
    public abstract record ValueBase<T> :
        Byteable
        where T : struct
    {
        protected ValueBase(T value)
            => this.Value = value;

        public T Value { get; init; }
    }
}
