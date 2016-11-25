namespace BigNote.Controls
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    // re-implementation of viewbox but animation between the different scale transform states
    public class AnimatingViewBox : Decorator
    {
        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch",
                                                                                                typeof (Stretch),
                                                                                                typeof (AnimatingViewBox
                                                                                                    ),
                                                                                                new FrameworkPropertyMetadata
                                                                                                    (Stretch.Uniform,
                                                                                                     FrameworkPropertyMetadataOptions
                                                                                                         .AffectsMeasure),
                                                                                                StretchIsValid);

        public static readonly DependencyProperty StretchDirectionProperty =
            DependencyProperty.Register("StretchDirection", typeof (StretchDirection), typeof (AnimatingViewBox),
                                        new FrameworkPropertyMetadata(StretchDirection.Both,
                                                                      FrameworkPropertyMetadataOptions.AffectsMeasure),
                                        StretchDirectionIsValid);

        private readonly DoubleAnimation scaleXAnimation = new DoubleAnimation();
        private readonly DoubleAnimation scaleYAnimation = new DoubleAnimation();

        private Size currentScale;
        private ContainerVisual internalVisual;

        public AnimatingViewBox()
        {
            scaleXAnimation.EasingFunction = new BackEase {EasingMode = EasingMode.EaseInOut, Amplitude = 0.2};
            scaleXAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 450));

            scaleYAnimation.EasingFunction = new BackEase {EasingMode = EasingMode.EaseInOut, Amplitude = 0.3};
            scaleYAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
        }

        private ContainerVisual InternalVisual
        {
            get
            {
                if (internalVisual == null)
                {
                    internalVisual = new ContainerVisual();
                    AddVisualChild(internalVisual);
                }
                return internalVisual;
            }
        }

        private UIElement InternalChild
        {
            get
            {
                VisualCollection children = InternalVisual.Children;
                if (children.Count != 0)
                {
                    return children[0] as UIElement;
                }
                return null;
            }
            set
            {
                VisualCollection children = InternalVisual.Children;
                if (children.Count != 0)
                {
                    children.Clear();
                }
                children.Add(value);
            }
        }

        private ScaleTransform InternalTransform
        {
            get { return InternalVisual.Transform as ScaleTransform; }
            set { InternalVisual.Transform = value; }
        }

        public override UIElement Child
        {
            get { return InternalChild; }
            set
            {
                UIElement internalChild = InternalChild;
                if (internalChild != value)
                {
                    base.RemoveLogicalChild(internalChild);
                    if (value != null)
                    {
                        AddLogicalChild(value);
                    }
                    InternalChild = value;
                    InvalidateMeasure();
                }
            }
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (InternalChild == null)
                {
                    return null;
                }
                return new SingleChildEnumerator(InternalChild);
            }
        }

        public Stretch Stretch
        {
            get { return (Stretch) GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public StretchDirection StretchDirection
        {
            get { return (StretchDirection) GetValue(StretchDirectionProperty); }
            set { SetValue(StretchDirectionProperty, value); }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException("index", index, "Index was out of range");
            }
            return InternalVisual;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size result = default(Size);
            
            var internalChild = InternalChild;
            if (internalChild != null)
            {
                var availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
                internalChild.Measure(availableSize);
                var desiredSize = internalChild.DesiredSize;
                var size = ComputeScaleFactor(constraint, desiredSize, Stretch, StretchDirection);
                result.Width = size.Width*desiredSize.Width;
                result.Height = size.Height*desiredSize.Height;
            }
            return result;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var internalChild = InternalChild;
            if (internalChild != null)
            {
                var desiredSize = internalChild.DesiredSize;
                var oldScale = currentScale;
                currentScale = ComputeScaleFactor(arrangeSize, desiredSize, Stretch, StretchDirection);
                if (InternalTransform == null)
                {
                    InternalTransform = new ScaleTransform(currentScale.Width, currentScale.Height);
                }
                else
                {
                    scaleXAnimation.From = oldScale.Width;
                    scaleXAnimation.To = currentScale.Width;

                    scaleYAnimation.From = oldScale.Height;
                    scaleYAnimation.To = currentScale.Height;

                    InternalTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
                    InternalTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
                }

                internalChild.Arrange(new Rect(default(Point), internalChild.DesiredSize));
                arrangeSize.Width = currentScale.Width*desiredSize.Width;
                arrangeSize.Height = currentScale.Height*desiredSize.Height;
            }
            return arrangeSize;
        }


        internal static Size ComputeScaleFactor(Size availableSize, Size contentSize, Stretch stretch,
                                                StretchDirection stretchDirection)
        {
            double scaleX = 1.0;
            double scaleY = 1.0;
            bool finiteWidth = !double.IsPositiveInfinity(availableSize.Width);
            bool finiteHeight = !double.IsPositiveInfinity(availableSize.Height);
            if ((stretch == Stretch.Uniform || stretch == Stretch.UniformToFill || stretch == Stretch.Fill) &&
                (finiteWidth || finiteHeight))
            {
                scaleX = (MathUtil.IsZero(contentSize.Width) ? 0.0 : (availableSize.Width/contentSize.Width));
                scaleY = (MathUtil.IsZero(contentSize.Height) ? 0.0 : (availableSize.Height/contentSize.Height));
                if (!finiteWidth)
                {
                    scaleX = scaleY;
                }
                else
                {
                    if (!finiteHeight)
                    {
                        scaleY = scaleX;
                    }
                    else
                    {
                        ScaleByStretch(stretch, ref scaleX, ref scaleY);
                    }
                }
                ScaleByDirectionOnly(stretchDirection, ref scaleX, ref scaleY);
            }
            return new Size(scaleX, scaleY);
        }

        private static void ScaleByStretch(Stretch stretch, ref double scaleX, ref double scaleY)
        {
            switch (stretch)
            {
                case Stretch.Uniform:
                    {
                        double uniformScale = (scaleX < scaleY) ? scaleX : scaleY;
                        scaleY = (scaleX = uniformScale);
                        break;
                    }
                case Stretch.UniformToFill:
                    {
                        double fillScale = (scaleX > scaleY) ? scaleX : scaleY;
                        scaleY = (scaleX = fillScale);
                        break;
                    }
            }
        }

        private static void ScaleByDirectionOnly(StretchDirection stretchDirection, ref double scaleX, ref double scaleY)
        {
            switch (stretchDirection)
            {
                case StretchDirection.UpOnly:
                    {
                        if (scaleX < 1.0)
                        {
                            scaleX = 1.0;
                        }
                        if (scaleY < 1.0)
                        {
                            scaleY = 1.0;
                        }
                        break;
                    }
                case StretchDirection.DownOnly:
                    {
                        if (scaleX > 1.0)
                        {
                            scaleX = 1.0;
                        }
                        if (scaleY > 1.0)
                        {
                            scaleY = 1.0;
                        }
                        break;
                    }
            }
        }

        private static bool StretchIsValid(object value)
        {
            return Enum.IsDefined(typeof (Stretch), value);
        }

        private static bool StretchDirectionIsValid(object value)
        {
            return Enum.IsDefined(typeof (StretchDirection), value);
        }
    }

    internal class SingleChildEnumerator : IEnumerator
    {
        private readonly object child;
        private readonly int count;
        private int index = -1;

        internal SingleChildEnumerator(object Child)
        {
            child = Child;
            count = ((Child == null) ? 0 : 1);
        }

        #region IEnumerator Members

        object IEnumerator.Current
        {
            get
            {
                if (index != 0)
                {
                    return null;
                }
                return child;
            }
        }

        bool IEnumerator.MoveNext()
        {
            index++;
            return index < count;
        }

        void IEnumerator.Reset()
        {
            index = -1;
        }

        #endregion
    }
}