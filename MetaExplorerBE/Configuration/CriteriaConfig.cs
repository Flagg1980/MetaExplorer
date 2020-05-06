using MetaExplorer.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace MetaExplorer.Common
{
    public interface ICriteriaConfig
    { 
        public List<Criterion> Criteria { get; }
    }

    public class CriteriaConfig : ICriteriaConfig
    {
        public void LoadFromAppSettings(IConfiguration criterionConfig)
        {
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

        public List<Criterion> Criteria
        {
            get;
            set;
        }
    }
}
