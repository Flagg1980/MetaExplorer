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
        public Video ConvertFrom(string input)
        {
            Video mm = new Video();
            mm.File = new FileInfo(input);

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
                    if (crit.Mandatory || tokens.Count > crit.IndexInFilename)
                    {
                        mm.criteriaContents[crit.Name] = tokens[crit.IndexInFilename].Split(this.SEPARATOR2).ToList();
                        mm.criteriaContents[crit.Name].RemoveAll(x => String.IsNullOrWhiteSpace(x));
                    }
                }

                //segregate stars
                var starsToken = tokens.FirstOrDefault(x => Regex.IsMatch(x, @"^\dstar$", RegexOptions.IgnoreCase));
                if (starsToken != null)
                {
                    mm.Stars = int.Parse(starsToken[0].ToString());
                }

                return mm;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Error while building Meta Model for filename <{0}>. Message: <{1}>", input, e.Message));
            }
        }

        public string ConvertTo(Video input)
        {
            throw new NotImplementedException();
        }
    }
}
