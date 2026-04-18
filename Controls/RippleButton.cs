using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MusicDownload.Controls
{
    public class RippleButton : Button
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(RippleButton),
                new FrameworkPropertyMetadata(new CornerRadius(14), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty RippleBrushProperty =
            DependencyProperty.Register(
                nameof(RippleBrush),
                typeof(Brush),
                typeof(RippleButton),
                new FrameworkPropertyMetadata(
                    new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)),
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShadowColorProperty =
            DependencyProperty.Register(
                nameof(ShadowColor),
                typeof(Color),
                typeof(RippleButton),
                new FrameworkPropertyMetadata(Color.FromRgb(79, 139, 255), FrameworkPropertyMetadataOptions.AffectsRender));

        private FrameworkElement _rippleElement;
        private ScaleTransform _rippleScaleTransform;
        private TranslateTransform _rippleTranslateTransform;
        private Storyboard _activeRippleStoryboard;

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public Brush RippleBrush
        {
            get { return (Brush)GetValue(RippleBrushProperty); }
            set { SetValue(RippleBrushProperty, value); }
        }

        public Color ShadowColor
        {
            get { return (Color)GetValue(ShadowColorProperty); }
            set { SetValue(ShadowColorProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rippleElement = GetTemplateChild("PART_Ripple") as FrameworkElement;
            _rippleScaleTransform = GetTemplateChild("PART_RippleScale") as ScaleTransform;
            _rippleTranslateTransform = GetTemplateChild("PART_RippleTranslate") as TranslateTransform;

            ResetRippleVisual();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            StartRipple(e.GetPosition(this));
        }

        private void StartRipple(Point origin)
        {
            if (_rippleElement == null || _rippleScaleTransform == null || _rippleTranslateTransform == null)
            {
                return;
            }

            var rippleSize = Math.Max(ActualWidth, ActualHeight) * 2.2;
            if (rippleSize <= 0)
            {
                return;
            }

            StopRippleStoryboard();

            _rippleElement.Width = rippleSize;
            _rippleElement.Height = rippleSize;
            _rippleTranslateTransform.X = origin.X - (rippleSize / 2);
            _rippleTranslateTransform.Y = origin.Y - (rippleSize / 2);
            _rippleScaleTransform.ScaleX = 0;
            _rippleScaleTransform.ScaleY = 0;
            _rippleElement.Opacity = 0;

            var duration = TimeSpan.FromMilliseconds(560);
            var scaleAnimation = new DoubleAnimation(0, 1, duration)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.28, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80))));
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(duration)));

            _activeRippleStoryboard = new Storyboard
            {
                FillBehavior = FillBehavior.Stop
            };

            Storyboard.SetTarget(scaleAnimation, _rippleScaleTransform);
            Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
            _activeRippleStoryboard.Children.Add(scaleAnimation);

            var scaleYAnimation = scaleAnimation.Clone();
            Storyboard.SetTarget(scaleYAnimation, _rippleScaleTransform);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
            _activeRippleStoryboard.Children.Add(scaleYAnimation);

            Storyboard.SetTarget(opacityAnimation, _rippleElement);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            _activeRippleStoryboard.Children.Add(opacityAnimation);

            _activeRippleStoryboard.Completed += ActiveRippleStoryboard_Completed;
            _activeRippleStoryboard.Begin();
        }

        private void ActiveRippleStoryboard_Completed(object sender, EventArgs e)
        {
            if (_activeRippleStoryboard != null)
            {
                _activeRippleStoryboard.Completed -= ActiveRippleStoryboard_Completed;
                _activeRippleStoryboard = null;
            }

            ResetRippleVisual();
        }

        private void StopRippleStoryboard()
        {
            if (_activeRippleStoryboard == null)
            {
                return;
            }

            _activeRippleStoryboard.Completed -= ActiveRippleStoryboard_Completed;
            _activeRippleStoryboard.Stop();
            _activeRippleStoryboard = null;
        }

        private void ResetRippleVisual()
        {
            if (_rippleElement != null)
            {
                _rippleElement.Opacity = 0;
            }

            if (_rippleScaleTransform != null)
            {
                _rippleScaleTransform.ScaleX = 0;
                _rippleScaleTransform.ScaleY = 0;
            }
        }
    }
}
