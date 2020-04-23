using Moritz.Xml;

namespace Moritz.Symbols
{
    public class StaffMetrics : GroupMetrics
    {
        public StaffMetrics(double left, double right, double height)
            : base(CSSObjectClass.staff)
        {
            _top = 0F;
            _right = right;
            _bottom = height;
            _left = left;

            _stafflinesTop = _top;
            _stafflinesBottom = _bottom;
            _stafflinesLeft = _left;
            _stafflinesRight = _right;
        }

        public override void Move(double dx, double dy)
        {
            base.Move(dx, dy);
            _stafflinesTop += dy;
            _stafflinesBottom += dy;
        }

		//public override void ResetBoundary()
		//{
		//	base.ResetBoundary();
		//}

		private void ExpandMetrics(Metrics metrics)
		{
			if(metrics != null)
			{
				_top = _top < metrics.Top ? _top : metrics.Top;
				_right = _right > metrics.Right ? _right : metrics.Right;
				_bottom = _bottom > metrics.Bottom ? _bottom : metrics.Bottom;
				_left = _left < metrics.Left ? _left : metrics.Left;
			}
		}

        public double StafflinesTop { get { return _stafflinesTop; } }
        private double _stafflinesTop = 0F;

        public double StafflinesBottom { get { return _stafflinesBottom; } }
        private double _stafflinesBottom = 0F;

        public double StafflinesLeft { get { return _stafflinesLeft; } }
        private readonly double _stafflinesLeft = 0F;

        public double StafflinesRight { get { return _stafflinesRight; } }
        private readonly double _stafflinesRight = 0F;
    }
}
