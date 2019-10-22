using MetaExplorer.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MetaExplorer.Common
{
    public static class CriteriaConfig
    {
        static CriteriaConfig()
        {
        }

        public static void LoadFromAppSettings(string criterionConfig)
        {
            var criterionConfigJson = criterionConfig;
            Criteria = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Criterion>>(criterionConfigJson);
        }

        public static List<Criterion> Criteria
        {
            get;
            set;
        }
    }
}
