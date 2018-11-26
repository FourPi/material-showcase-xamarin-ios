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
        UIView _tapView;
        private MaterialShowcaseInstructionView _instructionView;
        private UIStatusBarStyle _previousStyle;

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

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            var newFrame = new CGRect(x: 0, y: 0, width: UIScreen.MainScreen.Bounds.Width,
                height: UIScreen.MainScreen.Bounds.Height);
            if (!newFrame.Equals(Frame))
            {
                Frame = newFrame;
                //            InitViews(true);
                LayoutIfNeeded();
                UpdateViewFrames();
            }

            //            Show(false);
        }

        // Shows it over current screen after completing setup process
        public void Show(bool animated = true, Action completionHandler = null)
        {
            InitViews(animated);
            //			var showcaseWindow = GetShowcaseWindow();
            UIWindow highestWindow = GetShowcaseWindow(_targetView);
            if (highestWindow == null)
            {
                return;
            }
            _containerView = highestWindow;

            Alpha = 0.0f;

            if (!_containerView.Subviews.Contains(this))
                _containerView.AddSubview(this);

            LayoutIfNeeded();
            UpdateViewFrames(animated);

            //            var center = _backgroundView.Center;
            var frameCenter = Frame.Center();
            UpdateStatusBar(animated);
            if (animated)
            {
                UIView.Animate(AniComeInDuration, () =>
                {
                    _targetHolderView.Transform = CGAffineTransform.MakeIdentity(); //CGAffineTransform.MakeScale(1, 1);
                    _targetHolderView.Center = frameCenter;
                    _backgroundView.Transform = CGAffineTransform.MakeIdentity(); //CGAffineTransform.MakeScale(1, 1);
                    _backgroundView.Center = frameCenter;
                    Alpha = 1.0f;

                }, StartAnimations);
            }
            else
            {
                _targetHolderView.Transform = CGAffineTransform.MakeIdentity(); //CGAffineTransform.MakeScale(1, 1);
                _targetHolderView.Center = frameCenter;
                _backgroundView.Transform = CGAffineTransform.MakeIdentity(); //CGAffineTransform.MakeScale(1, 1);
                _backgroundView.Center = frameCenter;
                Alpha = 1.0f;
            }

            completionHandler?.Invoke();
        }

        private void UpdateStatusBar(bool animated = false)
        {
            _previousStyle = UIKit.UIApplication.SharedApplication.StatusBarStyle;
            // https://stackoverflow.com/questions/2509443/check-if-uicolor-is-dark-or-bright
            BackgroundPromptColor.GetRGBA(out var r, out var g, out var b, out var a);
            var luminosityThingo = ((r * 255 * 299) + (g * 255 * 587) + (b * 255 * 114)) / 1000.0;
            //below 125 = use white
            UIKit.UIApplication.SharedApplication.SetStatusBarStyle(
                luminosityThingo < 125 ? UIKit.UIStatusBarStyle.LightContent : UIKit.UIStatusBarStyle.Default,
                animated);
        }

        private void UndoStatusBar(bool animated = false)
        {
            UIKit.UIApplication.SharedApplication.SetStatusBarStyle(_previousStyle, animated);
        }

        private void UpdateViewFrames(bool animated = false)
        {
            var center = CalculateCenter(_targetView, _containerView);
            if (_targetView != null)
            {
                _targetRippleView.Bounds = new CGRect(x: 0, y: 0, width: _targetView.Bounds.Width + TargetHolderRadius,
                    height: _targetView.Bounds.Height + TargetHolderRadius);
                _targetRippleView.Center = center;
            }

            if (_targetHolderView != null && _targetRippleView != null)
            {
                _targetHolderView.Bounds = new CGRect(x: 0, y: 0, width: _targetRippleView.Bounds.Width,
                    height: _targetRippleView.Bounds.Height);

                _targetHolderView.Center = center;
            }

            if (_hiddenTargetHolderView != null && _targetHolderView != null)
            {
                _hiddenTargetHolderView.Frame = _targetHolderView.Frame;
            }

            if (_targetCopyView != null)
            {
                var targetCopyViewWidth = _targetCopyView.Frame.Width;
                var targetCopyViewHeight = _targetCopyView.Frame.Height;

                _targetCopyView.Bounds =
                    new CGRect(x: 0, y: 0, width: targetCopyViewWidth, height: targetCopyViewHeight);
                _targetCopyView.Center = center;
            }

            if (_instructionView != null)
            {
                nfloat yPosition = 0;
                nfloat instructionViewWidth = 0;
                nfloat instructionViewHeight = 0;
                var safeAreaInsets = new UIEdgeInsets();
                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                    safeAreaInsets = SafeAreaInsets;
                Console.WriteLine(
                    $"AddInstructionView {safeAreaInsets.Left}, {safeAreaInsets.Top}, {safeAreaInsets.Right}, {safeAreaInsets.Bottom}");
                var xPosition = safeAreaInsets.Left;
                if (TargetShape == TargetShape.None)
                {
                    yPosition = safeAreaInsets.Top;
                    instructionViewHeight = _containerView.Frame.Height - safeAreaInsets.Top - safeAreaInsets.Bottom;
                }
                else if (_targetView != null)
                {
                    if (GetTargetPosition(_targetView, _containerView) == TargetPosition.Above)
                    {
                        yPosition = center.Y + ((_targetView.Bounds.Height) * 0.5f) + TargetHolderRadius;
                        instructionViewHeight = _containerView.Frame.Height - yPosition - safeAreaInsets.Top -
                                                safeAreaInsets.Bottom;
                    }
                    else
                    {
                        yPosition = safeAreaInsets.Top; //center.Y - TEXT_CENTER_OFFSET - LABEL_DEFAULT_HEIGHT * 2;
                        instructionViewHeight = center.Y - (_targetView.Bounds.Height * 0.5f) - safeAreaInsets.Top;
                    }
                }

                instructionViewWidth =
                    _containerView.Frame.Width - safeAreaInsets.Left -
                    safeAreaInsets.Right; // - (LABEL_MARGIN + LABEL_MARGIN);

                _instructionView.Frame = new CGRect(x: xPosition, y: yPosition, width: instructionViewWidth,
                    height: instructionViewHeight);

                _instructionView.LayoutIfNeeded();
            }

            if (_backgroundView != null)
            {
                switch (BackgroundViewType)
                {
                    case BackgroundTypeStyle.Full:
                        _backgroundView.Frame = new CGRect(x: 0, y: 0, width: UIScreen.MainScreen.Bounds.Width,
                            height: UIScreen.MainScreen.Bounds.Height);
                        break;
                }

                AddBackgroundMask(TargetHolderRadius, _backgroundView);

                var scale = TARGET_HOLDER_RADIUS / (_backgroundView.Frame.Width / 2);
                _backgroundView.Transform = CGAffineTransform.MakeScale(scale, scale); // Initial set to support animation
                _backgroundView.Center = _targetHolderView.Center;
            }

            //TODO move this somewhere better
            if (!animated)
            {
                var frameCenter = Frame.Center();
                _targetHolderView.Transform = CGAffineTransform.MakeIdentity(); //CGAffineTransform.MakeScale(1, 1);
                _targetHolderView.Center = frameCenter;
                _backgroundView.Transform = CGAffineTransform.MakeIdentity(); //CGAffineTransform.MakeScale(1, 1);
                _backgroundView.Center = frameCenter;
            }

            if (_tapView != null)
                _tapView.Frame = Frame;
        }

        private static UIWindow GetShowcaseWindow(UIView targetView)
        {
            return targetView.Window;
        }

        /// Initializes default view properties
        void Configure()
        {
            BackgroundColor = UIColor.Clear;
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
            foreach (var subview in Subviews)
            {
                subview.RemoveFromSuperview();
            }

            AddTargetRipple();
            AddTargetHolder();

            // if color is not UIColor.clear, then add the target snapshot
            if (!Equals(TargetHolderColor, UIColor.Clear) && _targetView != null)
            {
                AddTarget();
            }

            AddInstructionView(animated);

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
                _tapView = new UIView();
                _tapView.BackgroundColor = UIColor.Clear;
                AddSubview(_tapView);
                SendSubviewToBack(_tapView);
                _tapView.AddGestureRecognizer(GetTapGestureRecognizer(animated));
                _instructionView.UserInteractionEnabled = true;
                _instructionView.AddGestureRecognizer(GetTapGestureRecognizer(animated));
            }
        }

        private void AddBackground()
        {
            switch (BackgroundViewType)
            {
                case BackgroundTypeStyle.Full:
                    _backgroundView = new UIView();
                    break;
            }

            _backgroundView.UserInteractionEnabled = false;
            InsertSubviewBelow(_backgroundView, _targetRippleView);
        }

        private void AddBackgroundMask(float radius, UIView view)
        {
            _backgroundView.BackgroundColor = BackgroundPromptColor.ColorWithAlpha(BackgroundPromptColorAlpha);
            var center = _targetRippleView.Center;
            var mutablePath = new CGPath();
            mutablePath.AddRect(view.Bounds);
            if (TargetShape == TargetShape.Circle)
                mutablePath.AddArc(center.X, center.Y, (_targetRippleView.Bounds.Width) * 0.5f, startAngle: 0.0f, endAngle: (nfloat)(2 * Math.PI), clockwise: false);
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
        private void AddTargetRipple()
        {
            _targetRippleView = new UIView();

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
        private void AddTargetHolder()
        {
            _hiddenTargetHolderView = new UIView();

            _hiddenTargetHolderView.Hidden = true;

            _targetHolderView = new UIView();

            _targetHolderView.BackgroundColor = TargetHolderColor;

            if (TargetShape == TargetShape.Circle)
                _targetHolderView.AsCircle();

            _hiddenTargetHolderView.UserInteractionEnabled = false;

            _targetHolderView.Transform = CGAffineTransform.MakeScale(1 / ANI_TARGET_HOLDER_SCALE, 1 / ANI_TARGET_HOLDER_SCALE); // Initial set to support animation
            _targetHolderView.UserInteractionEnabled = false;

            AddSubview(_hiddenTargetHolderView);
            AddSubview(_targetHolderView);
        }

        // Create a copy view of target view
        // It helps us not to affect the original target view
        private void AddTarget()
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

            _targetCopyView.TranslatesAutoresizingMaskIntoConstraints = true;
            _targetCopyView.UserInteractionEnabled = false;

            AddSubview(_targetCopyView);
        }

        // Configures and adds primary label view
        private void AddInstructionView(bool animated = true)
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

            AddSubview(_instructionView);
        }

        public void CompleteShowcase(bool animated = true, bool skipped = false)
        {
            UndoStatusBar(animated);
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
