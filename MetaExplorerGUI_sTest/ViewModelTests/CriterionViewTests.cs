using MetaExplorer.Common;
using MetaExplorer.Domain;
using MetaExplorerBE;
using MetaExplorerGUI;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace MetaExplorerGUI_sTest.ViewModelTests
{
    public class CriterionViewTests
    {
        Criterion dummyCriterion;
        List<CriterionInstance> dummyCriterionInstances;

        Mock<ICriteriaConfig> myCriteriaConfigMock;
        Mock<ICriterionCache> myCriterionCacheMock;
        Mock<IVideoFileCache> myVideoFileCacheMock;
        Mock<IVideoThumbnailCache> myVideoThumbnailCache;
        Mock<IVideoPropertiesCache> myVideoPropertiesCache;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            dummyCriterion = CreateDummyCriterion();
            dummyCriterionInstances = CreateDummyCriterionInstances(dummyCriterion, 5);

            myCriteriaConfigMock = SetupCriteriaConfigMock(dummyCriterion);
            myCriterionCacheMock = SetupCriterionCacheMock(dummyCriterion);
            myVideoFileCacheMock = SetupVideoFileCacheMock();
            myVideoThumbnailCache = SetupVideoThumbnailCacheMock();
            myVideoPropertiesCache = SetupVideoPropertiesCacheMock();
        }

        [Test]
        public void ChangeFromVideoViewToCriterionView_PropertyChangedEventRaised()
        {
            var myVideoMetaModelCache = new VideoMetaModelCache(myVideoFileCacheMock.Object,
                myVideoThumbnailCache.Object,
                myVideoPropertiesCache.Object,
                myCriterionCacheMock.Object);

            var myViewModel = new ViewModel(myCriterionCacheMock.Object,
                myVideoFileCacheMock.Object,
                myVideoMetaModelCache,
                myCriteriaConfigMock.Object);

            var firedEvents = new List<string>();
            myViewModel.PropertyChanged += ((sender, e) => firedEvents.Add(e.PropertyName));

            myViewModel.SwitchToCriterionThumbnailView(dummyCriterion);

            Assert.That(firedEvents.Count,
                Is.EqualTo(1),
                "Number of fired Events is not as expected.");

            Assert.That(firedEvents, Does.Contain(""),
                "Fired Events not as expected.");
        }

        [Test]
        public void ChangeFromVideoViewToCriterionView_ThumbnailViewContainsOnlyCriterionInstances()
        {
            var myVideoMetaModelCache = new VideoMetaModelCache(myVideoFileCacheMock.Object, 
                myVideoThumbnailCache.Object, 
                myVideoPropertiesCache.Object, 
                myCriterionCacheMock.Object);

            var myViewModel = new ViewModel(myCriterionCacheMock.Object, 
                myVideoFileCacheMock.Object, 
                myVideoMetaModelCache, 
                myCriteriaConfigMock.Object);

            myViewModel.SwitchToCriterionThumbnailView(dummyCriterion);

            var resultSet = myViewModel.ThumbnailViewContent;

            Assert.That(resultSet.Count, 
                Is.EqualTo(dummyCriterionInstances.Count), 
                "Result Set does not contain the right number of items.");
            
            Assert.That(resultSet as List<CriterionInstance>, 
                Is.Not.Null, 
                "Result Set does not contain items of type CriterionInstance.");
        }

        private Mock<IVideoPropertiesCache> SetupVideoPropertiesCacheMock()
        { 
            return new Mock<IVideoPropertiesCache>();
        }

        private Mock<IVideoThumbnailCache> SetupVideoThumbnailCacheMock()
        { 
            return new Mock<IVideoThumbnailCache>();
        }

        private Mock<IVideoFileCache> SetupVideoFileCacheMock()
        { 
            return new Mock<IVideoFileCache>();
        }

        private Mock<ICriterionCache> SetupCriterionCacheMock(Criterion shallContainCriterion)
        {
            var myCriterionCacheMock = new Mock<ICriterionCache>();
            myCriterionCacheMock.Setup(x => x.GetCriterionInstances(shallContainCriterion)).Returns(dummyCriterionInstances);

            return myCriterionCacheMock;
        }

        private Mock<ICriteriaConfig> SetupCriteriaConfigMock(Criterion shallReturnCriterion)
        {
            var myCriteriaConfigMock = new Mock<ICriteriaConfig>();
            myCriteriaConfigMock.Setup(x => x.Criteria).Returns(new List<Criterion> { shallReturnCriterion });

            return myCriteriaConfigMock;
        }

        private Criterion CreateDummyCriterion()
        {
            var criterion = new Criterion
            {
                Name = "Crit1",
            };

            return criterion;
        }

        private List<CriterionInstance> CreateDummyCriterionInstances(Criterion criterion, int numberOfCriterionInstances)
        {
            var criterionInstances = new List<CriterionInstance>();

            for (int i = 0; i < numberOfCriterionInstances; i++)
            {
                criterionInstances.Add(new CriterionInstance
                {
                    Name = $"CritInstance_{i}",
                    Criterion = criterion
                });
            }

            return criterionInstances;
        }
    }
}