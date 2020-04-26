﻿
using System.Collections.Generic;
using MNX.Globals;

namespace Moritz.Symbols
{
    /// <summary>
    /// Public values are in viewbox pixel units.
    /// </summary>
    public class PageFormat
    {
        public readonly int ViewBoxMagnification = 10;

        #region Attributes set by constructor
        public int RightVBPX = 0;
        public int BottomVBPX = 0;
        public int TopMarginPage1VBPX;
        public int TopMarginOtherPagesVBPX;
        public int RightMarginPosVBPX;
        public int LeftMarginPosVBPX;
        public int BottomMarginPosVBPX;
        public double StafflineStemStrokeWidthVBPX;
        public double GapVBPX;
        public double DefaultDistanceBetweenStavesVBPX; // The distance between staves when they are not vertically justified.
        public double DefaultDistanceBetweenSystemsVBPX; // The view box pixel distance between systems when they are not vertically justified.
        public List<int> SystemStartBars = null;
        public double MillisecondsPerTick = 0;

        public IReadOnlyList<IReadOnlyList<int>> MIDIChannelsPerStaff = null;

        #endregion Attributes set by contructor

        #region derived attributes
        #endregion

        #region other attributes
        #region page 1 titles
        public double Page1TitleHeightVBPX;
        public double Page1AuthorHeightVBPX;
        public double Page1TitleYVBPX;
        #endregion
        #endregion other attributes

        public PageFormat(Form1Data form1Data, IReadOnlyList<IReadOnlyList<int>> midiChannelsPerStaff)
        {
            RightVBPX = form1Data.Page.Width * ViewBoxMagnification;
            BottomVBPX = form1Data.Page.Height * ViewBoxMagnification;
            TopMarginPage1VBPX = form1Data.Page.MarginTopPage1 * ViewBoxMagnification;
            TopMarginOtherPagesVBPX = form1Data.Page.MarginTopOther * ViewBoxMagnification;
            RightMarginPosVBPX = RightVBPX - (form1Data.Page.MarginRight * ViewBoxMagnification);
            LeftMarginPosVBPX = form1Data.Page.MarginLeft * ViewBoxMagnification;
            BottomMarginPosVBPX = BottomVBPX - (form1Data.Page.MarginBottom * ViewBoxMagnification);

            StafflineStemStrokeWidthVBPX = form1Data.Notation.StafflineStemStrokeWidth * ViewBoxMagnification;
            GapVBPX = form1Data.Notation.Gap * ViewBoxMagnification;

            DefaultDistanceBetweenStavesVBPX = form1Data.Notation.Gap * form1Data.Notation.MinGapsBetweenStaves * ViewBoxMagnification;
            DefaultDistanceBetweenSystemsVBPX = form1Data.Notation.Gap * form1Data.Notation.MinGapsBetweenSystems * ViewBoxMagnification;
 
            SystemStartBars = form1Data.Notation.SystemStartBars;

            MillisecondsPerTick = 60000 / (M.TicksPerCrotchet * form1Data.Notation.CrotchetsPerMinute);

            MIDIChannelsPerStaff = midiChannelsPerStaff;
        }

        #region frame
        public double LeftScreenMarginPos { get { return LeftMarginPosVBPX / ViewBoxMagnification; } }
        public int FirstPageFrameHeight { get { return BottomMarginPosVBPX - TopMarginPage1VBPX; } }
        public int OtherPagesFrameHeight { get { return BottomMarginPosVBPX - TopMarginOtherPagesVBPX; } }

        #endregion

        #region notation
        /// <summary>
        /// All written scores set the ChordSymbolType to one of the values in M.ChordTypes.
        /// M.ChordTypes does not include "none", because it is used to populate ComboBoxes.
        /// The value "none" is used to signal that there is no written score. It is used by
        /// AudioButtonsControl inside palettes, just to play a sound.
        /// </summary>
        public string ChordSymbolType = "none";
        #region standard chord notation options
        /// <summary>
        /// Moritz uses this value to decide the duration class of a DurationSymbol
        /// </summary>
        public int MinimumCrotchetDuration;
        public bool BeamsCrossBarlines;
        #endregion


        
		public List<string> ClefPerStaff = null;
		public List<string> InitialClefPerMIDIChannel = null;
		public List<int> StafflinesPerStaff = null;
        public List<int> StaffGroups = null;
        public List<string> LongStaffNames = null;
        public List<string> ShortStaffNames = null;
        private Form1Data form1Data;
        private IReadOnlyList<IReadOnlyList<int>> midiChannelsPerStaff;

        public int DefaultNumberOfBarsPerSystem { get { return 5; } }

        

        #region constants
        // The relatve size of cautionary and small objects
        public double SmallSizeFactor { get { return 0.8; } }
        // The opacity of opaque beams
        // (Opaque beams are written between the beam and stafflines to make the stafflines appear grey.)
        public double OpaqueBeamOpacity { get { return 0.65; } }
        #endregion

  
        #region font heights
        /// <summary>
        /// the normal font size on staves having Gap sized spaces (after experimenting with cLicht). 
        /// </summary>
        public double MusicFontHeight { get { return (GapVBPX * 4) * 0.98; } }
        /// Arial (new 26.06.2017)
        public double TimeStampFontHeight { get { return GapVBPX * 2.25; } }
        public double StaffNameFontHeight { get { return GapVBPX * 2.2; } }
		public double BarNumberNumberFontHeight { get { return GapVBPX * 1.9992; } }
		public double RegionInfoStringFontHeight { get { return GapVBPX * 3; } }
		public double LyricFontHeight { get { return GapVBPX * 1.96; } }
        public double ClefOctaveNumberHeight { get { return GapVBPX * 2.6264; } }
        public double ClefXFontHeight { get { return GapVBPX * 1.568; } }
		/// Open Sans, Open Sans Condensed (new 26.06.2017)
		public double OrnamentFontHeight { get { return GapVBPX * 2.156; } }
		/// CLicht (new 26.06.2017)
		public double DynamicFontHeight { get { return MusicFontHeight * 0.75; } }
        #endregion

        #region stroke widths

        public double NormalBarlineStrokeWidth { get { return StafflineStemStrokeWidthVBPX * 2; } }
		public double ThinBarlineStrokeWidth { get { return NormalBarlineStrokeWidth / 2; } } // a component of double barlines.
		public double ThickBarlineStrokeWidth { get { return NormalBarlineStrokeWidth * 2; } } // a component of double barlines.
		public double NoteheadExtenderStrokeWidth { get { return StafflineStemStrokeWidthVBPX * 3.4; } }
		public double BarNumberFrameStrokeWidth { get { return StafflineStemStrokeWidthVBPX * 1.2; } }
		public double RegionInfoFrameStrokeWidth { get { return BarNumberFrameStrokeWidth * 1.5; } }
		#endregion

        public double BeamThickness { get { return GapVBPX * 0.42; } }

        #endregion

        #region derived properties
        /// <summary>
        /// A list having one value per staff in the system
        /// </summary>
        public List<bool> BarlineContinuesDownList
        {
            get
            {
                List<bool> barlineContinuesDownPerStaff = new List<bool>();
                foreach(int nStaves in StaffGroups)
                {
                    int remainingStavesInGroup = nStaves - 1;
                    while(remainingStavesInGroup > 0)
                    {
                        barlineContinuesDownPerStaff.Add(true);
                        --remainingStavesInGroup;
                    }
                    barlineContinuesDownPerStaff.Add(false);
                }
                return barlineContinuesDownPerStaff;
            }
        }
        #endregion derived properties

    }
}