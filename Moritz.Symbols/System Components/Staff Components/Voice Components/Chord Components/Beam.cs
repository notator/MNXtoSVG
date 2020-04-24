
using Moritz.Xml;

namespace Moritz.Symbols
{
	public abstract class Beam
	{
        /// <summary>
        /// Creates a horizontal Beam whose top edge is at 0F.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="y"></param>
        public Beam(double left, double right)
        {
            LeftX = left;
            RightX = right;
            _leftTopY = 0;
            _rightTopY = 0;
        }

        public void MoveYs(double dLeftY, double dRightY)
        {
            _leftTopY += dLeftY;
            _rightTopY += dRightY;
        }

        public abstract void ShiftYsForBeamBlock(double outerLeftY, double gap, VerticalDir stemDirection, double beamThickness);

        /// <summary>
        /// Shifts a horizontal beam vertically to the correct position (wrt the beamBlock) for its duration class 
        /// </summary>
        /// <param name="outerLeftY"></param>
        /// <param name="gap"></param>
        /// <param name="stemDirection"></param>
        /// <param name="beamThickness"></param>
        /// <param name="nGaps"></param>
        protected void ShiftYsForBeamBlock(double outerLeftY, double gap, VerticalDir stemDirection, double beamThickness, int nGaps)
        {
            double dy = 0;
            if(stemDirection == VerticalDir.down)
            {
                dy = -(beamThickness + (gap * nGaps));
            }
            else
            {
                dy = gap * nGaps;
            }
            dy += outerLeftY - _leftTopY ;
            MoveYs(dy, dy);
        }

        /// <summary>
        /// Exposed as public function by each IBeamStub
        /// </summary>
        protected void ShearStub(double shearAxis, double tanAlpha, double stemX)
        {
            if(LeftX == stemX || RightX == stemX)
            {
                double dLeftY = (LeftX - shearAxis) * tanAlpha;
                double dRightY = (RightX - shearAxis) * tanAlpha;
                MoveYs(dLeftY, dRightY);
            }
            // else do nothing
        }

        public readonly double LeftX;
        public readonly double RightX;
        public readonly double StrokeWidth;
        public double LeftTopY { get { return _leftTopY; } }
        public double RightTopY { get { return _rightTopY; } }

        protected double _leftTopY;
        protected double _rightTopY;
    }

    internal class QuaverBeam : Beam
    {
        public QuaverBeam(double left, double right)
            : base(left, right)
        {
        }

        public override void ShiftYsForBeamBlock(double outerLeftY, double gap, VerticalDir stemDirection, double beamThickness)
        {
            ShiftYsForBeamBlock(outerLeftY, gap, stemDirection, beamThickness, 0);
        }
    }
    internal class SemiquaverBeam : Beam
    {
        public SemiquaverBeam(double left, double right)
            : base(left, right)
        {
        }

        public override void ShiftYsForBeamBlock(double outerLeftY, double gap, VerticalDir stemDirection, double beamThickness)
        {
            ShiftYsForBeamBlock(outerLeftY, gap, stemDirection, beamThickness, 1);
        }

    }
    internal class ThreeFlagsBeam : Beam
    {
        public ThreeFlagsBeam(double left, double right)
            : base(left, right)
        {
        }

        public override void ShiftYsForBeamBlock(double outerLeftY, double gap, VerticalDir stemDirection, double beamThickness)
        {
            ShiftYsForBeamBlock(outerLeftY, gap, stemDirection, beamThickness, 2);
        }

    }
    internal class FourFlagsBeam : Beam
    {
        public FourFlagsBeam(double left, double right)
            : base(left, right)
        {
        }

        public override void ShiftYsForBeamBlock(double outerLeftY, double gap, VerticalDir stemDirection, double beamThickness)
        {
            ShiftYsForBeamBlock(outerLeftY, gap, stemDirection, beamThickness, 3);
        }

    }
    internal class FiveFlagsBeam : Beam
    {
        public FiveFlagsBeam(double left, double right)
            : base(left, right)
        {
        }

        public override void ShiftYsForBeamBlock(double outerLeftY, double gap, VerticalDir stemDirection, double beamThickness)
        {
            ShiftYsForBeamBlock(outerLeftY, gap, stemDirection, beamThickness, 4);
        }
    }

	/**********************************************************************************************/
	public interface IBeamStub
	{
		DurationClass DurationClass { get; }
        void ShearBeamStub(double shearAxis, double tanAlpha, double stemX);
    }

	internal class SemiquaverBeamStub : SemiquaverBeam, IBeamStub
	{
		public SemiquaverBeamStub(double left, double right)
			: base(left, right)
		{
		}

		public void ShearBeamStub(double shearAxis, double tanAlpha, double stemX)
		{
			base.ShearStub(shearAxis, tanAlpha, stemX);
		}

		public DurationClass DurationClass { get { return DurationClass.semiquaver; } }
	}
    internal class ThreeFlagsBeamStub : ThreeFlagsBeam, IBeamStub
    {
        public ThreeFlagsBeamStub(double left, double right)
            : base(left, right)
        {
        }

        public void ShearBeamStub(double shearAxis, double tanAlpha, double stemX)
        {
            base.ShearStub(shearAxis, tanAlpha, stemX);
        }

		public DurationClass DurationClass { get { return DurationClass.threeFlags; } }
	}
    internal class FourFlagsBeamStub : FourFlagsBeam, IBeamStub
    {
        public FourFlagsBeamStub(double left, double right)
            : base(left, right)
        {
        }

        public void ShearBeamStub(double shearAxis, double tanAlpha, double stemX)
        {
            base.ShearStub(shearAxis, tanAlpha, stemX);
        }

		public DurationClass DurationClass { get { return DurationClass.fourFlags; } }
	}
    internal class FiveFlagsBeamStub : FiveFlagsBeam, IBeamStub
    {
        public FiveFlagsBeamStub(double left, double right)
            : base(left, right)
        {
        }

        public void ShearBeamStub(double shearAxis, double tanAlpha, double stemX)
        {
            base.ShearStub(shearAxis, tanAlpha, stemX);
        }

		public DurationClass DurationClass { get { return DurationClass.fiveFlags; } }
	}
}
