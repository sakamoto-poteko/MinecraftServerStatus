using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCServerStatus
{
    public class Status
    {
        public class _Description
        {
            public string Text { get; set; }
        }

        public class _Version
        {
            public string Name { get; set; }
            public int Protocol { get; set; }
        }

        public class _Players
        {
            public int Max { get; set; }
            public int Online { get; set; }

            public class _Sample
            {
                public string Name { get; set; }
                public string Id { get; set; }
            }

            public List<_Sample> Sample { get; set; }

        }

        public _Description Description { get; set; }

        public _Version Version { get; set; }

        public _Players Players { get; set; }

        public string Favicon { get; set; }

    }
}
