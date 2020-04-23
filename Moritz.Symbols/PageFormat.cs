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
        public PageFormat()
        {
        }

        public PageFormat(SVGData svgData, string page1Title, string page1Author)
        {
            RightVBPX = svgData.pageWidth * ViewBoxMagnification;
            BottomVBPX = svgData.pageHeight * ViewBoxMagnification;
            TopMarginPage1 = svgData.marginTopPage1 * ViewBoxMagnification;
            TopMarginOtherPages = svgData.marginTopOther * ViewBoxMagnification;
            RightMarginPos = RightVBPX - (svgData.marginRight * ViewBoxMagnification);
            LeftMarginPos = svgData.marginLeft * ViewBoxMagnification;
            BottomMarginPos = BottomVBPX - (svgData.marginBottom * ViewBoxMagnification);

            StafflineStemStrokeWidth = svgData.stafflineStemStrokeWidth;


            Page1Title = page1Title;
            Page1Author = page1Author;

            DefaultDistanceBetweenStaves = svgData.gap * svgData.minGapsBetweenStaves * ViewBoxMagnification;
            DefaultDistanceBetweenSystems = svgData.gap * svgData.minGapsBetweenSystems * ViewBoxMagnification;

            SystemStartBars = svgData.systemStartBars;

            CrotchetsPerMinute = svgData.crotchetsPerMinute;

        }

        public readonly int ViewBoxMagnification = 10;

        #region paper size
        public double Right { get { return RightVBPX; } }
        public double Bottom 
        { 
            get 
            { 
                int nGaps = (int)( BottomVBPX / Gap);
                return nGaps * Gap;
            }
        }
        public double ScreenRight { get { return RightVBPX / ViewBoxMagnification; } }
        public double ScreenBottom { get { return BottomVBPX / ViewBoxMagnification; } }

        public string PaperSize; // default
        public bool IsLandscape = false;
        public int RightVBPX = 0;
        public int BottomVBPX = 0;
        public readonly double HorizontalPixelsPerMillimeter = 3.4037F; // on my computer (December 2010).
        public readonly double VerticalPixelsPerMillimeter = 2.9464F; // on my computer (December 2010).
		#endregion

		#region page 1 titles
		public string Page1Title;
		public string Page1Author;
        public double Page1ScreenTitleY { get { return Page1TitleY / ViewBoxMagnification; } }
        public double Page1TitleHeight;
        public double Page1AuthorHeight;
        public double Page1TitleY;
        #endregion

        #region frame
        public double LeftScreenMarginPos { get { return LeftMarginPos / ViewBoxMagnification; } }
        public int FirstPageFrameHeight { get { return BottomMarginPos - TopMarginPage1; } }
        public int OtherPagesFrameHeight { get { return BottomMarginPos - TopMarginOtherPages; } }
        public int TopMarginPage1;
        public int TopMarginOtherPages;
        public int RightMarginPos;
        public int LeftMarginPos;
        public int BottomMarginPos;
        #endregion

        #region website links
        /// <summary>
        /// the text of the link to the "about" file
        /// </summary>
        public string AboutLinkText;
        /// <summary>
        /// the "about" file's URL.
        /// </summary>
        public string AboutLinkURL;
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
        /// This value is used to find the duration class of DurationSymbols
        /// </summary>
        public int MinimumCrotchetDuration;
        public bool BeamsCrossBarlines;
        #endregion

        /// <summary>
        /// The view box pixel distance between staves when they are not vertically justified.
        /// </summary>
        public double DefaultDistanceBetweenStaves;
        /// <summary>
        /// The view box pixel distance between systems when they are not vertically justified.
        /// </summary>
        public double DefaultDistanceBetweenSystems;
        public List<List<byte>> OutputMIDIChannelsPerStaff = null;
		public List<string> ClefPerStaff = null;
		public List<string> InitialClefPerMIDIChannel = null;
		public List<int> StafflinesPerStaff = null;
        public List<int> StaffGroups = null;
        public List<string> LongStaffNames = null;
        public List<string> ShortStaffNames = null;

        public List<int> SystemStartBars = null;
        public int DefaultNumberOfBarsPerSystem { get { return 5; } }

        public double CrotchetsPerMinute = 0;

        #region constants
        public double SmallSizeFactor { get { return 0.8F; } } // The relatve size of cautionary and small objects
        public double OpaqueBeamOpacity { get { return 0.65F; } } // The opacity of opaque beams
        #endregion

        public double Gap;
        #region font heights
        /// <summary>
        /// the normal font size on staves having Gap sized spaces (after experimenting with cLicht). 
        /// </summary>
        public double MusicFontHeight { get { return (Gap * 4) * 0.98F; } }
        /// Arial (new 26.06.2017)
        public double TimeStampFontHeight { get { return Gap * 2.25F; } }
        public double StaffNameFontHeight { get { return Gap * 2.2F; } }
		public double BarNumberNumberFontHeight { get { return Gap * 1.9992F; } }
		public double RegionInfoStringFontHeight { get { return Gap * 3F; } }
		public double LyricFontHeight { get { return Gap * 1.96F; } }
        public double ClefOctaveNumberHeight { get { return Gap * 2.6264F; } }
        public double ClefXFontHeight { get { return Gap * 1.568F; } }
		/// Open Sans, Open Sans Condensed (new 26.06.2017)
		public double OrnamentFontHeight { get { return Gap * 2.156F; } }
		/// CLicht (new 26.06.2017)
		public double DynamicFontHeight { get { return MusicFontHeight * 0.75F; } }
        #endregion

        #region stroke widths
        public double StafflineStemStrokeWidth;
        private SVGData svgData;

        public double NormalBarlineStrokeWidth { get { return StafflineStemStrokeWidth * 2F; } }
		public double ThinBarlineStrokeWidth { get { return NormalBarlineStrokeWidth / 2; } } // a component of double barlines.
		public double ThickBarlineStrokeWidth { get { return NormalBarlineStrokeWidth * 2; } } // a component of double barlines.
		public double NoteheadExtenderStrokeWidth { get { return StafflineStemStrokeWidth * 3.4F; } }
		public double BarNumberFrameStrokeWidth { get { return StafflineStemStrokeWidth * 1.2F; } }
		public double RegionInfoFrameStrokeWidth { get { return BarNumberFrameStrokeWidth * 1.5F; } }
		#endregion

        public double BeamThickness { get { return Gap * 0.42F; } }

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
