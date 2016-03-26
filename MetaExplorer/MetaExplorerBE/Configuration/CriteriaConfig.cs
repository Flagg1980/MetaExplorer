using MetaExplorer.MetaModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MetaExplorer.Configuration
{
    public static class CriteriaConfig
    {
        private static readonly string PATH = Path.Combine(Directory.GetCurrentDirectory(), "CriteriaConfig.xml");

        public static void Load()
        {
            using (TextReader reader = new StreamReader(PATH))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Criterion>));
                Criteria = serializer.Deserialize(reader) as List<Criterion>;
            }
        }

        public static List<Criterion> Criteria
        {
            get;
            set;
        }

    }
}
