using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDRClient.VDR
{
    public class Configuration
    {
        public static XmlApiVersion MinXmlApiVersion = new XmlApiVersion("1.2.0");

        private static VDRList vdrs = new VDRList();

        public static VDRList VDRs
        {
            get { return vdrs; }
        }
    }
}
