using MNX.Globals;
using Moritz.Xml;
using System;

namespace Moritz.Symbols
{
    /// <summary>
    /// The SlurTieMetrics class is only ever used to move a slur or tie vertically (together with a system).
    /// It is created only *after* the related noteheads have been moved to their final left-right positions on a system.
    /// </summary>
    class SlurTieMetrics : Metrics
    {
        internal SlurTieMetrics(CSSObjectClass slurOrTie, double gap, double originX, double originY, double rightX, bool slurTieOver)
            : base(slurOrTie)
        {
            _left = originX; // never changes
            _right = rightX; // never changes
            _originX = originX; // never changes

            _originY = originY;
            if(slurTieOver)
            {
                _bottom = originY;
                _top = originY - (gap * 12 / 32);
            }
            else
            {
                _top = originY;
                _bottom = originY + (gap * 12 / 32);
            }
        }

        public override void Move(double dx, double dy)
        {
            M.Assert(dx == 0);
            _top += dy;
            _bottom += dy;
            _originY += dy;
        }

        /// <summary>
        /// This function should never be called
        /// </summary>
        /// <param name="w"></param>
        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }
    }
}