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
    public class ClefMetrics : Metrics // defined objects in SVG
    {
        public ClefMetrics(Clef clef, double gap, CSSObjectClass cssClefClass, ClefID clefID)
            : base(cssClefClass)
        {
            double trebleTop = -4.35F * gap;
            double trebleRight = 3.1F * gap;
            double highTrebleTop = -5.9F * gap;
            double trebleBottom = 2.7F * gap;
            #region treble clefs
            switch(clef.ClefType)
            {
                case "t":
                    _top = trebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t1": // trebleClef8
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t2": // trebleClef2x8
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t3": // trebleClef3x8
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                default: // can be a bass clef ( see below)
                    break;
            }

            if(_right > 0F)
            {
                Move(0, 3 * gap);
            }
            #endregion treble clefs

            if(!(_right > 0F))
            {
                double bassTop = -gap;
                double bassRight = trebleRight;
                double bassBottom = gap * 3;
                double lowBassBottom = gap * 4.5;
                #region bass clefs
                switch(clef.ClefType)
                {
                    case "b":
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = bassBottom;
                        break;
                    case "b1": // bassClef8
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b2": // bassClef2x8
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b3": // bassClef3x8
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    default:
                        M.Assert(false, "Unknown clef type.");
                        break;
                }
                if(_right > 0F)
                {
                    Move(0, gap);
                }

            }
            #endregion

            ClefID = clefID;
            if(!_usedClefIDs.Contains((ClefID) clefID))
            {
                _usedClefIDs.Add((ClefID) clefID);
            }
        }

        public static void ClearUsedClefIDsList()
        {
            _usedClefIDs.Clear();
        }

        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

        public ClefID ClefID { get { return _clefID; } private set { _clefID = value; } }
        private ClefID _clefID = ClefID.none;      
        public static IReadOnlyList<ClefID> UsedClefIDs { get { return _usedClefIDs as IReadOnlyList<ClefID>; } }
        private static List<ClefID> _usedClefIDs = new List<ClefID>();
    }
	internal class SmallClefMetrics : ClefMetrics
	{
		public SmallClefMetrics(Clef clef, double gap, CSSObjectClass cssClass, ClefID clefID)
			: base(clef, gap, cssClass, clefID)
		{
			_right = 3.5F * gap; // small clefs have proportionally more empty space on the right.

			if(clef.ClefType[0] == 'b' && clef.ClefType.Length > 1)
			{
				//double lowBassBottom = gap * 4.5;
				_bottom = gap * 4.65; // small bass clef octaves are lower than for normal bass clefs
			}
		}
	}

	///// tempSmallClefs have no width or height
	//internal class SmallClefTempMetrics : SmallClefMetrics
	//{
	//	public SmallClefTempMetrics(Clef clef, double gap, CSSObjectClass cssClass, ClefID clefID)
	//		: base(clef, gap, cssClass, clefID)
	//	{
	//		_top = _originY;
	//		_right = _originX;
	//		_bottom =
	//		_left = _originX;


	//		if(clef.ClefType[0] == 'b' && clef.ClefType.Length > 1)
	//		{
	//			//double lowBassBottom = gap * 4.5;
	//			_bottom = gap * 4.65; // small bass clef octaves are lower than for normal bass clefs
	//		}
	//	}
	//}
}
