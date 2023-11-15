using DevLynx.Packaging.Models;
using System.Diagnostics;

namespace DevLynx.Packaging.Lab
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Item> items = new List<Item>
            {
                new Item(70, 104, 24, 4),
                new Item(14, 104, 48, 2),
                new Item(40, 52, 36, 3)
            };

            // TODO: Revert back
            // new Container(104, 96, 84)

            BinPack pack = new BinPack(items, new Container(104, 96, 84));

            pack.Pack();
        }
    }
}