using System;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Xml;
using MNX.Globals;

namespace Moritz.Symbols
{
    public class BeamBlock : LineMetrics
    {       
        public BeamBlock(CSSObjectClass beamBlockClass, Clef clef, List<OutputChordSymbol> chordsBeamedTogether, VerticalDir voiceStemDirection, double strokeThickness, PageFormat pageFormat, bool isCautionary)
            : base(beamBlockClass, strokeThickness, "black", "black")
        {
            Chords = new List<ChordSymbol>(chordsBeamedTogether);
            SetBeamedGroupStemDirection(clef, chordsBeamedTogether, voiceStemDirection);
            foreach(ChordSymbol chord in chordsBeamedTogether)
                chord.BeamBlock = this; // prevents an isolated flag from being created

            _gap = Chords[0].Voice.Staff.Gap;
            _beamThickness = pageFormat.BeamThickness;
            if(isCautionary)
            {
                _gap *= pageFormat.SmallSizeFactor;
                _beamThickness *= pageFormat.SmallSizeFactor;
            }
            _beamSeparation = _gap;            
            _strokeThickness = strokeThickness;
            _stemDirection = Chords[0].Stem.Direction;

            /******************************************************************************
             * Important to set stem tips to this value before justifying horizontally.
             * Allows collisions between the objects outside the tips (e.g. dynamics or ornaments)
             * to be detected correctly. */
            _defaultStemTipY = GetDefaultStemTipY(clef, chordsBeamedTogether);
        }

        /// <summary>
        /// This algorithm follows Gardner Read when the stemDirection is "none" (i.e. not forced):
        /// If there were no beam, and the majority of the stems would go up, then all the stems in the beam go up.
        /// ji: if there are the same number of default up and default down stems, then the direction is decided by
        /// the most extreme notehead in the beam group. If both extremes are the same (e.g. 1 ledgeline up and down)
        /// then the stems are all down.
        /// </summary>
        /// <param name="currentClef"></param>
        /// <param name="chordsBeamedTogether"></param>
        private void SetBeamedGroupStemDirection(Clef currentClef, List<OutputChordSymbol> chordsBeamedTogether, VerticalDir voiceStemDirection)
        {
            // chordsBeamedtogether.Count can be 1
            // (when a solitary chord has a continued beam at the beginning of a staff)
            VerticalDir groupStemDirection = voiceStemDirection;
            if(voiceStemDirection == VerticalDir.none)
            {   // here, there is only one voice in the staff, so the direction depends on the height of the noteheads.
                int upStems = 0;
                int downStems = 0;
                foreach(OutputChordSymbol chord in chordsBeamedTogether)
                {
                    VerticalDir direction = chord.DefaultStemDirection(currentClef);
                    if(direction == VerticalDir.up)
                        upStems++;
                    else
                        downStems++;
                }

                if(upStems == downStems)
                    groupStemDirection = GetDirectionFromExtremes(currentClef, chordsBeamedTogether);
                else if(upStems > downStems)
                    groupStemDirection = VerticalDir.up;
                else
                    groupStemDirection = VerticalDir.down;
            }
            foreach(ChordSymbol chord in chordsBeamedTogether)
            {
                chord.Stem.Direction = groupStemDirection;
            }
        }

        private VerticalDir GetDirectionFromExtremes(Clef currentClef, List<OutputChordSymbol> chordsBeamedTogether)
        {
            double headMinTop = double.MaxValue;
            double headMaxBottom = double.MinValue;
            double gap = chordsBeamedTogether[0].Voice.Staff.Gap;
            int numberOfStafflines = chordsBeamedTogether[0].Voice.Staff.NumberOfStafflines;

            foreach(ChordSymbol chord in chordsBeamedTogether)
            {
                foreach(Head head in chord.HeadsTopDown)
                {
                    double headY = head.GetOriginY(currentClef, gap);
                    headMinTop = headMinTop < headY ? headMinTop : headY;
                    headMaxBottom = headMaxBottom > headY ? headMaxBottom : headY;
                }
            }
            headMaxBottom -= (gap * (numberOfStafflines - 1));
            headMinTop *= -1;
            if(headMaxBottom > headMinTop)
                return VerticalDir.up;
            else
                return VerticalDir.down;
        }

        private double GetDefaultStemTipY(Clef currentClef, List<OutputChordSymbol> chordsBeamedTogether)
        {
            double headMinTop = double.MaxValue;
            double headMaxBottom = double.MinValue;
            double gap = chordsBeamedTogether[0].Voice.Staff.Gap;
            int numberOfStafflines = chordsBeamedTogether[0].Voice.Staff.NumberOfStafflines;
            VerticalDir direction = chordsBeamedTogether[0].Stem.Direction;

            foreach(OutputChordSymbol chord in chordsBeamedTogether)
            {
                foreach(Head head in chord.HeadsTopDown)
                {
                    double headY = head.GetOriginY(currentClef, gap);
                    headMinTop = headMinTop < headY ? headMinTop : headY;
                    headMaxBottom = headMaxBottom > headY ? headMaxBottom : headY;
                }
            }

            if(direction == VerticalDir.up)
                return headMinTop - (gap * numberOfStafflines);
            else
                return headMaxBottom + (gap * numberOfStafflines);
        }

        /// <summary>
        /// This beam is attached to chords in one voice of a 2-voice staff.
        /// The returned chords are the chords in the other voice whose msPosition
        /// is greater than or equal to the msPosition at the start of this beamBlock,
        /// and less than or equal to the msPosition at the end of this beamBlock. 
        /// </summary>
        public List<ChordSymbol> EnclosedChords(Voice otherVoice)
        {
            M.Assert(Chords.Count > 1);
            int startMsPos = Chords[0].AbsMsPosition;
            int endMsPos = Chords[Chords.Count - 1].AbsMsPosition;
            List<ChordSymbol> enclosedChordSymbols = new List<ChordSymbol>();
            foreach(ChordSymbol otherChord in otherVoice.ChordSymbols)
            {
                if(otherChord.AbsMsPosition >= startMsPos && otherChord.AbsMsPosition <= endMsPos)
                    enclosedChordSymbols.Add(otherChord);
                if(otherChord.AbsMsPosition > endMsPos)
                    break;
            }
            return enclosedChordSymbols;
        }
        /// <summary>
        /// The system has been justified horizontally, so all objects are at their final horizontal positions.
        /// This function
        ///  1. creates all the contained beams horizontally with their top edges at 0F.
        ///  2. expands the beams vertically, and moves them to the closest position on the correct side of their noteheads.
        ///  3. shears the beams.
        ///  4. moves the stem tips and related objects (dynamics, ornament numbers) to their correct positions wrt the outer beam.
        /// Especially note that neither this beam Block or its contained beams ever move _horizontally_.
        /// </summary>
        public void FinalizeBeamBlock(MNX.Common.BeamBlock beamBlockDef, double rightBarlineX)
        {
            #region create the individual beams all with top edge horizontal at 0F.
            HashSet<DurationClass> durationClasses = new HashSet<DurationClass>()
            {
                DurationClass.fiveFlags,
                DurationClass.fourFlags,
                DurationClass.threeFlags,
                DurationClass.semiquaver,
                DurationClass.quaver
            };

            foreach(DurationClass durationClass in durationClasses)
            {
                List<Beam> beams = CreateBeams(durationClass, beamBlockDef, rightBarlineX);
                _left = double.MaxValue;
                _right = double.MinValue;
                foreach(Beam beam in beams)
                {
                    _left = _left < beam.LeftX ? _left : beam.LeftX;
                    _right = _right > beam.RightX ? _right : beam.RightX;
                    Beams.Add(beam);
                }
                _originX = _left;
                // _left, _right and _originX never change again after they have been set here
            }
            SetBeamHooks(Beams);
            SetVerticalBounds();
            #endregion
            Dictionary<DurationClass, double> durationClassBeamThicknesses = GetBeamThicknessesPerDurationClass(durationClasses);
            List<ChordMetrics> chordsMetrics = GetChordsMetrics();
            ExpandVerticallyAtNoteheads(chordsMetrics, durationClassBeamThicknesses);
            Shear(chordsMetrics, rightBarlineX);
            FinalAdjustmentReNoteheads(chordsMetrics, durationClassBeamThicknesses[DurationClass.quaver]);
            FinalAdjustmentReAccidentals(chordsMetrics, durationClassBeamThicknesses[DurationClass.quaver]);
            MoveStemTips();
        }

        /// <summary>
        /// If the minimum distance between an inner beam and a notehead is less than 2.5 gaps,
        /// The beamBlock is moved away from the noteheads until the minimum distance is 2.5 gaps
        /// </summary>
        private void FinalAdjustmentReNoteheads(List<ChordMetrics> chordsMetrics, double singleBeamThickness)
        {
            double beamTan = GetBeamTan();

            foreach(ChordMetrics chordMetrics in chordsMetrics)
            {
                if(this._stemDirection == VerticalDir.down)
                {
                    double minimumSeparationY = double.MaxValue;
                    HeadMetrics bottomHeadMetrics = chordMetrics.BottomHeadMetrics;
                    double bhLeft = bottomHeadMetrics.Left;
                    double bhRight = bottomHeadMetrics.Right;
                    double bhOriginX = bottomHeadMetrics.OriginX;
                    foreach(Beam beam in Beams)
                    {
                        if(beam.LeftX <= bhRight && beam.RightX >= bhLeft)
                        {
                            double beamTopAtHeadOriginX = beam.LeftTopY + ((bhOriginX - beam.LeftX) * beamTan);
                            double localSeparationY = beamTopAtHeadOriginX - bottomHeadMetrics.OriginY;
                            minimumSeparationY = (localSeparationY < minimumSeparationY) ? localSeparationY : minimumSeparationY;
                        }
                    }

                    double shiftY = (_gap * 2.5F) - minimumSeparationY;
                    if(shiftY > 0)
                    {
                        Move(shiftY);
                    }
                }

                if(this._stemDirection == VerticalDir.up)
                {
                    double minimumSeparationY = double.MaxValue;
                    HeadMetrics topHeadMetrics = chordMetrics.TopHeadMetrics;
                    double thLeft = topHeadMetrics.Left;
                    double thRight = topHeadMetrics.Right;
                    double thOriginX = topHeadMetrics.OriginX;
                    foreach(Beam beam in Beams)
                    {
                        if(beam.LeftX <= thRight && beam.RightX >= thLeft)
                        {
                            double beamBottomAtHeadOriginX = beam.LeftTopY + ((thOriginX - beam.LeftX) * beamTan) + singleBeamThickness;
                            double localSeparationY = topHeadMetrics.OriginY - beamBottomAtHeadOriginX;
                            minimumSeparationY = (localSeparationY < minimumSeparationY) ? localSeparationY : minimumSeparationY;
                        }
                    }

                    double shiftY = (_gap * 2.5F) - minimumSeparationY;
                    if(shiftY > 0)
                    {
                        Move(shiftY * -1);
                    }
                }
            }
        }

        /// <summary>
        /// If the minimum distance between an inner beam overlaps an accidental,
        /// The beamBlock is moved away from the accidentals.
        /// </summary>
        private void FinalAdjustmentReAccidentals(List<ChordMetrics> chordsMetrics, double singleBeamThickness)
        {
            double beamTan = GetBeamTan();

            foreach(ChordMetrics chordMetrics in chordsMetrics)
            {
                if(this._stemDirection == VerticalDir.down)
                {
                    double positiveDeltaY = -1;
                    if(chordMetrics.AccidentalsMetrics.Count > 0)
                    {
                        AccidentalMetrics bottomAccidentalMetrics = chordMetrics.AccidentalsMetrics[chordMetrics.AccidentalsMetrics.Count - 1];
                        double baLeft = bottomAccidentalMetrics.Left;
                        double baRight = bottomAccidentalMetrics.Right;
                        double baOriginX = bottomAccidentalMetrics.OriginX;
                        foreach(Beam beam in Beams)
                        {
                            if(beam.LeftX <= baRight && beam.RightX >= baLeft)
                            {
                                double beamTopAtAccidentalOriginX = beam.LeftTopY + ((baOriginX - beam.LeftX) * beamTan);
                                if(bottomAccidentalMetrics.Bottom > beamTopAtAccidentalOriginX)
                                {
                                    double localdeltaY = bottomAccidentalMetrics.Bottom - beamTopAtAccidentalOriginX;
                                    positiveDeltaY = (localdeltaY > positiveDeltaY) ? localdeltaY : positiveDeltaY;
                                }
                            }
                        }
                    }
                    if(positiveDeltaY >= 0)
                    {
                        double shiftY = 0;
                        while(shiftY < positiveDeltaY)
                        {
                            shiftY += _gap * 0.75;
                        }
                        Move(shiftY);
                    }
                }

                if(this._stemDirection == VerticalDir.up)
                {
                    double negativeDeltaY = 1;
                    if(chordMetrics.AccidentalsMetrics.Count > 0)
                    {
                        AccidentalMetrics topAccidentalMetrics = chordMetrics.AccidentalsMetrics[0];
                        double taLeft = topAccidentalMetrics.Left;
                        double taRight = topAccidentalMetrics.Right;
                        double taOriginX = topAccidentalMetrics.OriginX;
                        foreach(Beam beam in Beams)
                        {
                            if(beam.LeftX <= taRight && beam.RightX >= taLeft)
                            {
                                double beamBottomAtAccidentalOriginX = beam.LeftTopY + ((taOriginX - beam.LeftX) * beamTan) + singleBeamThickness;
                                if(topAccidentalMetrics.Top < beamBottomAtAccidentalOriginX)
                                {
                                    double localdeltaY = topAccidentalMetrics.Top - beamBottomAtAccidentalOriginX;
                                    negativeDeltaY = (localdeltaY < negativeDeltaY) ? localdeltaY : negativeDeltaY;
                                }
                            }
                        }
                    }
                    if(negativeDeltaY <= 0)
                    {
                        double shiftY = 0;
                        while(shiftY > negativeDeltaY)
                        {
                            shiftY -= _gap * 0.75;
                        }
                        Move(shiftY);
                    }
                }
            }
        }

        private double GetBeamTan()
        {
            Beam longestbeam = null;
            double maxwidth = double.MinValue;
            double width = 0;

            foreach(Beam beam in Beams)
            {
                width = beam.RightX - beam.LeftX;
                if(width > maxwidth)
                {
                    longestbeam = beam;
                    maxwidth = width;
                }
            }

            double beamTan = (longestbeam.RightTopY - longestbeam.LeftTopY) / maxwidth;

            return beamTan;
        }

        private void SetBeamHooks(HashSet<Beam> beamsHash)
        {
            List<Beam> beams = new List<Beam>(beamsHash);
            double hookWidth = _gap * 1.2;

            for(int i = 0; i < beams.Count; ++i)
            {
                Beam beam = beams[i];
                if(beam.BeamHookDirection != MNX.Common.BeamHookDirection.none)
                {
                    beamsHash.Remove(beam);
                    DurationClass durationClass = (beam as IBeamHook).DurationClass;
                    Beam newBeamHook;
                    if(beam.BeamHookDirection == MNX.Common.BeamHookDirection.right)
                    {
                        // add a short beam to the right of the stem
                        newBeamHook = NewBeam(durationClass, beam.LeftX, beam.LeftX + hookWidth, MNX.Common.BeamHookDirection.none);
                    }
                    else
                    {
                        // add a short beam to the left of the stem
                        newBeamHook = NewBeam(durationClass, beam.LeftX - hookWidth, beam.LeftX, MNX.Common.BeamHookDirection.none);
                    }

                    beamsHash.Add(newBeamHook);
                }
            }
        }

        private Beam LeftEndLongBeam(List<Beam> beams, double stemX)
        {
            double leftX = int.MaxValue;
            Beam rval = null;

            foreach(Beam beam in beams)
            {
                if(!(beam is IBeamHook) && beam.LeftX < leftX)
                {
                    rval = beam;
                    leftX = beam.LeftX;
                }
            }

            foreach(Beam beam in beams)
            {
                if(stemX == beam.LeftX && !(beam is IBeamHook) && !(rval == beam))
                {
                    rval = beam;
                }
            }

            M.Assert(!(rval is IBeamHook));

            return rval;
        }

        List<ChordMetrics> GetChordsMetrics()
        {
            List<ChordMetrics> chordsMetrics = new List<ChordMetrics>();
            foreach(ChordSymbol chord in Chords)
            {
                chordsMetrics.Add((ChordMetrics)chord.Metrics);
            }
            return chordsMetrics;
        }

        private void SetVerticalBounds()
        {
            _top = double.MaxValue;
            _bottom = double.MinValue;
            foreach(Beam beam in Beams)
            {
                double beamBoundsTop = beam.LeftTopY < beam.RightTopY ? beam.LeftTopY : beam.RightTopY;
                double beamBoundsBottom = beam.LeftTopY > beam.RightTopY ? beam.LeftTopY : beam.RightTopY;
                beamBoundsBottom += _beamThickness;
                _bottom = _bottom > beamBoundsBottom ? _bottom : beamBoundsBottom;
                _top = _top < beamBoundsTop ? _top : beamBoundsTop;
            }
            _originY = _top;
        }

        /// <summary>
        /// Returns a list of beams having depth corresponding to the durationClass.
        /// The beams' left and right edge coordinates are set.
        /// </summary>
        /// <param name="durationClass"></param>
        /// <param name="beamBlockDef"></param>
        /// <returns></returns>
        private List<Beam> CreateBeams(DurationClass durationClass, MNX.Common.BeamBlock beamBlockDef, double rightBarlineX)
        {
            List<Beam> newBeams = new List<Beam>();

            List<MNX.Common.IBeamBlockComponent> beamBlockComponents = GetBeamDefs(beamBlockDef, durationClass);
            for(int i = 0; i < beamBlockComponents.Count; i++)
            {
                MNX.Common.IBeamBlockComponent beamBlockComponent = beamBlockComponents[i];
                double beamLeftX = double.MaxValue;
                double beamRightX = double.MinValue;

                List<ChordSymbol> chordsInBeam = GetChordsInBeam(Chords, beamBlockComponent);

                foreach(ChordSymbol chord in chordsInBeam)
                {
                    ChordMetrics chordMetrics = (ChordMetrics)chord.Metrics;
                    double stemX = chordMetrics.StemMetrics.OriginX;

                    beamLeftX = (beamLeftX < stemX) ? beamLeftX : stemX;
                    beamRightX = (beamRightX > stemX) ? beamRightX : stemX;
                }

                if(chordsInBeam[0].BeamBlockDef != null && (chordsInBeam[0].IsBeamRestart || chordsInBeam[0].IsBeamEnd))
                {
                    beamLeftX = ((ChordMetrics)chordsInBeam[0].Metrics).StemMetrics.OriginX - _gap;
                }
                if(durationClass == DurationClass.quaver && chordsInBeam[chordsInBeam.Count - 1].IsBeamEnd == false)
                {
                    beamRightX = rightBarlineX + (_gap * 0.75);
                }

                // BeamHooks are initially created as Beams having LeftX == RightX == stemX.
                // They are replaced by Beams having the proper width when the BeamBlock is complete.
                // (See SetBeamHooks() above.)
                Beam newBeam = null; ;
                if(beamBlockComponent is MNX.Common.BeamHook beamHook)
                {
                    M.Assert(beamLeftX == beamRightX);
                    newBeam = NewBeam(durationClass, beamLeftX, beamRightX, beamHook.BeamHookDirection);
                }
                else
                {
                    newBeam = NewBeam(durationClass, beamLeftX, beamRightX, MNX.Common.BeamHookDirection.none);
                }
                newBeams.Add(newBeam);
            }

            return newBeams;
        }

        private List<ChordSymbol> GetChordsInBeam(List<ChordSymbol> allBeamBlockEvents, MNX.Common.IBeamBlockComponent bbComponent)
        {
            List<ChordSymbol> chordsInBeam = new List<ChordSymbol>();
            if(bbComponent is MNX.Common.Beam beamDef)
            {
                foreach(var eventID in beamDef.EventIDs)
                {
                    ChordSymbol cs = allBeamBlockEvents.Find(x => x.EventID == eventID);
                    if(cs != null) // is null if the Event was a rest 
                    {
                        chordsInBeam.Add(cs);
                    }
                }
            }
            else if(bbComponent is MNX.Common.BeamHook beamHook)
            {
                ChordSymbol cs = allBeamBlockEvents.Find(x => x.EventID == beamHook.EventID);
                chordsInBeam.Add(cs);
            }
            return chordsInBeam;
        }

        private List<MNX.Common.IBeamBlockComponent> GetBeamDefs(MNX.Common.BeamBlock beamBlockDef, DurationClass durationClass)
        {
            int depth = GetDepth(durationClass);

            List<MNX.Common.IBeamBlockComponent> beamDefs = beamBlockDef.Components.FindAll(x => x.Depth == depth); 

            return beamDefs;
        }

        private int GetDepth(DurationClass durationClass)
        {
            int depth = 0;
            switch(durationClass)
            {
                case DurationClass.quaver:
                    depth = 0;
                    break;
                case DurationClass.semiquaver:
                    depth = 1;
                    break;
                case DurationClass.threeFlags:
                    depth = 2;
                    break;
                case DurationClass.fourFlags:
                    depth = 3;
                    break;
                case DurationClass.fiveFlags:
                    depth = 4;
                    break;
                case DurationClass.sixFlags:
                    depth = 5;
                    break;
                case DurationClass.sevenFlags:
                    depth = 6;
                    break;
                case DurationClass.eightFlags:
                    depth = 7;
                    break;
                default:
                    M.Assert(false, "This duration class has no beams!");
                    break;
            }
            return depth;
        }

        /// <summary>
        /// returns true if the currentDC has a less than or equal number of beams than the stemDC
        /// </summary>
        /// <param name="currentDC"></param>
        /// <param name="stemDC"></param>
        /// <returns></returns>
        private bool HasLessThanOrEqualBeams(DurationClass currentDC, DurationClass stemDC)
        {
            bool hasLessThanOrEqualBeams = false;
            switch(currentDC)
            {
                case DurationClass.fiveFlags:
                    if(stemDC == DurationClass.fiveFlags)
                        hasLessThanOrEqualBeams = true;
                    break;
                case DurationClass.fourFlags:
                    if(stemDC == DurationClass.fiveFlags
                    || stemDC == DurationClass.fourFlags)
                        hasLessThanOrEqualBeams = true;
                    break;
                case DurationClass.threeFlags:
                    if(stemDC == DurationClass.fiveFlags
                    || stemDC == DurationClass.fourFlags
                    || stemDC == DurationClass.threeFlags)
                        hasLessThanOrEqualBeams = true;
                    break;
                case DurationClass.semiquaver:
                    if(stemDC == DurationClass.fiveFlags
                    || stemDC == DurationClass.fourFlags
                    || stemDC == DurationClass.threeFlags
                    || stemDC == DurationClass.semiquaver)
                        hasLessThanOrEqualBeams = true;
                    break;
                case DurationClass.quaver:
                    if(stemDC == DurationClass.fiveFlags
                    || stemDC == DurationClass.fourFlags
                    || stemDC == DurationClass.threeFlags
                    || stemDC == DurationClass.semiquaver
                    || stemDC == DurationClass.quaver)
                        hasLessThanOrEqualBeams = true;
                    break;
            }
            return hasLessThanOrEqualBeams;
        }
        private Beam NewBeam(DurationClass durationClass, double leftX, double rightX, MNX.Common.BeamHookDirection beamHookDirection)
        {
            Beam newBeam = null;
            bool isBeamHook = beamHookDirection != MNX.Common.BeamHookDirection.none;
            switch(durationClass)
            {
                case DurationClass.fiveFlags:
                    if(isBeamHook)
                        newBeam = new FiveFlagsBeamHook(leftX, beamHookDirection);
                    else
                        newBeam = new FiveFlagsBeam(leftX, rightX, MNX.Common.BeamHookDirection.none);
                    break;
                case DurationClass.fourFlags:
                    if(isBeamHook)
                        newBeam = new FourFlagsBeamHook(leftX, beamHookDirection);
                    else
                        newBeam = new FourFlagsBeam(leftX, rightX, MNX.Common.BeamHookDirection.none);
                    break;
                case DurationClass.threeFlags:
                    if(isBeamHook)
                        newBeam = new ThreeFlagsBeamHook(leftX, beamHookDirection);
                    else
                        newBeam = new ThreeFlagsBeam(leftX, rightX, MNX.Common.BeamHookDirection.none);
                    break;
                case DurationClass.semiquaver:
                    if(isBeamHook)
                        newBeam = new SemiquaverBeamHook(leftX, beamHookDirection);
                    else
                        newBeam = new SemiquaverBeam(leftX, rightX, MNX.Common.BeamHookDirection.none);
                    break;
                case DurationClass.quaver:
                    newBeam = new QuaverBeam(leftX, rightX);
                    break;
                default:
                    M.Assert(false, "Illegal beam duration class");
                    break;
            }
            return newBeam;
        }
        public void Move(double dy)
        {
            foreach(Beam beam in Beams)
            {
                beam.MoveYs(dy, dy);
            }
            SetVerticalBounds();
        }
        /// <summary>
        /// Moves the horizontal beams to their correct vertical positions re the chords
        /// leaving the outer leftY of the beamBlock at leftY
        /// </summary>
        /// <param name="outerLeftY"></param>
        private void ExpandVerticallyAtNoteheads(List<ChordMetrics> chordsMetrics, Dictionary<DurationClass, double> durationClassBeamThicknesses)
        {
            ExpandVerticallyOnStaff();
            MoveToNoteheads(chordsMetrics, durationClassBeamThicknesses);
            SetVerticalBounds();
        }

        /// <summary>
        /// Expands the beamBlock vertically, leaving its outer edge on the top line of the staff 
        /// </summary>
        private void ExpandVerticallyOnStaff()
        {
            double staffOriginY = Chords[0].Voice.Staff.Metrics.OriginY;
            foreach(Beam beam in Beams)
            {
                beam.ShiftYsForBeamBlock(staffOriginY, _gap, _stemDirection, _beamThickness);
            }
        }
        /// <summary>
        /// The beamBlock has been vertically expanded. It is currently horizontal, and its outer edge is on the top staffline (at staffOriginY).
        /// This function moves the beamBlock vertically until it is on the right side (above or below) the noteheads, and as close as possible
        /// to the noteheads.
        /// If there is only a single (quaver) beam, it ends up with its inner edge one octave from the OriginY of the closest notehead in the group.
        /// Otherwise the smallest distance between any beam and the closest notehead will be a sixth.
        /// </summary>
        /// <returns></returns>
        private void MoveToNoteheads(List<ChordMetrics> chordsMetrics, Dictionary<DurationClass, double> durationClassBeamThicknesses)
        {
            double staffOriginY = Chords[0].Voice.Staff.Metrics.OriginY;

            double localGap = _gap;
            if(CSSObjectClass == CSSObjectClass.cautionaryBeamBlock)
            {
                localGap *= 1.35;
            }

            if(_stemDirection == VerticalDir.down)
            {
                double lowestBottomReStaff = double.MinValue;
                for(int i = 0; i < Chords.Count; ++i)
                {
                    ChordMetrics chordMetrics = chordsMetrics[i];
                    HeadMetrics bottomHeadMetrics = chordMetrics.BottomHeadMetrics;
                    double bottomNoteOriginYReStaff = bottomHeadMetrics.OriginY - staffOriginY;
                    double beamBottomReStaff = 0;
                    double beamThickness = durationClassBeamThicknesses[Chords[i].DurationClass];
                    if(bottomNoteOriginYReStaff < (localGap * -1)) // above the staff
                    {
                        if(Chords[i].DurationClass == DurationClass.quaver)
                        {
                            beamBottomReStaff = (localGap * 2) + beamThickness;
                        }
                        else
                        {
                            beamBottomReStaff = localGap + beamThickness;
                        }
                    }
                    else if(bottomNoteOriginYReStaff <= (localGap * 2)) // above the mid line
                    {
                        if(Chords[i].DurationClass == DurationClass.quaver)
                        {
                            beamBottomReStaff = bottomNoteOriginYReStaff + (localGap * 3.5F) + beamThickness;
                        }
                        else
                        {
                            beamBottomReStaff = bottomNoteOriginYReStaff + (localGap * 2.5F) + beamThickness;
                        }
                    }
                    else
                    {
                        beamBottomReStaff = bottomNoteOriginYReStaff + (localGap * 3F) + beamThickness;
                    }

                    lowestBottomReStaff = lowestBottomReStaff > beamBottomReStaff ? lowestBottomReStaff : beamBottomReStaff;
                }

                Move(lowestBottomReStaff);
            }
            else // stems up
            {
                double highestTopReStaff = double.MaxValue;
                for(int i = 0; i < Chords.Count; ++i)
                {
                    ChordMetrics chordMetrics = chordsMetrics[i];
                    HeadMetrics topHeadMetrics = chordMetrics.TopHeadMetrics;
                    double topNoteOriginYReStaff = topHeadMetrics.OriginY - staffOriginY;
                    double beamTopReStaff = 0;
                    double beamThickness = durationClassBeamThicknesses[Chords[i].DurationClass];

                    if(topNoteOriginYReStaff > (localGap * 5)) // below the staff
                    {
                        if(Chords[i].DurationClass == DurationClass.quaver)
                        {
                            beamTopReStaff = (localGap * 2) - beamThickness;
                        }
                        else
                        {
                            beamTopReStaff = (localGap * 3) - beamThickness;
                        }
                    }
                    else if(topNoteOriginYReStaff >= (localGap * 2)) // below the mid line
                    {
                        if(Chords[i].DurationClass == DurationClass.quaver)
                        {
                            beamTopReStaff = topNoteOriginYReStaff - (localGap * 3.5F) - beamThickness;
                        }
                        else
                        {
                            beamTopReStaff = topNoteOriginYReStaff - (localGap * 2.5F) - beamThickness;
                        }
                    }
                    else // above the mid-line
                    {
                        beamTopReStaff = topNoteOriginYReStaff - (localGap * 3F) - beamThickness;
                    }

                    highestTopReStaff = highestTopReStaff < beamTopReStaff ? highestTopReStaff : beamTopReStaff;
                }

                Move(highestTopReStaff);
            }
        }
        private Dictionary<DurationClass, double> GetBeamThicknessesPerDurationClass(HashSet<DurationClass> durationClasses)
        {
            Dictionary<DurationClass, double> btpdc = new Dictionary<DurationClass, double>();
            double thickness = 0;
            foreach(DurationClass dc in durationClasses)
            {
                switch(dc)
                {
                    case DurationClass.fiveFlags:
                        thickness = (4 * _beamSeparation) + _beamThickness;
                        btpdc.Add(DurationClass.fiveFlags, thickness);
                        break;
                    case DurationClass.fourFlags:
                        thickness = (3 * _beamSeparation) + _beamThickness;
                        btpdc.Add(DurationClass.fourFlags, thickness);
                        break;
                    case DurationClass.threeFlags:
                        thickness = (2 * _beamSeparation) + _beamThickness;
                        btpdc.Add(DurationClass.threeFlags, thickness);
                        break;
                    case DurationClass.semiquaver:
                        thickness = _beamSeparation + _beamThickness;
                        btpdc.Add(DurationClass.semiquaver, thickness);
                        break;
                    case DurationClass.quaver:
                        thickness = _beamThickness;
                        btpdc.Add(DurationClass.quaver, thickness);
                        break;
                }
            }
            return btpdc;
        }

        private void Shear(List<ChordMetrics> chordsMetrics, double rightBarlineX)
        {
            double tanAlpha = ShearAngle(chordsMetrics);
            double shearAxis = ShearAxis(chordsMetrics, tanAlpha);
            if(Beams.Count == 1 && (tanAlpha > 0.02 || tanAlpha < -0.02))
            {
                if(_stemDirection == VerticalDir.up)
                    Move(_gap * 0.75F);
                else
                    Move(-_gap * 0.75F);
            }
            foreach(ChordMetrics chordMetrics in chordsMetrics)
            {
                double width = chordMetrics.StemMetrics.OriginX - shearAxis;
                double stemX = chordMetrics.StemMetrics.OriginX;
                double dy = width * tanAlpha;
                foreach(Beam beam in Beams)
                {
                    if(beam is IBeamHook beamHook)
                    {
                        beamHook.ShearBeamHook(shearAxis, tanAlpha, stemX);
                    }
                    else
                    {
                        if(beam.LeftX == stemX)
                            beam.MoveYs(dy, 0F);
                        else if(beam.RightX == stemX || beam.RightX > rightBarlineX)
                            beam.MoveYs(0, dy);
                    }
                }
            }
            SetVerticalBounds();
            SetToStaff();
        }

        private double ShearAngle(List<ChordMetrics> chordsMetrics)
        {
            double tanAlpha = 0;
            if(chordsMetrics.Count > 1)
            {
                ChordMetrics leftChordMetrics = chordsMetrics[0];
                ChordMetrics rightChordMetrics = chordsMetrics[chordsMetrics.Count - 1];
                double height =
                        (((rightChordMetrics.TopHeadMetrics.OriginY + rightChordMetrics.BottomHeadMetrics.OriginY) / 2)
                       - ((leftChordMetrics.TopHeadMetrics.OriginY + leftChordMetrics.BottomHeadMetrics.OriginY) / 2));

                double width = rightChordMetrics.StemMetrics.OriginX - leftChordMetrics.StemMetrics.OriginX;
                tanAlpha = (height / width) / 3;

                if(tanAlpha > 0.10)
                    tanAlpha = 0.10;
                if(tanAlpha < -0.10)
                    tanAlpha = -0.10;
            }

            return tanAlpha;
        }

        /// </summary>
        /// <param name="chordDaten"></param>
        /// <returns></returns>
        private double ShearAxis(List<ChordMetrics> chordsMetrics, double tanAlpha)
        {
            List<double> innerNoteheadHeights = GetInnerNoteheadHeights(chordsMetrics);
            double smallestDistance = double.MaxValue;
            foreach(double distance in innerNoteheadHeights)
                smallestDistance = smallestDistance < distance ? smallestDistance : distance;

            List<int> indices = new List<int>();
            for(int i = 0; i < innerNoteheadHeights.Count; ++i)
            {
                if(innerNoteheadHeights[i] == smallestDistance)
                    indices.Add(i);
            }
            if((_stemDirection == VerticalDir.down && tanAlpha <= 0)
            || (_stemDirection == VerticalDir.up && tanAlpha > 0))
                return chordsMetrics[indices[indices.Count - 1]].StemMetrics.OriginX;
            else
                return chordsMetrics[indices[0]].StemMetrics.OriginX;
        }

        private List<double> GetInnerNoteheadHeights(List<ChordMetrics> chordsMetrics)
        {
            List<double> distances = new List<double>();
            if(_stemDirection == VerticalDir.down)
            {
                foreach(ChordMetrics chordMetrics in chordsMetrics)
                {
                    distances.Add(-(chordMetrics.BottomHeadMetrics.OriginY));
                }
            }
            else
            {
                foreach(ChordMetrics chordMetrics in chordsMetrics)
                {
                    distances.Add(chordMetrics.TopHeadMetrics.OriginY);
                }
            }
            return distances;
        }

        /// <summary>
        /// Resets the height of the beamBlock, if it is too high or too low.
        /// </summary>
        /// <param name="chordsMetrics"></param>
        private void SetToStaff()
        {
            double staffTopY = Chords[0].Voice.Staff.Metrics.OriginY;
            double staffBottomY = staffTopY + (_gap * (Chords[0].Voice.Staff.NumberOfStafflines - 1));
            double staffMiddleY = (staffTopY + staffBottomY) / 2;
            double deltaY = 0;
            if(this._stemDirection == VerticalDir.up)
            {
                if(Beams.Count == 1)
                {
                    deltaY = staffMiddleY + (_gap * 0.35F) - this._bottom;
                }
                else if(this._bottom >= staffBottomY)
                {
                    deltaY = staffMiddleY + (_gap * 1.35F) - this._bottom;
                }
                if(deltaY < 0F)
                    Move(deltaY);
            }
            else // this._stemDirection == VerticalDir.down
            {
                if(Beams.Count == 1)
                {
                    deltaY = staffMiddleY - (_gap * 0.35F) - this._top;
                }
                else if(this._top <= staffTopY)
                {
                    deltaY = staffMiddleY - (_gap * 1.35F) - this._top;
                }
                if(deltaY > 0F)
                    Move(deltaY);
            }
        }

        private void MoveStemTips()
        {
            double staffOriginY = Chords[0].Voice.Staff.Metrics.OriginY;
            QuaverBeam quaverBeam = null;
            foreach(Beam beam in Beams)
            {
                quaverBeam = beam as QuaverBeam;
                if(quaverBeam != null)
                    break;
            }
            M.Assert(quaverBeam != null);
            double tanAlpha = (quaverBeam.RightTopY - quaverBeam.LeftTopY) / (quaverBeam.RightX - quaverBeam.LeftX);

            foreach(ChordSymbol chord in Chords)
            {
                ChordMetrics chordMetrics = ((ChordMetrics)chord.Metrics);
                StemMetrics stemMetrics = chordMetrics.StemMetrics; // a clone

                M.Assert(chord.Stem.Direction == _stemDirection); // just to be sure.

                double stemTipDeltaY = ((stemMetrics.OriginX - this._left) * tanAlpha);
                double stemTipY = quaverBeam.LeftTopY + stemTipDeltaY;
                chordMetrics.MoveOuterStemTip(stemTipY, _stemDirection); // dont just move the clone! Moves the auxilliaries too.
            }
        }

        /// <summary>
        /// The tangent of this beam's angle. This value is positive if the beam slopes upwards to the right.
        /// </summary>
        /// <returns></returns>
        private double TanAngle
        {
            get
            {
                QuaverBeam qBeam = null;
                foreach(Beam beam in Beams)
                {
                    qBeam = beam as QuaverBeam;
                    if(qBeam != null)
                        break;
                }
                M.Assert(qBeam != null);
                double tan = ((qBeam.LeftTopY - qBeam.RightTopY) / (qBeam.RightX - qBeam.LeftX));
                return tan;
            }
        }
        /// <summary>
        /// Returns a list of HLine representing the outer edge of the outer (=quaver) beam.
        /// </summary>
        /// <returns></returns>
        public List<HLine> OuterEdge()
        {
            QuaverBeam qBeam = null;
            foreach(Beam beam in Beams)
            {
                qBeam = beam as QuaverBeam;
                if(qBeam != null)
                    break;
            }
            M.Assert(qBeam != null);

			List<HLine> outerEdge = new List<HLine>();
			double hlineY = 0;
			if(_stemDirection == VerticalDir.up)
				hlineY = qBeam.LeftTopY;
			else
				hlineY = qBeam.LeftTopY + _beamThickness;

			double heightDiff = qBeam.LeftTopY - qBeam.RightTopY;
			//double heightDiff = qBeam.RightTopY - qBeam.LeftTopY;
			double stepHeight = (_beamThickness * 0.2F);
			int nSteps = (int) Math.Round(heightDiff / stepHeight);
			if(nSteps == 0)
			{
				outerEdge.Add(new HLine(_left, _right, hlineY));
			}
			else
			{
				if(nSteps < 0)
					nSteps *= -1;
				stepHeight = heightDiff / nSteps;

				double stepWidth = (_right - _left) / nSteps;
				double left = _left;
				double tanAlpha = stepHeight / stepWidth;

				for(int i = 0; i < nSteps; i++)
				{
					outerEdge.Add(new HLine(left, left + stepWidth, hlineY));
					left += stepWidth;
					hlineY -= (stepWidth * tanAlpha);
				}
			}

            return outerEdge;
        }

        public void ShiftStemsForOtherVoice(Voice otherVoice)
        {
            double minMsPosition = Chords[0].AbsMsPosition;
            double maxMsPosition = Chords[Chords.Count - 1].AbsMsPosition;
            List<ChordSymbol> otherChords = new List<ChordSymbol>();
            foreach(ChordSymbol otherChordSymbol in otherVoice.ChordSymbols)
            {
                if(otherChordSymbol.AbsMsPosition >= minMsPosition && otherChordSymbol.AbsMsPosition <= maxMsPosition)
                    otherChords.Add(otherChordSymbol);
                if(otherChordSymbol.AbsMsPosition > maxMsPosition)
                    break;
            }
            if(otherChords.Count > 0)
            {
                double minimumDistanceToChords = _gap * 2;
                double distanceToChords = DistanceToChords(otherChords);
                if(_stemDirection == VerticalDir.up) // move the beam up
                {
                    if(distanceToChords < minimumDistanceToChords)
                    {
                        double deltaY = -(minimumDistanceToChords - distanceToChords);
                        this.Move(deltaY);
                        foreach(ChordSymbol chord in Chords)
                        {
                            double newStemTipY = chord.ChordMetrics.StemMetrics.Top + deltaY;
                            chord.ChordMetrics.MoveOuterStemTip(newStemTipY, VerticalDir.up);
                        }
                    }
                }
                else // _stemDirection == VerticalDir.down, move the beam down
                {
                    if(distanceToChords < minimumDistanceToChords)
                    {
                        double deltaY = minimumDistanceToChords - distanceToChords;
                        this.Move(deltaY);
                        foreach(ChordSymbol chord in Chords)
                        {
                            double newStemTipY = chord.ChordMetrics.StemMetrics.Bottom + deltaY;
                            chord.ChordMetrics.MoveOuterStemTip(newStemTipY, VerticalDir.down);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the smallest distance between the inner edge of this beam and the noteheads
        /// in the other chords. The other chords are in a second voice on this staff.
        /// </summary>
        private double DistanceToChords(List<ChordSymbol> otherChords)
        {
            double minimumDistanceToChords = double.MaxValue;
            foreach(ChordSymbol chord in otherChords)
            {
                double distanceToChord = double.MaxValue;
                if(_stemDirection == VerticalDir.up)
                    distanceToChord = VerticalDistanceToHead(chord.ChordMetrics.TopHeadMetrics, chord.AbsMsPosition);
                else
                    distanceToChord = VerticalDistanceToHead(chord.ChordMetrics.BottomHeadMetrics, chord.AbsMsPosition);

                minimumDistanceToChords = minimumDistanceToChords < distanceToChord ? minimumDistanceToChords : distanceToChord;
            }
            return minimumDistanceToChords;
        }

        /// <summary>
        /// The distance between the inner edge of this beamBlock and the headmetrics.
        /// This is a positive value
        ///     a) if the _stemDirection is VerticalDir.up and the headMetrics is completely below this beamBlock,
        /// or  b) if the _stemDirection is VerticalDir.down and the headMetrics is completely above this beamBlock.
        /// </summary>
        private double VerticalDistanceToHead(HeadMetrics headMetrics, double headMsPosition)
        {
            double headX = headMetrics.OriginX;
            double headY = headMetrics.Top;
            double minDistanceToHead = double.MaxValue;
            double tanA = this.TanAngle;
            if(_stemDirection == VerticalDir.up)
            {
                double beamBottomAtHeadY;
                foreach(Beam beam in Beams)
                {
                    double beamBeginMsPosition = BeamBeginMsPosition(beam);
                    double beamEndMsPosition = BeamEndMsPosition(beam);
                    if(beamBeginMsPosition <= headMsPosition && beamEndMsPosition >= headMsPosition)
                    {
                        beamBottomAtHeadY = beam.LeftTopY - ((headX - beam.LeftX) * tanA) + _beamThickness;
                        double distanceToHead = headY - beamBottomAtHeadY;
                        minDistanceToHead = minDistanceToHead < distanceToHead ? minDistanceToHead : distanceToHead;
                    }
                }
            }
            else // _stemDirection == VerticalDir.down
            {
                headY = headMetrics.Bottom;
                double beamTopAtHeadY;
                foreach(Beam beam in Beams)
                {
                    double beamBeginMsPosition = BeamBeginMsPosition(beam);
                    double beamEndMsPosition = BeamEndMsPosition(beam);
                    if(beamBeginMsPosition <= headMsPosition && beamEndMsPosition >= headMsPosition)
                    {
                        beamTopAtHeadY = beam.LeftTopY - ((headX - beam.LeftX) * tanA);
                        double distanceToHead = beamTopAtHeadY - headY;
                        minDistanceToHead = minDistanceToHead < distanceToHead ? minDistanceToHead : distanceToHead;
                    }
                }
            }
            return minDistanceToHead;
        }

        private double BeamBeginMsPosition(Beam beam)
        {
            M.Assert(this.Beams.Contains(beam));
            double beamBeginAbsMsPosition = double.MinValue;
            foreach(ChordSymbol chord in Chords)
            {
                double stemX = chord.ChordMetrics.StemMetrics.OriginX;
                if(stemX == beam.LeftX || stemX == beam.RightX) // rightX can be a beam hook
                {
                    beamBeginAbsMsPosition = chord.AbsMsPosition;
                    break;
                }
            }
            M.Assert(beamBeginAbsMsPosition != double.MinValue);
            return beamBeginAbsMsPosition;
        }

        private double BeamEndMsPosition(Beam beam)
        {
            M.Assert(this.Beams.Contains(beam));
            double beamEndAbsMsPosition = double.MinValue;
            for(int i = Chords.Count - 1; i >= 0; --i)
            {
                ChordSymbol chord = Chords[i];
                double stemX = chord.ChordMetrics.StemMetrics.OriginX;
                if(stemX == beam.LeftX || stemX == beam.RightX) // rightX can be a beam hook
                {
                    beamEndAbsMsPosition = chord.AbsMsPosition;
                    break;
                }
            }
            M.Assert(beamEndAbsMsPosition != double.MinValue);
            return beamEndAbsMsPosition;
        }

        public override void WriteSVG(SvgWriter w)
        {
            bool isCautionary = (CSSObjectClass == CSSObjectClass.cautionaryBeamBlock);

            w.SvgStartGroup(CSSObjectClass.ToString());
            foreach(Beam beam in Beams)
            {
                if(!(beam is QuaverBeam))
                {
                    // draw an opaque beam between the other beams
                    double topLeft = 0;
                    double topRight = 0;
                    if(_stemDirection == VerticalDir.down)
                    {
                        topLeft = beam.LeftTopY + _beamThickness;
                        topRight = beam.RightTopY + _beamThickness;
                    }
                    else
                    {
                        topLeft = beam.LeftTopY - _beamThickness;
                        topRight = beam.RightTopY - _beamThickness;
                    }
                    
                    w.SvgBeam(beam.LeftX, beam.RightX, topLeft, topRight, _beamThickness * 1.5, true, isCautionary);
                }
                w.SvgBeam(beam.LeftX, beam.RightX, beam.LeftTopY, beam.RightTopY, _beamThickness, false, isCautionary);
            }
            w.SvgEndGroup();
        }

        public readonly List<ChordSymbol> Chords = null;
        public double DefaultStemTipY { get { return _defaultStemTipY; } }
        private readonly double _defaultStemTipY;
        private readonly double _gap;
        private readonly double _beamSeparation; // the distance from the top of one beam to the top of the next beam
        private readonly double _beamThickness;
        private readonly double _strokeThickness;
        private readonly VerticalDir _stemDirection = VerticalDir.none;

        public HashSet<Beam> Beams = new HashSet<Beam>();
    }
}
