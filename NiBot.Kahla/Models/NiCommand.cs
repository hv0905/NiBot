using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kahla.SDK.Events;

namespace NiBot.Kahla.Models
{
    public delegate Task CommandHandler(string arg, NewMessageEvent eventContext);

    public class NiCommand
    {
        public string Cmd { get; set; }
        public string ArgFormat { get; set; }
        public string Disruption { get; set; }

        public CommandHandler Handler { get; set; }
    }
}
