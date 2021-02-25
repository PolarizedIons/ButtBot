using System.Collections.Generic;

namespace ButtBot.Website.Models
{
    public class LinkedModel
    {
        public UserInfo User { get; set; }
        public IEnumerable<Connection> Connections { get; set; }
    }
}
