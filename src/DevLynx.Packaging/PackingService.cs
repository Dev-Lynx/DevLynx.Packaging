using DevLynx.Packaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevLynx.Packaging
{
    public class BinContainer : PackingContainer
    {
        public int Id { get; }

        public BinContainer(int id, float width, float height, float depth) : base(width, height, depth)
        {
            Id = id;
        }
    }

    public class PackingResult : BinPackResult
    {
        public BinContainer Container { get; }

        public PackingResult(BinPackResult r, BinContainer container) : base(r.Iterations, r.TotalBoxes, r.PackedDimension, r.PackedBoxes)
        {
            Container = container;
        }
    }

    public interface IPackingService
    {
        PackingResult Pack(IEnumerable<PackItem> items, IEnumerable<BinContainer> containers, CancellationToken cancellation = default);
        BinContainer FindSuitableContainer(IEnumerable<PackItem> items, IEnumerable<BinContainer> containers, CancellationToken cancellation = default);
        Task<BinContainer> FindSuitableContainerAsync(IEnumerable<PackItem> items, IEnumerable<BinContainer> containers, CancellationToken cancellation = default);
    }

    public class PackingService : IPackingService
    {
        public PackingResult Pack(IEnumerable<PackItem> items, IEnumerable<BinContainer> containers, CancellationToken cancellation = default)
        {
            BinPackResult res;
            foreach (var container in containers.OrderBy(x => x.Volume))
            {
                BinPack pack = new BinPack(items, container);
                res = pack.Pack(cancellation);

                if (res.WasFullyPacked)
                    return new PackingResult(res, container);
            }

            return null;
        }

        public BinContainer FindSuitableContainer(IEnumerable<PackItem> items, IEnumerable<BinContainer> containers, CancellationToken cancellation = default)
        {
            BinPackResult res;
            foreach (var container in containers.OrderBy(x => x.Volume))
            {
                BinPack pack = new BinPack(items, container);
                res = pack.Pack(cancellation);

                if (res.WasFullyPacked)
                    return container;
            }

            return null;
        }

        public Task<BinContainer> FindSuitableContainerAsync(IEnumerable<PackItem> items, IEnumerable<BinContainer> containers, CancellationToken cancellation = default)
        {
            return Task.Run(() => 
                FindSuitableContainer(items, containers, cancellation), cancellation);
        }
    }
}
