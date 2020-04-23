
using Moritz.Xml;

namespace Moritz.Symbols
{
	public class FramePadding
	{
        public FramePadding(TextFrameType frameType, double paddingTop, double paddingRight, double paddingBottom, double paddingLeft)
        {
            FrameType = frameType;
            Top = paddingTop;
            Right = paddingRight;
            Bottom = paddingBottom;
            Left = paddingLeft;
        }

		public TextFrameType FrameType { get; }
		public double Top { get; }
		public double Right { get; }
		public double Bottom { get; }
		public double Left { get; }
	}
}
