using DryIoc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DevLynx.Packaging.Visualizer.UI
{
    public static class DependencyWatcher
    {
        public static DependencyWatcher<T> Create<T>(T target) where T : DependencyObject
        {
            return new DependencyWatcher<T>(target);
        }

        public static DependencyWatcher<T> Create<T>(T target, DependencyProperty prop) where T : DependencyObject
        {
            return new DependencyWatcher<T>(target, prop);
        }

        public static DependencyWatcher<T> Create<T>(T target, DependencyProperty prop1, DependencyProperty prop2) where T : DependencyObject
        {
            return new DependencyWatcher<T>(target, new List<DependencyProperty>()
            {
                prop1, prop2
            });
        }

        public static DependencyWatcher<T> Create<T>(T target, DependencyProperty prop1, DependencyProperty prop2, DependencyProperty prop3) where T : DependencyObject
        {
            return new DependencyWatcher<T>(target, new List<DependencyProperty>()
            {
                prop1, prop2, prop3
            });
        }

        public static DependencyWatcher<T> Create<T>(T target, DependencyProperty prop1, DependencyProperty prop2, DependencyProperty prop3, DependencyProperty prop4) where T : DependencyObject
        {
            return new DependencyWatcher<T>(target, new List<DependencyProperty>()
            {
                prop1, prop2, prop3, prop4
            });
        }

        public static DependencyWatcher<T> Create<T>(T target, DependencyProperty prop1, DependencyProperty prop2, DependencyProperty prop3, DependencyProperty prop4, DependencyProperty prop5) where T : DependencyObject
        {
            return new DependencyWatcher<T>(target, new List<DependencyProperty>()
            {
                prop1, prop2, prop3, prop4, prop5
            });
        }

        public static DependencyWatcher<T> Create<T>(T target, DependencyProperty prop1, DependencyProperty prop2, DependencyProperty prop3, DependencyProperty prop4, DependencyProperty prop5, DependencyProperty prop6) where T : DependencyObject
        {
            return new DependencyWatcher<T>(target, new List<DependencyProperty>()
            {
                prop1, prop2, prop3, prop4, prop5, prop6
            });
        }

        public static DependencyWatcher<T> Create<T>(T target, DependencyProperty prop1, DependencyProperty prop2, DependencyProperty prop3, DependencyProperty prop4, DependencyProperty prop5, DependencyProperty prop6, DependencyProperty prop7) where T : DependencyObject
        {
            return new DependencyWatcher<T>(target, new List<DependencyProperty>()
            {
                prop1, prop2, prop3, prop4, prop5, prop6, prop7
            });
        }

        public static DependencyWatcher<T> Create<T>(T target, DependencyProperty prop1, DependencyProperty prop2, DependencyProperty prop3, DependencyProperty prop4, DependencyProperty prop5, DependencyProperty prop6, DependencyProperty prop7, DependencyProperty prop8) where T : DependencyObject
        {
            return new DependencyWatcher<T>(target, new List<DependencyProperty>()
            {
                prop1, prop2, prop3, prop4, prop5, prop6, prop7, prop8
            });
        }

        public static DependencyWatcher<T> Create<T>(T target, DependencyProperty prop1, DependencyProperty prop2, DependencyProperty prop3, DependencyProperty prop4, DependencyProperty prop5, DependencyProperty prop6, DependencyProperty prop7, DependencyProperty prop8, DependencyProperty prop9) where T : DependencyObject
        {
            return new DependencyWatcher<T>(target, new List<DependencyProperty>()
            {
                prop1, prop2, prop3, prop4, prop5, prop6, prop7, prop8, prop9
            });
        }

        public static DependencyWatcher<T> Create<T>(T target, List<DependencyProperty> props) where T : DependencyObject
        {
            return new DependencyWatcher<T>(target, props);
        }
    }

    public sealed class DependencyEventArgs : EventArgs
    {
        public DependencyProperty Property { get; }
        public DependencyPropertyDescriptor Descriptor { get; }

        public DependencyEventArgs(DependencyProperty prop, DependencyPropertyDescriptor descriptor)
        {
            Property = prop;
            Descriptor = descriptor;
        }
    }

    /// <summary>
    /// Watches for changes within a WPF Element
    /// </summary>
    /// <typeparam name="T">Element to be watched</typeparam>
    public class DependencyWatcher<T> : IDisposable where T : DependencyObject
    {
        public readonly T _target;

        private readonly bool _recursive;
        private readonly List<DependencyPropertyDescriptor> _descriptors;
        private readonly List<EventHandler> _handlers;
        private readonly List<DependencyProperty> _props;

        private bool _init;
        private bool _active;

        public bool IsActive => _active;
        public event EventHandler<DependencyEventArgs> PropertyChanged;

        public DependencyWatcher(T target, bool watchInheritedProps = true)
        {
            _target = target;
            _descriptors = new List<DependencyPropertyDescriptor>();
            _handlers = new List<EventHandler>();
            _recursive = watchInheritedProps;

            if (_props == null) _props = new List<DependencyProperty>();
        }

        public DependencyWatcher(T target, DependencyProperty prop) : this(target, false)
        {
            _props = new List<DependencyProperty>
            {
                prop
            };
        }

        public DependencyWatcher(T target, List<DependencyProperty> props) : this(target, false)
        {
            _props = new List<DependencyProperty>();
            _props = props;
        }



        void EnsureInit()
        {
            if (_init) return;
            _init = true;

            FieldInfo field;
            FieldInfo[] fields;
            EventHandler handler;
            DependencyPropertyDescriptor desc;
            Type type = typeof(T);

            if (_props.Count <= 0)
            {
                do
                {
                    fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

                    for (int i = 0; i < fields.Length; i++)
                    {
                        field = fields[i];

                        if (field.FieldType != typeof(DependencyProperty))
                            continue;

                        if (field.GetValue(null) is not DependencyProperty prop)
                            continue;

                        _props.Add(prop);
                    }

                    type = type.GetBaseType();
                }
                while (_recursive && type != null && type.IsAssignableTo(typeof(DependencyObject)));
            }

            foreach (var prop in _props)
            {
                desc = DependencyPropertyDescriptor.FromProperty(prop, typeof(T));
                handler = (s, e) => HandlePropertyChanged(s, prop, desc);

                _descriptors.Add(desc);
                _handlers.Add(handler);
            }
        }

        public void Start()
        {
            EnsureInit();

            if (_active) return;

            _active = true;
            for (int i = 0; i < _descriptors.Count; i++)
            {
                var desc = _descriptors[i];
                var handler = _handlers[i];

                desc.AddValueChanged(_target, handler);
            }
        }

        public void Stop()
        {
            if (!_active) return;

            for (int i = 0; i < _descriptors.Count; i++)
            {
                var desc = _descriptors[i];
                var handler = _handlers[i];

                desc.RemoveValueChanged(_target, handler);
            }

            _active = false;
        }

        private void HandlePropertyChanged(object sender, DependencyProperty prop, DependencyPropertyDescriptor descriptor)
        {
            PropertyChanged?.Invoke(sender, new DependencyEventArgs(prop, descriptor));
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
