using DevLynx.Packaging.Models;

namespace DevLynx.Packaging.Tests
{
    public class Simple
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void OriginalTest()
        {
            List<PackItem> items = new List<PackItem>();

            items.Add(new PackItem(70, 104, 24, 4));
            items.Add(new PackItem(14, 104, 48, 2));
            items.Add(new PackItem(40, 52, 36, 3));


            BinPack pack = new BinPack(items, new PackingContainer(104, 96, 84));

            var res = pack.Pack();

            Assert.IsFalse(res.WasFullyPacked);
            Assert.That(res.Iterations == 18, "Iterations should be equal to 18");
        }
    }
}