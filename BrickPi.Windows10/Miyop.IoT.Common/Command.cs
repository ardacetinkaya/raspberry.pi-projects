using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miyop.IoT.Common
{
    /*
    {
      "CommandText": "takephoto",
      "Properties": {
        "a":"b",
        "c":1,
        "e":"f"
      }
    }
    */

    public class CommandContainer
    {
        public string CommandText { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
