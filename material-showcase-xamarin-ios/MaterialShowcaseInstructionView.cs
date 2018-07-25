using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;

namespace MaterialShowcase
{
    class MaterialShowcaseInstructionView : UIView
    {
        //		private const int SkipSpacing = 40;

        internal static float PRIMARY_TEXT_SIZE = 20f;
        internal static float SECONDARY_TEXT_SIZE = 15f;
        internal static float NEXT_TEXT_SIZE = 15f;
        internal static float SKIP_TEXT_SIZE = 15f;
        internal static UIColor PRIMARY_TEXT_COLOR = UIColor.White;
        internal static UIColor SECONDARY_TEXT_COLOR = UIColor.White.ColorWithAlpha(0.87f);
        internal static UIColor NEXT_TEXT_COLOR = UIColor.White.ColorWithAlpha(0.87f);
        internal static UIColor SKIP_TEXT_COLOR = UIColor.White.ColorWithAlpha(0.87f);
        internal static string PRIMARY_DEFAULT_TEXT = "Awesome action";
        internal static string SECONDARY_DEFAULT_TEXT = "Tap here to do some awesome thing";
        internal static string NEXT_DEFAULT_TEXT = "Next";
        internal static string SKIP_DEFAULT_TEXT = "Skip";

        UILabel _primaryLabel;
        UILabel _secondaryLabel;
        UILabel _nextLabel;
        UILabel _skipLabel;

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

        public event Action<bool> ButtonPressed;

        public MaterialShowcaseInstructionView() : base(new CGRect(x: 0, y: 0, width: UIScreen.MainScreen.Bounds.Width, height: 0))
        {
            Configure();
        }

        /// Initializes default view properties
        void Configure()
        {
            SetDefaultProperties();
        }

        void SetDefaultProperties()
        {
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
        }

        // Configures and adds primary label view
        private void AddPrimaryLabel()
        {
            _primaryLabel = new UILabel();

            _primaryLabel.Font = PrimaryTextFont != null ? PrimaryTextFont : UIFont.BoldSystemFontOfSize(PrimaryTextSize);
            _primaryLabel.TextColor = PrimaryTextColor;
            _primaryLabel.TextAlignment = PrimaryTextAlignment;
            _primaryLabel.Lines = 0;
            _primaryLabel.LineBreakMode = UILineBreakMode.WordWrap;
            _primaryLabel.Text = PrimaryText;
            _primaryLabel.SizeToFit();
            var height = _primaryLabel.Frame.Height;
            var width = _primaryLabel.Frame.Width;
            var maxWidth = Frame.Width - (MaterialShowcase.LABEL_MARGIN * 2);
            if (width > maxWidth)
                width = maxWidth;
            _primaryLabel.Frame = new CGRect(x: (Bounds.Width - width) * 0.5, y: Bounds.Center().Y - height, width: width, height: height);
            _primaryLabel.SizeToFitHeight();
            _primaryLabel.UserInteractionEnabled = false;
            AddSubview(_primaryLabel);
        }

        // Configures and adds secondary label view
        private void AddSecondaryLabel()
        {
            _secondaryLabel = new UILabel();

            _secondaryLabel.Font = SecondaryTextFont != null ? SecondaryTextFont : UIFont.SystemFontOfSize(SecondaryTextSize);
            _secondaryLabel.TextColor = SecondaryTextColor;
            _secondaryLabel.TextAlignment = SecondaryTextAlignment;
            _secondaryLabel.LineBreakMode = UILineBreakMode.WordWrap;
            _secondaryLabel.Text = SecondaryText;
            _secondaryLabel.Lines = 3;
            _secondaryLabel.SizeToFit();
            var height = _secondaryLabel.Frame.Height;
            var width = _secondaryLabel.Frame.Width;
            var maxWidth = Frame.Width - (MaterialShowcase.LABEL_MARGIN * 2);
            if (width > maxWidth)
                width = maxWidth;
            _secondaryLabel.Frame = new CGRect(x: (Bounds.Width - width) * 0.5, y: _primaryLabel.Frame.Bottom, width: width, height: height);
            _secondaryLabel.SizeToFitHeight();
            _secondaryLabel.UserInteractionEnabled = false;
            AddSubview(_secondaryLabel);
        }

        private UIGestureRecognizer GetTapGestureRecognizer(bool skipped)
        {
            var tapGesture = new UITapGestureRecognizer(() => { ButtonPressed?.Invoke(skipped); })
            {
                NumberOfTapsRequired = 1,
                NumberOfTouchesRequired = 1
            };
            return tapGesture;
        }

        private void AddNextLabel()
        {
            _nextLabel = new UILabel();

            _nextLabel.Font = NextTextFont != null ? NextTextFont : UIFont.SystemFontOfSize(NextTextSize);
            _nextLabel.TextColor = NextTextColor;
            _nextLabel.TextAlignment = UITextAlignment.Center;
            _nextLabel.LineBreakMode = UILineBreakMode.WordWrap;
            _nextLabel.Text = NextText;
            _nextLabel.Lines = 3;
            //			_nextLabel.BackgroundColor = UIColor.Green;
            _nextLabel.SizeToFit();
            var height = _nextLabel.Frame.Height + (MaterialShowcase.LABEL_MARGIN * 2);
            var width = _nextLabel.Frame.Width + (MaterialShowcase.LABEL_MARGIN * 2);
            _nextLabel.Frame = new CGRect(x: Frame.Width - width, y: Frame.Height - height, width: width, height: height);
            if (!string.IsNullOrEmpty(NextText))
            {
                UserInteractionEnabled = true;
                _nextLabel.UserInteractionEnabled = true;
                // Add next tap gesture
                _nextLabel.AddGestureRecognizer(GetTapGestureRecognizer(false));
            }
            else
            {
                _nextLabel.UserInteractionEnabled = false;
                UserInteractionEnabled = false;
            }

            AddSubview(_nextLabel);
        }

        private void AddSkipLabel()
        {
            _skipLabel = new UILabel();

            _skipLabel.Font = SkipTextFont != null ? SkipTextFont : UIFont.SystemFontOfSize(SkipTextSize);
            _skipLabel.TextColor = SkipTextColor;
            _skipLabel.TextAlignment = UITextAlignment.Center;
            _skipLabel.LineBreakMode = UILineBreakMode.WordWrap;
            _skipLabel.Text = SkipText;
            _skipLabel.Lines = 3;
            //			_skipLabel.BackgroundColor = UIColor.Red;
            _skipLabel.SizeToFit();
            var height = _skipLabel.Frame.Height + (MaterialShowcase.LABEL_MARGIN * 2);
            var width = _skipLabel.Frame.Width + (MaterialShowcase.LABEL_MARGIN * 2);
            _skipLabel.Frame = new CGRect(x: 0, y: Frame.Height - height, width: width, height: height);
            if (!string.IsNullOrEmpty(SkipText))
            {
                UserInteractionEnabled = true;
                _skipLabel.UserInteractionEnabled = true;
                // Add skip tap gesture
                _skipLabel.AddGestureRecognizer(GetTapGestureRecognizer(true));
            }
            else
            {
                _skipLabel.UserInteractionEnabled = false;
                UserInteractionEnabled = false;
            }

            AddSubview(_skipLabel);
        }

        /// Overrides this to add subviews. They will be drawn when calling show()
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            AddPrimaryLabel();
            AddSecondaryLabel();
            AddNextLabel();
            AddSkipLabel();
        }
    }
}