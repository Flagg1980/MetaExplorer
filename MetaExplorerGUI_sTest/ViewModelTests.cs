using MetaExplorerBE;
using MetaExplorerGUI;
using NUnit.Framework;
using Moq;
using System.Linq;
using System.Collections.ObjectModel;
using MetaExplorer.Domain;
using System;
using MetaExplorer.Common;
using System.Collections.Generic;

namespace MetaExplorerGUI_sTest
{
    public class ViewModelTests
    {


        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ChangeFromVideoViewToCriterionView_PropertyChangedEventRaised()
        {
            var dummyCriterion = CreateDummyCriterion(5);

            var myCriteriaConfigMock = SetupCriteriaConfigMock(dummyCriterion);
            var myCriterionCacheMock = SetupCriterionCacheMock(dummyCriterion);

            var myVideoFileCache = new Mock<IVideoFileCache>();
            var myVideoThumbnailCache = new Mock<IVideoThumbnailCache>();
            var myVideoPropertiesCache = new Mock<IVideoPropertiesCache>();
            var myVideoMetaModelCache = new VideoMetaModelCache(myVideoFileCache.Object,
                myVideoThumbnailCache.Object,
                myVideoPropertiesCache.Object,
                myCriteriaConfigMock.Object);

            var myViewModel = new ViewModel(myCriterionCacheMock.Object,
                myVideoFileCache.Object,
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
            var dummyCriterion = CreateDummyCriterion(5);

            var myCriteriaConfigMock = SetupCriteriaConfigMock(dummyCriterion);
            var myCriterionCacheMock = SetupCriterionCacheMock(dummyCriterion);

            var myVideoFileCache = new Mock<IVideoFileCache>();
            var myVideoThumbnailCache = new Mock<IVideoThumbnailCache>();
            var myVideoPropertiesCache = new Mock<IVideoPropertiesCache>();
            var myVideoMetaModelCache = new VideoMetaModelCache(myVideoFileCache.Object, 
                myVideoThumbnailCache.Object, 
                myVideoPropertiesCache.Object, 
                myCriteriaConfigMock.Object);

            var myViewModel = new ViewModel(myCriterionCacheMock.Object, 
                myVideoFileCache.Object, 
                myVideoMetaModelCache, 
                myCriteriaConfigMock.Object);

            myViewModel.SwitchToCriterionThumbnailView(dummyCriterion);

            var resultSet = myViewModel.ThumbnailViewContent;

            Assert.That(resultSet.Count, 
                Is.EqualTo(dummyCriterion.Instances.Count), 
                "Result Set does not contain the right number of items.");
            
            Assert.That(resultSet as ObservableCollection<CriterionInstance>, 
                Is.Not.Null, 
                "Result Set does not contain items of type CriterionInstance.");
        }

        private Mock<ICriterionCache> SetupCriterionCacheMock(Criterion shallContainCriterion)
        {
            var myCriterionCacheMock = new Mock<ICriterionCache>();
            myCriterionCacheMock.Setup(x => x.GetCriterionInstances(shallContainCriterion)).Returns(new ObservableCollection<CriterionInstance>(shallContainCriterion.Instances));

            return myCriterionCacheMock;
        }

        private Mock<ICriteriaConfig> SetupCriteriaConfigMock(Criterion shallReturnCriterion)
        {
            var myCriteriaConfigMock = new Mock<ICriteriaConfig>();
            myCriteriaConfigMock.Setup(x => x.Criteria).Returns(new List<Criterion> { shallReturnCriterion });

            return myCriteriaConfigMock;
        }

        private Criterion CreateDummyCriterion(int numberOfCriterionInstances)
        {
            var criterionInstances = new List<CriterionInstance>();

            for (int i = 0; i < numberOfCriterionInstances; i++)
            {
                criterionInstances.Add(new CriterionInstance
                {
                    Name = $"CritInstance_{i}"
                });
            }

            return new Criterion
            {
                Name = "Crit1",
                Instances = criterionInstances
            };
        }
    }
}