using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDRClient.VDR
{
    public class XmlApiVersion
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }

        public XmlApiVersion(string version)
        {
            string[] parts = version.Split('.');
            if(parts.Length > 2)
            {
                try
                {
                    Major = Convert.ToInt32(parts[0]);
                    Minor = Convert.ToInt32(parts[1]);
                    Patch = Convert.ToInt32(parts[2]);
                }
                catch
                {
                    Major = 0;
                    Minor = 0;
                    Patch = 0;
                }
            }
            else
            {
                Major = 0;
                Minor = 0;
                Patch = 0;
            }
        }

        public XmlApiVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public static bool operator != (XmlApiVersion v1, XmlApiVersion v2)
        {
            return !(v1 == v2);
        }

        public static bool operator == (XmlApiVersion v1, XmlApiVersion v2)
        {
            return v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Patch == v2.Patch;
        }

        public static bool operator < (XmlApiVersion v1, XmlApiVersion v2)
        {
            if (v1.Major < v2.Major)
                return true;
            else if (v1.Major > v2.Major)
                return false;
            else
            {
                if (v1.Minor < v2.Minor)
                    return true;
                else if (v1.Minor > v2.Minor)
                    return false;
                else
                {
                    if (v1.Patch < v2.Patch)
                        return true;
                    else
                        return false;
                }
            }
        }

        public static bool operator > (XmlApiVersion v1, XmlApiVersion v2)
        {
            return !(v1 < v2);
        }

        public static bool operator <= (XmlApiVersion v1, XmlApiVersion v2)
        {
            if (v1 == v2)
                return true;
            else if (v1 < v2)
                return true;
            else
                return false;
        }

        public static bool operator >= (XmlApiVersion v1, XmlApiVersion v2)
        {
            if (v1 == v2)
                return true;
            else if (v1 > v2)
                return true;
            else
                return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            XmlApiVersion v = (XmlApiVersion)obj;
            if (v == null)
                return false;
            if (v == this)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return (Major.ToString() + Minor.ToString() + Patch.ToString()).GetHashCode();
        }
    }
}
