using Kahla.SDK.Events;
using System;
using System.Threading.Tasks;

namespace NiBot.Kahla.Models
{
    public class NiCommand
    {
        public string Cmd { get; set; }
        public string ArgFormat { get; set; }
        public string Disruption { get; set; }

        public Func<string, NewMessageEvent, Task> Handler { get; set; }
    }
}
