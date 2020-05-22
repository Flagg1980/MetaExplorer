using MetaExplorer.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public Criterion GetCriterionByName(string name)
        {
            var criterion = Criteria.FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (criterion == null)
            {
                throw new Exception(String.Format("Could not find a matching criterion for criterion instance <{0}> in the criteria config.",
                    name));
            }
            return criterion;
        }
    }
}
