namespace AutoCall.Common.Collections {
    public class Wrapper<T> {
        public T Value { get; set; }
        public Wrapper(T t) {
            Value = t;
        }
    }
}
