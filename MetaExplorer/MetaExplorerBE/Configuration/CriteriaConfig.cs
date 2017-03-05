﻿using MetaExplorerBE.MetaModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MetaExplorerBE.Configuration
{
    public static class CriteriaConfig
    {
        private static readonly string fileName = "CriteriaConfig.xml";
        private static readonly string PATH = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        public static void Load()
        {
            if (!File.Exists(PATH))
            {
                throw new Exception(String.Format("The file <{0}> could not be found.", PATH));
            }

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