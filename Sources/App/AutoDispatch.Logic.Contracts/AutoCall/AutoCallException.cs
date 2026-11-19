using System;

namespace AutoCall.Logic.Exceptions {
    public class AutoCallException :Exception{
        public AutoCallException() : base() { }
        public AutoCallException(string message) : base(message) { }
    }
}
