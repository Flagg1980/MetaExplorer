
using System.Collections.Generic;

namespace MetaExplorer.Domain
{
    public class Criterion
    {
        public List<CriterionInstance> Instances { get; set; }

        public string Name { get; set; }

        public int IndexInFilename { get; set; }

        public bool Mandatory { get; set; }
    }
}
