using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExplorerBE.ExtendedFileProperties
{
    public enum ExtendedFilePropertiesTechnology
    {
        Shell32
    }

    public class ExtendedFilePropertiesProvider 
    {
        public IExtendedFilePropertiesProvider Provider
        {
            get;
            private set;
        }

        public ExtendedFilePropertiesProvider(ExtendedFilePropertiesTechnology technology)
        {
            if (technology == ExtendedFilePropertiesTechnology.Shell32)
                Provider = new Shellprovider();
            else
            {
                string errorMsg = String.Format("Unsupported Technology: <{0}>", technology.ToString());
                throw new Exception(errorMsg);
            }
        }


    }
}
