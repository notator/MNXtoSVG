﻿using System;
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
        public FlagsMetrics(DurationClass durationClass, double fontHeight, VerticalDir stemDirection)
            : base(CSSObjectClass.flag)
        {
			_left = 0;           
            
            // (0.31809F * fontHeight) is maximum x in the flag def.
            _right = (0.31809F * fontHeight);
            if(stemDirection == VerticalDir.up)
            {
                double rightPadding = (0.06F * fontHeight);
                _right += rightPadding;
            }
            
			_originX = 0;
			_originY = 0;

			double offset = 0;
			switch(durationClass)
			{
                // Bravura says there is a maximum of 8 flags
				case DurationClass.quaver:
					if(stemDirection == VerticalDir.up)
						_flagID = FlagID.right1Flag;
					else
						_flagID = FlagID.left1Flag;
					break;
				case DurationClass.semiquaver:
					if(stemDirection == VerticalDir.up)
						_flagID = FlagID.right2Flags;
					else
						_flagID = FlagID.left2Flags;
					offset = 0.25;
					break;
				case DurationClass.threeFlags:
					if(stemDirection == VerticalDir.up)
						_flagID = FlagID.right3Flags;
					else
						_flagID = FlagID.left3Flags;
					offset = 0.5;
					break;
				case DurationClass.fourFlags:
					if(stemDirection == VerticalDir.up)
						_flagID = FlagID.right4Flags;
					else
						_flagID = FlagID.left4Flags;
					offset = 0.75;
					break;
                case DurationClass.fiveFlags:
                    if(stemDirection == VerticalDir.up)
                        _flagID = FlagID.right5Flags;
                    else
                        _flagID = FlagID.left5Flags;
                    offset = 1;
                    break;
                case DurationClass.sixFlags:
                    if(stemDirection == VerticalDir.up)
                        _flagID = FlagID.right6Flags;
                    else
                        _flagID = FlagID.left6Flags;
                    offset = 1.25;
                    break;
                case DurationClass.sevenFlags:
                    if(stemDirection == VerticalDir.up)
                        _flagID = FlagID.right7Flags;
                    else
                        _flagID = FlagID.left7Flags;
                    offset = 1.5;
                    break;
                case DurationClass.eightFlags:
                    if(stemDirection == VerticalDir.up)
                        _flagID = FlagID.right8Flags;
                    else
                        _flagID = FlagID.left8Flags;
                    offset = 1.75;
                    break;
                default:
					M.Assert(false, "This duration class has no flags.");
					break;
			}
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

            if(!_usedFlagIDs.Contains((FlagID) _flagID))
            {
                _usedFlagIDs.Add((FlagID) _flagID);
            }
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
