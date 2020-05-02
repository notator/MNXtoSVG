using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using MNX.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    internal class KeySignatureMetrics : GroupMetrics
    {
        public static Dictionary<string, List<CLichtCharacterMetrics>> KeySigDefs = new Dictionary<string, List<CLichtCharacterMetrics>>();

        private readonly List<CLichtCharacterMetrics> AccidentalMetrics = new List<CLichtCharacterMetrics>();
        private readonly string _keySigID = null;

        public KeySignatureMetrics(Graphics graphics, double gap, double musicFontHeight, string clefType, int fifths)
            :base(CSSObjectClass.keySig)
        {
            M.Assert(fifths > 0 || fifths < 0);
            string suffix = (fifths > 0) ? fifths.ToString() + "s" : (fifths * -1).ToString() + "f";
            _keySigID = CSSObjectClass.keySig.ToString() + "_" + suffix;

            if(KeySigDefs.Keys.Contains(_keySigID))
            {
                var def = KeySigDefs[_keySigID];
                foreach(var acc in def)
                {
                    AccidentalMetrics.Add(new CLichtCharacterMetrics(acc.CharacterString, acc.FontHeight, CSSObjectClass.accidental));
                }
            }
            else
            {
                List<CLichtCharacterMetrics> accMetrics = GetAccidentalMetrics(fifths, gap, musicFontHeight);
                _top = 0; _left = 0; _bottom = 0; _right = 0;
                // move into left->right positions
                double right = 0;
                for(var i = 0; i < accMetrics.Count; i++)
                {
                    var appCharMetric = accMetrics[i];
                    appCharMetric.Move(right - appCharMetric.Left, 0);
                    right = appCharMetric.Right;
                }
                _right = right;

                // now move to correct y-positions
                if(fifths < 0) // flats
                {
                    // the originY of the first "b" is two gaps below the top line of treble staff.
                    double accOriginY = gap * 2;
                    for(var i = 0; i < accMetrics.Count; i++)
                    {
                        CLichtCharacterMetrics accMetric = accMetrics[i];
                        accMetric.Move(0, accOriginY - accMetric.OriginY);
                        _top = (accMetric.Top < _top) ? accMetric.Top : _top;
                        _bottom = (accMetric.Bottom > _bottom) ? accMetric.Bottom : _bottom;
                        if(i % 2 == 0)
                        {
                            accOriginY -= (gap * 1.5);
                        }
                        else
                        {
                            accOriginY += (gap * 2);
                        }
                    }
                    if(clefType[0] == 'b') // bass clef
                    {
                        this.Move(0, gap);
                    }
                }
                else if(fifths > 0) // sharps
                {
                    // the originY of the first "#" is on the top line of a treble staff.
                    double originY = 0;
                    List<int> fourthDownIndices = new List<int>() { 0, 2, 3, 5 };
                    for(var i = 0; i < accMetrics.Count; i++)
                    {
                        CLichtCharacterMetrics accMetric = accMetrics[i];
                        accMetric.Move(0, originY - accMetric.OriginY);
                        _top = (accMetric.Top < _top) ? accMetric.Top : _top;
                        _bottom = (accMetric.Bottom > _bottom) ? accMetric.Bottom : _bottom;
                        if(fourthDownIndices.Contains(i))
                        {
                            // down a fourth
                            originY += (gap * 1.5);
                        }
                        else
                        {   // up a fifth
                            originY -= (gap * 2);
                        }
                    }
                    if(clefType[0] == 'b') // bass clef
                    {
                        this.Move(0, gap);
                    }
                }
                KeySigDefs.Add(_keySigID, accMetrics);
                foreach(var acc in accMetrics)
                {
                    AccidentalMetrics.Add(new CLichtCharacterMetrics(acc.CharacterString, acc.FontHeight, CSSObjectClass.accidental));
                }
            }
        }

        private List<CLichtCharacterMetrics> GetAccidentalMetrics(int fifths, double gap, double musicFontHeight)
        {
            List<CLichtCharacterMetrics> rval = new List<CLichtCharacterMetrics>();
            if(fifths > 0)
            {
                for(var i = 0; i < fifths; i++)
                {
                    rval.Add(new CLichtCharacterMetrics("#", musicFontHeight, TextHorizAlign.center, CSSObjectClass.accidental));
                }
            }
            else if(fifths < 0)
            {
                for(var i = 0; i > fifths; i--)
                {
                    rval.Add(new CLichtCharacterMetrics("b", musicFontHeight, TextHorizAlign.center, CSSObjectClass.accidental));
                }
            }
            return rval;
        }

        public override void Move(double dx, double dy)
        {
            base.Move(dx, dy);
            foreach(var c in AccidentalMetrics)
            {
                c.Move(dx, dy);
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgUseXY(CSSObjectClass.keySig, _keySigID, _originX, _originY);
        }
    }
}