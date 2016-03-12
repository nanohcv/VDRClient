using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDRClient.VDR
{
    public class Configuration
    {
        private static VDRList vdrs = new VDRList();

        public static VDRList VDRs
        {
            get { return vdrs; }
        }
    }
}
