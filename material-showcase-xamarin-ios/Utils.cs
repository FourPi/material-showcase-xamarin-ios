using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;

namespace MaterialShowcase
{
	class Utils
	{
		public static float TARGET_HOLDER_RADIUS = 44f;
		public static float GetOuterCircleRadius(CGPoint center, CGRect textBounds, CGRect targetBounds)
		{
			var targetCenterX = targetBounds.GetMidX();

			var targetCenterY = targetBounds.GetMidY();

			var expandedRadius = 1.1 * TARGET_HOLDER_RADIUS;

			var expandedBounds = new CGRect(x: targetCenterX, y: targetCenterY, width: 0, height: 0);

			expandedBounds.Inset((nfloat)(-expandedRadius), (nfloat)(-expandedRadius));

			var textRadius = MaxDistance(center, textBounds);

			var targetRadius = MaxDistance(center, expandedBounds);

			return Math.Max(textRadius, targetRadius) + 40;
		}

		public static float MaxDistance(CGPoint point, CGRect rect)
		{
			var tl = Distance(point, new CGPoint(x: rect.GetMinX(), y: rect.GetMinY()));
			var tr = Distance(point, new CGPoint(x: rect.GetMaxX(), y: rect.GetMinY()));
			var bl = Distance(point, new CGPoint(x: rect.GetMinX(), y: rect.GetMaxY()));
			var br = Distance(point, new CGPoint(x: rect.GetMaxX(), y: rect.GetMaxY()));

			return Math.Max(tl, Math.Max(tr, Math.Max(bl, br)));
		}

		public static float Distance(CGPoint a, CGPoint b)
		{
			var xDist = a.X - b.X;

			var yDist = a.Y - b.Y;
			return (float)System.Math.Sqrt((xDist * xDist) + (yDist * yDist));
		}
	}
}