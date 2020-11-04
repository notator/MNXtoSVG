﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using Moritz.Xml;
using MNX.Globals;

namespace Moritz.Symbols
{
    internal class ChordMetrics : Metrics
    {
        public ChordMetrics(Graphics graphics, ChordSymbol chord, VerticalDir voiceStemDirection, double gap, double stemStrokeWidthVBPX, CSSObjectClass chordClass)
            : base(chordClass)
        {
            _top = double.MaxValue;
            _right = double.MinValue;
            _bottom = double.MinValue;
            _left = double.MaxValue;
            _drawObjects = chord.DrawObjects;

            _gap = gap;

            GetStaffParameters(chord); // sets _clef to the most recent clef, and _nStafflines.

            // For each component, find its characterID, deltaX and deltaY re the chord's origin.
            // The chord's x-origin is the centre of the outermost notehead.
            // (deltaX of the outermost notehead is 0.)
            // The chord's y-origin is the top line of the staff.
            // (deltaY of a notehead on the top line of the staff is 0.)

            if(chord.BeamBlock != null && chord.BeamBlock.Chords[0] == chord)
                this.BeamBlock = chord.BeamBlock;

            SetHeadsMetrics(chord, stemStrokeWidthVBPX);
            if(chord.Stem.Draw) // false for cautionary chords
                SetStemAndFlags(chord, _headsMetricsTopDown, stemStrokeWidthVBPX);

            // if the chord is part of a beamGroup, the stem tips are all at one height here.
            // These objects are created with originX and originY at 0,0 (the chord's origin).
            CreateLedgerlineMetrics(_headsMetricsTopDown, stemStrokeWidthVBPX, chordClass);
            CreateAccidentalMetrics(chord.FontHeight, chord.HeadsTopDown, _headsMetricsTopDown, chordClass);
            CreateAugDotMetrics(chord.NAugmentationDots, chord.FontHeight, _headsMetricsTopDown, chordClass);

            ChordSymbol cautionaryChordSymbol = chord as CautionaryChordSymbol;
            if(cautionaryChordSymbol != null)
            {
                CreateCautionaryBracketsMetrics(cautionaryChordSymbol);
            }
            else
            {
                CSSObjectClass lyricClass = GetLyricClass(chord);
                _lyricMetrics = NewLyricMetrics(chord.Voice.StemDirection, graphics, gap, lyricClass);

                GetRelativePositions(chord.Voice.StemDirection, _lyricMetrics, out bool ornamentIsBelow, out bool dynamicIsBelow);

                _ornamentMetrics = NewOrnamentMetrics(graphics, ornamentIsBelow);

                CSSObjectClass dynamicClass = GetDynamicClass(chord);

                _dynamicMetrics = NewCLichtDynamicMetrics(gap, dynamicIsBelow, dynamicClass);

                MoveAuxilliaries(chord.Stem.Direction, gap);
            }

            SetExternalBoundary();
        }

        private void GetStaffParameters(NoteObject rootObject)
        {
            // If a staff has two voices, both should contain the same clefs.
            // The clefs are, however, different objects in the two voices:
            // clef.IsVisible may be true in one voice and false in the other one.

            Voice voice = rootObject.Voice;
            _staffOriginY = voice.Staff.Metrics.StafflinesTop;

            _nStafflines = voice.Staff.NumberOfStafflines;
            foreach(NoteObject noteObject in voice.NoteObjects)
            {
                if(noteObject is Clef cs)
                    _clef = cs;
                if(noteObject == rootObject)
                    break;
            }
        }

        #region set heads and accidentals

        /// <summary>
        /// chord.Heads are in top-down order.
        /// </summary>
        private void SetHeadsMetrics(ChordSymbol chord, double ledgerlineStemStrokeWidth)
        {
            _headsMetricsTopDown = new List<HeadMetrics>();
            CSSObjectClass mainHeadClass = GetMainHeadClass(chord);

            HeadMetrics hMetrics = new HeadMetrics(chord, null, _gap, mainHeadClass); // the head is horizontally aligned at 0 by default.
            double horizontalShift = hMetrics.RightStemX - hMetrics.LeftStemX - (ledgerlineStemStrokeWidth / 2F); // the distance to shift left or right if heads would collide
            double shiftRange = _gap * 0.75;

            if(chord.Stem.Direction == VerticalDir.up)
            {
                List<Head> bottomUpHeads = new List<Head>();
                foreach(Head head in chord.HeadsTopDown)
                    bottomUpHeads.Insert(0, head);
                List<HeadMetrics> bottomUpMetrics = new List<HeadMetrics>();

                foreach(Head head in bottomUpHeads)
                {
                    double newHeadOriginY = head.GetOriginY(_clef, _gap); // note that the CHORD's originY is always at the top line of the staff
                    double newHeadAlignX = 0;
                    foreach(Metrics headMetric in bottomUpMetrics)
                    {
                        double existingHeadAlignX = (headMetric.Left + headMetric.Right) / 2;
                        if((newHeadOriginY == headMetric.OriginY)
                        || (existingHeadAlignX == 0F
                            && newHeadAlignX < (existingHeadAlignX + horizontalShift)
                            && newHeadOriginY > (headMetric.OriginY - shiftRange)))
                        {
                            newHeadAlignX = existingHeadAlignX + horizontalShift; // shifts more than once for extreme clusters ( e.g. ,F#,G) 
                        }
                        else
                            newHeadAlignX = 0;
                    }

                    CSSObjectClass mainHeadClass1 = GetMainHeadClass(chord);
                    HeadMetrics headMetrics = new HeadMetrics(chord, head, _gap, mainHeadClass1);
                    headMetrics.Move(newHeadAlignX, newHeadOriginY); // moves head.originY to headY
                    bottomUpMetrics.Add(headMetrics);
                }
                for(int i = bottomUpMetrics.Count - 1; i >= 0; --i)
                {
                    _headsMetricsTopDown.Add(bottomUpMetrics[i]);
                }
            }
            else // stem is down
            {
                foreach(Head head in chord.HeadsTopDown)
                {
                    double newHeadOriginY = head.GetOriginY(_clef, _gap); // note that the CHORD's originY is always at the top line of the staff
                    double newHeadAlignX = 0;
                    foreach(HeadMetrics headMetric in _headsMetricsTopDown)
                    {
                        double existingHeadAlignX = (headMetric.Left + headMetric.Right) / 2;
                        if((newHeadOriginY == headMetric.OriginY)
                        || (existingHeadAlignX == 0F
                            && newHeadAlignX < (existingHeadAlignX + horizontalShift)
                            && newHeadOriginY < (headMetric.OriginY + shiftRange)))
                        {
                            newHeadAlignX -= horizontalShift; // can shift left more than once
                        }
                        else
                            newHeadAlignX = 0;
                    }

                    CSSObjectClass headClass2 = GetMainHeadClass(chord);
                    HeadMetrics headMetrics = new HeadMetrics(chord, head, _gap, headClass2);
                    headMetrics.Move(newHeadAlignX, newHeadOriginY); // moves head.originY to headY
                    _headsMetricsTopDown.Add(headMetrics);
                }
            }

            M.Assert(_originX == 0F);
            M.Assert(_headsMetricsTopDown.Count == chord.HeadsTopDown.Count);
        }

        private CSSObjectClass GetMainHeadClass(ChordSymbol chord)
        {
            CSSObjectClass headClass = CSSObjectClass.notehead; // OutputChordSymbol
            if(chord is CautionaryChordSymbol)
            {
                headClass = CSSObjectClass.cautionaryNotehead;
            }
            return headClass;
        }

        private CSSObjectClass GetAccidentalClass(CSSObjectClass chordClass)
        {
            CSSObjectClass accidentalClass = CSSObjectClass.accidental; // chordClass == chord
            if(chordClass == CSSObjectClass.cautionaryChord)
            {
                accidentalClass = CSSObjectClass.cautionaryAccidental;
            }
            return accidentalClass;
        }

        private CSSObjectClass GetAugDotClass(CSSObjectClass chordClass)
        {
            CSSObjectClass augDotClass = CSSObjectClass.augDot; // chordClass == chord
            if(chordClass == CSSObjectClass.cautionaryChord)
            {
                augDotClass = CSSObjectClass.cautionaryAugDot;
            }
            return augDotClass;
        }

        private CSSObjectClass GetDynamicClass(ChordSymbol chord)
        {
            CSSObjectClass dynamicClass = CSSObjectClass.dynamic; // OutputChordSymbol
            return dynamicClass;
        }

        private CSSObjectClass GetLyricClass(ChordSymbol chord)
        {
            CSSObjectClass lyricClass = CSSObjectClass.lyric; // OutputChordSymbol
            return lyricClass;
        }

        private void CreateLedgerlineMetrics(List<HeadMetrics> topDownHeadsMetrics, double ledgerlineStemStrokeWidth, CSSObjectClass chordClass)
        {
            double limbLength = (topDownHeadsMetrics[0].RightStemX - topDownHeadsMetrics[0].LeftStemX) / 2; // change to taste later

            CSSObjectClass llsClass = GetLedgerlinesClass(chordClass);
            _upperLedgerlineBlockMetrics = CreateUpperLedgerlineBlock(topDownHeadsMetrics, limbLength, ledgerlineStemStrokeWidth, llsClass);
            _lowerLedgerlineBlockMetrics = CreateLowerLedgerlineBlock(topDownHeadsMetrics, limbLength, ledgerlineStemStrokeWidth, llsClass);
        }

        private void CreateAccidentalMetrics(double fontHeight, List<Head> topDownHeads, List<HeadMetrics> topDownHeadsMetrics, CSSObjectClass chordClass)
        {
            List<AccidentalMetrics> existingAccidentalsMetrics = new List<AccidentalMetrics>();
            for(int i = 0; i < topDownHeads.Count; i++)
            {
                HeadMetrics headMetrics = topDownHeadsMetrics[i];
                Head head = topDownHeads[i];
                if(head.DisplayAccidental == DisplayAccidental.force)
                {
                    CSSObjectClass accidentalClass = GetAccidentalClass(chordClass);
                    string accCharstring = GetClichtAccidentalCharacterString(head);
                    AccidentalMetrics accidentalMetrics = new AccidentalMetrics(accCharstring, fontHeight, _gap, accidentalClass);
                    accidentalMetrics.Move(headMetrics.OriginX, headMetrics.OriginY);
                    MoveAccidentalLeft(accidentalMetrics, topDownHeadsMetrics, _stemMetrics,
                        _upperLedgerlineBlockMetrics, _lowerLedgerlineBlockMetrics,
                        existingAccidentalsMetrics);
                    existingAccidentalsMetrics.Add(accidentalMetrics);
                    if(_accidentalsMetricsTopDown == null)
                        _accidentalsMetricsTopDown = new List<AccidentalMetrics>();
                    this._accidentalsMetricsTopDown.Add(accidentalMetrics);
                }
            }
        }

        private void CreateAugDotMetrics(int nAugmentationDots, double fontHeight, List<HeadMetrics> topDownHeadsMetrics, CSSObjectClass chordClass)
        {
            double separationX = _gap * 0.5;
            if(nAugmentationDots > 0)
            {
                _augDotMetricsTopDown = new List<AugDotMetrics>();
                CSSObjectClass augDotClass = GetAugDotClass(chordClass);
                separationX = ( augDotClass == CSSObjectClass.cautionaryAugDot) ? separationX * 0.8 : separationX;
                List<AugDotMetrics> templateAugDotRowMetrics = GetTemplateAugDotRowMetrics(nAugmentationDots, fontHeight, separationX, augDotClass);
                List<Tuple<double, double>> shiftPairs = GetAugDotRowShiftPairs(topDownHeadsMetrics);
                foreach(var shiftPair in shiftPairs)
                {
                    List<AugDotMetrics> augDotRowMetrics = GetClone(templateAugDotRowMetrics);
                    foreach(var augDotMetrics in augDotRowMetrics)
                    {
                        augDotMetrics.Move(shiftPair.Item1 - augDotMetrics.OriginX, shiftPair.Item2 - augDotMetrics.OriginY);
                        _augDotMetricsTopDown.Add(augDotMetrics);
                    }
                }
            }
        }

        private List<AugDotMetrics> GetClone(List<AugDotMetrics> templateAugDotRowMetrics)
        {
            List<AugDotMetrics> rval = new List<AugDotMetrics>();
            foreach(var augDotMetrics in templateAugDotRowMetrics)
            {
                rval.Add( (AugDotMetrics) augDotMetrics.Clone());
            }
            return rval;
        }

        /// <summary>
        /// Returns a list of unique x, y shift pairs for moving AugDotRow clones.
        /// The returned list may be smaller than the number of Heads (in dense clusters).
        /// </summary>
        private List<Tuple<double, double>> GetAugDotRowShiftPairs(List<HeadMetrics> topDownHeadsMetrics)
        {
            List<Tuple<double, double>> shiftPairs = new List<Tuple<double, double>>();

            double headRightToFirstAugDotOriginX = _gap * 0.5;
            double augDotsOriginXForHeads = FindAugDotsFirstOriginX(topDownHeadsMetrics, headRightToFirstAugDotOriginX);
            double ledgerlineRightToFirstAugDotOriginX = _gap * 0.4;
            double? augDotsOriginXForUpperLedgerlines = FindAugDotsFirstOriginX(_upperLedgerlineBlockMetrics, ledgerlineRightToFirstAugDotOriginX);
            double? augDotsoriginXForLowerLedgerlines = FindAugDotsFirstOriginX(_lowerLedgerlineBlockMetrics, ledgerlineRightToFirstAugDotOriginX);

            for(int i = 0; i < topDownHeadsMetrics.Count; i++)
            {
                double shiftX = 0;
                HeadMetrics headMetrics = topDownHeadsMetrics[i];
                if(headMetrics.OriginY < (_gap / 2))
                {
                    shiftX = (double)augDotsOriginXForUpperLedgerlines;
                }
                else if(headMetrics.OriginY > (_gap * 4.5))
                {
                    shiftX = (double)augDotsoriginXForLowerLedgerlines;
                }
                else
                {
                    shiftX = augDotsOriginXForHeads;
                }

                double shiftY = headMetrics.OriginY;
                if(shiftY % _gap == 0)
                {
                    shiftY -= (_gap / 2);
                }
                if(shiftPairs.FindIndex(obj => obj.Item2 == shiftY) >= 0)
                {
                    shiftY += _gap;
                }

                if(shiftPairs.FindIndex(obj => obj.Item2 == shiftY) < 0)
                {
                    shiftPairs.Add(new Tuple<double, double>(shiftX, shiftY));
                }
            }

            return shiftPairs;
        }

        /// <summary>
        /// Returns the OriginX of the leftMost AugDot right of the ledgerlineBlock.
        /// Returns null if the ledgerlineBlock is null, or (ledgerlineBlockMetrics.Right + separationX)
        /// </summary>
        private double? FindAugDotsFirstOriginX(LedgerlineBlockMetrics ledgerlineBlockMetrics, double separationX)
        {
            double? originX = null;
            if(ledgerlineBlockMetrics != null)
            {
                originX = ledgerlineBlockMetrics.Right + separationX;
            }
            return originX;
        }

        /// <summary>
        /// Returns the OriginX of the leftMost AugDot.
        /// This is the rightmost edge of any head, + separationX.
        /// </summary>
        private double FindAugDotsFirstOriginX(List<HeadMetrics> topDownHeadsMetrics, double separationX)
        {
            double maxRight = topDownHeadsMetrics[0].Right;
            foreach(var headMetrics in topDownHeadsMetrics)
            {
                maxRight = (headMetrics.Right > maxRight) ? headMetrics.Right : maxRight;
            }
            return (maxRight + separationX);
        }

        /// <summary>
        /// The returned row of AugDotMetrics all have their OriginY at 0.
        /// The left-most AugDotMetrics has OriginX == 0.
        /// The other AugDotMetrics.OriginX values are separated by separationX from the previous AugDotMetrics.OriginX.
        /// </summary>
        private List<AugDotMetrics> GetTemplateAugDotRowMetrics(int nAugmentationDots, double fontHeight, double separationX, CSSObjectClass augDotClass)
        {
            var rval = new List<AugDotMetrics>();
            double previousOriginX = -separationX;
            string augDotCharstring = ".";

            for(var j = 0; j < nAugmentationDots; j++)
            {
                AugDotMetrics augDotMetrics = new AugDotMetrics(augDotCharstring, fontHeight, _gap, augDotClass);
                augDotMetrics.Move(previousOriginX - augDotMetrics.OriginX + separationX, 0 - augDotMetrics.OriginY);
                previousOriginX = augDotMetrics.OriginX;
                rval.Add(augDotMetrics);
            }
            return rval;
        }



        /// <summary>
        /// Accidentals
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetClichtAccidentalCharacterString(Head head)
        {
            string cLichtCharacterString = null;
            switch(head.Alteration)
            {
                case -1:
                    cLichtCharacterString = "b";
                    break;
                case 0:
                    cLichtCharacterString = "n";
                    break;
                case 1:
                    cLichtCharacterString = "#";
                    break;
                default:
                    M.Assert(false, "unknown accidental type");
                    break;
            }
            return cLichtCharacterString;
        }

        private CSSObjectClass GetLedgerlinesClass(CSSObjectClass chordClass)
        {
            CSSObjectClass llClass = CSSObjectClass.ledgerlines; // chordClass == chord
            if(chordClass == CSSObjectClass.cautionaryChord)
            {
                llClass = CSSObjectClass.ledgerlines; // N.B. cautionaryLedgerlines does not exist.
            }
            return llClass;
        }

        private void CreateCautionaryBracketsMetrics(ChordSymbol chord)
        {
            double gap = M.PageFormat.GapVBPX;
            double padding = M.PageFormat.StafflineStemStrokeWidthVBPX;
            double strokeWidth = M.PageFormat.StafflineStemStrokeWidthVBPX;
            GetAccidentalsAndHeadsBox(out double top, out double right, out double bottom, out double left, gap, padding);
            double leftBracketLeft = left - (gap / 2F);
            double rightBracketRight = right + (gap / 2F);
            // the left bracket
            CautionaryBracketMetrics leftBracketMetrics = new CautionaryBracketMetrics(true, top, left, bottom, leftBracketLeft, strokeWidth);
            // the right bracket
            CautionaryBracketMetrics rightBracketMetrics = new CautionaryBracketMetrics(false, top, rightBracketRight, bottom, right, strokeWidth);

            _cautionaryBracketsMetrics = new List<CautionaryBracketMetrics>() { leftBracketMetrics, rightBracketMetrics };
        }

        private void GetAccidentalsAndHeadsBox(out double top, out double right, out double bottom, out double left,
            double gap, double padding)
        {
            top = double.MaxValue;
            left = double.MaxValue;
            bottom = double.MinValue;
            right = double.MinValue;
            if(_accidentalsMetricsTopDown != null)
            {
                foreach(AccidentalMetrics am in _accidentalsMetricsTopDown)
                {
                    top = top < am.Top ? top : am.Top;
                    right = right > am.Right ? right : am.Right;
                    bottom = bottom > am.Bottom ? bottom : am.Bottom;
                    left = left < am.Left ? left : am.Left;
                }
            }
            if(_headsMetricsTopDown != null)
            {
                foreach(HeadMetrics hm in _headsMetricsTopDown)
                {
                    top = top < hm.Top ? top : hm.Top;
                    right = right > hm.Right ? right : hm.Right;
                    bottom = bottom > hm.Bottom ? bottom : hm.Bottom;
                    left = left < hm.Left ? left : hm.Left;
                }
            }

            if(this._upperLedgerlineBlockMetrics != null)
            {
                double ledgerlineLeft = _upperLedgerlineBlockMetrics.Left - padding;
                double ledgerlineRight = _upperLedgerlineBlockMetrics.Right + padding;
                left = left < ledgerlineLeft ? left : ledgerlineLeft;
                right = right > ledgerlineRight ? right : ledgerlineRight;
            }

            if(this._lowerLedgerlineBlockMetrics != null)
            {
                double ledgerlineLeft = _lowerLedgerlineBlockMetrics.Left - padding;
                double ledgerlineRight = _lowerLedgerlineBlockMetrics.Right + padding;
                left = left < ledgerlineLeft ? left : ledgerlineLeft;
                right = right > ledgerlineRight ? right : ledgerlineRight;
            }

            top -= padding;
            bottom += padding;
            if((bottom - top) < gap)
            {
                top -= gap / 3;
                bottom += gap / 3;
            }
        }

        /// <summary>
        /// The accidental is at the correct height (accidental.OriginY == head.OriginY), 
        /// but it has not yet been added to this.MetricsList, or used to set this chord's Boundary.
        /// The accidental is now moved left, such that it does not overlap noteheads, stem, ledgerlines or accidentals.
        /// It is added to this.MetricsList after this function returns.
        /// </summary>
        private void MoveAccidentalLeft(AccidentalMetrics accidentalMetrics, List<HeadMetrics> topDownHeadsMetrics,
            StemMetrics stemMetrics,
            LedgerlineBlockMetrics upperLedgerlineBlockMetrics, LedgerlineBlockMetrics lowerLedgerlineBlockMetrics,
            List<AccidentalMetrics> existingAccidentalsMetrics)
        {
            #region move left of ledgerline block
            if(upperLedgerlineBlockMetrics != null)
            {
                MoveAccidentalLeftOfLedgerlineBlock(accidentalMetrics, upperLedgerlineBlockMetrics);
            }
            if(lowerLedgerlineBlockMetrics != null)
            {
                MoveAccidentalLeftOfLedgerlineBlock(accidentalMetrics, lowerLedgerlineBlockMetrics);
            }
            #endregion
            #region move left of noteheads
            double topRange = accidentalMetrics.OriginY - (_gap * 1.51F);
            double bottomRange = accidentalMetrics.OriginY + (_gap * 1.51F);
            foreach(HeadMetrics head in topDownHeadsMetrics)
            {
                if(head.OriginY > topRange && head.OriginY < bottomRange && head.Overlaps(accidentalMetrics))
                {
                    double extraHorizontalSpace = 0;
                    if(accidentalMetrics.CharacterString == "b")
                        extraHorizontalSpace = accidentalMetrics.FontHeight * -0.03;

                    accidentalMetrics.Move(head.Left - extraHorizontalSpace - accidentalMetrics.Right, 0);
                }
            }
            #endregion
            #region move left of stem (can be in another chord)
            if(stemMetrics != null)
            {
                // Note that the length of the stem is ignored here.
                double maxRight = stemMetrics.Left - stemMetrics.StrokeWidth;
                if(maxRight < accidentalMetrics.Right)
                    accidentalMetrics.Move(maxRight - accidentalMetrics.Right, 0F);
            }
            #endregion
            #region move accidental left of existing accidentals
            MoveLeftOfExistingAccidentals(existingAccidentalsMetrics, 0, accidentalMetrics);
            #endregion
        }

        /// <summary>
        /// Recursive function.
        /// </summary>
        private void MoveLeftOfExistingAccidentals(List<AccidentalMetrics> existingAccidentals, int index, AccidentalMetrics accidental)
        {
            // This delta is very important. Without it, an accidental will collide with the accidental above it,
            // if the upper accidental has been moved left. For example, chord (D1,G1,D2) with an accidental on each notehead.
            double xDelta = accidental.FontHeight * 0.001;
            for(int i = index; i < existingAccidentals.Count; i++)
            {
                Metrics existingAccidental = existingAccidentals[i];
                if(existingAccidental.Overlaps(accidental))
                {
                    if(existingAccidental.OriginY < (accidental.OriginY - (_gap * 1.75)))
                    {
                        if(accidental.CharacterString == "n")
                            xDelta = accidental.FontHeight * -0.05;
                        else if(accidental.CharacterString == "b")
                            xDelta = accidental.FontHeight * -0.14;
                        //else if(accidental.CharacterString == "#")
                        //    xDelta = accidental.FontHeight * 0.03;
                    }

                    accidental.Move(existingAccidental.Left - xDelta - accidental.Right, 0);
                }
                if(i < (existingAccidentals.Count - 1))
                    MoveLeftOfExistingAccidentals(existingAccidentals, i + 1, accidental);
            }
        }

        private void MoveAccidentalLeftOfLedgerlineBlock(AccidentalMetrics accidentalM, LedgerlineBlockMetrics ledgerlineBlockM)
        {
            M.Assert(accidentalM != null && ledgerlineBlockM != null);

            //double top = ledgerlineBlockM.Top - (_gap * 0.51F);
            //double bottom = ledgerlineBlockM.Bottom + (_gap * 0.51F);
            double top = ledgerlineBlockM.Top - (_gap * 1.01F);
            double bottom = ledgerlineBlockM.Bottom + (_gap * 1.01F);
            if(accidentalM.OriginY > top && accidentalM.OriginY < bottom)
                accidentalM.Move(ledgerlineBlockM.Left - accidentalM.Right, 0F);
        }

        private LedgerlineBlockMetrics CreateUpperLedgerlineBlock(List<HeadMetrics> topDownHeadsMetrics, double limbLength, double strokeWidth, CSSObjectClass ledgerlinesClass)
        {
            M.Assert(topDownHeadsMetrics != null);
            #region upper ledgerline block
            double minLeftX = double.MaxValue;
            double maxRightX = double.MinValue;
            foreach(HeadMetrics head in topDownHeadsMetrics)
            {
                if(head.OriginY <= _gap * 0.75F)
                {
                    minLeftX = minLeftX < head.LeftStemX ? minLeftX : head.LeftStemX;
                    maxRightX = maxRightX > head.RightStemX ? maxRightX : head.RightStemX;
                }
            }
            double left = minLeftX - limbLength;
            double right = maxRightX + limbLength;
            Metrics topHeadMetrics = topDownHeadsMetrics[0];
            LedgerlineBlockMetrics upperLedgerlineBlockMetrics = null;
            if(topHeadMetrics.OriginY < -(_gap * 0.75F))
            {
                upperLedgerlineBlockMetrics = new LedgerlineBlockMetrics(left, right, strokeWidth, ledgerlinesClass); // contains no ledgerlines

                double topLedgerlineY = topHeadMetrics.OriginY;
                if((topLedgerlineY % _gap) < 0)
                {
                    topLedgerlineY += (_gap / 2F);
                }
                for(double y = topLedgerlineY; y < 0; y += _gap)
                {
                    upperLedgerlineBlockMetrics.AddLedgerline(y, _gap);
                }
            }
            #endregion upper ledgerline block
            return upperLedgerlineBlockMetrics;
        }
        private LedgerlineBlockMetrics CreateLowerLedgerlineBlock(List<HeadMetrics> topDownHeadsMetrics, double limbLength, double strokeWidth, CSSObjectClass ledgerlinesClass)
        {
            M.Assert(topDownHeadsMetrics != null);
            double minLeftX = double.MaxValue;
            double maxRightX = double.MinValue;
            foreach(HeadMetrics head in topDownHeadsMetrics)
            {
                if(head.OriginY >= _gap * _nStafflines)
                {
                    minLeftX = minLeftX < head.LeftStemX ? minLeftX : head.LeftStemX;
                    maxRightX = maxRightX > head.RightStemX ? maxRightX : head.RightStemX;
                }
            }
            double leftX = minLeftX - limbLength;
            double rightX = maxRightX + limbLength;
            Metrics bottomHeadMetrics = topDownHeadsMetrics[topDownHeadsMetrics.Count - 1];
            LedgerlineBlockMetrics lowerLedgerlineBlockMetrics = null;
            if(bottomHeadMetrics.OriginY > (_gap * 4.75))
            {
                lowerLedgerlineBlockMetrics = new LedgerlineBlockMetrics(leftX, rightX, strokeWidth, ledgerlinesClass); // contains no ledgerlines

                double bottomLedgerlineY = bottomHeadMetrics.OriginY;
                if((bottomLedgerlineY % _gap) > 0)
                {
                    bottomLedgerlineY -= (_gap / 2F);
                }
                for(double y = (_gap * _nStafflines); y <= bottomLedgerlineY; y += _gap)
                {
                    lowerLedgerlineBlockMetrics.AddLedgerline(y, _gap);
                }
            }
            return lowerLedgerlineBlockMetrics;
        }
        #endregion heads and accidentals

        #region set stem and flags
        private void SetStemAndFlags(ChordSymbol chord, List<HeadMetrics> topDownHeadsMetrics, double stemThickness)
        {
            DurationClass durationClass = chord.DurationClass;
            _flagsMetrics = null;
            if(chord.BeamBlock == null
            && (durationClass == DurationClass.quaver
                || durationClass == DurationClass.semiquaver
                || durationClass == DurationClass.threeFlags
                || durationClass == DurationClass.fourFlags
                || durationClass == DurationClass.fiveFlags))
            {
                _flagsMetrics = GetFlagsMetrics(topDownHeadsMetrics,
                                                                durationClass,
                                                                chord.FontHeight,
                                                                chord.Stem.Direction,
                                                                stemThickness);
            }

            if(durationClass == DurationClass.minim
            || durationClass == DurationClass.crotchet
            || durationClass == DurationClass.quaver
            || durationClass == DurationClass.semiquaver
            || durationClass == DurationClass.threeFlags
            || durationClass == DurationClass.fourFlags
            || durationClass == DurationClass.fiveFlags)
            {
                _stemMetrics = NewStemMetrics(topDownHeadsMetrics, chord, _flagsMetrics, stemThickness);
            }
        }

        /// <summary>
        /// Returns null if the durationClass does not have a flagsBlock,
        /// otherwise returns the metrics for the flagsBlock attached to this chord, correctly positioned wrt the noteheads.
        /// </summary>
        private FlagsMetrics GetFlagsMetrics(List<HeadMetrics> topDownHeadsMetrics, DurationClass durationClass, double fontSize, VerticalDir stemDirection, double stemThickness)
        {
            M.Assert(durationClass == DurationClass.quaver
                || durationClass == DurationClass.semiquaver
                || durationClass == DurationClass.threeFlags
                || durationClass == DurationClass.fourFlags
                || durationClass == DurationClass.fiveFlags);

            FlagsMetrics flagsMetrics = new FlagsMetrics(durationClass, fontSize, stemDirection);

            if(flagsMetrics != null)
            {
                // flagsMetrics contains a metrics for the flags block with the outermost point at OriginX=0, BaselineY=0
                // Now move the flagblock so that is positioned correctly wrt the noteheads.
                SetFlagsPositionReNoteheads(topDownHeadsMetrics, flagsMetrics, stemDirection, stemThickness);
            }
            return flagsMetrics;
        }
        /// <summary>
        /// The flagBlock is moved to the correct x-position wrt noteheads.
        /// The distance between the inner notehead's alignmentY and the closest edge of the flagsBlock is set to a little less than two spaces.
        /// If there is only one flag, the distance is increased to a little less than three spaces (1 octave).
        /// If the stem is up and the bottom of the flagBlock is too low, the flagBlock is moved up.
        /// If the stem is down and the top of the flagBlock is too high, the flagBlock is moved down.
        /// </summary>
        private void SetFlagsPositionReNoteheads(List<HeadMetrics> topDownHeadsMetrics, FlagsMetrics flagsMetrics, VerticalDir stemDirection, double stemThickness)
        {
            M.Assert(flagsMetrics != null);

            HeadMetrics outerNoteheadMetrics = FindOuterNotehead(topDownHeadsMetrics, stemDirection);
            HeadMetrics innerNoteheadMetrics = FindInnerNotehead(topDownHeadsMetrics, stemDirection);
            double innerNoteheadAlignmentY = (innerNoteheadMetrics.Bottom + innerNoteheadMetrics.Top) / 2;
            double minDist = _gap * 1.8; // constant found by experiment
            double deltaX = 0;
            double deltaY = 0;
            string flagIDString = flagsMetrics.FlagID.ToString();
            if(stemDirection == VerticalDir.up)
            {
                deltaY = minDist - (innerNoteheadAlignmentY - flagsMetrics.Bottom);
                
                if(flagIDString.Contains("ight1Flag"))
                    deltaY += _gap;
                deltaY *= -1;

                if(flagIDString.Contains("ight1Flag"))
                {
                    if((flagsMetrics.Bottom + deltaY) > (_gap * 2.5F))
                    {
                        deltaY = (_gap * 2.5F) - flagsMetrics.Bottom;
                    }
                }
                else // other right flag types
                    if((flagsMetrics.Bottom + deltaY) > (_gap * 3.5F))
                    {
                        deltaY = (_gap * 3.5F) - flagsMetrics.Bottom;
                    }

                deltaX = outerNoteheadMetrics.RightStemX - (stemThickness / 2F);
            }
            else // stem is down
            {
                deltaY = minDist - (flagsMetrics.Top - innerNoteheadAlignmentY);
                if(flagIDString.Contains("eft1Flag"))
                    deltaY += _gap;

                if(flagIDString.Contains("eft1Flag"))
                {
                    if((flagsMetrics.Top + deltaY) < (_gap * 1.5F))
                    {
                        deltaY = (_gap * 1.5F) - flagsMetrics.Top;
                    }
                }
                else // other left flag types
                    if((flagsMetrics.Top + deltaY) < (_gap * 0.5F))
                    {
                        deltaY = (_gap * 0.5F) - flagsMetrics.Top;
                    }

                deltaX = outerNoteheadMetrics.LeftStemX + (stemThickness / 2F);
            }
            flagsMetrics.Move(deltaX, deltaY);
            // the flagsBlockMetrics is added to either this.MetricsList or this.PostJustificationMetrics in a later function.
        }

        /// <summary>
        /// returns the notehead metrics closest to the outer stem tip.
        /// </summary>
        private HeadMetrics FindInnerNotehead(List<HeadMetrics> topDownHeadsMetrics, VerticalDir stemDirection)
        {
            M.Assert(topDownHeadsMetrics.Count > 0);
            HeadMetrics innerNotehead = null;
            if(stemDirection == VerticalDir.up)
            {
                innerNotehead = topDownHeadsMetrics[0];
            }
            else
            {
                innerNotehead = topDownHeadsMetrics[topDownHeadsMetrics.Count - 1];
            }
            return innerNotehead;
        }
        /// <summary>
        /// Returns the notehead to which the stem is attached.
        /// </summary>
        private HeadMetrics FindOuterNotehead(List<HeadMetrics> topDownHeadsMetrics, VerticalDir stemDirection)
        {
            M.Assert(topDownHeadsMetrics.Count > 0);
            HeadMetrics outerNotehead = null;
            if(stemDirection == VerticalDir.up)
            {
                outerNotehead = topDownHeadsMetrics[topDownHeadsMetrics.Count - 1];
            }
            else
            {
                outerNotehead = topDownHeadsMetrics[0];
            }
            return outerNotehead;
        }

        #endregion stem and flags

        #region private helper functions
        /// <summary>
        /// Called when the ChordMetrics is first constructed.
        /// It does not yet have any beams.
        /// </summary>
        private void MoveAuxilliaries(VerticalDir stemDirection, double gap)
        {
            MoveAuxilliaries(stemDirection, gap, 0, 0);
        }
        /// <summary>
        /// The upper beam padding and lower beam padding is used exclusively for auxilliaries placed next to beams.
        /// These values do not affect the positions of auxilliaries on ordinary flags.
        /// </summary
        private void MoveAuxilliaries(VerticalDir stemDirection, double gap, double upperBeamPadding, double lowerBeamPadding)
        {
            GetTopAndBottomBounds(stemDirection, out double topBoundary, out double bottomBoundary);

            topBoundary -= upperBeamPadding;
            bottomBoundary += lowerBeamPadding;

			// These are moved to a position relative to the outer stem tip or notehead.
			if(_ornamentMetrics != null)
				MoveOrnamentMetrics(gap, ref topBoundary, ref bottomBoundary);
			if(_lyricMetrics != null)
				MoveLyricMetrics(gap, ref topBoundary, ref bottomBoundary);
			if(_dynamicMetrics != null)
                MoveDynamicMetrics(gap, ref topBoundary, ref bottomBoundary);
        }

		private void MoveOrnamentMetrics(double gap, ref double topBoundary, ref double bottomBoundary)
		{
			// N.B. topPadding was (gap * 0.6F) bottomPadding was (gap * 0.4F).
			// These have both been increased, to (gap * 0.75F) and (gap * 0.55F).
			// (The topPadding has to be big enough to accomodate lower case characters that extend below the baseline (e.g. 'g').)
			MoveMetrics(_ornamentMetrics, _ornamentMetrics.IsBelow, ref topBoundary, (gap * 0.75F), ref bottomBoundary, (gap * 0.55F));
		}

		/// <summary>
		/// Moves the lyric to its correct position wrt the topBoundary or bottomBoundary.
		/// Does nothing if lyricMetrics is null.
		/// </summary>
		private void MoveLyricMetrics(double gap, ref double topBoundary, ref double bottomBoundary)
        {
            MoveMetrics(_lyricMetrics, _lyricMetrics.IsBelow, ref topBoundary, (gap * 0.6F), ref bottomBoundary, (gap * 0.4F));
        }
        /// <summary>
        /// Moves the dynamic to its correct position wrt the topBoundary or bottomBoundary.
        /// Does nothing if dynamicMetrics is null.
        /// </summary>
        private void MoveDynamicMetrics(double gap, ref double topBoundary, ref double bottomBoundary)
        {
            if(_dynamicMetrics != null)
            {
                double baseLineToTop = _dynamicMetrics.OriginY - _dynamicMetrics.Top;
                double bottomPadding = (gap * 2.3F) - baseLineToTop; // baselineToTop of forte is gap * 1.386
                bottomPadding = (bottomPadding >= 0) ? bottomPadding : 0;

                double bottomToBaseline = _dynamicMetrics.Bottom - _dynamicMetrics.OriginY;
                double topPadding = (gap * 0.2F) + bottomToBaseline;

                MoveMetrics(_dynamicMetrics, _dynamicMetrics.IsBelow, ref topBoundary, topPadding, ref bottomBoundary, bottomPadding);
            }
        }
        /// <summary>
        /// If isBelow, metrics.Top is moved to bottomBoundary + bottomPadding and bottomBoundary is then set to metrics.Bottom
        /// otherwise metrics.Bottom is moved to topBoundary - topPadding and topBoundary is then set to metrics.Top.
        /// Does nothing if metrics is null.
        /// </summary>
        private void MoveMetrics(Metrics metrics, bool isBelow,
            ref double topBoundary, double topPadding,
            ref double bottomBoundary, double bottomPadding)
        {
            M.Assert(NoteheadExtendersMetrics == null);
            if(isBelow)
            {
                MoveBelowBottomBoundary(metrics, ref bottomBoundary, bottomPadding);
            }
            else
            {
                MoveAboveTopBoundary(metrics, ref topBoundary, topPadding);
            }
            SetExternalBoundary();
        }

        private OrnamentMetrics NewOrnamentMetrics(Graphics graphics, bool ornamentIsBelow)
        {
			_ornamentMetrics = null;

            foreach(DrawObject drawObject in _drawObjects)
            {
				if(drawObject is OrnamentText ornamentText)
				{
					ornamentText.Metrics = new OrnamentMetrics(graphics, ornamentText.TextInfo, ornamentIsBelow);
					_ornamentMetrics = (OrnamentMetrics)ornamentText.Metrics;
					break;
				}
			}

            return _ornamentMetrics;
        }

        /// <summary>
        /// returns null if there is no lyric in the _drawObjects
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gap"></param>
        /// <param name="lyricIsBelow"></param>
        /// <returns></returns>
        private LyricMetrics NewLyricMetrics(VerticalDir voiceStemDirection, Graphics graphics, double gap, CSSObjectClass lyricClass)
        {
            LyricText lyric = null;
            foreach(DrawObject drawObject in _drawObjects)
            {
                lyric = drawObject as LyricText;
                if(lyric != null)
                    break;
            }
            _lyricMetrics = null;

            if(lyric != null)
            {
                bool lyricIsBelow = true; // voiceStemDirection == VerticalDir.none || voiceStemDirection == VerticalDir.down
                if(voiceStemDirection == VerticalDir.up)
                    lyricIsBelow = false;
                lyric.Metrics = new LyricMetrics(gap, graphics, lyric.TextInfo, lyricIsBelow, lyricClass);
                _lyricMetrics = (LyricMetrics)lyric.Metrics; 
            }

            return _lyricMetrics;
        }
        /// <summary>
        /// returns null if there is no dynamic text in _drawObjects
        /// </summary>
        /// <param name="gap"></param>
        /// <param name="dynamicIsBelow"></param>
        /// <returns></returns>
        private DynamicMetrics NewCLichtDynamicMetrics(double gap, bool dynamicIsBelow, CSSObjectClass dynamicClass)
        {
			List<string> clichtDynamics = new List<string>();
			clichtDynamics.AddRange(M.CLichtDynamicsCharacters.Values);

			Text dynamicText = null;
            foreach(DrawObject drawObject in _drawObjects)
            {
                if(drawObject is Text text)
                {
                    M.Assert(text.TextInfo != null);
                    M.Assert(!String.IsNullOrEmpty(text.TextInfo.Text));
                    M.Assert(!String.IsNullOrEmpty(text.TextInfo.FontFamily));
                    if(text.TextInfo.FontFamily == "CLicht" && clichtDynamics.Contains(text.TextInfo.Text))
                    {
                        dynamicText = text;
                        break;
                    }
                }
            }

            _dynamicMetrics = null;
            if(dynamicText != null)
            {
                dynamicText.Metrics = new  DynamicMetrics(gap, dynamicText.TextInfo, dynamicIsBelow, dynamicClass);
                _dynamicMetrics = (DynamicMetrics)dynamicText.Metrics;
            }

            return (DynamicMetrics)_dynamicMetrics;
        }
        private void GetRelativePositions(VerticalDir voiceStemDirection, LyricMetrics lyricMetrics,
            out bool ornamentIsBelow, out bool dynamicIsBelow)
        {
            dynamicIsBelow = true;
            ornamentIsBelow = false;
            switch(voiceStemDirection)
            {
                case VerticalDir.none:
                    // a 1-Voice staff
                    if(lyricMetrics != null && lyricMetrics.IsBelow)
                        dynamicIsBelow = false;
                    break;
                case VerticalDir.up:
                    // top voice of a 2-Voice staff
                    dynamicIsBelow = false;
                    ornamentIsBelow = false;
                    break;
                case VerticalDir.down:
                    // bottom voice of a 2-Voice staff
                    dynamicIsBelow = true;
                    ornamentIsBelow = true;
                    break;
            }
        }

        /// <summary>
        /// topBoundary is set to topStaffline
        /// bottomBoundary is set to bottomStaffline
        /// then these bounds are widened if the chord lies outside:
        /// If there is no stem,
        ///     topBoundary is set to topNotehead.Top or top of top accidental
        ///     bottomBoundary is set to bottomNotehead.Bottom (but not to bottom of bottom accidental).
        /// else if stem is up, 
        ///     topBoundardy is set to stemTopTipY, 
        ///     bottomBoundary is set to bottomNotehead.Bottom (but not to bottom of bottom accidental).
        /// else if stem is down, 
        ///     topBoundary is set to topNotehead.Top or top of top accidental, 
        ///     bottomBoundary is set to stemBottomTipY or bottomNoteheadBottom or bottom of bottom accidental. 
        /// </summary>
        private void GetTopAndBottomBounds(VerticalDir stemDirection, out double topBoundary, out double bottomBoundary)
        {
            double topStaffline = _clef.Voice.Staff.Metrics.StafflinesTop;

            topBoundary = topStaffline; // top of staff
            bottomBoundary = topStaffline + (_gap * 4F); // bottom of staff
            if(_stemMetrics == null)
            {
                double topOfTopHead = _headsMetricsTopDown[0].Top;
                topBoundary = (topBoundary < topOfTopHead) ? topBoundary : topOfTopHead;

                double bottomOfBottomHead = _headsMetricsTopDown[_headsMetricsTopDown.Count - 1].Bottom;
                bottomBoundary = (bottomBoundary > bottomOfBottomHead) ? bottomBoundary : bottomOfBottomHead;

                if(_accidentalsMetricsTopDown != null)
                {
                    M.Assert(_accidentalsMetricsTopDown.Count > 0);
                    double topOfTopAccidental = _accidentalsMetricsTopDown[0].Top;
                    topBoundary = (topBoundary < topOfTopAccidental) ? topBoundary : topOfTopAccidental;
                    //double bottomOfBottomAccidental = _topDownAccidentalsMetrics[_topDownAccidentalsMetrics.Count - 1].Bottom;
                    //bottomBoundary = (bottomBoundary > bottomOfBottomAccidental) ? bottomBoundary : bottomOfBottomAccidental;
                }
            }
            else if(_stemMetrics.VerticalDir == VerticalDir.up)
            {
                topBoundary = (topBoundary < _stemMetrics.Top) ? topBoundary : _stemMetrics.Top;

                double bottomOfBottomHead = _headsMetricsTopDown[_headsMetricsTopDown.Count - 1].Bottom;
                bottomBoundary = (bottomBoundary > bottomOfBottomHead) ? bottomBoundary : bottomOfBottomHead;
                //if(_topDownAccidentalsMetrics != null)
                //{
                //    M.Assert(_topDownAccidentalsMetrics.Count > 0);
                //    double bottomOfBottomAccidental = _topDownAccidentalsMetrics[_topDownAccidentalsMetrics.Count - 1].Bottom;
                //    bottomBoundary = (bottomBoundary > bottomOfBottomAccidental) ? bottomBoundary : bottomOfBottomAccidental;
                //}
            }
            else
            {
                double topOfTopHead = _headsMetricsTopDown[0].Top;
                topBoundary = (topBoundary < topOfTopHead) ? topBoundary : topOfTopHead;
                if(_accidentalsMetricsTopDown != null)
                {
                    M.Assert(_accidentalsMetricsTopDown.Count > 0);
                    double topOfTopAccidental = _accidentalsMetricsTopDown[0].Top;
                    topBoundary = (topBoundary < topOfTopAccidental) ? topBoundary : topOfTopAccidental;
                }

                bottomBoundary = (bottomBoundary > _stemMetrics.Bottom) ? bottomBoundary : _stemMetrics.Bottom;
            }
        }

        private void MoveBelowBottomBoundary(Metrics metrics, ref double bottomBoundary, double padding)
        {
            M.Assert(padding >= 0.0F);
            double newTop = bottomBoundary + padding;
            metrics.Move(0, newTop - metrics.Top);
            bottomBoundary = metrics.Bottom;
            SetExternalBoundary();
        }

        private void MoveAboveTopBoundary(Metrics metrics, ref double topBoundary, double padding)
        {
            M.Assert(padding >= 0.0F);
            double newBottom = topBoundary - padding;
            metrics.Move(0, newBottom - metrics.Bottom);
            topBoundary = metrics.Top;
            SetExternalBoundary();
        }

        /// <summary>
        /// This function resets the public Top, Right, Bottom and Left properties, and
        /// must be called before leaving any public function that moves any of this ChordMetrics'
        /// private objects.
        /// An attached BeamBlock or NoteheadExtender is not part of the external boundary.
        /// </summary>
        private void SetExternalBoundary()
        {
            _top = double.MaxValue;
            _right = double.MinValue;
            _bottom = double.MinValue;
            _left = double.MaxValue;

            if(_stemMetrics != null)
                SetBoundary(_stemMetrics);
            if(_flagsMetrics != null)
                SetBoundary(_flagsMetrics);
            if(_headsMetricsTopDown != null)
            {
                foreach(HeadMetrics headMetric in _headsMetricsTopDown)
                    SetBoundary(headMetric);
            }
            if(_accidentalsMetricsTopDown != null)
            {
                foreach(AccidentalMetrics accidentalMetric in _accidentalsMetricsTopDown)
                    SetBoundary(accidentalMetric);
            }
            if(_augDotMetricsTopDown != null)
            {
                foreach(AugDotMetrics augDotMetric in _augDotMetricsTopDown)
                    SetBoundary(augDotMetric);
            }
            if(_upperLedgerlineBlockMetrics != null)
            {
                SetBoundary(_upperLedgerlineBlockMetrics);
            }
            if(_lowerLedgerlineBlockMetrics != null)
            {
                SetBoundary(_lowerLedgerlineBlockMetrics);
            }
            if(_cautionaryBracketsMetrics != null)
            {
                foreach(CautionaryBracketMetrics cautionaryBracketMetrics in _cautionaryBracketsMetrics)
                    SetBoundary(cautionaryBracketMetrics);
            }

            if(_ornamentMetrics != null)
                SetBoundary(_ornamentMetrics);
            if(_lyricMetrics != null)
                SetBoundary(_lyricMetrics);
            if(_dynamicMetrics != null)
                SetBoundary(_dynamicMetrics);
        }

        private void SetBoundary(Metrics metrics)
        {
            _top = _top < metrics.Top ? _top : metrics.Top;
            _right = _right > metrics.Right ? _right : metrics.Right;
            _bottom = _bottom > metrics.Bottom ? _bottom : metrics.Bottom;
            _left = _left < metrics.Left ? _left : metrics.Left;
        }

        #endregion

        #region public interface

        public override void WriteSVG(SvgWriter w)
        {
            if(_stemMetrics != null)
                _stemMetrics.WriteSVG(w);
            if(_flagsMetrics != null)
                _flagsMetrics.WriteSVG(w);
            if(_headsMetricsTopDown != null)
            {
                foreach(HeadMetrics headMetrics in _headsMetricsTopDown)
                {
                    headMetrics.WriteSVG(w);
                }
            }
            if(_accidentalsMetricsTopDown != null)
            {
                foreach(AccidentalMetrics accidentalMetrics in _accidentalsMetricsTopDown)
                    accidentalMetrics.WriteSVG(w);
            }
            if(_augDotMetricsTopDown!= null)
            {
                foreach(AugDotMetrics augDotMetrics in _augDotMetricsTopDown)
                    augDotMetrics.WriteSVG(w);
            }
            if(_upperLedgerlineBlockMetrics != null)
            {
                _upperLedgerlineBlockMetrics.WriteSVG(w);
            }
            if(_lowerLedgerlineBlockMetrics != null)
            {
                _lowerLedgerlineBlockMetrics.WriteSVG(w);
            }
            if(_ornamentMetrics != null)
            {
                _ornamentMetrics.WriteSVG(w); // there are no cautionary ornaments 
            }
            if(_lyricMetrics != null)
            {
                _lyricMetrics.WriteSVG(w);               
            }

            if(_dynamicMetrics != null)
                _dynamicMetrics.WriteSVG(w); // cautionary dynamics don't exist.

            if(_cautionaryBracketsMetrics != null)
            {
                foreach(CautionaryBracketMetrics cautionaryBracketMetrics in _cautionaryBracketsMetrics)
                    cautionaryBracketMetrics.WriteSVG(w);
            }
            if(NoteheadExtendersMetricsBefore != null)
            {
                foreach(NoteheadExtenderMetrics nemb in NoteheadExtendersMetricsBefore)
                    nemb.WriteSVG(w);
            }
            if(NoteheadExtendersMetrics != null)
            {
                foreach(NoteheadExtenderMetrics nem in NoteheadExtendersMetrics)
                    nem.WriteSVG(w);
            }

            foreach(var slurTemplate in SlurTemplates)
            {
                slurTemplate.WriteSVG(w);
            }
            foreach(TieTemplate tieTemplate in TieTemplates)
            { 
                tieTemplate.WriteSVG(w);
            }
        }

        public override void Move(double dx, double dy)
        {
            base.Move(dx, dy);

            if(_stemMetrics != null)
                _stemMetrics.Move(dx, dy);
            if(_flagsMetrics != null)
                _flagsMetrics.Move(dx, dy);
            if(_headsMetricsTopDown != null)
            {
                foreach(HeadMetrics headMetric in _headsMetricsTopDown)
                    headMetric.Move(dx, dy);
            }
            if(_accidentalsMetricsTopDown != null)
            {
                foreach(AccidentalMetrics accidentalMetric in _accidentalsMetricsTopDown)
                    accidentalMetric.Move(dx, dy);
            }
            if(_augDotMetricsTopDown != null)
            {
                foreach(AugDotMetrics augDotMetrics in _augDotMetricsTopDown)
                    augDotMetrics.Move(dx, dy);
            }
            if(_upperLedgerlineBlockMetrics != null)
            {
                _upperLedgerlineBlockMetrics.Move(dx, dy);
            }
            if(_lowerLedgerlineBlockMetrics != null)
            {
                _lowerLedgerlineBlockMetrics.Move(dx, dy);
            }
            if(_cautionaryBracketsMetrics != null)
            {
                foreach(CautionaryBracketMetrics cautionaryBracketMetrics in _cautionaryBracketsMetrics)
                    cautionaryBracketMetrics.Move(dx, dy);
            }
            if(_ornamentMetrics != null)
                _ornamentMetrics.Move(dx, dy);
            if(_lyricMetrics != null)
                _lyricMetrics.Move(dx, dy);
            if(_dynamicMetrics != null)
                _dynamicMetrics.Move(dx, dy);

            /// These three are not part of the external boundary
            if(dy != 0F && BeamBlock != null)
                this.BeamBlock.Move(dy);
            if(NoteheadExtendersMetricsBefore != null)
            {
                foreach(NoteheadExtenderMetrics nemb in NoteheadExtendersMetricsBefore)
                    nemb.Move(dx, dy);
            }
            if(NoteheadExtendersMetrics != null)
            {
                foreach(NoteheadExtenderMetrics nem in NoteheadExtendersMetrics)
                    nem.Move(dx, dy);
            }
            if(_slurTieMetrics != null)
            {
                M.Assert(dx == 0); // slurs and ties are only ever moved vertically.
                foreach(var slurTieMetrics in _slurTieMetrics)
                {
                    slurTieMetrics.Move(dx, dy);
                }

                foreach(var slurTemplate in SlurTemplates)
                {
                    slurTemplate.Move(dy);
                }

                foreach(var tieTemplate in TieTemplates)
                {
                    tieTemplate.Move(dy);
                }
            }

            SetExternalBoundary();
        }

        public void MoveOuterStemTip(double stemTipY, VerticalDir stemDirection)
        {
            if(stemDirection == VerticalDir.up)
            {
                _stemMetrics.SetTop(stemTipY);
            }
            else //(_stemDirection == VerticalDir.down)
            {
                _stemMetrics.SetBottom(stemTipY);
            }

            MoveAuxilliaries(stemDirection, _gap, 0, _gap * 0.3);
            SetExternalBoundary();
        }

        /// <summary>
        /// This chord is synchronous with a chord in the other voice on the staff, or belongs to a beamBlock which
        /// encloses the other chord.
        /// If it has an outer stem tip (with or without flagsBlock) which needs to be moved outwards because it
        /// is too close to the noteheads in the other chord, the stem tip (and flagsBlock) is moved outwards.
        /// FlagsBlockMetrics exist if the duration class is small enough, and the chord is not owned by a beamBlock.
        /// </summary>
        public void AdjustStemLengthAndFlagBlock(DurationClass thisDurationClass, double thisFontHeight, List<HeadMetrics> otherHeadsMetrics)
        {
            if(_stemMetrics == null
            || (_stemMetrics.VerticalDir == VerticalDir.up && BottomHeadMetrics.Bottom <= otherHeadsMetrics[0].Top)
            || (_stemMetrics.VerticalDir == VerticalDir.down && TopHeadMetrics.Top >= otherHeadsMetrics[otherHeadsMetrics.Count -1].Bottom))
                return;

            if(_flagsMetrics != null)
            {
                FlagsMetrics dummyFlagsMetrics = GetFlagsMetrics(otherHeadsMetrics,
                                                thisDurationClass,
                                                thisFontHeight,
                                                _stemMetrics.VerticalDir,
                                                _stemMetrics.StrokeWidth);

                StemMetrics dummyStemMetrics =
                    DummyStemMetrics(otherHeadsMetrics, _stemMetrics.VerticalDir, thisFontHeight,
                        dummyFlagsMetrics, null, _stemMetrics.StrokeWidth);

                if(_stemMetrics.VerticalDir == VerticalDir.up)
                {
                    if(dummyStemMetrics.Top < _stemMetrics.Top)
                    {
                        MoveOuterStemTip(dummyStemMetrics.Top, _stemMetrics.VerticalDir);
                        _flagsMetrics.Move(0, dummyStemMetrics.Top - _flagsMetrics.Top); 
                    }
                }
                else
                {
                    if(dummyStemMetrics.Bottom > _stemMetrics.Bottom)
                    {
                        MoveOuterStemTip(dummyStemMetrics.Bottom, _stemMetrics.VerticalDir);
                        _flagsMetrics.Move(0, dummyStemMetrics.Bottom - _flagsMetrics.Bottom);
                    }
                }
            }
            else // a crotchet or minim (without flagsBlock)
            {
                StemMetrics dummyStemMetrics =
                    DummyStemMetrics(otherHeadsMetrics, _stemMetrics.VerticalDir, thisFontHeight, null, null,
                        _stemMetrics.StrokeWidth);

                if(_stemMetrics.VerticalDir == VerticalDir.up)
                {
                    if(dummyStemMetrics.Top < _stemMetrics.Top)
                    {
                        MoveOuterStemTip(dummyStemMetrics.Top + (_gap / 2F), _stemMetrics.VerticalDir);
                    }
                }
                else
                {
                    if(dummyStemMetrics.Bottom > _stemMetrics.Bottom)
                    {
                        MoveOuterStemTip(dummyStemMetrics.Bottom - (_gap / 2F), _stemMetrics.VerticalDir);
                    }
                }
            }
        }

        /// <summary>
        /// N.B. This function has not been thoroughly tested yet. 20.01.2012
        /// If there is a lyric in this chord:
        /// If voiceStemDirection is none or down, the top of the lyric is aligned to lyricTop,
        /// If voiceStemDirection is up, this chord is in the upper voice of a 2-voice staff, and
        /// the lyric is above the chord. In this case, the bottom of the lyric is aligned to lyricBottom.
        /// If there is a dynamic or ornament on the same side of the staff, these are also moved by
        /// the same amount.
        /// If this chord has no lyric, nothing happens.
        /// </summary>
        public void MoveAuxilliariesToLyricHeight(VerticalDir voiceStemDirection, double lyricTop, double lyricBottom)
        {
            if(_lyricMetrics != null)
            {
                GetRelativePositions(voiceStemDirection, _lyricMetrics, out bool ornamentIsBelow, out bool dynamicIsBelow);
                double delta = 0;
                if(_lyricMetrics.IsBelow)
                {
                    delta = lyricTop - _lyricMetrics.Top;
					delta *= 0.7; // this line added 12.08.2015
                    _lyricMetrics.Move(0, delta);
                    if(ornamentIsBelow && _ornamentMetrics != null)
                        _ornamentMetrics.Move(0, delta);
                    if(dynamicIsBelow && _dynamicMetrics != null)
                        _dynamicMetrics.Move(0, delta);
                }
                else // lyric is above
                {
                    delta = lyricBottom - _lyricMetrics.Bottom;
                    _lyricMetrics.Move(0, delta);
                    if(!ornamentIsBelow && _ornamentMetrics != null)
                        _ornamentMetrics.Move(0, delta);
                    if(!dynamicIsBelow && _dynamicMetrics != null)
                        _dynamicMetrics.Move(0, delta);
                }
            }
        }

        /// <summary>
        /// Sets the stem. Pass flagsBlockMetrics=null for duration classes having no flags.
        /// </summary>
        private StemMetrics NewStemMetrics(List<HeadMetrics> topDownHeadsMetrics, ChordSymbol chord, Metrics flagsBlockMetrics, double strokeWidth)
        {
            return NewStemMetrics(topDownHeadsMetrics, chord.Stem.Direction, chord.FontHeight, flagsBlockMetrics, chord.BeamBlock, strokeWidth);
        }

        private StemMetrics NewStemMetrics(
            List<HeadMetrics> topDownHeadsMetrics, 
            VerticalDir stemDirection, 
            double fontHeight, 
            Metrics flagsBlockMetrics,
            BeamBlock beamBlock,
            double strokeWidth)
        {
            HeadMetrics outerNotehead = FindOuterNotehead(topDownHeadsMetrics, stemDirection);
            HeadMetrics innerNotehead = FindInnerNotehead(topDownHeadsMetrics, stemDirection);
            string characterString = outerNotehead.CharacterString;
            NoteheadStemPositions_px nspPX = CLichtFontMetrics.ClichtNoteheadStemPositionsDictPX[characterString];
            double outerNoteheadAlignmentY = (outerNotehead.Bottom + outerNotehead.Top) / 2;
            double innerNoteheadAlignmentY = (innerNotehead.Bottom + innerNotehead.Top) / 2;
            double delta = _gap * 0.1;
            double octave = (_gap * 3.5) + delta; // a little more than 1 octave
            double sixth = (_gap * 2.5) + delta; // a little more than 1 sixth

            double top = 0;
            double bottom = 0;
            double x = 0;
            if(stemDirection == VerticalDir.up)
            {
                x = outerNotehead.RightStemX - (strokeWidth / 2);
                bottom = outerNoteheadAlignmentY + (nspPX.RightStemY_px * fontHeight);
                if(beamBlock != null)
                {
                    top = beamBlock.DefaultStemTipY;
                }
                else
                {
                    if(flagsBlockMetrics != null)
                    {
                        top = flagsBlockMetrics.Top;
                    }
                    else
                        top = innerNoteheadAlignmentY - octave;

                    if(top > (_gap * 2))
                    {
                        top = (_gap * 2) - delta;
                    }
                }
            }
            else // stem is down
            {
                x = outerNotehead.LeftStemX + (strokeWidth / 2);
                top = outerNoteheadAlignmentY + (nspPX.LeftStemY_px * fontHeight);
                if(beamBlock != null)
                {
                    bottom = beamBlock.DefaultStemTipY;
                }
                else
                {
                    if(flagsBlockMetrics != null)
                    {
                        bottom = flagsBlockMetrics.Bottom;
                    }
                    else
                        bottom = innerNoteheadAlignmentY + octave;

                    if(bottom < (_gap * 2))
                    {
                        bottom = (_gap * 2) + delta;
                    }
                }
            }

            StemMetrics stemMetrics = new StemMetrics(top, x, bottom, strokeWidth, stemDirection);
            return stemMetrics;
        }

        /// <summary>
        /// Returns the stem which the otherChordTopDownHeadsMetrics would have if their duration class was crotchet.
        /// DummyStemMetrics are used when aligning synchronous chords.
        /// </summary>
        private StemMetrics DummyStemMetrics(
            List<HeadMetrics> otherChordTopDownHeadsMetrics,
            VerticalDir stemDirection,
            double fontHeight,
            Metrics flagsBlockMetrics,
            BeamBlock beamBlock,
            double strokeWidth)
        {
            List<HeadMetrics> tempTopDownHeadsMetrics = new List<HeadMetrics>();
            foreach(HeadMetrics headMetrics in otherChordTopDownHeadsMetrics)
            {
                HeadMetrics newHeadMetrics = new HeadMetrics(headMetrics, DurationClass.crotchet);
                tempTopDownHeadsMetrics.Add(newHeadMetrics);
            }

            return NewStemMetrics(tempTopDownHeadsMetrics, stemDirection, fontHeight, flagsBlockMetrics, beamBlock, strokeWidth);
        }

        public BeamBlock BeamBlock = null;
        public List<NoteheadExtenderMetrics> NoteheadExtendersMetrics = null;
        public List<NoteheadExtenderMetrics> NoteheadExtendersMetricsBefore = null;

        public List<TieTemplate> TieTemplates = new List<TieTemplate>();        
        public List<SlurTemplate> SlurTemplates = new List<SlurTemplate>();

        /// <summary>
        /// Returns null or a clone of the _stemMetrics
        /// </summary>
        public StemMetrics StemMetrics
        {
            get
            {
                StemMetrics stemMetrics = null;

                if(_stemMetrics != null)
                    stemMetrics = (StemMetrics)_stemMetrics.Clone();

                return stemMetrics;
            }
        }
        /// <summary>
        /// Returns null or a clone of the _upperLedgerlineBlockMetrics
        /// </summary>
        public LedgerlineBlockMetrics UpperLedgerlineBlockMetrics
        {
            get
            {
                LedgerlineBlockMetrics upperLedgerlineBlockMetrics = null;
                if(_upperLedgerlineBlockMetrics != null)
                {
                    upperLedgerlineBlockMetrics = (LedgerlineBlockMetrics)_upperLedgerlineBlockMetrics.Clone();
                }
                return upperLedgerlineBlockMetrics;
            }
        }
        /// <summary>
        /// Returns null or a clone of the _lowerLedgerlineBlockMetrics
        /// </summary>
        public LedgerlineBlockMetrics LowerLedgerlineBlockMetrics
        {
            get
            {
                LedgerlineBlockMetrics lowerLedgerlineBlockMetrics = null;
                if(_lowerLedgerlineBlockMetrics != null)
                {
                    lowerLedgerlineBlockMetrics = (LedgerlineBlockMetrics)_lowerLedgerlineBlockMetrics.Clone();
                }
                return lowerLedgerlineBlockMetrics;
            }
        }
        /// <summary>
        /// Returns an empty list or a clone of the _cautionaryBracketsMetrics
        /// </summary>
        public List<CautionaryBracketMetrics> CautionaryBracketsMetrics
        {
            get
            {
                List<CautionaryBracketMetrics> cbmList = new List<CautionaryBracketMetrics>();
                if(_cautionaryBracketsMetrics != null)
                {
                    foreach(CautionaryBracketMetrics metrics in this._cautionaryBracketsMetrics)
                    {
                        cbmList.Add((CautionaryBracketMetrics)metrics.Clone());
                    }
                }
                return cbmList;
            }
        }
        /// <summary>
        /// Returns an empty list or a clone of _topDownAccidentalsMetrics
        /// The accidentals are in top-bottom order.
        /// </summary>
        public List<AccidentalMetrics> AccidentalsMetrics
        {
            get
            {
                List<AccidentalMetrics> accidentalsMetrics = new List<AccidentalMetrics>();
                if(_accidentalsMetricsTopDown != null)
                {
                    foreach(AccidentalMetrics metrics in this._accidentalsMetricsTopDown)
                    {
                        accidentalsMetrics.Add((AccidentalMetrics)metrics.Clone());
                    }
                }
                return accidentalsMetrics;
            }
        }
        /// <summary>
        /// Returns an empty list or a clone of _topDownHeadsMetrics
        /// The heads are in top-bottom order.
        /// </summary>
        public List<HeadMetrics> HeadsMetrics
        {
            get
            {
                List<HeadMetrics> headsMetrics = new List<HeadMetrics>();
                if(_headsMetricsTopDown != null)
                {
                    foreach(HeadMetrics metrics in this._headsMetricsTopDown)
                    {
                        headsMetrics.Add((HeadMetrics)metrics.Clone());
                    }
                }
                return headsMetrics;
            }
        }
        /// <summary>
        /// Returns null or a clone of the top head's HeadMetrics
        /// </summary>
        public HeadMetrics TopHeadMetrics
        {
            get
            {
                HeadMetrics topHeadMetrics = null;
                if(_headsMetricsTopDown != null && _headsMetricsTopDown.Count > 0)
                    topHeadMetrics = (HeadMetrics)_headsMetricsTopDown[0].Clone();
                return topHeadMetrics;
            }
        }
        /// <summary>
        /// returns null or a clone of the bottom head's HeadMetrics
        /// </summary>
        public HeadMetrics BottomHeadMetrics
        {
            get
            {
                HeadMetrics bottomHeadMetrics = null;
                if(_headsMetricsTopDown != null && _headsMetricsTopDown.Count > 0)
                    bottomHeadMetrics = (HeadMetrics)_headsMetricsTopDown[_headsMetricsTopDown.Count - 1].Clone();
                return bottomHeadMetrics;
            }
        }
        /// <summary>
        /// Returns an empty list or a list containing the OriginY of each notehead.
        /// The values are in top-down order.
        /// </summary>
        /// <returns></returns>
        public List<double> HeadsOriginYs
        {
            get
            {
                List<double> originYs = new List<double>();
                if(_headsMetricsTopDown != null)
                {
                    foreach(HeadMetrics headMetrics in _headsMetricsTopDown)
                    {
                        originYs.Add(headMetrics.OriginY);
                    }
                }
                return originYs;
            }
        }
        /// <summary>
        /// Returns an empty list, or a clone of the _accidentalsMetrics
        /// </summary>
        public List<AccidentalMetrics> TopDownAccidentalsMetrics
        {
            get
            {
                List<AccidentalMetrics> accidentalsMetrics = new List<AccidentalMetrics>();
                if(_accidentalsMetricsTopDown != null)
                {
                    foreach(AccidentalMetrics metrics in this._accidentalsMetricsTopDown)
                    {
                        accidentalsMetrics.Add((AccidentalMetrics)metrics.Clone());
                    }
                }
                return accidentalsMetrics;
            }
        }
        /// <summary>
        /// Returns null or a clone of the _lyricsMetrics
        /// </summary>
        public LyricMetrics LyricMetrics
        {
            get
            {
                LyricMetrics lyricMetrics = null;

                if(_lyricMetrics != null)
                    lyricMetrics = (LyricMetrics)_lyricMetrics.Clone();

                return lyricMetrics;
            }
        }

        public void AddToEdge(HorizontalEdge horizontalEdge)
        {
            TopEdge topEdge = horizontalEdge as TopEdge;
            BottomEdge bottomEdge = horizontalEdge as BottomEdge;
            #region _stemMetrics
            if(_stemMetrics != null)
            {
                if(topEdge != null)
                    topEdge.Add(_stemMetrics);
                else
                    bottomEdge.Add(_stemMetrics);
            }
            #endregion
            #region _flagsBlockMetrics
            if(_flagsMetrics != null)
            {
                if(topEdge != null)
                    topEdge.Add(_flagsMetrics);
                else
                    bottomEdge.Add(_flagsMetrics);
            }
            #endregion
            #region _topDownHeadsMetrics
            if(_headsMetricsTopDown != null)
            {
                if(topEdge != null)
                {
                    foreach(HeadMetrics headMetric in _headsMetricsTopDown)
                    {
                        topEdge.Add(headMetric);
                    }
                }
                else
                {
                    foreach(HeadMetrics headMetric in _headsMetricsTopDown)
                    {
                        bottomEdge.Add(headMetric);
                    }
                }
            }
            #endregion
            #region _accidentalsMetrics
            if(_accidentalsMetricsTopDown != null)
            {
                if(topEdge != null)
                {
                    foreach(AccidentalMetrics accidentalMetric in _accidentalsMetricsTopDown)
                    {
                        topEdge.Add(accidentalMetric);
                    }
                }
                else
                {
                    foreach(AccidentalMetrics accidentalMetric in _accidentalsMetricsTopDown)
                    {
                        bottomEdge.Add(accidentalMetric);
                    }
                }
            }
            #endregion
            #region ledgerlineBlocksMetrics
            if(topEdge != null && _upperLedgerlineBlockMetrics != null)
            {
                topEdge.Add(_upperLedgerlineBlockMetrics);
            }
            if(bottomEdge != null && _lowerLedgerlineBlockMetrics != null)
            {
                bottomEdge.Add(_lowerLedgerlineBlockMetrics);
            }
            #endregion
            #region _cautionaryBracketsMetrics
            if(_cautionaryBracketsMetrics != null)
            {
                if(topEdge != null)
                {
                    foreach(CautionaryBracketMetrics cautionaryBracketMetrics in _cautionaryBracketsMetrics)
                    {
                        topEdge.Add(cautionaryBracketMetrics);
                    }
                }
                else
                {
                    foreach(CautionaryBracketMetrics cautionaryBracketMetrics in _cautionaryBracketsMetrics)
                    {
                        bottomEdge.Add(cautionaryBracketMetrics);
                    }
                }
            }
            #endregion
            #region _ornamentMetrics
            if(_ornamentMetrics != null)
            {
                if(topEdge != null)
                    topEdge.Add(_ornamentMetrics);
                else
                    bottomEdge.Add(_ornamentMetrics);
            }
            #endregion
            #region _lyricMetrics
            if(_lyricMetrics != null)
            {
                if(topEdge != null)
                    topEdge.Add(_lyricMetrics);
                else
                    bottomEdge.Add(_lyricMetrics);
            }
            #endregion
            #region _dynamicMetrics
            if(_dynamicMetrics != null)
            {
                if(topEdge != null)
                    topEdge.Add(_dynamicMetrics);
                else
                    bottomEdge.Add(_dynamicMetrics);
            }
            #endregion
            #region BeamBlock
            if(BeamBlock != null)
            {
                List<HLine> hLines = BeamBlock.OuterEdge();
                if(topEdge != null)
                {
                    foreach(HLine hLine in hLines)
                    {
                        topEdge.Add(hLine);
                    }
                }
                else
                {
                    foreach(HLine hLine in hLines)
                    {
                        bottomEdge.Add(hLine);
                    }
                }
            }
            #endregion
            #region NoteheadExtendersMetricsBefore
            if(NoteheadExtendersMetricsBefore != null)
            {
                if(topEdge != null)
                {
                    foreach(NoteheadExtenderMetrics nemb in NoteheadExtendersMetricsBefore)
                    {
                        topEdge.Add(nemb);
                    }
                }
                else
                {
                    foreach(NoteheadExtenderMetrics nemb in NoteheadExtendersMetricsBefore)
                    {
                        bottomEdge.Add(nemb);
                    }
                }
            }
            #endregion
            #region NoteheadExtendersMetrics
            if(NoteheadExtendersMetrics != null)
            {
                if(topEdge != null)
                {
                    foreach(NoteheadExtenderMetrics nem in NoteheadExtendersMetrics)
                    {
                        topEdge.Add(nem);
                    }
                }
                else
                {
                    foreach(NoteheadExtenderMetrics nem in NoteheadExtendersMetrics)
                    {
                        bottomEdge.Add(nem);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// returns double.MinValue if no overlap is found
        /// </summary>
        /// <param name="followingMetrics"></param>
        /// <returns></returns>
        public new double OverlapWidth(Metrics followingMetrics)
        {
            double maxOverlapWidth = double.MinValue;
            double overlap = double.MinValue;
            #region _stemMetrics
            if(_stemMetrics != null)
            {
                overlap = followingMetrics.OverlapWidth(_stemMetrics);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region _flagsBlockMetrics
            if(_flagsMetrics != null)
            {
                overlap = followingMetrics.OverlapWidth(_flagsMetrics);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region _topDownHeadsMetrics
            if(_headsMetricsTopDown != null)
            {
                foreach(HeadMetrics headMetric in _headsMetricsTopDown)
                {
                    overlap = followingMetrics.OverlapWidth(headMetric);
                    maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
                }
            }
            #endregion
            #region _accidentalsMetrics
            if(_accidentalsMetricsTopDown != null)
            {
                foreach(AccidentalMetrics accidentalMetric in _accidentalsMetricsTopDown)
                {
                    overlap = followingMetrics.OverlapWidth(accidentalMetric);
                    maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
                }
            }
            #endregion
            #region ledgerlineBlocksMetrics
            if(_upperLedgerlineBlockMetrics != null)
            {
                overlap = followingMetrics.OverlapWidth(_upperLedgerlineBlockMetrics);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            if(_lowerLedgerlineBlockMetrics != null)
            {
                overlap = followingMetrics.OverlapWidth(_lowerLedgerlineBlockMetrics);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region
            if(_cautionaryBracketsMetrics != null)
            {
                foreach(CautionaryBracketMetrics cautionaryBracketMetrics in _cautionaryBracketsMetrics)
                {
                    overlap = followingMetrics.OverlapWidth(cautionaryBracketMetrics);
                    maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
                }
            }
            #endregion
            #region _ornamentMetrics
            if(_ornamentMetrics != null)
            {
                overlap = followingMetrics.OverlapWidth(_ornamentMetrics);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region _lyricMetrics
            if(_lyricMetrics != null)
            {
                overlap = followingMetrics.OverlapWidth(_lyricMetrics);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region _dynamicMetrics
            if(_dynamicMetrics != null)
            {
                overlap = followingMetrics.OverlapWidth(_dynamicMetrics);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region NoteheadExtendersMetrics
            // NoteheadExtenders should only be created after JustifyHorizontally(),
            // so they should be null here.
            M.Assert(NoteheadExtendersMetrics == null);
            #endregion
            return maxOverlapWidth;

        }
        public new double OverlapWidth(AnchorageSymbol previousAnchorageSymbol)
        {
            double maxOverlapWidth = double.MinValue;
            double overlap = 0;
            #region _stemMetrics
            if(_stemMetrics != null)
            {
                overlap = _stemMetrics.OverlapWidth(previousAnchorageSymbol);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region _flagsBlockMetrics
            if(_flagsMetrics != null)
            {
                overlap = _flagsMetrics.OverlapWidth(previousAnchorageSymbol);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region _topDownHeadsMetrics
            if(_headsMetricsTopDown != null)
            {
                foreach(HeadMetrics headMetric in _headsMetricsTopDown)
                {
                    overlap = headMetric.OverlapWidth(previousAnchorageSymbol);
                    maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
                }
            }
            #endregion
            #region _accidentalsMetrics
            if(_accidentalsMetricsTopDown != null)
            {
                foreach(AccidentalMetrics accidentalMetric in _accidentalsMetricsTopDown)
                {
                    overlap = accidentalMetric.OverlapWidth(previousAnchorageSymbol);
                    maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
                }
            }
            #endregion
            #region _ledgerlineBlocksMetrics
            if(_upperLedgerlineBlockMetrics != null)
            {
                overlap = _upperLedgerlineBlockMetrics.OverlapWidth(previousAnchorageSymbol);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            if(_lowerLedgerlineBlockMetrics != null)
            {
                overlap = _lowerLedgerlineBlockMetrics.OverlapWidth(previousAnchorageSymbol);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region _cautionaryBracketsMetrics
            if(_cautionaryBracketsMetrics != null)
            {
                foreach(CautionaryBracketMetrics cautionaryBracketMetrics in _cautionaryBracketsMetrics)
                {
                    overlap = cautionaryBracketMetrics.OverlapWidth(previousAnchorageSymbol);
                    maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
                }
            }
            #endregion
            #region _ornamentMetrics
            if(_ornamentMetrics != null)
            {
                overlap = _ornamentMetrics.OverlapWidth(previousAnchorageSymbol);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region _lyricMetrics
            if(_lyricMetrics != null)
            {
                overlap = _lyricMetrics.OverlapWidth(previousAnchorageSymbol);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region _dynamicMetrics
            if(_dynamicMetrics != null)
            {
                overlap = _dynamicMetrics.OverlapWidth(previousAnchorageSymbol);
                maxOverlapWidth = maxOverlapWidth > overlap ? maxOverlapWidth : overlap;
            }
            #endregion
            #region NoteheadExtendersMetrics
            // NoteheadExtenders never overlap the previousAnchorageSymbol,
            // and should only be created after JustifyHorizontally() anyway.
            // They should be null here.
            M.Assert(NoteheadExtendersMetrics == null);
            #endregion
            return maxOverlapWidth;
        }

        /// <summary>
        /// Returns the (positive) amount by which to raise the metrics argument
        /// so that its bottom is level with the top of a chord component which 
        /// it overlaps.
        /// The argument metrics is padded on all sides using the padding argument.
        /// </summary>
        public new double OverlapHeight(Metrics metrics, double padding)
        {
            double maxOverlapHeight = double.MinValue;
            double overlap = 0;
            #region _stemMetrics
            if(_stemMetrics != null)
            {
                overlap = _stemMetrics.OverlapHeight(metrics, padding);
                if(overlap != 0F)
                    maxOverlapHeight = maxOverlapHeight > overlap ? maxOverlapHeight : overlap;
            }
            #endregion
            #region _flagsBlockMetrics
            if(_flagsMetrics != null)
            {
                overlap = _flagsMetrics.OverlapHeight(metrics, padding);
                if(overlap != 0F)
                    maxOverlapHeight = maxOverlapHeight > overlap ? maxOverlapHeight : overlap;
            }
            #endregion
            #region _topDownHeadsMetrics
            if(_headsMetricsTopDown != null)
            {
                foreach(HeadMetrics headMetric in _headsMetricsTopDown)
                {
                    overlap = headMetric.OverlapHeight(metrics, padding);
                    if(overlap != 0F)
                        maxOverlapHeight = maxOverlapHeight > overlap ? maxOverlapHeight : overlap;
                }
            }
            #endregion
            #region _accidentalsMetrics
            if(_accidentalsMetricsTopDown != null)
            {
                foreach(AccidentalMetrics accidentalMetric in _accidentalsMetricsTopDown)
                {
                    overlap = accidentalMetric.OverlapHeight(metrics, padding);
                    if(overlap != 0F)
                        maxOverlapHeight = maxOverlapHeight > overlap ? maxOverlapHeight : overlap;
                }
            }
            #endregion
            #region _ledgerlineBlocksMetrics
            if(_upperLedgerlineBlockMetrics != null)
            {
                overlap = _upperLedgerlineBlockMetrics.OverlapHeight(metrics, padding);
                if(overlap != 0F)
                    maxOverlapHeight = maxOverlapHeight > overlap ? maxOverlapHeight : overlap;
            }
            if(_lowerLedgerlineBlockMetrics != null)
            {
                overlap = _lowerLedgerlineBlockMetrics.OverlapHeight(metrics, padding);
                if(overlap != 0F)
                    maxOverlapHeight = maxOverlapHeight > overlap ? maxOverlapHeight : overlap;
            }
            #endregion
            #region
            if(_cautionaryBracketsMetrics != null)
            {
                foreach(CautionaryBracketMetrics cautionaryBracketMetrics in _cautionaryBracketsMetrics)
                {
                    overlap = cautionaryBracketMetrics.OverlapHeight(metrics, padding);
                    if(overlap != 0F)
                        maxOverlapHeight = maxOverlapHeight > overlap ? maxOverlapHeight : overlap;
                }
            }
            #endregion
            #region _ornamentMetrics
            if(_ornamentMetrics != null)
            {
                overlap = _ornamentMetrics.OverlapHeight(metrics, padding);
                if(overlap != 0F)
                    maxOverlapHeight = maxOverlapHeight > overlap ? maxOverlapHeight : overlap;
            }
            #endregion
            #region _lyricMetrics
            if(_lyricMetrics != null)
            {
                overlap = _lyricMetrics.OverlapHeight(metrics, padding);
                if(overlap != 0F)
                    maxOverlapHeight = maxOverlapHeight > overlap ? maxOverlapHeight : overlap;
            }
            #endregion
            #region _dynamicMetrics
            if(_dynamicMetrics != null)
            {
                overlap = _dynamicMetrics.OverlapHeight(metrics, padding);
                if(overlap != 0F)
                    maxOverlapHeight = maxOverlapHeight > overlap ? maxOverlapHeight : overlap;
            }
            #endregion
            #region NoteheadExtendersMetricsBefore
            if(NoteheadExtendersMetricsBefore != null)
            {
                foreach(NoteheadExtenderMetrics nem in NoteheadExtendersMetricsBefore)
                {
                    overlap = nem.OverlapHeight(metrics, padding);
                    if(overlap != 0F)
                        maxOverlapHeight = maxOverlapHeight > overlap ? maxOverlapHeight : overlap;
                }
            }
            #endregion
            return maxOverlapHeight;

        }

        public void AddAccidentalMetrics(AccidentalMetrics newAccidentalMetrics)
        {
            #region conditions
            M.Assert(newAccidentalMetrics != null);
            if(_accidentalsMetricsTopDown != null)
            {
                if(_accidentalsMetricsTopDown.Count > 1)
                {
                    for(int i = 1; i < _accidentalsMetricsTopDown.Count; ++i)
                    {
                        M.Assert(_accidentalsMetricsTopDown[i].OriginY >= _accidentalsMetricsTopDown[i - 1].OriginY);
                    }
                }
            }
            #endregion
            int insertIndex = -1;
            if(_accidentalsMetricsTopDown != null)
            {
                for(int i = 0; i < _accidentalsMetricsTopDown.Count; ++i)
                {
                    if(_accidentalsMetricsTopDown[i].OriginY > newAccidentalMetrics.OriginY)
                    {
                        insertIndex = i;
                        break;
                    }
                }
                if(insertIndex == -1)
                    _accidentalsMetricsTopDown.Add(newAccidentalMetrics);
                else
                    _accidentalsMetricsTopDown.Insert(insertIndex, newAccidentalMetrics);
            }
            else
            {
                _accidentalsMetricsTopDown = new List<AccidentalMetrics>() { newAccidentalMetrics };
            }
        }

        /// <summary>
        /// This chordMetrics is in the lower of two voices on a staff, and there is a synchronous chordMetrics (the 1st argument)
        /// at the same MsPosition on the same staff. Both chordSymbols have Metrics, and the chord in the lower voice has
        /// been moved (either left or right) with all its accidentals, ledgerlines etc. so that there are no collisions between 
        /// noteheads.
        /// </summary>
        public void AdjustAccidentalsForTwoChords(ChordMetrics otherChordMetrics, double staffLineStemStrokeWidth)
        {
            // first adjust the accidentals in the rightmost chord so that they are left of the other chord
            ChordMetrics leftChordMetrics = null;
            ChordMetrics rightChordMetrics = null;
            #region get left and right ChordMetrics
            if(this.OriginX < otherChordMetrics.OriginX)
            {
                leftChordMetrics = this;
                rightChordMetrics = otherChordMetrics; 
            }
            else
            {
                leftChordMetrics = otherChordMetrics;
                rightChordMetrics = this;
            }
            #endregion
            #region get top and bottom ledgerlineBlocks (can be null)
            LedgerlineBlockMetrics upperLedgerlineBlockMetrics = 
                CombinedLedgerlineBlockMetrics(_upperLedgerlineBlockMetrics,
                otherChordMetrics.UpperLedgerlineBlockMetrics, staffLineStemStrokeWidth);
            LedgerlineBlockMetrics lowerLedgerlineBlockMetrics = 
                CombinedLedgerlineBlockMetrics(_lowerLedgerlineBlockMetrics,
                        otherChordMetrics.LowerLedgerlineBlockMetrics, staffLineStemStrokeWidth);
            #endregion

            List<AccidentalMetrics> existingAccidentalsMetrics = new List<AccidentalMetrics>();
            List<HeadMetrics> combinedHeadMetrics = AllHeadsTopDown(leftChordMetrics.HeadsMetrics, rightChordMetrics.HeadsMetrics);

            List<AccidentalMetrics> leftChordAccidentalsMetrics = leftChordMetrics.AccidentalsMetrics;
            List<AccidentalMetrics> rightChordAccidentalsMetrics = rightChordMetrics.AccidentalsMetrics;

            List<AccidentalMetrics> combinedAccidentalMetrics = AllAccidentalsTopDown(leftChordAccidentalsMetrics, rightChordAccidentalsMetrics);
            foreach(AccidentalMetrics accidentalMetrics in combinedAccidentalMetrics) // these are cloned accidentalsMetrics
            {
                if(leftChordAccidentalsMetrics.Contains(accidentalMetrics))
                {
                    leftChordMetrics.MoveAccidentalLeft(accidentalMetrics, combinedHeadMetrics, leftChordMetrics.StemMetrics,
                                        upperLedgerlineBlockMetrics, lowerLedgerlineBlockMetrics, existingAccidentalsMetrics);
                    /// accidentalMetrics is a clone of the original one contained in the ChordMetrics.
                    /// Now move the real accidental to the new position.
                    leftChordMetrics.SetAccidentalXPos(accidentalMetrics);
                }
                else
                {
                    rightChordMetrics.MoveAccidentalLeft(accidentalMetrics, combinedHeadMetrics, leftChordMetrics.StemMetrics,
                                        upperLedgerlineBlockMetrics, lowerLedgerlineBlockMetrics, existingAccidentalsMetrics);
                    rightChordMetrics.SetAccidentalXPos(accidentalMetrics);
                }
                existingAccidentalsMetrics.Add(accidentalMetrics);
            }
        }

        /// <summary>
        /// The argument is a clone of one of the accidentals in this ChordMetrics
        /// which has been moved to a new horizontal position.
        /// </summary>
        /// <param name="movedCloneAM"></param>
        public void SetAccidentalXPos(AccidentalMetrics movedCloneAM)
        {
            foreach(AccidentalMetrics am in this._accidentalsMetricsTopDown)
            {
                if(am.CharacterString == movedCloneAM.CharacterString && am.OriginY == movedCloneAM.OriginY)
                {
                    am.Move(movedCloneAM.OriginX - am.OriginX, 0F);
                }
            }
        }

        /// <summary>
        /// Returns all the head metrics in a single list, ordered from top to bottom and right to left.
        /// </summary>
        private List<HeadMetrics> AllHeadsTopDown(List<HeadMetrics> leftHeadMetrics, List<HeadMetrics> rightHeadMetrics)
        {
            List<HeadMetrics> allHeadsTopDown = new List<HeadMetrics>(leftHeadMetrics);
            double delta = _gap / 4;
            int index = 0;
            foreach(HeadMetrics rightHead in rightHeadMetrics)
            {
                for(int i = 0; i < allHeadsTopDown.Count; ++i)
                {
                    if(rightHead.OriginY > (allHeadsTopDown[i].Top + delta))
                    {
                        index = i;
                        break;
                    }
                }
                allHeadsTopDown.Insert(index, rightHead);
            }
            return allHeadsTopDown;
        }

        /// <summary>
        /// Returns all the accidental metrics in a single list, ordered from top to bottom and right to left.
        /// </summary>
        private List<AccidentalMetrics> AllAccidentalsTopDown(List<AccidentalMetrics> leftAccMetrics, List<AccidentalMetrics> rightAccMetrics)
        {
            List<AccidentalMetrics> allAccsTopDown = new List<AccidentalMetrics>(leftAccMetrics);
            double delta = _gap / 4;
            foreach(AccidentalMetrics rightAcc in rightAccMetrics)
            {
                int index = allAccsTopDown.Count;
                for(int i = 0; i < allAccsTopDown.Count; ++i)
                {
                    if(rightAcc.OriginY < (allAccsTopDown[i].OriginY + delta))
                    {
                        index = i;
                        break;
                    }
                }
                allAccsTopDown.Insert(index, rightAcc);
            }
            return allAccsTopDown;
        }

        private LedgerlineBlockMetrics CombinedLedgerlineBlockMetrics(LedgerlineBlockMetrics lbm1, LedgerlineBlockMetrics lbm2,
            double staffLineStemStrokeWidth)
        {
            LedgerlineBlockMetrics ledgerlineBlockMetrics = null;
            double top = double.MaxValue;
            double right = double.MaxValue;
            double bottom = double.MaxValue;
            double left = double.MaxValue;

			if (lbm1 != null && lbm2 != null)
            {
                top = lbm1.Top < lbm2.Top ? lbm1.Top : lbm2.Top;
                right = lbm1.Right > lbm2.Right ? lbm1.Right : lbm2.Right;
                bottom = lbm1.Bottom > lbm2.Bottom ? lbm1.Bottom : lbm2.Bottom;
                left = lbm1.Left < lbm2.Left ? lbm1.Left : lbm2.Left;
            }
            else if(lbm1 != null)
            {
                top = lbm1.Top;
                right = lbm1.Right;
                bottom = lbm1.Bottom;
                left = lbm1.Left;
			}
            else if(lbm2 != null)
            {
                top = lbm2.Top;
                right = lbm2.Right;
                bottom = lbm2.Bottom;
                left = lbm2.Left;
			}
            if(top != double.MaxValue)
            {
                ledgerlineBlockMetrics = new LedgerlineBlockMetrics(left, right, staffLineStemStrokeWidth, CSSObjectClass.ledgerlines);
                ledgerlineBlockMetrics.SetTop(top);
                ledgerlineBlockMetrics.SetBottom(bottom);
            }
            return ledgerlineBlockMetrics;
        }

        #endregion public interface

        #region private variables
        private readonly double _gap = 0;
        private int _nStafflines = 0;
        private double _staffOriginY = 0;
        private Clef _clef = null;

        private StemMetrics _stemMetrics = null;
        private FlagsMetrics _flagsMetrics = null;
        public IReadOnlyList<HeadMetrics> HeadsMetricsTopDown
        {
            get { return _headsMetricsTopDown; }
        }
        public IReadOnlyList<AugDotMetrics> AugDotMetricsTopDown
        {
            get { return _augDotMetricsTopDown; }
        }

        public void AddSlurTieMetrics(SlurTieMetrics slurTieMetrics)
        {
            if(_slurTieMetrics == null)
            {
                _slurTieMetrics = new List<SlurTieMetrics>();
            }
            _slurTieMetrics.Add(slurTieMetrics);
        }

        private List<HeadMetrics> _headsMetricsTopDown = null; // heads are always in top->bottom order
        private List<AccidentalMetrics> _accidentalsMetricsTopDown = null; // accidentals are always in top->bottom order
        private List<AugDotMetrics> _augDotMetricsTopDown = null; // augDots are always in top->bottom order (top row, then next row etc.)
        private LedgerlineBlockMetrics _upperLedgerlineBlockMetrics = null;
        private LedgerlineBlockMetrics _lowerLedgerlineBlockMetrics = null;
        private List<CautionaryBracketMetrics> _cautionaryBracketsMetrics = null;
        private OrnamentMetrics _ornamentMetrics = null;
        private LyricMetrics _lyricMetrics = null;
        private DynamicMetrics _dynamicMetrics = null;
        private List<SlurTieMetrics> _slurTieMetrics = null;

        private readonly List<DrawObject> _drawObjects;
        #endregion private variables
    }
}
