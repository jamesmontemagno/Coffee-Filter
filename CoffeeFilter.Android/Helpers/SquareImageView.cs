
using System;

using Android.Content;
using Android.Runtime;
using Android.Widget;
using Android.Util;

namespace CoffeeFilter.Helpers
{
	public class SquareImageView : ImageView
	{
		public SquareImageView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
		}

		public SquareImageView(Context context)
			: base(context)
		{
		}

		protected SquareImageView(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			this.SetMeasuredDimension(this.MeasuredWidth, this.MeasuredWidth);
		}
	}
}

