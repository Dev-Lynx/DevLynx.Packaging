using Accessibility;
using DevLynx.Packaging.Visualizer.Extensions;
using DryIoc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

namespace DevLynx.Packaging.Visualizer.Models.Contexts
{
    internal class Dim : BindableBase
    {
        public LazySingle Width { get; set; }
        public LazySingle Height { get; set; }
        public LazySingle Depth { get; set; }

        public Dim(float width, float height, float depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
        }
    }

    internal class NDim : BindableBase
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public float Depth { get; set; }
        public int Count { get; set; }

        public NDim(float width, float height, float depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Count = 1;
        }

        public NDim(Dim dim)
        {
            Width = dim.Width;
            Height = dim.Height;
            Depth = dim.Depth;
            Count = 1;
        }
    }

    internal class PackIteration : BindableBase
    {
        public int Id { get; }
        public DateTime StartedAt { get; } = DateTime.Now;
        public DateTime StopedAt { get; set; }

        public TimeSpan Duration => StopedAt - StartedAt;

        public List<PackInstance> Instances { get; } = new List<PackInstance>();

        public double PackedVolume => _vol;
        private double _vol;

        private readonly PackOrientation _orient;
        private readonly Transform3D _transform;

        public Quaternion Quaternion { get; set; }
        public Transform3D Transform => _transform;
        public PackOrientation Orientation => _orient;
        public Vector3 Container { get; }

        public PackIteration(int id, Vector3 container, PackOrientation orientation = PackOrientation.NORMAL)
        {
            Id = id;
            Container = container;
            _orient = orientation;

            var q = QuaternionExtensions.FromOrientation(orientation);
            q.Normalize();

            Quaternion = q;
            _transform = new RotateTransform3D(new QuaternionRotation3D(q));
        }

        public void AddInstance(PackInstance instance)
        {
            _vol +=  instance.Dim.X * instance.Dim.Y * instance.Dim.Z;
            RaisePropertyChanged(nameof(PackedVolume));

            var co = instance.Co;
            var dim = instance.Dim;

            var pt = _transform.Transform(new Point3D(co.X, co.Y, co.Z));
            var v = _transform.Transform(new Vector3D(dim.X, dim.Y, dim.Z));



            Console.WriteLine("{0}: {1}", instance.Id, _orient);


            //instance.Co = new Vector3((float)pt.X, (float)pt.Y, (float)pt.Z);
            //instance.Co = co;
            //instance.Dim = new Vector3((float)v.X, (float)v.Y, (float)v.Z);

            //var dim = instance.Dim;

            //switch (_orient)
            //{
            //    case PackOrientation.X_90:
            //        instance.Dim = new Vector3(dim.X, dim.Z, dim.Y);
            //        break;
            //    case PackOrientation.Y_90:
            //        instance.Dim = new Vector3(dim.Z, dim.Y, dim.X);
            //        break;
            //    case PackOrientation.Z_90:
            //        instance.Dim = new Vector3(dim.Y, dim.X, dim.Z);
            //        break;
            //    case PackOrientation.YX_90:
            //        instance.Dim = new Vector3(dim.Y, dim.Z, dim.X);
            //        break;
            //    case PackOrientation.YZ_90:
            //        instance.Dim = new Vector3(dim.Z, dim.X, dim.Y);
            //        break;
            //}

            Instances.Add(instance);
        }
    }

    internal class PackInstance : BindableBase
    {
        public int Id { get; }
        public DateTime PackedAt { get; } = DateTime.Now;
        
        public Vector3 Dim { get; set; }
        public Vector3 Co { get; set; }
        public PackedBox Box { get; set; }

        public string Color { get; set; }
        public Model3D Model { get; set; }

        public PackInstance(int id, PackedBox box)
        {
            Id = id;
            Box = box;
            Co = box.Coordinates;
            Dim = box.Dimensions;
        }
    }

    internal class PackageContext : BindableBase
    {
        public static double MIN_DIM = 1;
        public static double MAX_DIM = 700;

        
        public Dim Container { get; private set; } = new(10, 10, 10);
        public Vector3 SimContainer { get; internal set; }
        public double ContainerThickness { get; internal set; }

        public ObservableCollection<NDim> Items { get; } = new();

        public Dim NewItem { get; set; } = new(1, 1, 1);

        public bool Rendering { get; set; }


        public ObservableCollection<PackIteration> Iterations { get; } = new();
        public BinPackResult Result { get; set; }
    }

    public class LazySingle : BindableBase
    {
        private float _num;
        private string _value;
        private bool _raising;
        private bool _first = true;
        private readonly Debouncer _debouncer;

        public const int DEFAULT_DEBOUNCE_MS = 1000;
        
        public string Value
        {
            get => _value;
            set
            {
                if (_first) // Prevent initial set by textbox
                {
                    _first = false;

                    if (string.IsNullOrEmpty(value))
                        return;
                }
                
                _value = value;
            }
        }

        public LazySingle(float num, int debounceMS = DEFAULT_DEBOUNCE_MS)
        {
            _num = num;
            _value = _num.ToString();
            RaisePropertyChanged(nameof(Value));

            _debouncer = new Debouncer(TimeSpan.FromMilliseconds(debounceMS), DebounceTimerType.DispatcherTimer);
            PropertyChanged += HandlePropertyChanged;
        }

        private void HandlePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Value))
                return;

            if (_raising)
            {
                _raising = false;
                return;
            }

            if (!float.TryParse(_value, out _num))
                _num = ExtractNumber(_value);

            _debouncer.Debounce(UpdateValue);
        }

        private static float ExtractNumber(string value)
        {
            char c = '\0';
            string num = "";
            bool hasDec = false;

            const int PERIOD = 0x2E;

            for (int i = 0; i < value.Length; i++)
            {
                c = value[i];

                if (c == PERIOD)
                {
                    if (hasDec)
                    {
                        continue;
                    }

                    num += c;
                    hasDec = true;
                }
                else if (char.IsDigit(c))
                {
                    num += c;
                }
                else
                {
                    if (num.Length <= 0)
                        continue;
                }
            }

            switch (num.Length)
            {
                case 0: return 0;
                case 1:
                    if (c == PERIOD) return 0;
                    break;
            }

            return float.Parse(num);
        }

        private void UpdateValue()
        {
            _value = _num.ToString();
            
            _raising = true;
            RaisePropertyChanged(nameof(Value));
        }

        public static implicit operator float (LazySingle d) => d._num;
        public static implicit operator LazySingle(float f) => new LazySingle(f);
    }
}
