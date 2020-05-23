using System.Collections.Generic;

namespace MetaExplorer.Domain
{
    public class Criterion
    {
        //public Criterion()
        //{
        //    Instances = new List<CriterionInstance>();
        //}

        //todo: this might be not necessary. At least at the moment we have no read access and it consumes much memory
        //public List<CriterionInstance> Instances { get; set; }

        public string Name { get; set; }

        public int IndexInFilename { get; set; }

        public bool Mandatory { get; set; }
    }
}
