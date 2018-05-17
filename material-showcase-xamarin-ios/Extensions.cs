using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CoreGraphics;
using Foundation;
using UIKit;

namespace MaterialShowcase
{
	public static class Extensions
	{
		private static Regex NonAlphanumericRegex = new Regex("[^a-zA-Z0-9 -]");
		public static UIColor ToUIColor(this string hexString)
		{
			hexString = NonAlphanumericRegex.Replace(hexString, "");
			var val = uint.Parse(hexString, NumberStyles.HexNumber);
			uint a, r, g, b;

			switch (hexString.Length)
			{
				case 3: // RGB (12-bit)
					(a, r, g, b) = (255, (val >> 8) * 17, (val >> 4 & 0xF) * 17, (val & 0xF) * 17);
					break;
				case 6: // RGB (24-bit)
					(a, r, g, b) = (255, val >> 16, val >> 8 & 0xFF, val & 0xFF);
					break;
				case 8: // ARGB (32-bit)
					(a, r, g, b) = (val >> 24, val >> 16 & 0xFF, val >> 8 & 0xFF, val & 0xFF);
					break;
				default:
					return UIColor.Clear;
			}

			return new UIColor(red: r / 255f, green: g / 255f, blue: b / 255f, alpha: a / 255f);
		}

		public static CGPoint Center(this CGRect rect)
		{
			return new CGPoint(rect.GetMidX(), rect.GetMidY());
		}

		public static void AsCircle(this UIView view)
		{
			view.Layer.CornerRadius = view.Frame.Width / 2;
			view.Layer.MasksToBounds = true;
		}

		public static void SetTintColor(this UIView view, UIColor color, bool recursive)
		{
			view.TintColor = color;

			if (recursive)
			{
				foreach (var subview in view.Subviews)
				{
					subview.SetTintColor(color, true);
				}
			}
		}

		public static void SizeToFitHeight(this UILabel label)
		{
			var tempLabel = new UILabel(frame: new CGRect(x: 0, y: 0, width: label.Frame.Width, height: float.MaxValue));
			tempLabel.Lines = label.Lines;
			tempLabel.LineBreakMode = label.LineBreakMode;
			tempLabel.Font = label.Font;
			tempLabel.Text = label.Text;
			tempLabel.SizeToFit();
			label.Frame = new CGRect(x: label.Frame.GetMinX(), y: label.Frame.GetMinY(), width: label.Frame.Width, height: tempLabel.Frame.Height);
		}
	}
}