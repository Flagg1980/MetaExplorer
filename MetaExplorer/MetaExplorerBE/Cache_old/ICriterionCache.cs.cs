using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Configuration;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;
using MetaExplorerBE.MetaModels;

namespace MetaExplorerBE
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICriterionCache
    {
        List<CriterionInstance> GetCriterionInstances(Criterion criterion);

        List<CriterionInstance> GetCriterionInstances(string criterionName);

        Task GenerateDictAsync(Criterion criterion, List<VideoMetaModel> videoMetaModels, IProgress<int> progress, IProgress<string> progressFile);
    }
}
