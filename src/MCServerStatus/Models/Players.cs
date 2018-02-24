using System.Collections.Generic;

namespace MCServerStatus.Models
{
        public class Players
        {
            public int Max { get; set; }
            public int Online { get; set; }

            public IList<Player> Sample { get; set; }
    }
}
