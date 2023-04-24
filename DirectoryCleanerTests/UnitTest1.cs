using DirectoryCleaner;
namespace DirectoryCleanerTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            JunkIdentifier identifier = new("1w");
            Assert.IsTrue(identifier.DateOffsetToTimespan() == TimeSpan.FromDays(7));
            ;
            JunkIdentifier identifierTwo = new("1m");
            Assert.IsTrue(identifierTwo.DateOffsetToTimespan() == TimeSpan.FromDays(30));

            JunkIdentifier identifierThree = new("1d");
            Assert.IsTrue(identifierThree.DateOffsetToTimespan() == TimeSpan.FromDays(1));

            JunkIdentifier identifierFour = new("1m 1w");
            Assert.IsTrue(identifierFour.DateOffsetToTimespan() == TimeSpan.FromDays(37));
        }
    }
}