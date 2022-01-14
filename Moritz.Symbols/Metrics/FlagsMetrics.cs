using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Text;

using MNX.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public class FlagsMetrics : Metrics
    {
        /// <summary>
        /// Should be called with a duration class having a flag block
        /// </summary>
        public FlagsMetrics(CSSObjectClass flagType, DurationClass durationClass, double fontHeight, VerticalDir stemDirection)
            : base(flagType)
        {
            _left = 0;

            // (0.31809F * fontHeight) is maximum x in the normal flag def.
            double normalRight = (0.31809F * fontHeight);
            _right = (flagType == CSSObjectClass.cautionaryFlag) ? normalRight * M.PageFormat.SmallSizeFactor : normalRight;

            if(stemDirection == VerticalDir.up)
            {
                double rightPadding = (0.06F * fontHeight);
                _right += rightPadding;
            }

            _originX = 0;
            _originY = 0;

            double offset = 0;
            offset = GetFlagID(flagType, durationClass, stemDirection, offset);
            if(stemDirection == VerticalDir.up)
            {
                _top = 0;
                _bottom = (0.2467F + offset) * fontHeight;
            }
            else
            {
                _top = (-(0.2467F + offset)) * fontHeight;
                _bottom = 0;
            }

            if(!_usedFlagIDs.Contains((FlagID)_flagID))
            {
                _usedFlagIDs.Add((FlagID)_flagID);
            }
        }

        private double GetFlagID(CSSObjectClass flagType, DurationClass durationClass, VerticalDir stemDirection, double offset)
        {
            double GetNormalFlagID(DurationClass nDurationClass, VerticalDir nStemDirection, double nOffset)
            {
                switch(nDurationClass)
                {
                    // Bravura says there is a maximum of 8 flags
                    case DurationClass.quaver:
                        if(nStemDirection == VerticalDir.up)
                            _flagID = FlagID.right1Flag;
                        else
                            _flagID = FlagID.left1Flag;
                        break;
                    case DurationClass.semiquaver:
                        if(nStemDirection == VerticalDir.up)
                            _flagID = FlagID.right2Flags;
                        else
                            _flagID = FlagID.left2Flags;
                        nOffset = 0.25;
                        break;
                    case DurationClass.threeFlags:
                        if(nStemDirection == VerticalDir.up)
                            _flagID = FlagID.right3Flags;
                        else
                            _flagID = FlagID.left3Flags;
                        nOffset = 0.5;
                        break;
                    case DurationClass.fourFlags:
                        if(nStemDirection == VerticalDir.up)
                            _flagID = FlagID.right4Flags;
                        else
                            _flagID = FlagID.left4Flags;
                        nOffset = 0.75;
                        break;
                    case DurationClass.fiveFlags:
                        if(nStemDirection == VerticalDir.up)
                            _flagID = FlagID.right5Flags;
                        else
                            _flagID = FlagID.left5Flags;
                        nOffset = 1;
                        break;
                    case DurationClass.sixFlags:
                        if(nStemDirection == VerticalDir.up)
                            _flagID = FlagID.right6Flags;
                        else
                            _flagID = FlagID.left6Flags;
                        nOffset = 1.25;
                        break;
                    case DurationClass.sevenFlags:
                        if(nStemDirection == VerticalDir.up)
                            _flagID = FlagID.right7Flags;
                        else
                            _flagID = FlagID.left7Flags;
                        nOffset = 1.5;
                        break;
                    case DurationClass.eightFlags:
                        if(nStemDirection == VerticalDir.up)
                            _flagID = FlagID.right8Flags;
                        else
                            _flagID = FlagID.left8Flags;
                        nOffset = 1.75;
                        break;
                    default:
                        M.Assert(false, "This duration class has no flags.");
                        break;
                }

                return nOffset;
            }

            double GetCautionaryFlagID(DurationClass cDurationClass, VerticalDir cStemDirection, double cOffset)
            {
                double factor = M.PageFormat.SmallSizeFactor;

                switch(cDurationClass)
                {
                    // Bravura says there is a maximum of 8 flags
                    case DurationClass.quaver:
                        if(cStemDirection == VerticalDir.up)
                            _flagID = FlagID.cautionaryRight1Flag;
                        else
                            _flagID = FlagID.cautionaryLeft1Flag;
                        break;
                    case DurationClass.semiquaver:
                        if(cStemDirection == VerticalDir.up)
                            _flagID = FlagID.cautionaryRight2Flags;
                        else
                            _flagID = FlagID.cautionaryLeft2Flags;
                        cOffset = 0.25 * factor;
                        break;
                    case DurationClass.threeFlags:
                        if(cStemDirection == VerticalDir.up)
                            _flagID = FlagID.cautionaryRight3Flags;
                        else
                            _flagID = FlagID.cautionaryLeft3Flags;
                        cOffset = 0.5 * factor;
                        break;
                    case DurationClass.fourFlags:
                        if(cStemDirection == VerticalDir.up)
                            _flagID = FlagID.cautionaryRight4Flags;
                        else
                            _flagID = FlagID.cautionaryLeft4Flags;
                        cOffset = 0.75 * factor;
                        break;
                    case DurationClass.fiveFlags:
                        if(cStemDirection == VerticalDir.up)
                            _flagID = FlagID.cautionaryRight5Flags;
                        else
                            _flagID = FlagID.cautionaryLeft5Flags;
                        cOffset = 1 * factor;
                        break;
                    case DurationClass.sixFlags:
                        if(cStemDirection == VerticalDir.up)
                            _flagID = FlagID.cautionaryRight6Flags;
                        else
                            _flagID = FlagID.cautionaryLeft6Flags;
                        cOffset = 1.25 * factor;
                        break;
                    case DurationClass.sevenFlags:
                        if(cStemDirection == VerticalDir.up)
                            _flagID = FlagID.cautionaryRight7Flags;
                        else
                            _flagID = FlagID.cautionaryLeft7Flags;
                        cOffset = 1.5 * factor;
                        break;
                    case DurationClass.eightFlags:
                        if(cStemDirection == VerticalDir.up)
                            _flagID = FlagID.cautionaryRight8Flags;
                        else
                            _flagID = FlagID.cautionaryLeft8Flags;
                        cOffset = 1.75 * factor;
                        break;
                    default:
                        M.Assert(false, "This duration class has no flags.");
                        break;
                }

                return cOffset;
            }

            M.Assert(flagType == CSSObjectClass.flag || flagType == CSSObjectClass.cautionaryFlag);

            if(flagType == CSSObjectClass.flag)
            {
                offset = GetNormalFlagID(durationClass, stemDirection, offset);
            }
            else
            {
                offset = GetCautionaryFlagID(durationClass, stemDirection, offset);
            }

            return offset;
        }

        public static void ClearUsedFlagIDsList()
        {
            _usedFlagIDs.Clear();
        }

        public override void WriteSVG(SvgWriter w)
        { 
            string flagIDString = _flagID.ToString();

            if(flagIDString.Contains("ight")) // stemDirection is up
                w.SvgUseXY(CSSObjectClass, flagIDString, _left, _top);
            else
                w.SvgUseXY(CSSObjectClass, flagIDString, _left, _bottom);
        }

        public FlagID FlagID { get { return _flagID; } private set { _flagID = value; } }
        private FlagID _flagID = FlagID.none;
        public static IReadOnlyList<FlagID> UsedFlagIDs { get { return _usedFlagIDs as IReadOnlyList<FlagID>; } }
        private static List<FlagID> _usedFlagIDs = new List<FlagID>();
    }
}
