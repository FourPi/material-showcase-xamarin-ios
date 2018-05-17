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
		private const int SkipSpacing = 40;

		internal static float PRIMARY_TEXT_SIZE = 20f;
		internal static float SECONDARY_TEXT_SIZE = 15f;
		internal static float SKIP_TEXT_SIZE = 15f;
		internal static UIColor PRIMARY_TEXT_COLOR = UIColor.White;
		internal static UIColor SECONDARY_TEXT_COLOR = UIColor.White.ColorWithAlpha(0.87f);
		internal static UIColor SKIP_TEXT_COLOR = UIColor.White.ColorWithAlpha(0.87f);
		internal static string PRIMARY_DEFAULT_TEXT = "Awesome action";
		internal static string SECONDARY_DEFAULT_TEXT = "Tap here to do some awesome thing";
		internal static string SKIP_DEFAULT_TEXT = "Skip";

		UILabel _primaryLabel;
		UILabel _secondaryLabel;
		UILabel _skipLabel;

		public string PrimaryText { get; set; }
		public string SecondaryText { get; set; }
		public string SkipText { get; set; }
		public UIColor PrimaryTextColor { get; set; }
		public UIColor SecondaryTextColor { get; set; }
		public UIColor SkipTextColor { get; set; }
		public float PrimaryTextSize { get; set; }
		public float SecondaryTextSize { get; set; }
		public float SkipTextSize { get; set; }
		public UIFont PrimaryTextFont { get; set; }
		public UIFont SecondaryTextFont { get; set; }
		public UIFont SkipTextFont { get; set; }
		public UITextAlignment PrimaryTextAlignment { get; set; }
		public UITextAlignment SecondaryTextAlignment { get; set; }
		public UITextAlignment SkipTextAlignment { get; set; }

		public event Action Skipped;

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

			SkipText = MaterialShowcaseInstructionView.SKIP_DEFAULT_TEXT;

			PrimaryTextColor = MaterialShowcaseInstructionView.PRIMARY_TEXT_COLOR;

			SecondaryTextColor = MaterialShowcaseInstructionView.SECONDARY_TEXT_COLOR;

			SkipTextColor = MaterialShowcaseInstructionView.SKIP_TEXT_COLOR;

			PrimaryTextSize = MaterialShowcaseInstructionView.PRIMARY_TEXT_SIZE;

			SecondaryTextSize = MaterialShowcaseInstructionView.SECONDARY_TEXT_SIZE;

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

			_primaryLabel.Frame = new CGRect(x: 0, y: 0, width: GetWidth(), height: 0);
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
			_secondaryLabel.Frame = new CGRect(x: 0, y: _primaryLabel.Frame.Bottom, width: GetWidth(), height: 0);
			_secondaryLabel.SizeToFitHeight();
			_secondaryLabel.UserInteractionEnabled = false;
			AddSubview(_secondaryLabel);
		}

		private UIGestureRecognizer GetTapGestureRecognizer()
		{
			var tapGesture = new UITapGestureRecognizer(() => { Skipped?.Invoke(); })
			{
				NumberOfTapsRequired = 1,
				NumberOfTouchesRequired = 1
			};
			return tapGesture;
		}

		private void AddSkipLabel()
		{
			_skipLabel = new UILabel();

			_skipLabel.Font = SkipTextFont != null ? SkipTextFont : UIFont.SystemFontOfSize(SkipTextSize);
			_skipLabel.TextColor = SkipTextColor;
			_skipLabel.TextAlignment = SkipTextAlignment;
			_skipLabel.LineBreakMode = UILineBreakMode.WordWrap;
			_skipLabel.Text = SkipText;
			_skipLabel.Lines = 3;
			_skipLabel.Frame = new CGRect(x: 0, y: _secondaryLabel.Frame.Bottom + SkipSpacing, width: GetWidth(), height: 0);
			_skipLabel.SizeToFitHeight();
			if (!string.IsNullOrEmpty(SkipText))
			{
				UserInteractionEnabled = true;
				_skipLabel.UserInteractionEnabled = true;
				// Add skip tap gesture
				_skipLabel.AddGestureRecognizer(GetTapGestureRecognizer());
			}
			else
			{
				_skipLabel.UserInteractionEnabled = false;
				UserInteractionEnabled = false;
			}

			AddSubview(_skipLabel);

			Frame = new CGRect(x: Frame.GetMinX(), y: Frame.GetMinY(), width: GetWidth(), height: _primaryLabel.Frame.Height + _secondaryLabel.Frame.Height + SkipSpacing + _skipLabel.Frame.Height);
		}

		// Calculate width per device
		private float GetWidth()
		{
			//superview was left side
			if (Superview?.Frame.Location.X >= 0)
			{
				return (float)(Frame.Width - (Frame.GetMinX() / 2));
			}
			else if ((Superview?.Frame.Location.X + Superview?.Frame.Size.Width) > UIScreen.MainScreen.Bounds.Width)
			{ //superview was right side
				return (float)((Frame.Width - Frame.GetMinX()) / 2);

			}

			return (float)(Frame.Width - Frame.GetMinX());
		}

		/// Overrides this to add subviews. They will be drawn when calling show()
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			AddPrimaryLabel();
			AddSecondaryLabel();
			AddSkipLabel();
		}
	}
}