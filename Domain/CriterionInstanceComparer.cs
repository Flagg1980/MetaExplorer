using MetaExplorer.Domain;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MetaExplorer.Domain
{
    public class CriterionInstanceComparer : IEqualityComparer<CriterionInstance>
    {
        public bool Equals(CriterionInstance x, CriterionInstance y)
        {
            bool instanceEqual = x.Name.Equals(y.Name, StringComparison.InvariantCultureIgnoreCase);
            bool criterionEqual = x.Criterion.Name.Equals(y.Criterion.Name, StringComparison.InvariantCultureIgnoreCase);

            return instanceEqual && criterionEqual; 
        }
        public int GetHashCode(CriterionInstance codeh)
        {
            return codeh.Name.ToLower().GetHashCode() ^ codeh.Criterion.Name.ToLower().GetHashCode();
        }
    }
}
