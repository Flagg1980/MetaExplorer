using MetaExplorerBE;
using MetaExplorerGUI;
using NUnit.Framework;
using Moq;
using System.Linq;
using System.Collections.ObjectModel;
using MetaExplorer.Domain;
using System;

namespace MetaExplorerGUI_sTest
{
    public class ViewModelTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ChangeFromVideoViewToCriterionView_ThumbnailViewContainsOnlyCriterionInstances()
        {
            Criterion crit1 = new Criterion
            {
                Name = "Crit1",
                Instances = new System.Collections.Generic.List<CriterionInstance>() {
                    new CriterionInstance
                    {
                        Name = "CritInstance1",
                    }
                }
            };

            var myCriterionCacheMock = new Mock<ICriterionCache>();
            //var myVideoFileCacheMock = new Mock<VideoFileCache>();
            //var myVideoMetaModelCacheMock = new Mock<VideoMetaModelCache>();
            var myVideoFileCache = new Mock<IVideoFileCache>();
            var myVideoThumbnailCache = new Mock<IVideoThumbnailCache>();
            var myVideoPropertiesCache = new Mock<IVideoPropertiesCache>();
            var myVideoMetaModelCache = new VideoMetaModelCache(myVideoFileCache.Object, myVideoThumbnailCache.Object, myVideoPropertiesCache.Object);

            var myViewModel = new ViewModel(myCriterionCacheMock.Object, myVideoFileCache.Object, myVideoMetaModelCache);

            

            myViewModel.SwitchToCriterionThumbnailView(crit1);

            var resultSet = myViewModel.ThumbnailViewContent;
            Assert.That(resultSet.Count, Is.EqualTo(crit1.Instances.Count));
            Assert.That(resultSet as ObservableCollection<CriterionInstance>, Is.Not.Null);
        }
    }
}