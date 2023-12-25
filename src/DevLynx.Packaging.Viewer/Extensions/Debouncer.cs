using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ThreadingTimer = System.Threading.Timer;
using SystemTimer = System.Timers.Timer;

namespace DevLynx.Packaging.Visualizer.Extensions
{
    public enum DebounceTimerType
    {
        /// <summary>
        /// Use <cref="System.Threading.Timer"/>
        /// </summary>
        ThreadingTimer,
        /// <summary>
        /// Use <cref="System.Timers.Timer"/>
        /// </summary>
        SystemTimer,
        /// <summary>
        /// Use <cref="System.Windows.Threading.DispatcherTimer"/>
        /// </summary>
        DispatcherTimer
    }

    public class Debouncer : IDisposable
    {
        private readonly DebounceTimerType _timerType;
        private readonly DebounceTimer _timer;
        private TimeSpan _interval;
        private Action _lastAction;

        public TimeSpan Interval => _interval;
        public DebounceTimerType Type => _timerType;

        private bool _isActive;

        public Debouncer(TimeSpan interval, DebounceTimerType type = DebounceTimerType.ThreadingTimer)
        {
            _interval = interval;
            _timerType = type;

            switch (type)
            {
                case DebounceTimerType.ThreadingTimer:
                    _timer = new _ThreadingTimer(interval, HandleCompleted);
                    break;

                case DebounceTimerType.DispatcherTimer:
                    _timer = new _DispatcherTimer(interval, HandleCompleted);
                    break;

                case DebounceTimerType.SystemTimer:
                    _timer = new _SystemTimer(interval, HandleCompleted);
                    break;

                default:
                    throw new NotImplementedException($"The specified timer type '{type}' is unknown");
            }
        }

        public Debouncer(TimeSpan interval, DispatcherPriority priority)
        {
            _interval = interval;
            _timerType = DebounceTimerType.DispatcherTimer;
            _timer = new _DispatcherTimer(interval, HandleCompleted, priority);
        }

        public void Debounce(Action action, double milliseconds = -1)
        {
            _lastAction = action;
            _isActive = true;

            if (milliseconds > 0)
            {
                _timer.ChangeInterval(TimeSpan.FromMilliseconds(milliseconds));
            }

            _timer.Reset();
        }

        private void HandleCompleted()
        {
            _lastAction?.Invoke();
            _timer.Stop();
            _isActive = false;
        }

        public void Dispose()
        {
            _isActive = false;
            _timer.Dispose();
        }


        private abstract class DebounceTimer : IDisposable
        {
            public bool IsActive { get; protected set; }
            protected readonly Action Completed;

            protected TimeSpan Interval;

            public DebounceTimer(TimeSpan interval, Action completed)
            {
                Interval = interval;
                Completed = completed;
            }

            public abstract void Start();
            public abstract void Stop();

            public abstract void ChangeInterval(TimeSpan interval);
            public abstract void Reset();

            public virtual void Dispose()
            {
            }
        }

        private class _ThreadingTimer : DebounceTimer
        {
            private readonly ThreadingTimer _timer;

            public _ThreadingTimer(TimeSpan interval, Action completed) : base(interval, completed)
            {
                _timer = new ThreadingTimer(HandleTick, null, -1, -1);
            }

            public override void Start()
            {
                _timer.Change(Interval, Timeout.InfiniteTimeSpan);
            }

            public override void Stop()
            {
                _timer.Change(-1, -1);
            }

            public override void Reset()
            {
                _timer.Change(Interval, Timeout.InfiniteTimeSpan);
            }

            public override void ChangeInterval(TimeSpan interval)
            {
                Interval = interval;
                Reset();
            }

            private void HandleTick(object state)
            {
                Completed?.Invoke();
            }

            public override void Dispose()
            {
                _timer.Dispose();
                base.Dispose();
            }

        }

        private class _DispatcherTimer : DebounceTimer
        {
            private readonly DispatcherTimer _timer;

            public _DispatcherTimer(TimeSpan interval, Action completed) : base(interval, completed)
            {
                _timer = new DispatcherTimer();
                _timer.Interval = interval;
                _timer.Tick += HandleTick;
            }

            public _DispatcherTimer(TimeSpan interval, Action completed, DispatcherPriority priority) : base(interval, completed)
            {
                _timer = new DispatcherTimer(priority);
                _timer.Interval = interval;
                _timer.Tick += HandleTick;
            }

            public override void Start()
            {
                _timer.Start();
            }

            public override void Stop()
            {
                _timer.Stop();
            }

            public override void ChangeInterval(TimeSpan interval)
            {
                Interval = interval;
                _timer.Interval = Interval;
            }

            public override void Reset()
            {
                _timer.Stop();
                _timer.Start();
            }

            private void HandleTick(object sender, EventArgs e)
            {
                Completed?.Invoke();
            }
        }

        private class _SystemTimer : DebounceTimer
        {
            private readonly SystemTimer _timer;

            public _SystemTimer(TimeSpan interval, Action completed) : base(interval, completed)
            {
                _timer = new SystemTimer(interval.TotalMilliseconds)
                {
                    AutoReset = false,
                };

                _timer.Elapsed += HandleTick;
            }

            public override void ChangeInterval(TimeSpan interval)
            {
                Interval = interval;
            }

            public override void Reset()
            {
                _timer.Stop();
                _timer.Start();
            }

            public override void Start()
            {
                _timer.Start();
            }

            public override void Stop()
            {
                _timer.Stop();
            }

            private void HandleTick(object sender, System.Timers.ElapsedEventArgs e)
            {
                Completed?.Invoke();
            }

            public override void Dispose()
            {
                _timer.Dispose();
                base.Dispose();
            }
        }
    }
}
