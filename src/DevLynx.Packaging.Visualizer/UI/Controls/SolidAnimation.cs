using Accessibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace DevLynx.Packaging.Visualizer.UI
{
    public enum SolidAnimationKind
    {
        Scale,
        ScaleCenter,
        Translate,
        RotateCenter
    }

    /// <summary>
    /// Parallel 3D animation using double keyframes
    /// </summary>
    public class SolidAnimationEcho
    {
        private SolidAnimation _anim;
        private readonly SolidAnimationKind _kind;
        private readonly Animatable _animatable;
        

        private SolidAnimationEcho _next;
        private bool _isCompleted;

        public event EventHandler Completed;

        internal SolidAnimationEcho(Animatable animatable, SolidAnimation anim, SolidAnimationKind kind)
        {
            _anim = anim;
            _animatable = animatable;
            _kind = kind;

            PrepareAnim();
        }

        internal SolidAnimationEcho(Animatable animatable, SolidAnimationKind kind)
        {
            _animatable = animatable;
            _kind = kind;
        }

        private void PrepareAnim()
        {
            _anim.Completed -= HandleAnimCompleted;
            _anim.Completed += HandleAnimCompleted;
        }

        private void HandleAnimCompleted(object sender, EventArgs e)
        {
            _isCompleted = true;
            _anim.Completed -= HandleAnimCompleted;
            Completed?.Invoke(this, EventArgs.Empty);

            if (_next != null) 
                Reverb(this);
        }

        //internal void SetNext(SolidAnimationEcho echo)
        //{
        //    bool hadNext = _next != null;

        //    if (echo._anim == null)
        //        echo._anim = _anim;

        //    _next = echo;

        //    if (_isCompleted && !hadNext) Reverb(this);
        //}

        internal void Start()
        {
            if (_anim == null) return;

            _isCompleted = false;
            _anim.Completed += HandleAnimCompleted;
            _anim.Start(_animatable, _kind);
        }


        internal static void SetNext(SolidAnimationEcho echo, SolidAnimationEcho next)
        {
            bool hadNext = echo._next != null;

            if (next._anim == null)
                next._anim = echo._anim;

            echo._next = next;

            if (echo._isCompleted && !hadNext) Reverb(echo);
        }

        public static void Reverb(SolidAnimationEcho echo)
        {
            if (echo._next == null) return;

            echo._next.Start();
        }

        public SolidAnimationEcho Then(Action callback)
        {
            EventHandler handler = null;
            Completed += handler = (s, e) =>
            {
                Completed -= handler;
                callback();
            };

            return this;
        }
    }

    public class SolidAnimation
    {
        private readonly DoubleAnimationUsingKeyFrames[] _frames;
        private int _completed;

        public event EventHandler Starting;
        public event EventHandler Completed;

        public SolidAnimation(DoubleKeyFrameCollection frames, Duration duration, FillBehavior fillBehavior = FillBehavior.HoldEnd)
        {
            _frames = new DoubleAnimationUsingKeyFrames[3];
            DoubleAnimationUsingKeyFrames frame;

            for (int i = 0; i < _frames.Length; i++)
            {
                _frames[i] = frame = new DoubleAnimationUsingKeyFrames()
                {
                    Duration = duration,
                    FillBehavior = fillBehavior,
                    KeyFrames = frames
                };

                frame.Completed += HandleAnimCompleted;
            }
        }

        public void Start(Animatable animatable,  SolidAnimationKind kind)
        {
            var frames = _frames;
            Starting?.Invoke(this, EventArgs.Empty);

            switch (kind)
            {
                case SolidAnimationKind.Scale:
                    animatable.BeginAnimation(ScaleTransform3D.ScaleXProperty, frames[0]);
                    animatable.BeginAnimation(ScaleTransform3D.ScaleYProperty, frames[1]);
                    animatable.BeginAnimation(ScaleTransform3D.ScaleZProperty, frames[2]);
                    break;

                case SolidAnimationKind.ScaleCenter:
                    animatable.BeginAnimation(ScaleTransform3D.CenterXProperty, frames[0]);
                    animatable.BeginAnimation(ScaleTransform3D.CenterYProperty, frames[1]);
                    animatable.BeginAnimation(ScaleTransform3D.CenterZProperty, frames[2]);
                    break;

                case SolidAnimationKind.Translate:
                    animatable.BeginAnimation(TranslateTransform3D.OffsetXProperty, frames[0]);
                    animatable.BeginAnimation(TranslateTransform3D.OffsetYProperty, frames[1]);
                    animatable.BeginAnimation(TranslateTransform3D.OffsetZProperty, frames[2]);
                    break;

                case SolidAnimationKind.RotateCenter:
                    animatable.BeginAnimation(RotateTransform3D.CenterXProperty, frames[0]);
                    animatable.BeginAnimation(RotateTransform3D.CenterYProperty, frames[1]);
                    animatable.BeginAnimation(RotateTransform3D.CenterZProperty, frames[2]);
                    break;
            }
        }

        private void HandleAnimCompleted(object sender, EventArgs e)
        {
            if (++_completed % 3 != 0) return;

            Completed?.Invoke(this, EventArgs.Empty);
        }
    }

    public static class SolidAnimationExtensions
    {
        public static SolidAnimationEcho BeginAnimation(this Animatable animatable, SolidAnimation solidAnim, SolidAnimationKind kind)
        {
            SolidAnimationEcho echo = new SolidAnimationEcho(animatable, solidAnim, kind);
            echo.Start();

            return echo;
        }

        public static SolidAnimationEcho ThenBegin(this SolidAnimationEcho echo, Animatable animatable, SolidAnimationKind kind)
        {
            SolidAnimationEcho nextEcho = new SolidAnimationEcho(animatable, kind);
            SolidAnimationEcho.SetNext(echo, nextEcho);
            //echo.SetNext(nextEcho);


            return nextEcho;
        }
        
        public static SolidAnimationEcho ThenBegin(this SolidAnimationEcho echo, Animatable animatable, SolidAnimation solidAnim, SolidAnimationKind kind)
        {
            SolidAnimationEcho nextEcho = new SolidAnimationEcho(animatable, solidAnim, kind);
            SolidAnimationEcho.SetNext(echo, nextEcho);
            //echo.SetNext(nextEcho);

            return nextEcho;
        }

        private static void BeginAnimationInternal(Animatable animatable, DoubleAnimationUsingKeyFrames[] frames, SolidAnimationKind kind)
        {
            Console.WriteLine("Animating: {0}", animatable.GetHashCode());

            switch (kind)
            {
                case SolidAnimationKind.Scale:
                    animatable.BeginAnimation(ScaleTransform3D.ScaleXProperty, frames[0]);
                    animatable.BeginAnimation(ScaleTransform3D.ScaleYProperty, frames[1]);
                    animatable.BeginAnimation(ScaleTransform3D.ScaleZProperty, frames[2]);
                    break;

                case SolidAnimationKind.ScaleCenter:
                    animatable.BeginAnimation(ScaleTransform3D.CenterXProperty, frames[0]);
                    animatable.BeginAnimation(ScaleTransform3D.CenterYProperty, frames[1]);
                    animatable.BeginAnimation(ScaleTransform3D.CenterZProperty, frames[2]);
                    break;

                case SolidAnimationKind.Translate:
                    animatable.BeginAnimation(TranslateTransform3D.OffsetXProperty, frames[0]);
                    animatable.BeginAnimation(TranslateTransform3D.OffsetYProperty, frames[1]);
                    animatable.BeginAnimation(TranslateTransform3D.OffsetZProperty, frames[2]);
                    break;

                case SolidAnimationKind.RotateCenter:
                    animatable.BeginAnimation(RotateTransform3D.CenterXProperty, frames[0]);
                    animatable.BeginAnimation(RotateTransform3D.CenterYProperty, frames[1]);
                    animatable.BeginAnimation(RotateTransform3D.CenterZProperty, frames[2]);
                    break;
            }
        }
    }
}
