using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCall.Common.Helpers {
    public static class MessagesHelper1 {
        private static List<string> Messages = new List<string>();

        public static void AddMessage(string message) {
            Messages.Add(message);
        }
        public static List<string> TakeMessages() {
            var result = Messages.ToList();
            Messages.Clear();
            return result;
        }
    }
}
