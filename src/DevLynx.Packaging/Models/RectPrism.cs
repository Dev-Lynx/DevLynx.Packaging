using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevLynx.Packaging.Models
{
    public abstract class RectPrism
    {
        public float Width { get; }
        public float Height { get; }
        public float Depth { get; }
        public float Volume { get; }

        public RectPrism(float width, float height, float depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Volume = width * height * depth;
        }

        public RectPrism(double width, double height, double depth)
        {
            Width = Convert.ToSingle(width);
            Height = Convert.ToSingle(height);
            Depth = Convert.ToSingle(depth);
            Volume = Convert.ToSingle(width * height * depth);
        }
    }

    public class PackItem : RectPrism
    {
        public int Quantity { get; set; }

        public PackItem(float width, float height, float depth) : base (width, height, depth)
        {
            Quantity = 1;
        }

        public PackItem(float x, float y, float z, int qty) : base(x, y, z)
        {
            Quantity = qty;
        }

        public PackItem(double x, double y, double z, int qty) : base(x, y, z)
        {
            Quantity = qty;
        }
    }

    public class PackingContainer : RectPrism 
    {
        public PackingContainer(float width, float height, float depth) : base(width, height, depth)
        {
        }

        public PackingContainer(double width, double height, double depth) : base(width, height, depth)
        {
        }
    }
}
