using MetaExplorer.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace MetaExplorer.Common
{
    public static class CriteriaConfig
    {
        static CriteriaConfig()
        {
        }

        public static void LoadFromAppSettings(IConfiguration criterionConfig)
        {
            var criterionConfigJson = criterionConfig;
            Criteria = new List<Criterion>();

            foreach (var ee in criterionConfig.GetChildren())
            {
                Criteria.Add(new Criterion { 
                 Name = ee.Key,
                 IndexInFilename = Int32.Parse(ee.GetSection("IndexInFilename").Value),
                 Mandatory = Boolean.Parse(ee.GetSection("Mandatory").Value),
                });
            }
        }

        public static List<Criterion> Criteria
        {
            get;
            set;
        }
    }
}
