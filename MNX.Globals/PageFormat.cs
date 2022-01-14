
using System.Collections.Generic;

namespace MNX.Globals
{
    /// <summary>
    /// Public values are in viewbox pixel units.
    /// </summary>
    public class PageFormat
    {
        #region constants
        public readonly int ViewBoxMagnification = 10;

        // The relative size of cautionary and small objects
        public double SmallSizeFactor { get { return 0.8; } }
        // The opacity of opaque beams
        // (Opaque beams are written between the beam and stafflines to make the stafflines appear grey.)
        public double OpaqueBeamOpacity { get { return 0.65; } }

        public int DefaultNumberOfBarsPerSystem { get { return 5; } }
        #endregion

        #region Attributes set by constructor
        public readonly int RightVBPX = 0;
        public int BottomVBPX = 0; // is reset for scroll scores
        public int TopMarginPage1VBPX; // is reset for scroll scores
        public readonly int TopMarginOtherPagesVBPX;
        public readonly int RightMarginPosVBPX;
        public readonly int LeftMarginPosVBPX;
        public int BottomMarginPosVBPX; // is reset for scroll scores
        public readonly double StafflineStemStrokeWidthVBPX;
        public readonly double GapVBPX;
        public readonly double DefaultDistanceBetweenStavesVBPX; // The distance between staves when they are not vertically justified.
        public readonly double DefaultDistanceBetweenSystemsVBPX; // The view box pixel distance between systems when they are not vertically justified.
        public readonly double MillisecondsPerTick = 0;
        public readonly IReadOnlyList<int> SystemStartBars = null;
        public readonly IReadOnlyList<IReadOnlyList<int>> MIDIChannelsPerStaff = null;
        public readonly IReadOnlyList<int> NumberOfStavesPerPart = null;

        #endregion Attributes set by contructor

        #region derived attributes
        #region font heights
        /// <summary>
        /// the normal font size on staves having Gap sized spaces (after experimenting with cLicht). 
        /// </summary>
        public double MusicFontHeight { get { return (GapVBPX * 4) * 0.98; } }
        /// Arial (new 26.06.2017)
        public double TimeStampFontHeight { get { return 15 * ViewBoxMagnification; } } // was GapVBPX * 2.5
        public double StaffNameFontHeight { get { return GapVBPX * 2.2; } }
        public double BarNumberNumberFontHeight { get { return GapVBPX * 1.9992; } }
        public double RegionInfoStringFontHeight { get { return GapVBPX * 3; } }
        public double LyricFontHeight { get { return GapVBPX * 1.96; } }
        public double ClefOctaveNumberHeight { get { return GapVBPX * 2.6264; } }
        public double ClefXFontHeight { get { return GapVBPX * 1.568; } }
        public double TimeSignatureComponentFontHeight { get { return GapVBPX * 4.2; } }
        /// Open Sans, Open Sans Condensed (new 26.06.2017)
        public double OrnamentFontHeight { get { return GapVBPX * 2.156; } }
        /// CLicht (new 26.06.2017)
        public double DynamicFontHeight { get { return MusicFontHeight * 0.75; } }
        public double RepeatTimesStringFontHeight { get { return (GapVBPX * 1.8); } }
        #endregion

        #region stroke widths
        public double NormalBarlineStrokeWidth { get { return StafflineStemStrokeWidthVBPX * 2; } }
        public double ThinBarlineStrokeWidth { get { return NormalBarlineStrokeWidth / 2; } } // a component of double barlines.
        public double ThickBarlineStrokeWidth { get { return NormalBarlineStrokeWidth * 2; } } // a component of double barlines.
        public double NoteheadExtenderStrokeWidth { get { return StafflineStemStrokeWidthVBPX * 3.4; } }
        public double BarNumberFrameStrokeWidth { get { return StafflineStemStrokeWidthVBPX * 1.2; } }
        public double RegionInfoFrameStrokeWidth { get { return BarNumberFrameStrokeWidth * 1.5; } }
        public double BeamThickness { get { return GapVBPX * 0.42; } }
        public double TupletFontHeight { get { return GapVBPX * 1.8; } }
        public double TupletBracketStrokeWidth { get { return StafflineStemStrokeWidthVBPX; } }
        #endregion

        #region frame
        public double LeftScreenMarginPos { get { return LeftMarginPosVBPX / ViewBoxMagnification; } }
        public int FirstPageFrameHeight { get { return BottomMarginPosVBPX - TopMarginPage1VBPX; } }
        public int OtherPagesFrameHeight { get { return BottomMarginPosVBPX - TopMarginOtherPagesVBPX; } }
        #endregion

        /// <summary>
        /// A list having one value per staff in the system
        /// </summary>
        public List<bool> BarlineContinuesDownList
        {
            get
            {
                List<bool> barlineContinuesDownPerStaff = new List<bool>();
                foreach(var nStaves in NumberOfStavesPerPart)
                {
                    for(var i = 0; i < nStaves; i++)
                    {
                        bool barlineContinuesDown = ((i + 1) < nStaves) ? true : false;
                        barlineContinuesDownPerStaff.Add(barlineContinuesDown);
                    }
                }
                return barlineContinuesDownPerStaff;
            }
        }
        #endregion

        #region other attributes
        #region page 1 titles
        public readonly double Page1TitleHeightVBPX;
        public readonly double Page1AuthorHeightVBPX;
        public readonly double Page1TitleYVBPX;
        #endregion
        public readonly IReadOnlyList<string> LongStaffNames = null;
        public readonly IReadOnlyList<string> ShortStaffNames = null;
        public readonly IReadOnlyList<int> StafflinesPerStaff = null;

        /// <summary>
        /// (Moritz' other types are not used)
        /// </summary>
        public string ChordSymbolType { get { return "standard"; } }

        // See OctaveShiftExtender constructor
        public string OctaveShiftExtenderTextFontFamily { get { return "Arial"; } }
        public double OctaveShiftExtenderTextFontHeight { get { return (GapVBPX * 1.5); } }
        public string OctaveShiftExtenderTextFontWeight { get { return "bold"; } }  // SVGFontWeight.bold.ToString()
        public string OctaveShiftExtenderTextFontStyle { get { return "italic"; } } // SVGFontStyle.italic.ToString()
        public double OctaveShiftExtenderLineStrokeWidth { get { return (StafflineStemStrokeWidthVBPX * 1.2); } }

        #endregion other attributes

        #region unused Moritz option
        /// <summary>
        /// Moritz uses this value to decide the duration class of a DurationSymbol.
        /// MNXtoSVG uses XML to construct duration classes... and then sets Ticks...
        /// </summary>
        public int MinimumCrotchetDuration;
        #endregion

        public PageFormat(Form1Data form1Data, IReadOnlyList<IReadOnlyList<int>> voicesPerStaffPerPart)
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

            var numberOfStavesPerPart = new List<int>();
            int nParts = voicesPerStaffPerPart.Count;
            for(var partIndex = 0; partIndex < nParts; partIndex++)
            {
                numberOfStavesPerPart.Add(voicesPerStaffPerPart[partIndex].Count);
            }
            NumberOfStavesPerPart = numberOfStavesPerPart;

            var midiChannelsPerStaff = new List<List<int>>();
            int midiChannel = 0;
            for(var partIndex = 0; partIndex < nParts; partIndex++)
            {
                for(var staffIndex = 0; staffIndex < voicesPerStaffPerPart[partIndex].Count; staffIndex++)
                {
                    var nVoicesInStaff = voicesPerStaffPerPart[partIndex][staffIndex];
                    List<int> midiChannels = new List<int>();
                    for(var v = 0; v < nVoicesInStaff; v++)
                    {
                        midiChannels.Add(midiChannel++);
                    }
                    midiChannelsPerStaff.Add(midiChannels);
                }
            }
            MIDIChannelsPerStaff = midiChannelsPerStaff;

            int nStaves = MIDIChannelsPerStaff.Count;
            var longStaffNames = new List<string>();
            var shortStaffNames = new List<string>();
            var stafflinesPerStaff = new List<int>();
            for(var i = 0; i < nStaves; i++)
            {
                longStaffNames.Add("");
                shortStaffNames.Add("");
                stafflinesPerStaff.Add(5);
            }
            LongStaffNames = (IReadOnlyList<string>)longStaffNames;
            ShortStaffNames = (IReadOnlyList<string>)shortStaffNames;
            StafflinesPerStaff = (IReadOnlyList<int>)stafflinesPerStaff;
        }
    }
}
