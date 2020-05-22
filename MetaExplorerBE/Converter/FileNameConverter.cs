using MetaExplorer.Common;
using MetaExplorer.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MetaExplorerBE.Converter
{
    class FileNameConverter : IConverter
    {
        private readonly string SEPARATOR1 = "][";

        private readonly char SEPARATOR2 = '_';

        private readonly ICriteriaConfig myCriteriaConfig;


        public FileNameConverter(ICriteriaConfig criteriaConfig)
        {
            myCriteriaConfig = criteriaConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">file location</param>
        /// <returns></returns>
        public List<CriterionInstance> ConvertFrom(string input)
        {
            var result = new List<CriterionInstance>();

            try
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(input);

                //create tokens
                MatchCollection bla = Regex.Matches(fileNameWithoutExtension, @"\[.*?\]");
                List<string> tokens = bla.Cast<Match>().Select(m => m.Value).ToList();

                //remove separator chars from tokens
                tokens = tokens.Select(x => x.Trim(this.SEPARATOR1.ToCharArray())).ToList();

                //segregate predefined criteria
                foreach (Criterion crit in myCriteriaConfig.Criteria)
                {
                    // Case 1: The criterion is set in the filename
                    if (tokens.Count > crit.IndexInFilename)
                    {
                        List<string> criterionInstanceStringList = tokens[crit.IndexInFilename].Split(this.SEPARATOR2).ToList();
                        criterionInstanceStringList.RemoveAll(x => String.IsNullOrWhiteSpace(x));

                        var criterionInstanceList = criterionInstanceStringList.ConvertAll(x => new CriterionInstance()
                        {
                            Name = x,
                            Criterion = crit,
                        });

                        criterionInstanceList.ForEach(ci => ci.Criterion.Instances.Add(ci));

                        result.AddRange(criterionInstanceList);                       
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Error while building Meta Model for filename <{0}>. Message: <{1}>", input, e.Message));
            }
        }

        public string ConvertTo(List<CriterionInstance> input)
        {
            throw new NotImplementedException();
        }
    }
}
