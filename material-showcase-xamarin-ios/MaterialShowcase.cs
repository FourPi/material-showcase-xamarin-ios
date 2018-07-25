using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace MaterialShowcase
{
    public enum BackgroundTypeStyle { Full }
    public enum TargetShape { Circle, Rectangle, None }

    public class MaterialShowcase : UIView
    {
        public event Action<bool> ShowCaseWillDismiss;
        public event Action<bool> ShowCaseDidDismiss;
        public static float BACKGROUND_ALPHA = 0.96f;
        public static float TARGET_HOLDER_RADIUS = 44f;
        public static float TEXT_CENTER_OFFSET = 44 + 20f;
        public static float INSTRUCTIONS_CENTER_OFFSET = 20f;
        public static float LABEL_MARGIN = 20f;
        public static float TARGET_PADDING = 20f;

        public static float LABEL_DEFAULT_HEIGHT = 50f;
        public static UIColor BACKGROUND_DEFAULT_COLOR = "#2196F3".ToUIColor();
        public static UIColor TARGET_HOLDER_COLOR = UIColor.White;

        public static TargetShape DEFAULT_SHAPE = TargetShape.Circle;

        private float ANI_COMEIN_DURATION = 0.5f; // second
        private float ANI_GOOUT_DURATION = 0.5f;  // second
        private float ANI_TARGET_HOLDER_SCALE = 2.2f;
        private UIColor ANI_RIPPLE_COLOR = UIColor.White;
        private float ANI_RIPPLE_ALPHA = 0.5f;
        private float ANI_RIPPLE_START_SCALE = 1.1f;
        private float ANI_RIPPLE_START_DURATION = 0.5f;
        private float ANI_RIPPLE_END_SCALE = 1.6f;
        private float ANI_RIPPLE_END_DURATION = 0.5f;

        private float offsetThreshold = 88f;
        //  
        UIView _containerView;
        UIView _targetView;
        UIView _backgroundView;
        UIView _targetHolderView;
        UIView _hiddenTargetHolderView;
        UIView _targetRippleView;
        UIView _targetCopyView;
        private MaterialShowcaseInstructionView _instructionView;

        public UIColor BackgroundPromptColor { get; set; }
        public float BackgroundPromptColorAlpha { get; set; } = 0.0f;
        public BackgroundTypeStyle BackgroundViewType { get; set; } = BackgroundTypeStyle.Full;
        public TargetShape TargetShape { get; set; }
        // Tap zone settings
        // - false: recognize tap from all displayed showcase.
        // - true: recognize tap for targetView area only.
        public enum TapTargetType { All, Target, None }
        public TapTargetType TapTarget { get; set; } = TapTargetType.None;
        // Target
        public bool ShouldSetTintColor { get; set; } = true;
        public UIColor TargetTintColor { get; set; }
        public float TargetHolderRadius { get; set; } = 0.0f;
        public UIColor TargetHolderColor { get; set; }
        // Text
        public string PrimaryText { get; set; }
        public string SecondaryText { get; set; }
        public string NextText { get; set; }
        public string SkipText { get; set; }
        public UIColor PrimaryTextColor { get; set; }
        public UIColor SecondaryTextColor { get; set; }
        public UIColor NextTextColor { get; set; }
        public UIColor SkipTextColor { get; set; }
        public float PrimaryTextSize { get; set; }
        public float SecondaryTextSize { get; set; }
        public float NextTextSize { get; set; }
        public float SkipTextSize { get; set; }
        public UIFont PrimaryTextFont { get; set; }
        public UIFont SecondaryTextFont { get; set; }
        public UIFont NextTextFont { get; set; }
        public UIFont SkipTextFont { get; set; }
        public UITextAlignment PrimaryTextAlignment { get; set; }
        public UITextAlignment SecondaryTextAlignment { get; set; }

        // Animation
        public float AniComeInDuration { get; set; } = 0f;
        public float AniGoOutDuration { get; set; } = 0f;
        public float AniRippleStartScale { get; set; } = 0.0f;
        public float AniRippleStartDuration { get; set; } = 0.0f;
        public float AniRippleEndScale { get; set; } = 0.0f;
        public float AniRippleEndDuration { get; set; } = 0.0f;
        public UIColor AniRippleColor { get; set; }
        public float AniRippleAlpha { get; set; } = 0.0f;

        public MaterialShowcase() : base(new CGRect(x: 0, y: 0, width: UIScreen.MainScreen.Bounds.Width, height: UIScreen.MainScreen.Bounds.Height))
        {
            Configure();
        }

        // Sets a general UIView as target
        public void SetTargetView(UIView view)
        {
            _targetView = view;

            if (_targetView is UILabel label)
            {
                TargetTintColor = label.TextColor;
                BackgroundPromptColor = label.TextColor;
            }
            else if (_targetView is UIButton button)
            {
                var tintColor = button.TitleColor(UIControlState.Normal);
                TargetTintColor = tintColor;
                BackgroundPromptColor = tintColor;
            }
            else
            {
                TargetTintColor = _targetView.TintColor;
                BackgroundPromptColor = _targetView.TintColor;
            }
        }

        // Sets a UIBarButtonItem as target
        public void SetTargetView(UIBarButtonItem barButtonItem)
        {
            var view = (barButtonItem.ValueForKey(new NSString("view")) as UIView)?.Subviews?.FirstOrDefault();
            if (view != null)
            {
                _targetView = view;
            }
        }

        // Sets a UITabBar Item as target
        public void SetTargetView(UITabBar tabBar, int itemIndex)
        {
            var tabBarItems = OrderedTabBarItemViews(tabBar);
            if (itemIndex < tabBarItems.Count)
            {
                _targetView = tabBarItems[itemIndex];
                TargetTintColor = tabBar.TintColor;
                BackgroundPromptColor = tabBar.TintColor;
            }
            else
            {
                //	print("The tab bar item index is out of range")
            }
        }

        // Sets a UITableViewCell as target
        public void SetTargetView(UITableView tableView, int section, int row)
        {
            var indexPath = NSIndexPath.FromRowSection(row, section);
            _targetView = tableView.CellAt(indexPath)?.ContentView;
            // for table viewcell, we do not need target holder (circle view)
            // therefore, set its radius = 0
            TargetHolderRadius = 0;
        }

        // Shows it over current screen after completing setup process
        public void Show(bool animated = true, Action completionHandler = null)
        {
            InitViews(animated);

            Alpha = 0.0f;

            _containerView.AddSubview(this);
            LayoutIfNeeded();

            var scale = TARGET_HOLDER_RADIUS / (_backgroundView.Frame.Width / 2);
            var center = _backgroundView.Center;

            _backgroundView.Transform = CGAffineTransform.MakeScale(scale, scale); // Initial set to support animation
            _backgroundView.Center = _targetHolderView.Center;
            if (animated)
            {
                UIView.Animate(AniComeInDuration, () =>
                {
                    _targetHolderView.Transform = CGAffineTransform.MakeScale(1, 1);
                    _targetHolderView.Center = center;
                    _backgroundView.Transform = CGAffineTransform.MakeScale(1, 1);
                    _backgroundView.Center = center;
                    Alpha = 1.0f;

                }, StartAnimations);
            }
            else
            {
                _targetHolderView.Transform = CGAffineTransform.MakeScale(1, 1);
                _targetHolderView.Center = center;
                _backgroundView.Transform = CGAffineTransform.MakeScale(1, 1);
                _backgroundView.Center = center;
                Alpha = 1.0f;
            }
            completionHandler?.Invoke();
        }

        // Returns the current showcases displayed on screen.
        // It will return null if no showcase exists.
        public static List<MaterialShowcase> PresentedShowcases()
        {
            UIWindow highestWindow = KeyWindow();

            return (highestWindow?.Subviews)?.OfType<MaterialShowcase>().ToList();
        }

        private static UIWindow KeyWindow()
        {
            var windows = UIApplication.SharedApplication.Windows;
            UIWindow keyWindow = null;
            foreach (var window in windows)
            {
                if (keyWindow == null || (window.WindowLevel < keyWindow.WindowLevel) || (window.WindowLevel == keyWindow.WindowLevel && window.IsKeyWindow))
                    keyWindow = window;
            }

            return keyWindow;
            //
            //			var windows = UIApplication.SharedApplication.Windows;
            //			UIWindow lowestWindow = null;
            //			foreach (var window in windows)
            //			{
            //				if (lowestWindow == null || (window.WindowLevel < lowestWindow.WindowLevel))
            //					lowestWindow = window;
            //			}
            //
            //			return lowestWindow;
        }

        /// Initializes default view properties
        void Configure()
        {
            BackgroundColor = UIColor.Clear;
            UIWindow highestWindow = KeyWindow();
            if (highestWindow == null)
            {
                return;
            }
            _containerView = highestWindow;
            SetDefaultProperties();
        }

        void SetDefaultProperties()
        {
            // Background
            BackgroundPromptColor = BACKGROUND_DEFAULT_COLOR;

            BackgroundPromptColorAlpha = BACKGROUND_ALPHA;

            TargetShape = DEFAULT_SHAPE;

            // Target view
            TargetTintColor = BACKGROUND_DEFAULT_COLOR;

            TargetHolderColor = TARGET_HOLDER_COLOR;

            TargetHolderRadius = TARGET_HOLDER_RADIUS;
            // Text
            PrimaryText = MaterialShowcaseInstructionView.PRIMARY_DEFAULT_TEXT;

            SecondaryText = MaterialShowcaseInstructionView.SECONDARY_DEFAULT_TEXT;

            NextText = MaterialShowcaseInstructionView.NEXT_DEFAULT_TEXT;

            SkipText = MaterialShowcaseInstructionView.SKIP_DEFAULT_TEXT;

            PrimaryTextColor = MaterialShowcaseInstructionView.PRIMARY_TEXT_COLOR;

            SecondaryTextColor = MaterialShowcaseInstructionView.SECONDARY_TEXT_COLOR;

            NextTextColor = MaterialShowcaseInstructionView.NEXT_TEXT_COLOR;

            SkipTextColor = MaterialShowcaseInstructionView.SKIP_TEXT_COLOR;

            PrimaryTextSize = MaterialShowcaseInstructionView.PRIMARY_TEXT_SIZE;

            SecondaryTextSize = MaterialShowcaseInstructionView.SECONDARY_TEXT_SIZE;

            NextTextSize = MaterialShowcaseInstructionView.NEXT_TEXT_SIZE;

            SkipTextSize = MaterialShowcaseInstructionView.SKIP_TEXT_SIZE;

            // Animation
            AniComeInDuration = ANI_COMEIN_DURATION;

            AniGoOutDuration = ANI_GOOUT_DURATION;

            AniRippleAlpha = ANI_RIPPLE_ALPHA;

            AniRippleColor = ANI_RIPPLE_COLOR;

            AniRippleStartScale = ANI_RIPPLE_START_SCALE;

            AniRippleStartDuration = ANI_RIPPLE_START_SCALE;

            AniRippleEndScale = ANI_RIPPLE_END_SCALE;

            AniRippleEndDuration = ANI_RIPPLE_END_SCALE;
        }

        void StartAnimations()
        {
            //			var options = UIViewKeyframeAnimationOptions.Repeat | UIViewKeyframeAnimationOptions.CalculationModeCubicPaced UIViewKeyframeAnimationOptions = [.curveEaseInOut, .repeat]
            UIViewKeyframeAnimationOptions options = UIViewKeyframeAnimationOptions.Repeat | (UIViewKeyframeAnimationOptions)UIViewAnimationOptions.CurveEaseInOut;
            UIView.AnimateKeyframes(AniRippleStartScale + AniRippleEndScale, 0, options, () =>
             {
                 UIView.AddKeyframeWithRelativeStartTime(0, AniRippleStartDuration, () =>
                 {
                     _targetRippleView.Alpha = AniRippleAlpha;
                     _targetHolderView.Transform = CGAffineTransform.MakeScale(AniRippleEndScale, AniRippleEndScale);
                     _targetRippleView.Transform = CGAffineTransform.MakeScale(AniRippleEndScale, AniRippleEndScale);
                 });
                 UIView.AddKeyframeWithRelativeStartTime(AniRippleStartDuration, AniRippleEndDuration, () =>
                 {
                     _targetHolderView.Transform = CGAffineTransform.MakeIdentity();
                     _targetRippleView.Alpha = 0;
                     _targetRippleView.Transform = CGAffineTransform.MakeScale(AniRippleStartScale, AniRippleStartScale);
                 });

             }, success => { });
        }

        private UIGestureRecognizer GetTapGestureRecognizer(bool animated = true)
        {
            var tapGesture = new UITapGestureRecognizer(() => CompleteShowcase(animated))
            {
                NumberOfTapsRequired = 1,
                NumberOfTouchesRequired = 1
            };
            return tapGesture;
        }

        void InitViews(bool animated = true)
        {
            var center = CalculateCenter(_targetView, _containerView);
            AddTargetRipple(center);
            AddTargetHolder(center);

            // if color is not UIColor.clear, then add the target snapshot
            if (!Equals(TargetHolderColor, UIColor.Clear) && _targetView != null)
            {
                AddTarget(center);
            }

            AddInstructionView(center, animated);

            _instructionView.LayoutIfNeeded();

            AddBackground();

            if (TapTarget == TapTargetType.Target && !Equals(TargetHolderColor, UIColor.Clear))
            {
                //Add gesture recognizer for targetCopyView
                _targetCopyView.AddGestureRecognizer(GetTapGestureRecognizer(animated));
                _targetCopyView.UserInteractionEnabled = true;
            }
            else if (TapTarget != TapTargetType.None)
            {
                // Add gesture recognizer for both container and its subview
                var tapView = new UIView(Frame);
                tapView.BackgroundColor = UIColor.Clear;
                InsertSubviewBelow(tapView, _instructionView);
                //AddSubview(tapView);
                //SendSubviewToBack(tapView);
                tapView.AddGestureRecognizer(GetTapGestureRecognizer(animated));
                _instructionView.UserInteractionEnabled = true;
                _instructionView.AddGestureRecognizer(GetTapGestureRecognizer(animated));
            }
        }

        // Add background which is a big circle
        private void AddBackground()
        {
            switch (BackgroundViewType)
            {
                //				case BackgroundTypeStyle.Circle:
                //					float radius;
                //					var center = _targetRippleView.Center;//getOuterCircleCenterPoint(for: targetCopyView)
                //					if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                //					{
                //						radius = 300.0f;
                //					}
                //					else
                //					{
                //						radius = Utils.GetOuterCircleRadius(center: center, textBounds: _instructionView.Frame, targetBounds: _targetRippleView.Frame);
                //
                //					}
                //
                //					_backgroundView = new UIView(frame: new CGRect(x: 0, y: 0, width: radius * 2, height: radius * 2));
                //					_backgroundView.Center = center;
                //					_backgroundView.AsCircle();
                //					break;

                case BackgroundTypeStyle.Full:
                    _backgroundView = new UIView(frame: new CGRect(x: 0, y: 0, width: UIScreen.MainScreen.Bounds.Width, height: UIScreen.MainScreen.Bounds.Height));
                    break;
            }

            _backgroundView.BackgroundColor = BackgroundPromptColor.ColorWithAlpha(BackgroundPromptColorAlpha);
            _backgroundView.UserInteractionEnabled = false;
            InsertSubviewBelow(_backgroundView, _targetRippleView);
            AddBackgroundMask(TargetHolderRadius, _backgroundView);
        }
        //
        private void AddBackgroundMask(float radius, UIView view)
        {
            var center = _targetRippleView.Center;
            var mutablePath = new CGPath();
            mutablePath.AddRect(view.Bounds);
            if (TargetShape == TargetShape.Circle)
                mutablePath.AddArc(center.X, center.Y, (_targetRippleView.Bounds.Width) * 0.5f, startAngle: 0.0f, endAngle: 2 * 3.14f, clockwise: false);
            else if (TargetShape == TargetShape.Rectangle)
                mutablePath.AddRect(new CGRect(center.X - (_targetRippleView.Bounds.Width * 0.5f), center.Y - (_targetRippleView.Bounds.Height * 0.5f), _targetRippleView.Bounds.Width, _targetRippleView.Bounds.Height));
            var mask = new CAShapeLayer
            {
                Path = mutablePath,
                FillRule = CoreAnimation.CAShapeLayer.FillRuleEvenOdd
            };
            view.Layer.Mask = mask;
        }

        // A background view which add ripple animation when showing target view
        private void AddTargetRipple(CGPoint center)
        {
            _targetRippleView = new UIView();
            if (_targetView != null)
            {
                _targetRippleView.Frame = new CGRect(x: 0, y: 0, width: _targetView.Bounds.Width + TargetHolderRadius, height: _targetView.Bounds.Height + TargetHolderRadius);
            }

            _targetRippleView.Center = center;

            _targetRippleView.BackgroundColor = AniRippleColor;

            _targetRippleView.Alpha = 0.0f; //set it invisible

            if (TargetShape == TargetShape.Circle)
                _targetRippleView.AsCircle();
            else if (TargetShape == TargetShape.None)
                _targetRippleView.Hidden = true;

            _targetRippleView.UserInteractionEnabled = false;

            AddSubview(_targetRippleView);
        }


        /// A circle-shape background view of target view
        private void AddTargetHolder(CGPoint center)
        {
            _hiddenTargetHolderView = new UIView();

            _hiddenTargetHolderView.Hidden = true;

            _targetHolderView = new UIView()
            {
                Frame = new CGRect(x: 0, y: 0, width: _targetRippleView.Bounds.Width, height: _targetRippleView.Bounds.Height)
            };

            _targetHolderView.Center = center;
            _targetHolderView.BackgroundColor = TargetHolderColor;

            if (TargetShape == TargetShape.Circle)
                _targetHolderView.AsCircle();

            _hiddenTargetHolderView.Frame = _targetHolderView.Frame;
            _hiddenTargetHolderView.UserInteractionEnabled = false;

            _targetHolderView.Transform = CGAffineTransform.MakeScale(1 / ANI_TARGET_HOLDER_SCALE, 1 / ANI_TARGET_HOLDER_SCALE); // Initial set to support animation
            _targetHolderView.UserInteractionEnabled = false;

            AddSubview(_hiddenTargetHolderView);
            AddSubview(_targetHolderView);
        }

        // Create a copy view of target view
        // It helps us not to affect the original target view
        private void AddTarget(CGPoint center)
        {
            _targetCopyView = _targetView.SnapshotView(afterScreenUpdates: true);

            if (ShouldSetTintColor)
            {
                _targetCopyView.SetTintColor(TargetTintColor, true);

                if (_targetCopyView is UIButton buttonCopy)
                {
                    var button = _targetView as UIButton;

                    buttonCopy.SetImage(button.ImageForState(UIControlState.Normal)?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    buttonCopy.SetTitleColor(TargetTintColor, UIControlState.Normal);
                    buttonCopy.Enabled = true;
                }
                else if (_targetCopyView is UIImageView imageViewCopy)
                {
                    var imageView = _targetView as UIImageView;
                    imageViewCopy.Image = imageView.Image?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                }
                else if (_targetCopyView.Subviews.FirstOrDefault() is UIImageView imageSubviewCopy)
                {
                    var labelCopy = _targetCopyView.Subviews.LastOrDefault() as UILabel;
                    var imageView = _targetView.Subviews.FirstOrDefault() as UIImageView;
                    imageSubviewCopy.Image = imageView.Image?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    labelCopy.TextColor = TargetTintColor;
                }
                else if (_targetCopyView is UILabel label)
                {
                    label.TextColor = TargetTintColor;
                }
            }

            var width = _targetCopyView.Frame.Width;
            var height = _targetCopyView.Frame.Height;

            _targetCopyView.Frame = new CGRect(x: 0, y: 0, width: width, height: height);
            _targetCopyView.Center = center;
            _targetCopyView.TranslatesAutoresizingMaskIntoConstraints = true;
            _targetCopyView.UserInteractionEnabled = false;

            AddSubview(_targetCopyView);
        }

        // Configures and adds primary label view
        private void AddInstructionView(CGPoint center, bool animated = true)
        {
            _instructionView = new MaterialShowcaseInstructionView();
            _instructionView.ButtonPressed += skipped => { CompleteShowcase(animated, skipped); };
            _instructionView.PrimaryTextAlignment = PrimaryTextAlignment;
            _instructionView.PrimaryTextFont = PrimaryTextFont;
            _instructionView.PrimaryTextSize = PrimaryTextSize;
            _instructionView.PrimaryTextColor = PrimaryTextColor;
            _instructionView.PrimaryText = PrimaryText;

            _instructionView.SecondaryTextAlignment = SecondaryTextAlignment;
            _instructionView.SecondaryTextFont = SecondaryTextFont;
            _instructionView.SecondaryTextSize = SecondaryTextSize;
            _instructionView.SecondaryTextColor = SecondaryTextColor;
            _instructionView.SecondaryText = SecondaryText;

            _instructionView.NextTextFont = NextTextFont;
            _instructionView.NextTextSize = NextTextSize;
            _instructionView.NextTextColor = NextTextColor;
            _instructionView.NextText = NextText;

            _instructionView.SkipTextFont = SkipTextFont;
            _instructionView.SkipTextSize = SkipTextSize;
            _instructionView.SkipTextColor = SkipTextColor;
            _instructionView.SkipText = SkipText;
            // Calculate x position
            var xPosition = 0.0f;//LABEL_MARGIN;

            // Calculate y position
            nfloat yPosition;

            // Calculate instructionView width
            nfloat width;
            nfloat height;

            xPosition = 0;
            if (TargetShape == TargetShape.None)
            {
                yPosition = 0;
                height = _containerView.Frame.Height;
            }
            else
            {
                if (_targetView != null && GetTargetPosition(_targetView, _containerView) == TargetPosition.Above)
                {
                    yPosition = center.Y + (_targetView.Bounds.Height * 0.5f) + TargetHolderRadius;
                    height = _containerView.Frame.Height - yPosition;
                }
                else
                {
                    yPosition = 0; //center.Y - TEXT_CENTER_OFFSET - LABEL_DEFAULT_HEIGHT * 2;
                    height = center.Y - (_targetView.Bounds.Height * 0.5f);
                }
            }

            width = _containerView.Frame.Width;// - (LABEL_MARGIN + LABEL_MARGIN);

            _instructionView.Frame = new CGRect(x: xPosition, y: yPosition, width: width, height: height);

            AddSubview(_instructionView);
        }

        public void CompleteShowcase(bool animated = true, bool skipped = false)
        {
            ShowCaseWillDismiss?.Invoke(skipped);
            if (animated)
            {
                _targetRippleView.RemoveFromSuperview();
                UIView.AnimateKeyframes(AniGoOutDuration, 0, UIViewKeyframeAnimationOptions.CalculationModeLinear,
                () =>
                {
                    UIView.AddKeyframeWithRelativeStartTime(0, 3 / 5f, () =>
                    {
                        _targetHolderView.Transform = CGAffineTransform.MakeScale(0.4f, 0.4f);
                        _backgroundView.Transform = CGAffineTransform.MakeScale(1.3f, 1.3f);
                        _backgroundView.Alpha = 0;
                    });
                    UIView.AddKeyframeWithRelativeStartTime(3 / 5f, 2 / 5f, () => { Alpha = 0; });
                },
                success =>
                {
                    if (success)
                    {
                        RecycleSubviews();
                        // Remove it from current screen
                        RemoveFromSuperview();
                        ShowCaseDidDismiss?.Invoke(skipped);
                    }
                });
            }
            else
            {
                // Recycle subviews
                RecycleSubviews();
                // Remove it from current screen
                RemoveFromSuperview();
                ShowCaseDidDismiss?.Invoke(skipped);
            }
        }

        private void RecycleSubviews()
        {
            foreach (var subview in Subviews)
            {
                subview.RemoveFromSuperview();
            }
        }

        // Defines the position of target view
        // which helps to place texts at suitable positions
        enum TargetPosition
        {
            Above, // at upper screen part
            Below // at lower screen part
        }

        // Detects the position of target view relative to its container
        TargetPosition GetTargetPosition(UIView target, UIView container)
        {
            var center = CalculateCenter(target, container);
            if (center.Y < container.Frame.Height / 2)
            {
                return TargetPosition.Above;
            }
            else
            {
                return TargetPosition.Below;
            }
        }

        // Calculates the center point based on targetview
        CGPoint CalculateCenter(UIView targetView, UIView containerView)
        {
            if (targetView == null)
                return containerView.Center;
            var targetRect = targetView.ConvertRectToView(targetView.Bounds, containerView);
            return targetRect.Center();
        }

        // Gets all UIView from TabBarItem.
        private static List<UIView> OrderedTabBarItemViews(UITabBar tabBar)
        {
            var interactionViews = tabBar.Subviews.Where(view => view.UserInteractionEnabled);
            return interactionViews.OrderBy(view => view.Frame.GetMinX()).ToList();
        }
    }
}
