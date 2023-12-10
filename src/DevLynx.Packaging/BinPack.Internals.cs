using DevLynx.Packaging.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevLynx.Packaging
{
    public partial class BinPack
    {
        enum LayerResult { None, Evened, Full }

        [DebuggerDisplay("[{Index, nq}] {Dim,nq}")]
        class Box
        {
            public Vector3 Dim;
            public Vector3 Co;
            public Vector3 Pack;

            public int Index;
            public float Vol;
            public bool IsPacked;

            
            public Box(int n, float x, float y, float z)
            {
                Index = n;
                Dim = new Vector3(x, y, z);
                Vol = x * y * z;
            }

            public Box(int n, PackItem item)
            {
                Index = n;
                Dim = new Vector3(item.Width, item.Height, item.Depth);
                Vol = item.Volume;
            }

            public override string ToString()
            {
                return $"{Dim}";
            }
        }

        [DebuggerDisplay("Dim: {Dim,nq} Weight: {Weight,nq}")]
        struct Layer
        {
            public float Dim;
            public float Weight;

            public Layer(float dim)
            {
                Dim = dim;
            }
        }

        [DebuggerDisplay("CumX {CumX,nq} CumZ {CumZ,nq}")]
        class Cell : BiNode
        {
            public float CumX;
            public float CumZ;

            public new Cell Prev
            {
                get => (Cell)base.Prev;
                set => base.Prev = value;
            }
            public new Cell Next
            {
                get => (Cell)base.Next;
                set => base.Next = value;
            }

            public Cell() { }

            public Cell(float cumX, float cumZ)
            {
                CumX = cumX;
                CumZ = cumZ;
            }

            public Cell(Cell cell)
            {
                CumX = cell.CumX;
                CumZ = cell.CumZ;
            }
        }

    }
}
