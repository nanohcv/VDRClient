using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDRClient.VDR
{
    public class ChannelGroup : List<Channel>
    {
        public string Name { get; private set; }

        public ChannelGroup(string name)
        {
            Name = name;
        }
    }
}
