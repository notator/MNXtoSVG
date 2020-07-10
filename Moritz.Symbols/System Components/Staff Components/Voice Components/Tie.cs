using Moritz.Xml;
using System;
using System.Collections.Generic;
using System.Text;
using MNX.Globals;

namespace Moritz.Symbols
{
    /// <summary>
    /// Ties (and their Metrics) are created *after* the noteheads they connect have moved to their final
    /// left-right positions on the system, but before the system has moved to its final vertical position.
    /// Tie (and Slur) Metrics are therefore only ever moved vertically.
    /// Tie widths are related to the x-distance between the noteheads they connect.
    /// </summary>
    internal class Tie : DrawObject
    {
        private string _dString = null;
        private double _scaleX = 1; // Can be set to a value less than 1 to compress short ties.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originX">The left end of the tie points at (originX, originY) (the centre of its left notehead)</param>
        /// <param name="originY">The left end of the tie points at (originX, originY) (the centre of its left notehead)</param>
        /// <param name="rightX">The right end of the tie points at (rightX, originY)</param>
        /// <param name="gap"></param>
        /// <param name="tieOver"></param>
        public Tie(double originX, double originY, double rightX, double gap, bool tieOver)
        {
            SetPathAttributes(originX, originY, rightX, gap, tieOver);
            Metrics = new SlurTieMetrics(CSSObjectClass.tie, gap, originX, originY, tieOver);            
        }

         /// <summary>
        /// The path attributes are this class's private fields:
        ///    _dString and _scaleX
        /// This function should do collision checking with respect to the bordering chords.
        /// </summary>
        private void SetPathAttributes(double leftHeadCx, double leftHeadCy, double rightHeadCx, double gap, bool tieOver)
        {
            // At(gap = 32), a long tie with h = 0 is 164 units wide from leftNoteheadCX to rightNoteheadCX(164 = 12 + 70 + h + 70 + 12)
            //< g id = "LongTieUnder" class="slurTie">
            //  <path d="m 12 22 c 14 31 62 31 70 31 h 36 c 8 0 56 0 70 -31 c -14 21 -62 21 -70 21 h -36 c -8 0 -56 0 -70 -21" stroke="black" stroke-width="1"/>
            //</g>
            //<g id = "LongTieOver" >
            //  < path d="m 12 -22 c 14 -31 62 -31 70 -31 h 36 c 8 0 56 0 70 31 c -14 -21 -62 -21 -70 -21 h -36 c -8 0 -56 0 -70 21" fill="black" stroke="black" stroke-width="1"/>
            //</g>
            double headXSeparation = rightHeadCx - leftHeadCx;
            double scale = gap / 32; // multiply the values in the template path by this scale.
            // template values for tieUnder
            double h = 0;
            //var m = new List<double>() { 12, 22 };
            var m = new List<double>() { 12, 22 };
            var c1 = new List<List<double>>() { new List<double>() { 14, 31 }, new List<double>() { 62, 31 }, new List<double>() { 70, 31 } };
            var c2 = new List<List<double>>() { new List<double>() { 8, 0 }, new List<double>() { 56, 0 }, new List<double>() { 70, -31 } };
            var c3 = new List<List<double>>() { new List<double>() { -14, 21 }, new List<double>() { -62, 21 }, new List<double>() { -70, 21 } };
            var c4 = new List<List<double>>() { new List<double>() { -8, 0 }, new List<double>() { -56, 0 }, new List<double>() { -70, -21 } };
            var allCs = new List<List<List<double>>>() { c1, c2, c3, c4 };

            #region scale the template
            m[0] *= scale;
            m[1] *= scale;
            foreach(var c in allCs)
            {
                foreach(var point in c)
                {
                    point[0] *= scale;
                    point[1] *= scale;
                }
            }
            #endregion

            if(tieOver)
            {
                #region flip the tie over
                m[1] *= -1;
                foreach(var c in allCs)
                {
                    foreach(var point in c)
                    {
                        point[1] *= -1;
                    }
                }
                #endregion
            }

            double tieWidth = (m[0] * 2) + (c1[2][0] * 2); // h is currently 0, so this is (12 + 70 + 0 + 70 + 12) * scale;
            h = headXSeparation - tieWidth;

            #region convert to strings
            var ms = new List<string>();
            var cs1 = new List<List<string>>();
            var cs2 = new List<List<string>>();
            var cs3 = new List<List<string>>();
            var cs4 = new List<List<string>>();
            var allCSs = new List<List<List<string>>>() { cs1, cs2, cs3, cs4 };
            ms.Add(m[0].ToString(M.En_USNumberFormat));
            ms.Add(m[1].ToString(M.En_USNumberFormat));
            for(var i = 0; i < allCs.Count; i++)
            {
                var c = allCs[i];
                var cs = allCSs[i];
                for(var j = 0; j < 3; j++)
                {
                    var cPoint = c[j];
                    var csPoint = new List<string>
                    {
                        cPoint[0].ToString(M.En_USNumberFormat),
                        cPoint[1].ToString(M.En_USNumberFormat)
                    };
                    cs.Add(csPoint);
                }
            }
            #endregion

            StringBuilder sb = new StringBuilder();
            sb.Append($"m {ms[0]} {ms[1]} c {cs1[0][0]} {cs1[0][1]} {cs1[1][0]} {cs1[1][1]} {cs1[2][0]} {cs1[2][1]} ");
            if(h >= 0)
            {
                sb.Append($"h {h.ToString(M.En_USNumberFormat)} ");
            }
            sb.Append($"c {cs2[0][0]} {cs2[0][1]} {cs2[1][0]} {cs2[1][1]} {cs2[2][0]} {cs2[2][1]} ");
            sb.Append($"c {cs3[0][0]} {cs3[0][1]} {cs3[1][0]} {cs3[1][1]} {cs3[2][0]} {cs3[2][1]} ");
            if(h >= 0)
            {
                sb.Append($"h {(-h).ToString(M.En_USNumberFormat)} ");
            }
            sb.Append($"c {cs4[0][0]} {cs4[0][1]} {cs4[1][0]} {cs4[1][1]} {cs4[2][0]} {cs4[2][1]}");

            _dString = sb.ToString();

            if(h < 0)
            {
                _scaleX = headXSeparation / tieWidth;
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgPath(CSSObjectClass.tie, _dString, Metrics.OriginX, Metrics.OriginY, _scaleX);
        }

        /// <summary>
        /// Add a tie to each Head in the first OutputChordSymbol in the voice, starting the tie before the first barline: 
        /// </summary>
        /// <param name="voice"></param>
        internal static void TieFirstHeads(Voice voice, List<string> headIDsTiedToPreviousSystem)
        {
            var firstBarline = voice.NoteObjects.Find(obj => obj is Barline);
            var rightChord = voice.NoteObjects.Find(obj => obj is OutputChordSymbol) as OutputChordSymbol;
            var tiesData = GetTiesData(null, rightChord);

            foreach(var head in rightChord.HeadsTopDown)
            {
                M.Assert(headIDsTiedToPreviousSystem.Contains(head.ID));
                headIDsTiedToPreviousSystem.Remove(head.ID);
            }

            rightChord.AddTies(tiesData, firstBarline.Metrics.OriginX - M.PageFormat.GapVBPX);
        }

        /// <summary>
        /// Each returned Tuple contains tieOriginX, tieOriginY, tieRightX, tieOver, tieTargetHeadID for one tie.
        /// If the rightChord argument is null, the tie is to a notehead on the following system,
        /// tieRightX will be returned greater than systemRight, and Item5 will contains the
        /// ID of the tied notehead on the next system.
        /// If the leftChord argument is null, the tie is to a notehead on the previous system,
        /// tieOriginX will be returned 0.
        /// </summary>
        /// <param name="leftChord"></param>
        /// <param name="rightChord">If this is null, use systemRight</param>
        /// <param name="systemRight">Only use this if rightChord is null</param>
        /// <returns>A list of quintuples having Item1=tieOriginX, Item2=tieOriginY, Item3=tieRightX, Item4=tieIsOver, Item5=tieTargetHeadID</returns>
        internal static List<Tuple<double, double, double, bool, string>> GetTiesData(OutputChordSymbol leftChord, OutputChordSymbol rightChord, double systemRight = 0)
        {
            double gap = M.PageFormat.GapVBPX;

            List<Tuple<double, double, double, bool, string>> returnList = new List<Tuple<double, double, double, bool, string>>();

            double tieOriginX = 0;
            double tieOriginY = 0;
            double tieRightX = 0;
            bool tieIsOver = true;
            string tieTargetHeadID = null;

            var anchorChord = (leftChord != null) ? leftChord : rightChord;
            M.Assert(anchorChord != null);

            var headsMetricsTopDown = anchorChord.ChordMetrics.HeadsMetricsTopDown;

            List<bool> tieIsOverList = GetTieIsOverList(anchorChord, headsMetricsTopDown);

            var augDotMetricsTopDown = anchorChord.ChordMetrics.AugDotMetricsTopDown;
            var stemMetrics = anchorChord.ChordMetrics.StemMetrics;
            var nHeads = anchorChord.HeadsTopDown.Count;

            if(leftChord != null && rightChord != null)
            {
                M.Assert(tieIsOverList.Count == nHeads && tieIsOverList.Count == rightChord.HeadsTopDown.Count);
            }

            for(var j = 0; j < nHeads; j++)
            {
                tieIsOver = tieIsOverList[j];

                bool isOuterHead = (j == 0 | j == (nHeads - 1));

                if(leftChord != null) // otherwise tieOriginX stays 0.
                {
                    tieOriginX = GetTieOriginX(j, headsMetricsTopDown, augDotMetricsTopDown, stemMetrics, isOuterHead);
                }

                HeadMetrics headMetrics = headsMetricsTopDown[j];
                tieOriginY = (headMetrics.Top + headMetrics.Bottom) / 2;

                if(leftChord != null && rightChord != null)
                {
                    Head leftHead = leftChord.HeadsTopDown[j];
                    Head rightHead = rightChord.HeadsTopDown[j];
                    M.Assert(leftHead.Tied.Target == rightHead.ID);
                }

                if(rightChord != null)
                {
                    Head rightHead = rightChord.HeadsTopDown[j];
                    tieTargetHeadID = rightHead.ID;
                    HeadMetrics rightHeadMetrics = rightChord.ChordMetrics.HeadsMetricsTopDown[j];
                    tieRightX = (rightHeadMetrics.Left + rightHeadMetrics.Right) / 2;
                }
                else // leftChord != null && rightChord == null;
                {
                    Head leftHead = leftChord.HeadsTopDown[j];
                    tieTargetHeadID = leftHead.Tied.Target;
                    tieRightX = systemRight + (gap * 1.2);
                }

                var tieData = new Tuple<double, double, double, bool, string>(tieOriginX, tieOriginY, tieRightX, tieIsOver, tieTargetHeadID);

                returnList.Add(tieData);
            }
            return returnList;
        }

        private static List<bool> GetTieIsOverList(OutputChordSymbol leftChord, IReadOnlyList<HeadMetrics> headMetricsTopDown)
        {
            var tieIsOverList = new List<bool>();

            //if(voicesCount == 1)
            //{
                #region set tieOverList simply for stem direction

                var nHeads = leftChord.HeadsTopDown.Count;
                M.Assert(nHeads > 0);
                var nOver = nHeads / 2; // is 0 if nHeads == 1
                var nUnder = nHeads / 2; // is 0 if nHeads == 1
                if((nOver + nUnder) < nHeads)
                {
                    if(leftChord.Stem.Direction == VerticalDir.down)
                    {
                        nOver++;
                    }
                    else
                    {
                        nUnder++;
                    }
                }
                M.Assert(nOver + nUnder == nHeads);
                double currentHeadLeft = -1;
                for(var i = 0; i < nOver; i++)
                {
                    if(i > 0 && headMetricsTopDown[i].Left != currentHeadLeft)
                    {
                        nUnder = nHeads - (i + 1);
                        break;
                    }
                    tieIsOverList.Add(true);
                    currentHeadLeft = headMetricsTopDown[i].Left;
                }
                for(var i = 0; i < nUnder; i++)
                {
                    if(i > 0 && headMetricsTopDown[i].Left != currentHeadLeft)
                    {
                        tieIsOverList[i - 1] = true;
                    }
                    tieIsOverList.Add(false);
                    currentHeadLeft = headMetricsTopDown[i].Left;
                }

            #endregion set tieOverList simply for stem direction
            //}
            //else // voicesCount == 2, top voice has stems up, bottom voice has stems down)
            //{
            //    var staff = leftChord.Voice.Staff;
            //    var voicesCount = staff.Voices.Count;
            //    M.Assert(voicesCount == 1 || voicesCount == 2); // this app only supports 1 or 2 voices per staff
            //    var staffLinesTop = ((StaffMetrics)staff.Metrics).StafflinesTop;
            //    var staffLinesBottom = ((StaffMetrics)staff.Metrics).StafflinesBottom;
            //    var midStafflineY = (staffLinesTop + staffLinesBottom) / 2;

            //}


            return tieIsOverList;
        }

        private static double GetTieOriginX(int headIndex, IReadOnlyList<HeadMetrics> headMetricsTopDown, IReadOnlyList<AugDotMetrics> augDotMetricsTopDown, StemMetrics stemMetrics, bool isOuterHead)
        {
            var headMetrics = headMetricsTopDown[headIndex];
            double tieOriginX = (headMetrics.Left + headMetrics.Right) / 2;
            if(isOuterHead == false)
            {                
                if(augDotMetricsTopDown != null)
                {
                    tieOriginX = augDotMetricsTopDown[headIndex].Right;
                }
                tieOriginX = (stemMetrics.Right > tieOriginX) ? stemMetrics.Right : tieOriginX;
            }

            return tieOriginX;
        }
    }

    /// <summary>
    /// The SlurTieMetrics class is only ever used to move a slur or tie vertically (together with a system).
    /// It is created only *after* the related noteheads have been moved to their final left-right positions on a system.
    /// </summary>
    class SlurTieMetrics : Metrics
    {
        internal SlurTieMetrics(CSSObjectClass slurOrTie, double gap, double originX, double originY, bool slurTieOver)
            :base(slurOrTie)
        {
            _left = originX; // never changes
            _right = 0; // never used
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