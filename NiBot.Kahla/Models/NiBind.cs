using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NiBot.Kahla.Models
{
    class NiBind
    {
        public string Key { get; set; }
        public string Command { get; set; }
        public NiBindMode Mode { get; set; }

        public enum NiBindMode
        {
            Match,
            FullMatch,
            Regex
        }
    }
}
