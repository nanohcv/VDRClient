using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDRClient.VDR
{
    public class Settings
    {
        private string name;
        private string protocol;
        private string address;
        private string port;
        private string username;
        private string password;
        private string profile;
        private string audioprofile;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Protocol
        {
            get { return protocol; }
            set { protocol = value; }
        }

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        public string Port
        {
            get { return port; }
            set { port = value; }
        }

        public string UserName
        {
            get { return username; }
            set { username = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public string Profile
        {
            get { return profile; }
            set { profile = value; }
        }

        public string AudioProfile
        {
            get { return audioprofile; }
            set { audioprofile = value; }
        }

        public string BaseURL
        {
            get
            {
                return protocol + address + ":" + port + "/";
            }
        }

        public Settings()
        {
            name = "VDR";
            protocol = "http://";
            address = "0.0.0.0";
            port = "10080";
            username = "xmlapi";
            password = "";
            profile = "Mid";
            audioprofile = "Audio";
        }

        public Settings(Settings obj)
        {
            this.name = obj.name;
            this.address = obj.address;
            this.port = obj.port;
            this.username = obj.username;
            this.password = obj.password;
            this.profile = obj.profile;
            this.audioprofile = obj.audioprofile;
        }

        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Settings settings = (Settings)obj;
            if (settings == null)
                return false;
            if (settings.Name == this.Name)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

    }
}
