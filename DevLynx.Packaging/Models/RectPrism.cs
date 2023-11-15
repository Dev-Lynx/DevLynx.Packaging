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
    }

    public class Item : RectPrism
    {
        public int Quantity { get; set; }

        public Item(float width, float height, float depth) : base (width, height, depth)
        {
            Quantity = 1;
        }

        public Item(float x, float y, float z, int qty) : base(x, y, z)
        {
            Quantity = qty;
        }
    }

    public class Container : RectPrism 
    {
        public Container(float width, float height, float depth) : base(width, height, depth)
        {
        }
    }
}
