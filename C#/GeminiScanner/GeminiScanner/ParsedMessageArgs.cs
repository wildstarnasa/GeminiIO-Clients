using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeminiScanner
{
    class ParsedMessageArgs : EventArgs
    {
        public string path { get; set; }
        public string action { get; set; }
        public string message { get; set; }
    }
}
