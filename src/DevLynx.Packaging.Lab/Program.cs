using DevLynx.Packaging.Models;
using System.Diagnostics;

namespace DevLynx.Packaging.Lab
{    internal class Program
    {
        static void Main(string[] args)
        {
            List<PackItem> items = new List<PackItem>
            {
                new PackItem(70, 104, 24, 4),
                new PackItem(14, 104, 48, 2),
                new PackItem(40, 52, 36, 3)
            };

            // TODO: Revert back
            // new Container(104, 96, 84)

            BinPack pack = new BinPack(items, new PackingContainer(104, 96, 84));

            BinPackResult res = pack.Pack();

            Console.WriteLine("\n\n***************Result is ready: {0}", res.WasFullyPacked);
            foreach (var packed in res.PackedBoxes)
            {
                Console.WriteLine($"{packed.Dimensions} {packed.Coordinates}");
            }
        }
    }
}