using System.Diagnostics;
using System.Numerics;
using DevLynx.Packaging.Models;

namespace DevLynx.Packaging
{
    #region Abstractions
    [DebuggerDisplay("[{Index,nq}] {Dimensions,nq} {Coordinates,nq}")]
    public struct PackedBox
    {
        public int Index { get; }
        public Vector3 Dimensions { get; }
        public Vector3 Coordinates { get; }

        public PackedBox(int index, Vector3 dim, Vector3 coord)
        {
            Index = index;
            Dimensions = dim;
            Coordinates = coord;
        }
    }

    public class BinPackResult
    {
        public int Iterations { get; }

        public int TotalBoxes { get; }
        public int TotalBoxesPacked { get; }

        public bool WasFullyPacked { get; }
        public PackedBox[] PackedBoxes { get; }
        
        public Vector3 PackedDimension { get; }

        public BinPackResult(int iterations, int totalBoxes, Vector3 packedDim, PackedBox[] boxes)
        {
            Iterations = iterations;
            TotalBoxes = totalBoxes;
            TotalBoxesPacked = boxes.Length;
            PackedDimension = packedDim;
            PackedBoxes = boxes;

            WasFullyPacked = TotalBoxes == TotalBoxesPacked;
        }
    }

    public enum PackOrientation
    {
        NORMAL,
        X_90,
        Y_90,
        Z_90,
        YX_90,
        YZ_90
    }

    public class IterationEventArgs : EventArgs
    {
        public int Index { get; }
        public Vector3 Container { get; }

        public float PackedVolume { get; init; }
        public bool IsFit { get; init; }

        public int TotalPacked { get; init; }

        public IterationEventArgs(int index, Vector3 container)
        {
            Index = index;
            Container = container;
        }
    }
    #endregion

    public partial class BinPack
    {
        Vector3 _bf, _bbf, _dim, _pt, _remP;
        Vector4 _box, _bbox, _cbox;

        float _preLayer;
        float _layerInLayer;
        float _packedY;
        float _lilz;

        float _totalVol;
        float _totalBoxVol;
        float _packedVolume;
        int _totalPacked;
        int _iterations;

        int _variant;

        Cell _firstCell, _smallZ;

        private readonly List<Box> _boxes;
        private readonly int[] _registry;
        private List<Layer> _layers;

        private bool _active;
        private BinPackResult _res;

        public event EventHandler<PackedBox> BoxPacked;
        public event EventHandler<IterationEventArgs> IterationComplete;

        public BinPack(IEnumerable<PackItem> items, PackingContainer container)
        {
            _boxes = new List<Box>();
            _layers = new List<Layer>();
            _firstCell = new Cell();
            

            _dim = new Vector3(container.Width, container.Height, container.Depth);
            _totalVol = container.Volume;

            Box box;
            int n = 0;
            foreach (PackItem item in items)
            {
                for (int i = 0; i < item.Quantity; i++)
                {
                    // TODO: Improve Box design
                    box = new Box(n, item);

                    _boxes.Add(box);
                    _totalBoxVol += box.Vol;

                    n++;
                }
            }
            
            _registry = new int[_boxes.Count];
        }

        public BinPackResult Pack(CancellationToken cancellationToken = default)
        {
            if (_res != null) return _res;
            _active = !cancellationToken.IsCancellationRequested;

            if (!_active) return GetResult();
            
            cancellationToken.Register(() => _active = false);

            float x = _dim.X, y = _dim.Y, z = _dim.Z;

            for (_variant = 1; _variant <= 6 && _active; _variant++)
            {
                switch (_variant)
                {
                    case 1:
                        _pt = new Vector3(x, y, z);
                        break;

                    case 2:
                        _pt = new Vector3(z, y, x);
                        break;

                    case 3:
                        _pt = new Vector3(z, x, y);
                        break;

                    case 4:
                        _pt = new Vector3(y, x, z);
                        break;

                    case 5:
                        _pt = new Vector3(x, z, y);
                        break;

                    case 6:
                        _pt = new Vector3(y, z, x);
                        break;
                }

                PrepareLayers();
                EvaluateLayers();

                if (x == y && y == z && _variant == 1)
                    break;
            }

            return GetResult();
        }

        private BinPackResult GetResult()
        {
            if (_res != null) return _res;
            _res = new BinPackResult(_iterations, _boxes.Count, Vector3.Zero, Array.Empty<PackedBox>());

            return _res;
        }

        private void PrepareLayers()
        {
            _layers.Clear();
            Vector3 dim = new Vector3();
            Box box, zbox;

            for (int x = 0; x < _boxes.Count; x++)
            {
                box = _boxes[x];
                for (int y = 1; y <= 3; y++)
                {
                    switch (y)
                    {
                        case 1:
                            dim = box.Dim;
                            break;

                        case 2:
                            dim = new Vector3(box.Dim.Y, box.Dim.X, box.Dim.Z);
                            break;

                        case 3:
                            dim = new Vector3(box.Dim.Z, box.Dim.X, box.Dim.Y);
                            break;
                    }


                    //if ((exdim > py) || (((dimen2 > px) || (dimen3 > pz)) && ((dimen3 > px) || (dimen2 > pz)))) continue;

                    bool wontFit = dim.Y > _pt.X || dim.Z > _pt.Z;
                    wontFit &= dim.Z > _pt.X || dim.Y > _pt.Z;
                    wontFit |= dim.X > _pt.Y;

                    //if ((dim.X > _pt.Y) || (((dim.Y > _pt.X || dim.Z > _pt.Z)) && ((dim.Z > _pt.X || dim.Y > _pt.Z))))
                    if (wontFit) continue;

                    bool same = false;

                    for (int k = 0; k < _layers.Count; k++)
                    {
                        if (dim.X == _layers[k].Dim)
                        {
                            same = true;
                            break;
                        }
                    }

                    if (same) continue;

                    float diff, dimDiff;

                    Layer layer = new Layer(dim.X);

                    for (int z = 0; z < _boxes.Count; z++)
                    {
                        if (x == z)
                            continue;

                        zbox = _boxes[z];

                        dimDiff = Math.Abs(dim.X - zbox.Dim.X);
                        diff = Math.Abs(dim.X - zbox.Dim.Y);

                        if (diff < dimDiff)
                            dimDiff = diff;

                        diff = Math.Abs(dim.X - zbox.Dim.Z);

                        if (diff < dimDiff)
                            dimDiff = diff;

                        layer.Weight += dimDiff;
                    }

                    // printf("Layer: [%d] Eval: %d Dim: %d\n", layerlistlen, layers[layerlistlen].layereval, layers[layerlistlen].layerdim);
                    //Console.WriteLine("Layer: [{0}]\tEval: {1}\tDim: {2}", _layers.Count, layer.Weight, layer.Dim);

                    _layers.Add(layer);

                }
            }

            _layers = _layers.OrderBy(x => x.Weight).ToList();
        }

        private void EvaluateLayers()
        {
            Box box;
            Layer layer;
            float layerThickness;

            for (int i = 0; i < _layers.Count; i++)
            {
                

                

                _iterations++;


                //if (NewIteration != null)
                //{
                //    PackOrientation po = PackOrientation.NORMAL;
                //    switch (_variant)
                //    {
                //        case 2:
                //            po = PackOrientation.Y_90;
                //            break;

                //        case 3:
                //            po = PackOrientation.YZ_90;
                //            break;

                //        case 4:
                //            po = PackOrientation.Z_90;
                //            break;

                //        case 5:
                //            po = PackOrientation.X_90;
                //            break;

                //        case 6:
                //            po = PackOrientation.YX_90;
                //            break;
                //    }

                //    NewIteration.Invoke(this, new IterationEventArgs(po, _pt));
                //}

                layer = _layers[i];
                layerThickness = layer.Dim;

                _totalPacked = 0;
                _packedVolume = _packedY = 0;
                _remP.Y = _pt.Y;
                _remP.Z = _pt.Z;

                for (int x = 0; x < _boxes.Count; x++)
                {
                    box = _boxes[x];
                    box.IsPacked = false;
                    _registry[x] = -1;
                }

                float prepackedY, preRemPy;

                do
                {
                    _layerInLayer = 0;

                    PackLayer(ref layerThickness);

                    _packedY += layerThickness;
                    _remP.Y = _pt.Y - _packedY;

                    if (_layerInLayer > 0)
                    {
                        prepackedY = _packedY;
                        preRemPy = _remP.Y;
                        _remP.Y = layerThickness - _preLayer;
                        _packedY = _packedY - layerThickness + _preLayer;
                        _remP.Z = _lilz;
                        layerThickness = _layerInLayer;

                        PackLayer(ref layerThickness);

                        _packedY = prepackedY;
                        _remP.Y = preRemPy;
                        _remP.Z = _pt.Z;
                    }

                    //Console.WriteLine("[REM]: {0}\t\t[PACKED] {1}", _remP.Y, _packedY);

                    layerThickness = FindLayer(_remP.Y);

                    if (layerThickness <= 0 || layerThickness > _remP.Y)
                        break;
                }
                while (_active);

                if (IterationComplete != null)
                {
                    IterationComplete.Invoke(this, new IterationEventArgs(_iterations, _pt)
                    {
                        PackedVolume = _packedVolume,
                        TotalPacked = _totalPacked,
                        IsFit = _totalPacked == _boxes.Count // Alternatively compare volumes
                    });
                }
            }
        }

        private Cell FindSmallestZCell()
        {
            Cell cell, next, smallZ;
            cell = smallZ = _firstCell;

            while ((next = cell.Next) != null)
            {
                cell = next;
                if (next.CumZ < smallZ.CumZ)
                    smallZ = next;
            }

            _smallZ = smallZ;

            return _smallZ;
        }

        private void PackLayer(ref float layerThickness)
        {
            if (layerThickness <= 0) return;

            Box box;
            LayerResult res;
            float lenx, lenz, lpz;

            _firstCell.CumX = _pt.X;
            _firstCell.CumZ = 0;

            while (true)
            {
                Cell smallZ = FindSmallestZCell();

                
                box = _boxes[(int)_cbox.W];

                if (smallZ.IsSingle)
                {
                    //*** SITUATION-1: NO BOXES ON THE RIGHT AND LEFT SIDES ***

                    lenx = smallZ.CumX;
                    lpz = _remP.Z - smallZ.CumZ;

                    FindBox(lenx, layerThickness, _remP.Y, lpz, lpz);

                    res = ExamineLayer(ref layerThickness);
                    box = _boxes[(int)_cbox.W];

                    if (res == LayerResult.Full) break;
                    else if (res == LayerResult.Evened) continue;

                    box.Co = new Vector3(0, _packedY, smallZ.CumZ);

                    if (_cbox.X == smallZ.CumX)
                    {
                        smallZ.CumZ += _cbox.Z;
                    }
                    else
                    {
                        smallZ.InsertAfter(new Cell(smallZ));

                        smallZ.CumX = _cbox.X;
                        smallZ.CumZ += _cbox.Z;
                    }
                }
                else if (!smallZ.HasPrev)
                {
                    //*** SITUATION-2: NO BOXES ON THE LEFT SIDE ***

                    lenx = smallZ.CumX;
                    lenz = smallZ.Next.CumZ - smallZ.CumZ;
                    lpz = _remP.Z - smallZ.CumZ;
                    FindBox(lenx, layerThickness, _remP.Y, lenz, lpz);

                    res = ExamineLayer(ref layerThickness);
                    box = _boxes[(int)_cbox.W];

                    if (res == LayerResult.Full) break;
                    else if (res == LayerResult.Evened) continue;

                    box.Co.Y = _packedY;
                    box.Co.Z = smallZ.CumZ;

                    if (_cbox.X == smallZ.CumX)
                    {
                        box.Co.X = 0;
                        if (smallZ.CumZ + _cbox.Z == smallZ.Next.CumZ)
                        {
                            Cell next = smallZ.Next;

                            smallZ.CumX = next.CumX;
                            smallZ.CumZ = next.CumZ;

                            smallZ.RemoveNext();
                        }
                        else
                        {
                            smallZ.CumZ += _cbox.Z;
                        }
                    }
                    else
                    {
                        box.Co.X = smallZ.CumX - _cbox.X;

                        if (smallZ.CumZ + _cbox.Z == smallZ.Next.CumZ)
                        {
                            smallZ.CumX -= _cbox.X;
                        }
                        else
                        {
                            smallZ.InsertAfter(new Cell(smallZ.CumX, smallZ.CumZ + _cbox.Z));
                            smallZ.CumX -= _cbox.X;
                        }
                    }
                }
                else if (!smallZ.HasNext)
                {
                    //*** SITUATION-3: NO BOXES ON THE RIGHT SIDE ***

                    lenx = smallZ.CumX - smallZ.Prev.CumX;
                    lenz = smallZ.Prev.CumZ - smallZ.CumZ;
                    lpz = _remP.Z - smallZ.CumZ;

                    FindBox(lenx, layerThickness, _remP.Y, lenz, lpz);

                    res = ExamineLayer(ref layerThickness);
                    box = _boxes[(int)_cbox.W];

                    if (res == LayerResult.Full) break;
                    else if (res == LayerResult.Evened) continue;

                    box.Co = new Vector3(smallZ.Prev.CumX, _packedY, smallZ.CumZ);

                    if (_cbox.X == smallZ.CumX - smallZ.Prev.CumX)
                    {
                        if (smallZ.CumZ + _cbox.Z == smallZ.Prev.CumZ)
                        {
                            smallZ.Prev.CumX = smallZ.CumX;
                            smallZ.Prev.Next = null;

                            smallZ.RemoveSelf();
                        }
                        else
                        {
                            smallZ.CumZ += _cbox.Z;
                        }
                    }
                    else
                    {
                        if (smallZ.CumZ + _cbox.Z == smallZ.Prev.CumZ)
                        {
                            smallZ.Prev.CumX += _cbox.X;
                        }
                        else
                        {
                            smallZ.InsertBefore(
                                new Cell(smallZ.Prev.CumX + _cbox.X, smallZ.CumZ + _cbox.Z));
                        }
                    }
                }
                else if (smallZ.Prev.CumZ == smallZ.Next.CumZ)
                {
                    //*** SITUATION-4: THERE ARE BOXES ON BOTH OF THE SIDES *** 
                    //*** SUBSITUATION-4A: SIDES ARE EQUAL TO EACH OTHER ***

                    lenx = smallZ.CumX - smallZ.Prev.CumX;
                    lenz = smallZ.Prev.CumZ - smallZ.CumZ;
                    lpz = _remP.Z - smallZ.CumZ;

                    FindBox(lenx, layerThickness, _remP.Y, lenz, lpz);
                    res = ExamineLayer(ref layerThickness);
                    box = _boxes[(int)_cbox.W];

                    if (res == LayerResult.Full) break;
                    else if (res == LayerResult.Evened) continue;

                    box.Co.Y = _packedY;
                    box.Co.Z = smallZ.CumZ;

                    if (_cbox.X == smallZ.CumX - smallZ.Prev.CumX)
                    {
                        box.Co.X = smallZ.Prev.CumX;

                        if (smallZ.CumZ + _cbox.Z == smallZ.Next.CumZ)
                        {
                            smallZ.Prev.CumX = smallZ.Next.CumX;

                            if (smallZ.Next.HasNext)
                            {
                                // Not actually sure why this is the design, but we'll see
                                smallZ.RemoveNext();
                            }

                            smallZ.RemoveSelf();
                        }
                        else
                        {
                            smallZ.CumZ = smallZ.CumZ + _cbox.Z;
                        }
                    }
                    else if (smallZ.Prev.CumX < _pt.X - smallZ.CumX)
                    {
                        if (smallZ.CumZ + _cbox.Z == _smallZ.Prev.CumZ)
                        {
                            smallZ.CumX = smallZ.CumX - _cbox.X;

                            // https://github.com/davidmchapman/3DContainerPacking/commit/1f18269960f9681dccb6f084438a580445145adb
                            box.Co.X = smallZ.CumX;
                        }
                        else
                        {
                            box.Co.X = smallZ.Prev.CumX;

                            smallZ.InsertBefore(
                                new Cell(smallZ.Prev.CumX + _cbox.X, smallZ.CumZ + _cbox.Z));
                        }
                    }
                    else
                    {
                        if (smallZ.CumZ + _cbox.Z == smallZ.Prev.CumZ)
                        {
                            smallZ.Prev.CumX += _cbox.X;
                            box.Co.X = smallZ.Prev.CumX;
                        }
                        else
                        {
                            box.Co.X = smallZ.CumX - _cbox.X;

                            smallZ.InsertAfter(
                                new Cell(smallZ.CumX, smallZ.CumZ + _cbox.Z));

                            smallZ.CumX -= _cbox.X;
                        }
                    }
                }
                else
                {
                    //*** SUBSITUATION-4B: SIDES ARE NOT EQUAL TO EACH OTHER ***

                    lenx = smallZ.CumX - smallZ.Prev.CumX;
                    lenz = smallZ.Prev.CumZ - smallZ.CumZ;
                    lpz = _remP.Z - smallZ.CumZ;
                    
                    FindBox(lenx, layerThickness, _remP.Y, lenz, lpz);
                    res = ExamineLayer(ref layerThickness);
                    box = _boxes[(int)_cbox.W];

                    if (res == LayerResult.Full) break;
                    else if (res == LayerResult.Evened) continue;

                    box.Co = new Vector3(smallZ.Prev.CumX, _packedY, smallZ.CumZ);

                    if (_cbox.X == smallZ.CumX - smallZ.Prev.CumX)
                    {
                        if (smallZ.CumZ + _cbox.Z == smallZ.Prev.CumZ)
                        {
                            smallZ.Prev.CumX = smallZ.CumX;
                            smallZ.RemoveSelf();
                        }
                        else
                        {
                            smallZ.CumZ += _cbox.Z;
                        }
                    }
                    else
                    {
                        if (smallZ.CumZ + _cbox.Z == smallZ.Prev.CumZ)
                        {
                            smallZ.Prev.CumX += _cbox.X;
                        }
                        else if (smallZ.CumZ + _cbox.Z == smallZ.Next.CumZ)
                        {
                            box.Co.X = smallZ.CumX - _cbox.X;
                            smallZ.CumX -= _cbox.X;
                        }
                        else
                        {
                            smallZ.InsertBefore(
                                new Cell(smallZ.Prev.CumX + _cbox.X, smallZ.CumZ + _cbox.Z));
                        }
                    }
                }

                UpdatePackedVolume();
            }
        }

        private void UpdatePackedVolume()
        {
            Box box = _boxes[(int)_cbox.W];
            box.IsPacked = true;
            box.Pack = new Vector3(_cbox.X, _cbox.Y, _cbox.Z);

            //Console.WriteLine("Co: {0}\t\tPack: {1}", box.Co, box.Pack);

            if (BoxPacked != null)
            {
                BoxPacked.Invoke(this, new PackedBox(box.Index, box.Pack, box.Co));
            }

            
            _packedVolume += box.Vol;

            //Console.WriteLine("Packed: [{0}] {1} {2}", _cbox.W, box.Pack, box.Co);
            _registry[_totalPacked] = box.Index;
            _totalPacked++;

            if (_packedVolume == _totalVol || _packedVolume == _totalBoxVol)
            {
                PrepareReport();
            }
        }

        private void PrepareReport()
        {
            _active = false;

            Box box;
            int index, j = 0;
            PackedBox[] packed = new PackedBox[_totalPacked];

            Vector3 cum, packedDim = new();
            

            for (int i = 0; i < _boxes.Count; i++)
            {
                index = _registry[i];

                if (index < 0) continue;

                box = _boxes[index];

                cum = box.Pack + box.Co;

                if (cum.X > packedDim.X)
                    packedDim.X = cum.X;

                if (cum.Y > packedDim.Y)
                    packedDim.Y = cum.Y;

                if (cum.Z > packedDim.Z)
                    packedDim.Z = cum.Z;


                packed[j++] = new PackedBox(index, box.Pack, box.Co);
            }

            _res = new BinPackResult(_iterations, _boxes.Count, packedDim, packed);

            //Console.WriteLine("**************[FINISH PACKING]************** [{0:N2} %]", (_packedVolume / _totalBoxVol) * 100);
        }

        private void FindBox(float hmx, float hy, float hmy, float hz, float hmz)
        {
            Box box;
            _bf = new Vector3(float.MaxValue);
            _bbf = new Vector3(float.MaxValue);

            _box.W = _bbox.W = -1;

            Vector3 h = new Vector3(0, hy, hz);
            Vector3 hm = new Vector3(hmx, hmy, hmz);
            Vector3 dim;

            // TODO: Add packed fast forward mechanism
            // Boxes of the same dimensions might only need to be analyzed once 🤷‍
            for (int y = 0; y < _boxes.Count; y++)
            {
                box = _boxes[y];
                if (box.IsPacked) continue;

                AnalyzeBox(y, h, hm, box.Dim);

                if (box.Dim.X == box.Dim.Z && box.Dim.Y == box.Dim.Z) continue;

                dim = box.Dim;

                AnalyzeBox(y, h, hm, new Vector3(dim.X, dim.Z, dim.Y));

                AnalyzeBox(y, h, hm, new Vector3(dim.Y, dim.X, dim.Z));
                AnalyzeBox(y, h, hm, new Vector3(dim.Y, dim.Z, dim.X));

                AnalyzeBox(y, h, hm, new Vector3(dim.Z, dim.X, dim.Y));
                AnalyzeBox(y, h, hm, new Vector3(dim.Z, dim.Y, dim.X));
            }
        }

        private void AnalyzeBox(int index, Vector3 h, Vector3 hm, Vector3 dim)
        {
            bool valid = false;
            float xdiff, ydiff, zdiff;

            xdiff = hm.X - dim.X;
            zdiff = Math.Abs(h.Z - dim.Z);

            if (dim.X <= hm.X && dim.Y <= hm.Y && dim.Z <= hm.Z)
            {
                if (dim.Y <= h.Y)
                {
                    ydiff = h.Y - dim.Y;

                    valid |= ydiff < _bf.Y;
                    valid |= ydiff == _bf.Y && xdiff < _bf.X;
                    valid |= ydiff == _bf.Y && xdiff == _bf.X && zdiff < _bf.Z;

                    if (valid)
                    {
                        _box = new Vector4(dim, index);
                        _bf = new Vector3(xdiff, ydiff, zdiff);
                    }
                }
                else
                {
                    ydiff = dim.Y - h.Y;

                    valid |= ydiff < _bbf.Y;
                    valid |= ydiff == _bbf.Y && xdiff < _bbf.X;
                    valid |= ydiff == _bbf.Y && xdiff == _bbf.X && zdiff < _bbf.Z;

                    if (valid)
                    {
                        _bbox = new Vector4(dim, index);
                        _bbf = new Vector3(xdiff, ydiff, zdiff);
                    }
                }
            }
        }

        private float FindLayer(float thickness)
        {
            Vector3 dim = default;
            float diff, dimDiff;
            Box box, zbox;

            float layerEval;
            float layerThickness = 0;
            float eval = 1000000; // TODO: Investigate max eval

            for (int x = 0; x < _boxes.Count; x++)
            {
                box = _boxes[x];

                if (box.IsPacked) continue;

                for (int y = 1; y <= 3; y++)
                {
                    switch (y)
                    {
                        case 1:
                            dim = box.Dim;
                            break;

                        case 2:
                            dim = new Vector3(box.Dim.Y, box.Dim.X, box.Dim.Z);
                            break;

                        case 3:
                            dim = new Vector3(box.Dim.Z, box.Dim.X, box.Dim.Y);
                            break;
                    }

                    layerEval = 0;

                    //if (!((dim.X <= thickness) && (((dim.Y <= _pt.X) && (dim.Z <= _pt.Z)) || ((dim.Z <= _pt.X) && dim.Y <= _pt.Z))))
                    //    continue;

                    bool canProceed = (dim.Y <= _pt.X) && (dim.Z <= _pt.Z);
                    canProceed |= ((dim.Z <= _pt.X) && dim.Y <= _pt.Z);
                    canProceed &= dim.X <= thickness;

                    if (!canProceed) continue;
                    

                    for (int z = 0; z < _boxes.Count; z++)
                    {
                        zbox = _boxes[z];
                        //if (x == z || zbox.IsPacked)
                        //    continue;

                        if (!(x == z) && !(zbox.IsPacked))
                        {
                            dimDiff = Math.Abs(dim.X - zbox.Dim.X);
                            diff = Math.Abs(dim.X - zbox.Dim.Y);

                            if (diff < dimDiff)
                                dimDiff = diff;

                            diff = Math.Abs(dim.X - zbox.Dim.Z);

                            if (diff < dimDiff)
                                dimDiff = diff;

                            layerEval += dimDiff;
                        }

                        //dimDiff = Math.Abs(dim.X - zbox.Dim.X);
                        //diff = Math.Abs(dim.X - zbox.Dim.Y);

                        //if (diff < dimDiff)
                        //    dimDiff = diff;

                        //diff = Math.Abs(dim.X - zbox.Dim.Z);

                        //if (diff < dimDiff)
                        //    dimDiff = diff;

                        //layerEval += dimDiff;


                    }

                    if (layerEval < eval)
                    {
                        eval = layerEval;
                        layerThickness = dim.X;
                    }
                }   
            }
            return layerThickness;

        }

        private LayerResult ExamineLayer(ref float layerThickness)
        {
            LayerResult res = LayerResult.None;

            if (_box.W >= 0)
            {
                _cbox = _box;
            }
            else
            {
                if ((_bbox.W >= 0) && (_layerInLayer > 0 || _smallZ.IsSingle))
                {
                    if (_layerInLayer <= 0)
                    {
                        _preLayer = layerThickness;
                        _lilz = _smallZ.CumZ;
                    }

                    _cbox = _bbox;
                    _layerInLayer += _bbox.Y - layerThickness;
                    layerThickness = _bbox.Y;
                }
                else
                {
                    Cell curr = _smallZ;

                    if (_smallZ.IsSingle)
                    {
                        res = LayerResult.Full;
                    }
                    else
                    {
                        res = LayerResult.Evened;
                        if (!curr.HasPrev)
                        {
                            Cell next = curr.Next;

                            curr.CumX = next.CumX;
                            curr.CumZ = next.CumZ;
                            curr.Next = next = next.Next;

                            if (next != null)
                                next.Prev = curr;
                        }
                        else if (!curr.HasNext)
                        {
                            curr.Prev.Next = null;
                            curr.Prev.CumX = _smallZ.CumX;

                            _smallZ = null;
                        }
                        else
                        {
                            if (curr.Prev.CumZ == curr.Next.CumZ)
                            {
                                curr.Prev.Next = curr.Next.Next;

                                if (curr.Next.HasNext)
                                {
                                    curr.Next.Next.Prev = curr.Prev;
                                }

                                _smallZ.Next = null;
                                _smallZ = null;
                            }
                            else
                            {
                                curr.Prev.Next = curr.Next;
                                curr.Next.Prev = curr.Prev;

                                if (curr.Prev.CumZ < curr.Next.CumZ)
                                {
                                    curr.Prev.CumX = curr.CumX;
                                }

                                _smallZ = null;
                            }
                        }
                    }
                }
            }

            return res;
        }
    }

}