using MetaExplorer.Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MetaExplorer.Common
{
    public static class CriteriaConfig
    {
        static CriteriaConfig()
        {
            LoadFromAppSettings();
        }

        public static void LoadFromAppSettings()
        {
            var criterionConfigJson = ConfigurationManager.AppSettings["CriterionConfig"];
            Criteria = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Criterion>>(criterionConfigJson);
        }

        public static List<Criterion> Criteria
        {
            get;
            set;
        }
    }
}
