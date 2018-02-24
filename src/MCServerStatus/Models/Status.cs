using System.Collections.Generic;

namespace MCServerStatus.Models
{
    public class Status
    {
        public Description Description { get; set; }

        public Version Version { get; set; }

        public Players Players { get; set; }

        public string Favicon { get; set; }

    }
}
