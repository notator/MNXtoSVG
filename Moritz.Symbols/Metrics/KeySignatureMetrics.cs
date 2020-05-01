using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using MNX.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    internal class KeySignatureMetrics : GroupMetrics
    {
        readonly List<CLichtCharacterMetrics> AccidentalMetrics = null;

        public KeySignatureMetrics(Graphics graphics, double gap, double musicFontHeight, string clefType, int fifths)
            :base(CSSObjectClass.keySignature)
        {
            M.Assert(fifths > 0 || fifths < 0);
            List<CLichtCharacterMetrics> accMetrics = GetAccidentalMetrics(fifths, gap, musicFontHeight);
            AccidentalMetrics = accMetrics;

            _top = 0; _left = 0; _bottom = 0; _right = 0;
            // move into left->right positions
            double right = 0;
            for(var i = 0; i < accMetrics.Count; i++)
            {
                var appCharMetric = accMetrics[i];
                appCharMetric.Move(right - appCharMetric.Left - (gap * 0.1), 0);
                right = appCharMetric.Right;
            }
            _right = right;

            // now move to correct y-positions
            if(fifths < 0) // flats
            {
                // first, as if treble clef:
                double originY = 0;
                for(var i = 0; i < accMetrics.Count; i++)
                {
                    CLichtCharacterMetrics accMetric = accMetrics[i];
                    accMetric.Move(0, originY - accMetric.OriginY);
                    _top = (accMetric.Top < _top) ? accMetric.Top : _top;
                    _bottom = (accMetric.Bottom > _bottom) ? accMetric.Bottom : _bottom;
                    if(i % 2 == 0)
                    {
                        originY -= (gap * 1.5);
                    }
                    else
                    {
                        originY += (gap * 2);
                    }
                }
                if(clefType[0] == 'b') // bass clef
                {
                    this.Move(0, gap);
                }
            }
            else if(fifths > 0) // sharps
            {
                // first, as if treble clef:
                double originY = gap * 2;
                List<int> fourthDownIndices = new List<int>() { 0, 2, 3, 5 };
                for(var i = 0; i < accMetrics.Count; i++)
                {
                    CLichtCharacterMetrics accMetric = accMetrics[i];
                    accMetric.Move(0, originY - ((accMetric.Bottom - accMetric.Top) / 2));
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
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup(CSSObjectClass.ToString());
            foreach(var metric in AccidentalMetrics)
            {
                w.SvgText(CSSObjectClass.keySignatureComponent, metric.CharacterString, metric.OriginX, metric.OriginY);
            }
            w.SvgEndGroup();
        }
    }
}