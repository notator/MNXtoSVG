using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using MNX.Globals;
using MNX.Common;


using Moritz.Spec;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public class SvgScore
    {
        #region fields

        #region constructor
        internal readonly string TargetFolder = null;
        internal readonly string FileNameWithoutExtension = null; // The base file name with ".svg" (appears in info string at top of each page)
        internal Form1OptionsData Form1Options = null;

        #endregion constructor

        #region subclass constructor
        public MetadataWithDate MetadataWithDate = null;
        #endregion

        public Notator Notator = null;
        public int PageCount { get { return _pages.Count; } }
        protected ScoreData ScoreData = null;
        protected List<SvgPage> _pages = new List<SvgPage>();

        #endregion fields

        /// <param name="targetFolder">The complete path to the folder that will contain the output file.</param>
        /// <param name="targetFilenameWithoutExtension">The file name for the score, without '.svg' extension.</param>
        public SvgScore(string targetFolder, string targetFilenameWithoutExtension, Form1OptionsData form1Options)
        {
            M.CreateDirectoryIfItDoesNotExist(targetFolder);
            TargetFolder = targetFolder;
            FileNameWithoutExtension = targetFilenameWithoutExtension;
            Form1Options = form1Options;
        }

        #region functions

        #region save multi-page score
        /// <summary>
        /// Silently overwrites the .html and all current .svg pages.
        /// An SVGScore consists of an .html file which references one .svg file per page of the score.
        /// When graphicsOnly is true, the following are omitted (for ease of use in CorelDraw):
        ///     a) the metadata element, and all its required namespaces,
        ///     b) the score namespace, and all its enclosed (temporal and alignment) information.
        /// </summary>
        /// <returns>The path to an HTML file containing the SVGs</returns>
        public string SaveMultiPageScore(bool graphicsOnly, bool printTitleAndAuthorOnPage1)
        {
            if(printTitleAndAuthorOnPage1)
            {
                M.PageFormat.TopMarginPage1VBPX = M.PageFormat.TopMarginOtherPagesVBPX;
            }
            List<string> svgPagenames = SaveSVGPages(graphicsOnly, printTitleAndAuthorOnPage1);

            string filePath = TargetFolder + "/" + FileNameWithoutExtension + ".html";

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                CloseOutput = true,
                NewLineOnAttributes = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                Encoding = Encoding.GetEncoding("utf-8")
            };

            using(XmlWriter w = XmlWriter.Create(filePath, settings))
            {
                w.WriteDocType("html", null, null, null);
                w.WriteStartElement("html", "http://www.w3.org/1999/xhtml");
                w.WriteAttributeString("lang", "en");
                w.WriteAttributeString("xml", "lang", null, "en");

                WriteHTMLScoreHead(w, Path.GetFileNameWithoutExtension(filePath));

                w.WriteStartElement("body");
                w.WriteStartElement("div");
                string styleString = "position:relative; text-align: left; top: 0px; padding-top: 0px; margin-top: 0px; width: " +
                    M.PageFormat.BottomVBPX.ToString();
                w.WriteAttributeString("style", styleString);

                w.WriteStartElement("div");
                w.WriteAttributeString("class", "svgPages");
                w.WriteAttributeString("style", "line-height: 0px;");

                foreach(string svgPagename in svgPagenames)
                {
                    w.WriteStartElement("embed");
                    w.WriteAttributeString("src", svgPagename);
                    w.WriteAttributeString("content-type", "image/svg+xml");
                    w.WriteAttributeString("class", "svgPage");
                    w.WriteAttributeString("width", M.DoubleToShortString(M.PageFormat.RightVBPX / M.PageFormat.ViewBoxMagnification));
                    w.WriteAttributeString("height", M.DoubleToShortString(M.PageFormat.BottomVBPX / M.PageFormat.ViewBoxMagnification));
                    w.WriteEndElement();
                    w.WriteStartElement("br");
                    w.WriteEndElement();
                }

                w.WriteEndElement(); // end svgPages div element

                w.WriteEndElement(); // end centredReferenceDiv div element

                w.WriteEndElement(); // end body element
                w.WriteEndElement(); // end html element

                w.Close(); // close actually unnecessary because of the using statement.
            }

            return filePath; // html
        }

        private void WriteHTMLScoreHead(XmlWriter w, string title)
        {
            w.WriteStartElement("head");
            w.WriteStartElement("title");
            w.WriteString(title);
            w.WriteEndElement(); // title
            w.WriteEndElement(); // head
        }

        private List<string> SaveSVGPages(bool graphicsOnly, bool printTitleAndAuthorOnPage1)
        {
            CreatePages(graphicsOnly);

            List<string> pageNames = new List<string>();

            int pageNumber = 1;
            foreach(SvgPage page in _pages)
            {
                string pagePath = GetSVGFilePath(pageNumber);
                string pageFilename = Path.GetFileName(pagePath);

                pageNames.Add(pageFilename);

                SaveSVGPage(pagePath, page, this.MetadataWithDate, false, graphicsOnly, printTitleAndAuthorOnPage1);
                pageNumber++;
            }

            return pageNames;
        }

        /// <summary>
        /// Puts up a Warning MessageBox, and returns false if systems cannot be fit
        /// vertically on the page. Otherwise true.
        /// </summary>
        protected bool CreatePages(bool graphicsOnly)
        {
            bool success = true;
            int pageNumber = 1;
            int systemIndex = 0;
            while(systemIndex < Systems.Count)
            {
                int oldSystemIndex = systemIndex;
                SvgPage newPage = NewSvgPage(GetSVGFilePath(pageNumber), pageNumber++, ref systemIndex, graphicsOnly);
                if(oldSystemIndex == systemIndex)
                {
                    MessageBox.Show("The systems are too high for the page height.\n\n" +
                        "Reduce the height of the systems, or increase the page height.",
                        "Height Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    success = false;
                    break;
                }
                _pages.Add(newPage);
            }
            return success;
        }

        internal void WriteScoreData(SvgWriter w)
        {
            if(ScoreData != null)
            {
                this.ScoreData.WriteSVG(w);
            }
        }

        /// <summary>
        /// Writes an SVG file containing one page of the score.
        /// </summary>
        public void SaveSVGPage(string pagePath, SvgPage page, MetadataWithDate metadataWithDate, bool isSinglePageScore, bool graphicsOnly, bool printTitleAndAuthorOnScorePage1)
        {
            if(File.Exists(pagePath))
            {
                File.Delete(pagePath);
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                CloseOutput = true,
                NewLineOnAttributes = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                Encoding = Encoding.GetEncoding("utf-8")
            };

            using(SvgWriter w = new SvgWriter(pagePath, settings))
            {
                page.WriteSVG(w, metadataWithDate, isSinglePageScore, graphicsOnly, printTitleAndAuthorOnScorePage1);
            }
        }

        public void WriteDefs(SvgWriter w, int pageNumber)
        {
            M.Assert(Notator != null);
            w.SvgStartDefs(null);
            WriteStyle(w, pageNumber);
            Notator.SymbolSet.WriteSymbolDefinitions(w, M.PageFormat);
            w.SvgEndDefs(); // end of defs
        }

        private void WriteStyle(SvgWriter w, int pageNumber)
        {
            StringBuilder css = GetStyles(M.PageFormat, pageNumber);

            w.WriteStartElement("style");
            w.WriteAttributeString("type", "text/css");
            w.WriteCData(css.ToString());
            w.WriteEndElement();
        }

        private StringBuilder GetStyles(PageFormat pageFormat, int pageNumber)
        {

            List<CSSObjectClass> usedCSSObjectClasses = new List<CSSObjectClass>(Metrics.UsedCSSObjectClasses);
            List<CSSColorClass> usedCSSColorClasses = new List<CSSColorClass>(Metrics.UsedCSSColorClasses);
            List<ClefID> usedClefIDs = new List<ClefID>(ClefMetrics.UsedClefIDs) as List<ClefID>;
            List<FlagID> usedFlagIDs = new List<FlagID>(FlagsMetrics.UsedFlagIDs) as List<FlagID>;

            string fontDefs =
            #region fontDefs string
@"
			@font-face
			{
				font-family: 'CLicht';
				src: url('https://github.com/notator/MNXtoSVG/fonts/clicht_plain-webfont.eot');
				src: url('https://github.com/notator/MNXtoSVG/fonts/clicht_plain-webfont.eot?#iefix') format('embedded-opentype'),
				url('https://github.com/notator/MNXtoSVG/fonts/clicht_plain-webfont.woff') format('woff'),
				url('https://github.com/notator/MNXtoSVG/fonts/clicht_plain-webfont.ttf') format('truetype'),
				url('https://github.com/notator/MNXtoSVG/fonts/clicht_plain-webfont.svg#webfontl9D2oOyX') format('svg');
				font-weight: normal;
				font-style: normal;
			}
			@font-face
			{
				font-family: 'Arial';
				src: url('https://github.com/notator/MNXtoSVG/fonts/arial.ttf') format('truetype');
				font-weight:400;
				font-style: normal;
			}
			@font-face
			{
				font-family: 'Open Sans';
				src: url('https://github.com/notator/MNXtoSVG/fonts/OpenSans-Regular.ttf') format('truetype');
				font-weight:400;
				font-style: normal;
			}
			@font-face
			{
				font-family: 'Open Sans Condensed';
				src: url('https://github.com/notator/MNXtoSVG/fonts/OpenSans-CondBold.ttf') format('truetype');
				font-weight:600;
				font-style: normal;
			}
		";
            #endregion fontDefs string
            StringBuilder stylesSB = new StringBuilder(fontDefs);
            stylesSB.Append("    "); // just for formatting
            stylesSB.Append(FontStyles(pageFormat, pageNumber, usedCSSObjectClasses, usedCSSColorClasses, usedClefIDs));
            bool defineFlagStyle = HasFlag(usedFlagIDs);
            stylesSB.Append(LineStyles(pageFormat, usedCSSObjectClasses, pageNumber, defineFlagStyle));

            return stylesSB;
        }

        private bool HasFlag(List<FlagID> usedFlagIDs)
        {
            bool rval = false;
            foreach(FlagID flagID in usedFlagIDs)
            {
                rval = true;
                break;
            }
            return rval;
        }

        #region font styles

        private StringBuilder FontStyles(PageFormat pageFormat, int pageNumber, List<CSSObjectClass> usedCSSObjectClasses,
                                            List<CSSColorClass> usedCSSColorClasses, List<ClefID> usedClefIDs)
        {
            StringBuilder fontStyles = new StringBuilder();
            #region text
            #region Open Sans (Titles)        
            if(pageNumber < 2) // pageNumber is 0 for scroll score.
            {
                string openSans = "\"Open Sans\"";
                string page1TitleHeight = M.DoubleToShortString(pageFormat.Page1TitleHeightVBPX);
                StringBuilder mainTitleType = TextStyle("." + CSSObjectClass.mainTitle.ToString(), openSans, page1TitleHeight, "middle");
                fontStyles.Append(mainTitleType);

                string page1AuthorHeight = M.DoubleToShortString(pageFormat.Page1AuthorHeightVBPX);
                StringBuilder authorType = TextStyle("." + CSSObjectClass.author.ToString(), openSans, page1AuthorHeight, "end");
                fontStyles.Append(authorType);
            } // end if(pageNumber < 2)
            #endregion Open Sans (Titles)

            #region CLicht
            string musicFontHeight = M.DoubleToShortString(pageFormat.MusicFontHeight);
            StringBuilder existingCLichtClasses = GetExistingClichtClasses(usedCSSObjectClasses, usedClefIDs);
            StringBuilder cLichtStyle = TextStyle(existingCLichtClasses.ToString(), "CLicht", "", "");
            fontStyles.Append(cLichtStyle);

            //".rest, .notehead, .accidental, .clef"
            StringBuilder standardSizeClasses = GetStandardSizeClasses(usedCSSObjectClasses, usedClefIDs);
            StringBuilder fontSizeStyle = TextStyle(standardSizeClasses.ToString(), "", musicFontHeight, "");
            fontStyles.Append(fontSizeStyle);

            StringBuilder colorStyles = GetColorStyles(usedCSSColorClasses);
            fontStyles.Append(colorStyles);

            if(usedCSSObjectClasses.Contains(CSSObjectClass.dynamic))
            {
                string dynamicFontHeight = M.DoubleToShortString(pageFormat.DynamicFontHeight);
                fontSizeStyle = TextStyle("." + CSSObjectClass.dynamic.ToString(), "", dynamicFontHeight, "");
                fontStyles.Append(fontSizeStyle);
            }

            // .smallClef, .cautionaryNotehead, .cautionaryAccidental
            StringBuilder smallClasses = GetSmallClasses(usedCSSObjectClasses, usedClefIDs);
            if(smallClasses.Length > 0)
            {
                string smallMusicFontHeight = M.DoubleToShortString(pageFormat.MusicFontHeight * pageFormat.SmallSizeFactor);
                fontSizeStyle = TextStyle(smallClasses.ToString(), "", smallMusicFontHeight, "");
                fontStyles.Append(fontSizeStyle);
            }

            if(OctavedClefExists(usedClefIDs))
            {
                string clefOctaveNumberFontSize = M.DoubleToShortString(pageFormat.ClefOctaveNumberHeight);
                fontSizeStyle = TextStyle("." + CSSObjectClass.clefOctaveNumber.ToString(), "", clefOctaveNumberFontSize, "");
                fontStyles.Append(fontSizeStyle);
            }
            if(OctavedSmallClefExists(usedClefIDs))
            {
                string smallClefOctaveNumberFontSize = M.DoubleToShortString(pageFormat.ClefOctaveNumberHeight * pageFormat.SmallSizeFactor);
                fontSizeStyle = TextStyle("." + CSSObjectClass.smallClefOctaveNumber.ToString(), "", smallClefOctaveNumberFontSize, "");
                fontStyles.Append(fontSizeStyle);
            }

            #endregion CLicht

            #region Arial
            StringBuilder existingArialClasses = GetExistingArialClasses(usedCSSObjectClasses, usedClefIDs);
            StringBuilder arialStyle = TextStyle(existingArialClasses.ToString(), "Arial", "", "");
            fontStyles.Append(arialStyle);

            string timeStampHeight = M.DoubleToShortString(pageFormat.TimeStampFontHeight);
            StringBuilder timeStampType = TextStyle("." + CSSObjectClass.timeStamp.ToString(), "", timeStampHeight, "");
            fontStyles.Append(timeStampType);

            if(usedCSSObjectClasses.Contains(CSSObjectClass.timeSig))
            {
                string timeSigComponentFontHeight = M.DoubleToShortString(pageFormat.TimeSignatureComponentFontHeight);
                string typenames = "." + CSSObjectClass.timeSigNumerator.ToString() + ", " + "." + CSSObjectClass.timeSigDenominator.ToString();
                StringBuilder timeSigType = TextStyle(typenames, "", timeSigComponentFontHeight, "");
                fontStyles.Append(timeSigType);
            }

            if(usedCSSObjectClasses.Contains(CSSObjectClass.staffName))
            {
                string staffNameFontHeight = M.DoubleToShortString(pageFormat.StaffNameFontHeight);
                StringBuilder staffNameHeight = TextStyle("." + CSSObjectClass.staffName.ToString(), "", staffNameFontHeight, "middle");
                fontStyles.Append(staffNameHeight);
            }
            if(usedCSSObjectClasses.Contains(CSSObjectClass.lyric))
            {
                string lyricFontHeight = M.DoubleToShortString(pageFormat.LyricFontHeight);
                StringBuilder lyricHeight = TextStyle("." + CSSObjectClass.lyric.ToString(), "", lyricFontHeight, "middle");
                fontStyles.Append(lyricHeight);
            }
            if(usedCSSObjectClasses.Contains(CSSObjectClass.barNumber))
            {
                string barNumberNumberFontHeight = M.DoubleToShortString(pageFormat.BarNumberNumberFontHeight);
                StringBuilder barNumberNumberHeight = TextStyle("." + CSSObjectClass.barNumberNumber.ToString(), "", barNumberNumberFontHeight, "middle");
                fontStyles.Append(barNumberNumberHeight);
            }
            if(usedCSSObjectClasses.Contains(CSSObjectClass.framedRegionInfo))
            {
                string regionInfoStringFontHeight = M.DoubleToShortString(pageFormat.RegionInfoStringFontHeight);
                StringBuilder regionInfoHeight = TextStyle("." + CSSObjectClass.regionInfoString.ToString(), "", regionInfoStringFontHeight, "middle");
                fontStyles.Append(regionInfoHeight);
            }
            if(usedCSSObjectClasses.Contains(CSSObjectClass.octaveShiftExtender))
            {
                string objectClass = "." + CSSObjectClass.octaveShiftExtenderText.ToString();
                string fontHeight = M.DoubleToShortString(pageFormat.OctaveShiftExtenderTextFontHeight);
                string fontWeight = pageFormat.OctaveShiftExtenderTextFontWeight;
                string fontStyle = pageFormat.OctaveShiftExtenderTextFontStyle;
                StringBuilder textStyle = TextStyle(objectClass, null, fontHeight, fontWeight, fontStyle, null);
                fontStyles.Append(textStyle);
            }

            if(ClefXExists(usedClefIDs))
            {
                string clefXFontHeight = M.DoubleToShortString(pageFormat.ClefXFontHeight);
                StringBuilder clefXStyle = TextStyle("." + CSSObjectClass.clefX.ToString(), "", clefXFontHeight, "");
                fontStyles.Append(clefXStyle);
            }
            if(SmallClefXExists(usedClefIDs))
            {
                string smallClefXFontHeight = M.DoubleToShortString(pageFormat.ClefXFontHeight * pageFormat.SmallSizeFactor);
                StringBuilder smallClefXStyle = TextStyle("." + CSSObjectClass.smallClefX.ToString(), "", smallClefXFontHeight, "");
                fontStyles.Append(smallClefXStyle);
            }
            if(usedCSSObjectClasses.Contains(CSSObjectClass.repeatTimes))
            {
                string repeatTimesStringFontHeight = M.DoubleToShortString(pageFormat.RepeatTimesStringFontHeight);
                StringBuilder repeatTimesInfoHeight = TextStyle("." + CSSObjectClass.repeatTimes.ToString(), "", repeatTimesStringFontHeight,
                    SVGFontWeight.bold.ToString(), "", "");

                fontStyles.Append(repeatTimesInfoHeight);
            }
            #endregion Arial

            #region Open Sans Condensed (ornament)
            if(usedCSSObjectClasses.Contains(CSSObjectClass.ornament))
            {
                string openSansCondensed = "\"Open Sans Condensed\"";
                string ornamentFontHeight = M.DoubleToShortString(pageFormat.OrnamentFontHeight);
                StringBuilder ornamentType = TextStyle("." + CSSObjectClass.ornament.ToString(), openSansCondensed, ornamentFontHeight, "middle");
                fontStyles.Append(ornamentType);
            }
            if(usedCSSObjectClasses.Contains(CSSObjectClass.tupletText))
            {
                string openSansCondensed = "\"Open Sans Condensed\"";
                string tupletFontHeight = M.DoubleToShortString(pageFormat.TupletFontHeight);
                StringBuilder ornamentType = TextStyle("." + CSSObjectClass.tupletText.ToString(), openSansCondensed, tupletFontHeight, SVGFontWeight.bold.ToString(),
                    SVGFontStyle.italic.ToString(), TextHorizAlign.center.ToString());
                fontStyles.Append(ornamentType);
            }
            #endregion Open Sans Condensed (ornament)
            #endregion text

            return fontStyles;
        }

        private StringBuilder GetColorStyles(List<CSSColorClass> usedCSSColorClasses)
        {
            StringBuilder rval = new StringBuilder();
            rval.Append(FillDefinition(CSSColorClass.fffColor, usedCSSColorClasses));
            rval.Append(FillDefinition(CSSColorClass.ffColor, usedCSSColorClasses));
            rval.Append(FillDefinition(CSSColorClass.fColor, usedCSSColorClasses));
            rval.Append(FillDefinition(CSSColorClass.mfColor, usedCSSColorClasses));
            rval.Append(FillDefinition(CSSColorClass.mpColor, usedCSSColorClasses));
            rval.Append(FillDefinition(CSSColorClass.pColor, usedCSSColorClasses));
            rval.Append(FillDefinition(CSSColorClass.ppColor, usedCSSColorClasses));
            rval.Append(FillDefinition(CSSColorClass.pppColor, usedCSSColorClasses));
            rval.Append(FillDefinition(CSSColorClass.ppppColor, usedCSSColorClasses));

            return rval;
        }

        /// <summary>
        /// Returns an empty StringBuilder if the cssColorClass is not in the usedCSSClasses
        /// </summary>
        private StringBuilder FillDefinition(CSSColorClass cssColorClass, List<CSSColorClass> usedCSSClasses)
        {
            StringBuilder def = new StringBuilder();
            if(usedCSSClasses.Contains(cssColorClass))
            {
                def.Append("." + cssColorClass.ToString());
                def.Append(@"
			{");
                def.Append($@"
			    fill:{M.GetEnumDescription(cssColorClass)}");
                def.Append(@"
			}
			");
            }

            return def;
        }

        private StringBuilder GetExistingClichtClasses(List<CSSObjectClass> usedCSSClasses, List<ClefID> usedClefIDs)
        {
            //.rest, .notehead, .accidental,
            //.cautionaryNotehead, .cautionaryAccidental,
            //.clef, .smallClef,
            //.dynamic,
            //.clefOctaveNumber, .smallClefOctaveNumber

            StringBuilder rval = new StringBuilder();
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.rest);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.notehead);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.accidental);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.dot);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.cautionaryNotehead);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.cautionaryAccidental);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.cautionaryDot);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.clef);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.smallClef);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.dynamic);
            if(OctavedClefExists(usedClefIDs))
            {
                ExtendRval(rval, "." + CSSObjectClass.clefOctaveNumber.ToString());
            }
            if(OctavedSmallClefExists(usedClefIDs))
            {
                ExtendRval(rval, "." + CSSObjectClass.smallClefOctaveNumber.ToString());
            }

            return rval;
        }

        private StringBuilder GetStandardSizeClasses(List<CSSObjectClass> usedCSSClasses, List<ClefID> usedClefIDs)
        {
            //".rest, .notehead, .accidental, .dot, .clef"

            StringBuilder rval = new StringBuilder();
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.rest);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.notehead);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.accidental);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.dot);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.clef);
            return rval;
        }

        private StringBuilder GetSmallClasses(List<CSSObjectClass> usedCSSClasses, List<ClefID> usedClefIDs)
        {
            // .smallClef, .cautionaryNotehead, .cautionaryAccidental, .cautionaryAugDot

            StringBuilder rval = new StringBuilder();
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.cautionaryNotehead);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.cautionaryAccidental);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.cautionaryDot);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.smallClef);

            return rval;
        }

        private StringBuilder GetExistingArialClasses(List<CSSObjectClass> usedCSSClasses, List<ClefID> usedClefIDs)
        {
            //.timeStamp,
            //.staffName,
            //.lyric,
            //.barNumberNumber, 
            //.clefX, .smallClefX
            //.octaveShiftExtender)

            // timestamp is not recorded, but exists on every page
            StringBuilder rval = new StringBuilder("." + CSSObjectClass.timeStamp.ToString());

            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.staffName);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.lyric);
            if(usedCSSClasses.Contains(CSSObjectClass.barNumber))
            {
                ExtendRval(rval, "." + CSSObjectClass.barNumberNumber.ToString());
            }
            if(usedCSSClasses.Contains(CSSObjectClass.framedRegionInfo))
            {
                ExtendRval(rval, "." + CSSObjectClass.regionInfoString.ToString());
            }
            if(ClefXExists(usedClefIDs))
            {
                ExtendRval(rval, "." + CSSObjectClass.clefX.ToString());
            }
            if(SmallClefXExists(usedClefIDs))
            {
                ExtendRval(rval, "." + CSSObjectClass.smallClefX.ToString());
            }
            if(usedCSSClasses.Contains(CSSObjectClass.timeSig))
            {
                ExtendRval(rval, "." + CSSObjectClass.timeSigNumerator.ToString());
                ExtendRval(rval, "." + CSSObjectClass.timeSigDenominator.ToString());
            }
            if(usedCSSClasses.Contains(CSSObjectClass.octaveShiftExtender))
            {
                ExtendRval(rval, "." + CSSObjectClass.octaveShiftExtenderText.ToString());
            }
            if(usedCSSClasses.Contains(CSSObjectClass.repeatTimes))
            {
                ExtendRval(rval, "." + CSSObjectClass.repeatTimes.ToString());
            }

            return rval;
        }

        private void ExtendRvalWith(StringBuilder rval, List<CSSObjectClass> usedCSSClasses, CSSObjectClass cssClass)
        {
            if(usedCSSClasses.Contains(cssClass))
            {
                ExtendRval(rval, "." + cssClass.ToString());
            }
        }
        private void ExtendRval(StringBuilder rval, string className)
        {
            if(rval.Length > 0)
            {
                rval.Append(", ");
            }
            rval.Append(className);
        }

        private StringBuilder TextStyle(string element, string fontFamily,
            string fontSize, string textAnchor)

        {
            return (TextStyle(element, fontFamily, fontSize, null, null, textAnchor));
        }

        private StringBuilder TextStyle(string element, string fontFamily,
            string fontSize, string fontWeight, string fontStyle, string textAnchor)
        {
            StringBuilder local = new StringBuilder();
            if(!String.IsNullOrEmpty(fontFamily))
            {
                local.Append($@"font-family:{fontFamily}");
                local.Append($@";
                ");
            }
            if(!String.IsNullOrEmpty(fontSize))
            {
                local.Append($@"font-size:{fontSize}px");
                local.Append($@";
                ");
            }
            if(!String.IsNullOrEmpty(fontWeight))
            {
                local.Append($@"font-weight:{fontWeight}");
                local.Append($@";
                ");
            }
            if(!String.IsNullOrEmpty(fontStyle))
            {
                local.Append($@"font-style:{fontStyle}");
                local.Append($@";
                ");
            }
            if(!String.IsNullOrEmpty(textAnchor))
            {
                local.Append($@"text-anchor:{textAnchor}");
                local.Append($@";
                ");
            }

            // remove ';' newline and spaces
            char c = local[local.Length - 1];
            while(c != ';')
            {
                local.Length -= 1;
                c = local[local.Length - 1];
            }
            local.Length -= 1;

            StringBuilder rval = new StringBuilder(
            $@"{element}
            {{
                {local.ToString()}
            }}
            ");

            return rval;
        }

        private bool OctavedClefExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.trebleClef8)
            || usedClefIDs.Contains(ClefID.bassClef8)
            || usedClefIDs.Contains(ClefID.trebleClef2x8)
            || usedClefIDs.Contains(ClefID.bassClef2x8)
            || usedClefIDs.Contains(ClefID.trebleClef3x8)
            || usedClefIDs.Contains(ClefID.bassClef3x8);

            return rval;
        }
        private bool OctavedSmallClefExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.smallTrebleClef8)
            || usedClefIDs.Contains(ClefID.smallBassClef8)
            || usedClefIDs.Contains(ClefID.smallTrebleClef2x8)
            || usedClefIDs.Contains(ClefID.smallBassClef2x8)
            || usedClefIDs.Contains(ClefID.smallTrebleClef3x8)
            || usedClefIDs.Contains(ClefID.smallBassClef3x8);

            return rval;
        }

        private bool ClefXExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.trebleClef2x8)
            || usedClefIDs.Contains(ClefID.bassClef2x8)
            || usedClefIDs.Contains(ClefID.trebleClef3x8)
            || usedClefIDs.Contains(ClefID.bassClef3x8);

            return rval;
        }
        private bool SmallClefXExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.smallTrebleClef2x8)
            || usedClefIDs.Contains(ClefID.smallBassClef2x8)
            || usedClefIDs.Contains(ClefID.smallTrebleClef3x8)
            || usedClefIDs.Contains(ClefID.smallBassClef3x8);

            return rval;
        }
        #endregion font styles

        #region line styles
        private StringBuilder LineStyles(PageFormat pageFormat, List<CSSObjectClass> usedCSSClasses, int pageNumber, bool defineFlagStyle)
        {
            StringBuilder lineStyles = new StringBuilder();

            string strokeWidth = M.DoubleToShortString(pageFormat.StafflineStemStrokeWidthVBPX);
            StringBuilder standardLineClasses = GetStandardLineClasses(usedCSSClasses, defineFlagStyle);

            lineStyles.Append($@".backgroundFill
            {{
                fill:white                
            }}
            ");

            //".staffline, .ledgerline, .stem, .beam, .flag, regionFrameConnector
            lineStyles.Append($@"{standardLineClasses.ToString()}
            {{
                stroke:black;
                stroke-width:{strokeWidth}px;
                fill:black
            }}
            ");

            if(usedCSSClasses.Contains(CSSObjectClass.stem))
            {
                lineStyles.Append($@".stem
            {{
                stroke-linecap:round                
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.normalBarline))
            {
                strokeWidth = M.DoubleToShortString(pageFormat.NormalBarlineStrokeWidth);
                lineStyles.Append($@".{CSSObjectClass.normalBarline.ToString()}
            {{
                stroke:black;
                stroke-width:{strokeWidth}px
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.thinBarline))
            {
                strokeWidth = M.DoubleToShortString(pageFormat.ThinBarlineStrokeWidth);
                lineStyles.Append($@".{CSSObjectClass.thinBarline.ToString()}
            {{
                stroke:black;
                stroke-width:{strokeWidth}px
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.thickBarline))
            {
                strokeWidth = M.DoubleToShortString(pageFormat.ThickBarlineStrokeWidth);
                lineStyles.Append($@".{CSSObjectClass.thickBarline.ToString()}
            {{
                stroke:black;
                stroke-width:{strokeWidth}px
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.noteExtender))
            {
                strokeWidth = M.DoubleToShortString(pageFormat.NoteheadExtenderStrokeWidth);
                lineStyles.Append($@".{CSSObjectClass.noteExtender.ToString()}
            {{
                stroke:black;
                stroke-width:{strokeWidth}px
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.barNumber))
            {
                strokeWidth = M.DoubleToShortString(pageFormat.BarNumberFrameStrokeWidth);
                lineStyles.Append($@".barNumberFrame
            {{
                stroke:black;
                stroke-width:{strokeWidth}px;
                fill:none
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.framedRegionInfo))
            {
                strokeWidth = M.DoubleToShortString(pageFormat.RegionInfoFrameStrokeWidth);
                lineStyles.Append($@".regionInfoFrame
            {{
                stroke:black;
                stroke-width:{strokeWidth}px;
                fill:none
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.cautionaryBracket))
            {
                strokeWidth = M.DoubleToShortString(pageFormat.StafflineStemStrokeWidthVBPX);
                lineStyles.Append($@".cautionaryBracket
            {{
                stroke:black;
                stroke-width:{strokeWidth};
                fill:none                
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.octaveShiftExtender))
            {
                strokeWidth = M.DoubleToShortString(pageFormat.OctaveShiftExtenderLineStrokeWidth);
                lineStyles.Append($@".octaveShiftExtenderHLine, .octaveShiftExtenderVLine
            {{
                stroke:black;
                stroke-width:{strokeWidth}px;
                fill:none
            }}
            ");
                lineStyles.Append($@".octaveShiftExtenderVLine
            {{
                stroke-linecap:square
            }}
            ");
            }

            if(pageNumber > 0) // pageNumber is 0 for scroll
            {
                strokeWidth = M.DoubleToShortString(pageFormat.StafflineStemStrokeWidthVBPX);
                lineStyles.Append($@".frame
            {{
                stroke:black;
                stroke-width:{strokeWidth}px;                
            }}
            ");
            }

            strokeWidth = M.DoubleToShortString(pageFormat.StafflineStemStrokeWidthVBPX);
            if(usedCSSClasses.Contains(CSSObjectClass.beamBlock))
            {
                lineStyles.Append($@".opaqueBeam
            {{
                stroke:white;
                stroke-width:{strokeWidth}px;
                fill:white;
                opacity:0.65                
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.slurTemplate))
            {
                strokeWidth = M.DoubleToShortString(pageFormat.StafflineStemStrokeWidthVBPX / 3);
                lineStyles.Append($@".slur
            {{
                stroke:black;
                stroke-width:{strokeWidth}px;
                stroke-linejoin:round
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.tieTemplate))
            {
                strokeWidth = M.DoubleToShortString(pageFormat.StafflineStemStrokeWidthVBPX / 3);
                lineStyles.Append($@".tie
            {{
                stroke:black;
                stroke-width:{strokeWidth}px;
                stroke-linejoin:round
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.tupletBracket))
            {
                strokeWidth = M.DoubleToShortString(pageFormat.TupletBracketStrokeWidth);
                lineStyles.Append($@".tupletBracket
            {{
                stroke:black;
                stroke-width:{strokeWidth};
                fill:none                
            }}
            ");
            }

            return lineStyles;
        }

        private StringBuilder GetStandardLineClasses(List<CSSObjectClass> usedCSSClasses, bool defineFlagStyle)
        {
            //.staffline, .ledgerline, .stem, regionFrameConnector, .beam
            StringBuilder rval = new StringBuilder();
            if(usedCSSClasses.Contains(CSSObjectClass.staff))
            {
                ExtendRval(rval, ".staffline");
            }
            if(usedCSSClasses.Contains(CSSObjectClass.ledgerlines))
            {
                ExtendRval(rval, ".ledgerline");
            }
            if(usedCSSClasses.Contains(CSSObjectClass.stem))
            {
                ExtendRval(rval, ".stem");
            }
            if(usedCSSClasses.Contains(CSSObjectClass.regionFrameConnector))
            {
                ExtendRval(rval, ".regionFrameConnector");
            }
            if(usedCSSClasses.Contains(CSSObjectClass.beamBlock))
            {
                ExtendRval(rval, ".beam");
            }
            if(defineFlagStyle)
            {
                ExtendRval(rval, ".flag");
            }

            return rval;
        }

        #endregion line styles

        #endregion save multi-page score

        #region save single svg score
        /// <summary>
        /// Writes the "scroll" version of the score. This is a standalone SVG file.
        /// When graphicsOnly is true, the following are omitted:
        ///     a) the metadata element, and all its required namespaces,
        ///     b) all temporal (i.e.MIDI etc.) informaton the score namespace, and all its enclosed (temporal and alignment) information.
        /// If printTitleAndAuthorOnScorePage1 is false then the main title and author information is omitted on page 1, and page 1
        /// has the margins otherwise allocated for all the other pages.
        /// </summary>
        public string SaveSVGScrollScore(bool graphicsOnly, bool printTitleAndAuthorOnPage1)
        {
            if(printTitleAndAuthorOnPage1)
            {
                M.PageFormat.TopMarginPage1VBPX = M.PageFormat.TopMarginOtherPagesVBPX;
            }

            string filePath = GetSVGFilePath(0);

            TextInfo infoTextInfo = GetInfoTextAtTopOfPage(filePath);

            SvgPage singlePage = new SvgPage(this, M.PageFormat, 0, infoTextInfo, this.Systems, true);

            SaveSVGPage(filePath, singlePage, MetadataWithDate, true, graphicsOnly, printTitleAndAuthorOnPage1);

            return filePath;
        }


        #endregion save single svg score


        protected string GetSVGFilePath(int pageNumber)
        {
            string pageFilename = FileNameWithoutExtension;

            if(!Form1Options.IncludeMIDIData)
            {
                pageFilename += " (graphics only)";
            }

            if(pageNumber > 0)
            {
                pageFilename += (" page " + pageNumber.ToString());
            }

            string pagePath = TargetFolder + @"\" + pageFilename + ".svg";

            return pagePath;
        }

        /// <summary>
        /// The score's systems
        /// </summary>
        public List<SvgSystem> Systems = new List<SvgSystem>();

        /// <summary>
        /// Adds the staff name to the first barline of each visible staff in the score.
        /// </summary>
        private void AddStaffNames()
        {
            foreach(SvgSystem system in Systems)
            {
                for(int staffIndex = 0; staffIndex < system.Staves.Count; staffIndex++)
                {
                    Staff staff = system.Staves[staffIndex];
                    foreach(NoteObject noteObject in staff.Voices[0].NoteObjects)
                    {
                        if(noteObject is Barline firstBarline && !String.IsNullOrEmpty(staff.Staffname))
                        {
                            double fontHeight = M.PageFormat.StaffNameFontHeight;
                            StaffNameText staffNameText = new StaffNameText(firstBarline, staff.Staffname, fontHeight);
                            firstBarline.DrawObjects.Add(staffNameText);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// There is currently still one bar per system.
        /// Each voice ends with a barline.
        /// </summary>
        protected virtual void ReplaceConsecutiveRestsInBars(int minimumCrotchetDuration)
        {
            foreach(SvgSystem system in Systems)
            {
                foreach(Staff staff in system.Staves)
                {
                    foreach(Voice voice in staff.Voices)
                    {
                        M.Assert(voice.NoteObjects[voice.NoteObjects.Count - 1] is Barline);
                        // contains lists of consecutive rest indices
                        List<List<int>> restIndexLists = new List<List<int>>();
                        #region find the consecutive rests
                        List<int> consecutiveRestIndexList = new List<int>();
                        for(int i = 0; i < voice.NoteObjects.Count - 1; i++)
                        {
                            RestSymbol rest2 = voice.NoteObjects[i + 1] as RestSymbol;
                            if(voice.NoteObjects[i] is RestSymbol rest1 && rest2 != null)
                            {
                                if(!consecutiveRestIndexList.Contains(i))
                                {
                                    consecutiveRestIndexList.Add(i);
                                }
                                consecutiveRestIndexList.Add(i + 1);
                            }
                            else
                            {
                                if(consecutiveRestIndexList != null && consecutiveRestIndexList.Count > 0)
                                {
                                    restIndexLists.Add(consecutiveRestIndexList);
                                    consecutiveRestIndexList = new List<int>();
                                }
                            }
                        }
                        #endregion
                        #region replace the consecutive rests
                        if(restIndexLists.Count > 0)
                        {
                            for(int i = restIndexLists.Count - 1; i >= 0; i--)
                            {
                                List<int> consecutiveRestIndices = restIndexLists[i];
                                int msDuration = 0;
                                RestSymbol rest = null;
                                // remove all but the first rest
                                for(int j = consecutiveRestIndices.Count - 1; j > 0; j--)
                                {
                                    rest = voice.NoteObjects[consecutiveRestIndices[j]] as RestSymbol;
                                    M.Assert(rest != null);
                                    msDuration += rest.MsDuration;
                                    voice.NoteObjects.RemoveAt(consecutiveRestIndices[j]);
                                }
                                rest = voice.NoteObjects[consecutiveRestIndices[0]] as RestSymbol;
                                msDuration += rest.MsDuration;
                                rest.MsDuration = msDuration;
                            }
                        }
                        #endregion
                    }
                }
            }
        }
        /// <summary>
        /// If barNumbers is null, systems will be given 5 bars each by default.
        /// Otherwise:
        /// Each barnumber in the argument barNumbers will be at the start of a system when this function returns.
        /// All barNumbers must be greater than 0 and less or equal to than the current Systems.Count.
        /// Barnumber 1 must be present. No barNumbers may be repeated.
        /// barNumbers beyond the end of the score are silently ignored.
        /// </summary>
        public void SetSystemsToBeginAtBars(IReadOnlyList<int> globalBarNumbers)
        {
            List<int> barNumbers = new List<int>(globalBarNumbers);

            M.Assert(barNumbers != null);
            if(barNumbers.Count == 0)
            {
                int barNumber = 1;
                while(barNumber <= Systems.Count)
                {
                    barNumbers.Add(barNumber);
                    barNumber += M.PageFormat.DefaultNumberOfBarsPerSystem;
                }
            }

            M.Assert(barNumbers[0] == 1);

            // barNumbers beyond the end of the score are silently ignored.
            List<int> systemStartBarIndices = new List<int>();
            for(int i = 1; i <= Systems.Count; i++)
            {
                if(barNumbers.Contains(i))
                    systemStartBarIndices.Add(i - 1);
            }

            JoinBarsToSystems(systemStartBarIndices);

            SetCautionaryChordSymbolVisibility();
        }

        private void SetCautionaryChordSymbolVisibility()
        {
            foreach(SvgSystem system in Systems)
            {
                foreach(Staff staff in system.Staves)
                {
                    foreach(Voice voice in staff.Voices)
                    {
                        bool visible = true;
                        foreach(NoteObject noteObject in voice.NoteObjects)
                        {
                            if(noteObject is CautionaryChordSymbol ccs)
                            {
                                ccs.Visible = visible;
                            }
                            else if(noteObject is OutputChordSymbol ocs)
                            {
                                visible = false;
                            }
                        }
                    }
                }
            }
        }

        #region private for JoinSystems() and SetBarsPerSystem()

        private void JoinBarsToSystems(List<int> systemStartBarIndices)
        {
            for(int systemIndex = 0; systemIndex < systemStartBarIndices.Count; systemIndex++)
            {
                int nBarsToJoin;
                if(systemIndex == systemStartBarIndices.Count - 1)
                {
                    nBarsToJoin = Systems.Count - systemIndex - 1;
                }
                else
                {
                    nBarsToJoin = systemStartBarIndices[systemIndex + 1] - systemStartBarIndices[systemIndex] - 1;
                }

                for(int b = 0; b < nBarsToJoin; ++b)
                {
                    JoinNextBarToSystem(systemIndex);
                }
            }
        }

        /// <summary>
        /// Copies Systems[systemIndex+1]'s content to the end of Systems[systemIndex]
        /// (taking account of running clefs, keySignatures and timeSignatures),
        /// then removes Systems[systemIndex+1] from the Systems list.
        /// </summary>
        /// <param name="barlineIndex"></param>
        private void JoinNextBarToSystem(int systemIndex)
        {
            M.Assert(Systems.Count > 1 && Systems.Count > systemIndex + 1);
            SvgSystem system1 = Systems[systemIndex];
            SvgSystem system2 = Systems[systemIndex + 1];
            M.Assert(system1.Staves.Count == system2.Staves.Count);
            foreach(Staff staff in system2.Staves)
            {
                foreach(Voice voice in staff.Voices)
                {
                    M.Assert(voice.NoteObjects[0] is Clef);
                }
            }

            for(int staffIndex = 0; staffIndex < system2.Staves.Count; staffIndex++)
            {
                for(int voiceIndex = 0; voiceIndex < system2.Staves[staffIndex].Voices.Count; voiceIndex++)
                {
                    Voice voice1 = system1.Staves[staffIndex].Voices[voiceIndex];
                    GetFinalDirections(voice1.NoteObjects,
                        out Clef v1FinalClef, out KeySignature v1FinalKeySignature, out TimeSignature v1FinalTimeSignature, out RepeatEnd v1EndRepeatEnd);
                    Voice voice2 = system2.Staves[staffIndex].Voices[voiceIndex];
                    GetInitialDirections(voice2.NoteObjects,
                        out Clef v2InitialClef, out KeySignature v2InitialKeySignature, out TimeSignature v2InitialTimeSignature);
                    M.Assert(v1FinalClef != null);

                    if(v2InitialClef != null && v2InitialClef.ClefType == v1FinalClef.ClefType)
                    {
                        voice2.NoteObjects.RemoveAt(0);
                    }
                    if(v2InitialKeySignature != null && v2InitialKeySignature.Fifths == v1FinalKeySignature.Fifths)
                    {
                        voice2.NoteObjects.Remove(v2InitialKeySignature);
                    }
                    if(v2InitialTimeSignature != null && v2InitialTimeSignature.Signature == v1FinalTimeSignature.Signature)
                    {
                        voice2.NoteObjects.Remove(v2InitialTimeSignature);
                    }

                    voice1.AppendNoteObjects(voice2.NoteObjects);
                }
            }
            Systems.Remove(system2);
            system2 = null;
        }

        private void GetInitialDirections(List<NoteObject> noteObjects,
            out Clef initialClef, out KeySignature initialKeySignature, out TimeSignature initialTimeSignature)
        {
            initialClef = null;
            initialKeySignature = null;
            initialTimeSignature = null;

            for(var i = 0; i < 3; ++i)
            {
                var noteObject = noteObjects[i];
                if(noteObject is Clef clef)
                {
                    initialClef = clef;
                }
                if(noteObject is KeySignature keySignature)
                {
                    initialKeySignature = keySignature;
                }
                if(noteObject is TimeSignature timeSignature)
                {
                    initialTimeSignature = timeSignature;
                }
            }
        }

        private void GetFinalDirections(List<NoteObject> noteObjects,
            out Clef outClef, out KeySignature outKeySignature, out TimeSignature outTimeSignature, out RepeatEnd outEndRepeatEnd)
        {
            outClef = null;
            outKeySignature = null;
            outTimeSignature = null;
            outEndRepeatEnd = null;

            foreach(var noteObject in noteObjects)
            {
                if(noteObject is Clef clef)
                {
                    outClef = clef;
                }
                if(noteObject is KeySignature keySignature)
                {
                    outKeySignature = keySignature;
                }
                if(noteObject is TimeSignature timeSignature)
                {
                    outTimeSignature = timeSignature;
                }
            }

            for(int i = noteObjects.Count - 1; i >= noteObjects.Count - 2; --i)
            {
                if(noteObjects[i] is RepeatEnd repeatEnd)
                {
                    outEndRepeatEnd = repeatEnd;
                }
            }
        }

        #endregion

        #region protected functions

        /// <summary>
        /// There is currently one bar per System. 
        /// All Duration Symbols have been constructed in voice.NoteObjects (possibly including CautionaryChordSymbols at the beginnings of staves).
        /// There are no barlines or repeatSymbols in the score yet.
        /// Add a NormalBarline at the beginning of the Systems[0] (after the clef).
        /// Now add a NormalBarline or EndOfScoreBarline at the end of each system=bar. (Cautionary keySigs and timeSigs may be added later.)
        /// </summary>
        /// <param name="barlineType"></param>
        /// <param name="systemNumbers"></param>
        private void AddBarlines()
        {
            // There is currently one bar per System, so systemIndex is bar index here.
            for(int systemIndex = 0; systemIndex < Systems.Count; ++systemIndex)
            {
                var staves = Systems[systemIndex].Staves;
                for(var staffIndex = 0; staffIndex < staves.Count; ++staffIndex)
                {
                    var voices = staves[staffIndex].Voices;
                    for(var voiceIndex = 0; voiceIndex < voices.Count; ++voiceIndex)
                    {
                        var voice = voices[voiceIndex];
                        var noteObjects = voice.NoteObjects;
                        M.Assert(noteObjects.Count > 0 && !(noteObjects[noteObjects.Count - 1] is Barline));

                        if(systemIndex == 0)
                        {
                            var initialBarline = new NormalBarline(voice);
                            var index = noteObjects.FindIndex(noteObject => (!(noteObject is Clef)));
                            noteObjects.Insert(index, initialBarline);
                        }

                        var endBarline = (systemIndex == Systems.Count - 1) ? new EndOfScoreBarline(voice) : new NormalBarline(voice);
                        noteObjects.Add(endBarline);
                    }
                }
            }
        }

        private List<int> GetGroupBottomStaffIndices(List<int> groupSizes, int nStaves)
        {
            List<int> returnList = new List<int>();
            if(groupSizes == null || groupSizes.Count == 0)
            {
                for(int i = 0; i < nStaves; i++)
                    returnList.Add(i);
            }
            else
            {
                int bottomOfGroup = groupSizes[0] - 1;
                returnList.Add(bottomOfGroup);
                int i = 1;
                while(i < groupSizes.Count)
                {
                    bottomOfGroup += groupSizes[i++];
                    returnList.Add(bottomOfGroup);
                }
            }
            return returnList;
        }

        /// <summary>
        /// When this function is called, every system still contains one bar, and all systems have the same number
        /// of staves and voices as System[0]. Now:
        /// 1. add a NormalBarline, RepeatBegin, RepeatEnd or EndOfScoreBarline at the beginning and end of each system=bar (after the clef (and keySignature, if any))
        /// 2. join the bars into systems according to the user's options, setting RepeatBarlines as necessary...
        /// 3. set the visibility of naturals (if the chords have any noteheads)
        /// 4. add a barnumber to the first barline on each system.
        /// 5. add the staff's name to the first barline on each staff. 
        /// 6. if there are no ordinary (MNX) repeats, add regionStart- and regionEnd- info to the appropriate NormalBarlines, then
        /// 7. convert the NormalBarlines to the appropriate region barline class
        /// </summary>
        protected void FinalizeSystemStructure(List<Bar> bars, List<MNX.Common.BeamBlock> allBeamBlocks)
        {
            #region preconditions
            int nStaves = Systems[0].Staves.Count;
            foreach(SvgSystem system in Systems)
            {
                M.Assert(system.Staves.Count == nStaves);
            }
            List<int> nVoices = new List<int>();
            foreach(Staff staff in Systems[0].Staves)
                nVoices.Add(staff.Voices.Count);

            foreach(SvgSystem system in Systems)
            {
                for(int i = 0; i < system.Staves.Count; ++i)
                {
                    M.Assert(system.Staves[i].Voices.Count == nVoices[i]);
                    foreach(Voice voice in system.Staves[i].Voices)
                    {
                        M.Assert(voice.NoteObjects[0] is Clef);
                    }
                }
            }
            #endregion preconditions

            // Add a NormalBarline or EndOfScoreBarline at the end of each system=bar (after the clef (and keySignature, if any))
            AddBarlines();

            ReplaceConsecutiveRestsInBars(M.PageFormat.MinimumCrotchetDuration);

            SetSystemsToBeginAtBars(M.PageFormat.SystemStartBars); // 2. join the bars into systems according to the user's options, setting RepeatBarlines as necessary...

            SetSystemAbsEndMsPositions();

            SetBeamBlockDefs(allBeamBlocks);

            NormalizeSmallClefs();

            AddBarNumbers(); // 4.add a barnumber to the first Barline on each system.
            AddStaffNames(); // 5. adds the staff's name to the first Barline on each staff.

            // Regions are not implemented for MNXtoSVG, so region code is currently unused and not debugged!
            //
            //if(this.ScoreData != null)
            //{
            //    // 6. add regionStart- and regionEnd- info to the appropriate NormalBarlines
            //    AddRegionStartInfo();
            //    AddRegionEndInfo();
            //}

            //SetRegionBarlineTypes(); // 7. converts each NormalBarline to a Barline of the appropriate Region class
        }


        /// <summary>
        /// Sets the BeamBlockDef for each OutputChordSymbol that either IsBeamStart or
        /// IsBeamRestart and is the first OutputChordSymbol in the (system) voice.
        /// Note that OutputChordSymbol.BeamBlockDef is _only_ set if a beamBlock or
        /// beamBlock continuation begins at the OutputChordSymbol.
        /// </summary>
        private void SetBeamBlockDefs(List<MNX.Common.BeamBlock> allBeamBlockDefs)
        {
            if(allBeamBlockDefs.Count > 0)
            {
                foreach(var system in Systems)
                {
                    foreach(var staff in system.Staves)
                    {
                        foreach(var voice in staff.Voices)
                        {
                            var noteObjects = voice.NoteObjects;
                            OutputChordSymbol firstChord = (OutputChordSymbol)noteObjects.Find(x => x is OutputChordSymbol ocs);
                            foreach(var noteObject in noteObjects)
                            {
                                if(noteObject is OutputChordSymbol ocs)
                                {
                                    MNX.Common.BeamBlock bb = ((MNX.Common.BeamBlock)allBeamBlockDefs.Find(x => x is MNX.Common.BeamBlock bbk && ((MNX.Common.Beam)bbk.Components[0]).EventIDs.Contains(ocs.EventID)));
                                    if(bb != null)
                                    {
                                        var quaverBeamEventIDs = ((MNX.Common.Beam)bb.Components[0]).EventIDs;

                                        if((ocs.IsBeamStart) && ocs.EventID == quaverBeamEventIDs[0]
                                        || (ocs == firstChord && ocs.IsBeamRestart && quaverBeamEventIDs.Contains(ocs.EventID))
                                        || (ocs == firstChord && ocs.IsBeamEnd && quaverBeamEventIDs[quaverBeamEventIDs.Count - 1] == ocs.EventID))
                                        {
                                            ocs.BeamBlockDef = bb;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// replaces NormalBarlines by barlines having the appropriate type.
        /// </summary>
        private void SetRegionBarlineTypes()
		{
			Dictionary<int, CSSObjectClass> msPosBarlineClassDict = new Dictionary<int, CSSObjectClass>();
			foreach(SvgSystem system in Systems)
			{
				#region set msPosBarlineClassDict
				msPosBarlineClassDict.Clear();
				List<NoteObject> noteObjects = system.Staves[0].Voices[0].NoteObjects;
				bool isLastSystem = (system == Systems[Systems.Count - 1]);
				for(int i = 0; i < noteObjects.Count; ++i)
				{
					if(noteObjects[i] is NormalBarline normalBarline)
					{
						bool isLastBarlineInScore = (isLastSystem && (i == noteObjects.Count - 1));
						int barlineMsPos = BarlineMsPos(noteObjects, i);
						CSSObjectClass barlineClass = GetBarlineClass(normalBarline.DrawObjects, isLastBarlineInScore);
						msPosBarlineClassDict.Add(barlineMsPos, barlineClass);
					}
				}
				#endregion

				foreach(Staff staff in system.Staves)
				{
					foreach(Voice voice in staff.Voices)
					{
						List<NoteObject> noteObjs = voice.NoteObjects;
						for(int i = 0; i < noteObjs.Count; ++i)
						{
							if(noteObjs[i] is NormalBarline normalBarline)
							{
								int barlineMsPos = BarlineMsPos(noteObjs, i);
								CSSObjectClass barlineClass = msPosBarlineClassDict[barlineMsPos];
								switch(barlineClass)
								{
									case CSSObjectClass.normalBarline: // NormalBarline (no region info)
										break;
									case CSSObjectClass.startRegionBarline:
										voice.NoteObjects[i] = new StartRegionBarline(voice, normalBarline.DrawObjects);
										break;
									case CSSObjectClass.endRegionBarline:
										voice.NoteObjects[i] = new EndRegionBarline(voice, normalBarline.DrawObjects);
										break;
									case CSSObjectClass.endAndStartRegionBarline:
										voice.NoteObjects[i] = new EndAndStartRegionBarline(voice, normalBarline.DrawObjects);
										break;
									case CSSObjectClass.endOfScoreRegionBarline:
										voice.NoteObjects[i] = new EndOfScoreRegionBarline(voice, normalBarline.DrawObjects);
										break;
								}
							}
						}
					}
				}
			}
		}

		private int BarlineMsPos(List<NoteObject> noteObjects, int i)
		{
			M.Assert(noteObjects[i] is Barline);
			int barlineMsPos = 0;
			if(i > 0 && i == noteObjects.Count - 1)
			{
				DurationSymbol prevDurationSymbol = noteObjects[i - 1] as DurationSymbol;
				M.Assert(prevDurationSymbol != null);
				if((prevDurationSymbol is RestSymbol restSymbol)
				|| (prevDurationSymbol is ChordSymbol cSymbol && cSymbol.MsDurationToNextBarline == null))
				{
					barlineMsPos = prevDurationSymbol.AbsMsPosition + prevDurationSymbol.MsDuration;
				}
				else if(prevDurationSymbol is ChordSymbol chordSymbol)
				{
					barlineMsPos = chordSymbol.AbsMsPosition + ((int)chordSymbol.MsDurationToNextBarline);
				}
			}
			else
			{
                int index = i + 1;
                if(noteObjects[index] is KeySignature)
                {
                    index++;
                }
                if(noteObjects[index] is TimeSignature)
                {
                    index++;
                }
                DurationSymbol ds = noteObjects[index] as DurationSymbol;
				barlineMsPos = ds.AbsMsPosition;
			}
			return barlineMsPos;
		}

		private CSSObjectClass GetBarlineClass(List<DrawObject> drawObjects, bool isLastBarlineInScore)
		{
			bool hasStartRegionInfo = false;
			bool hasEndRegionInfo = false;

			foreach(DrawObject drawObject in drawObjects)
			{
				if(drawObject is FramedRegionStartText)
				{
					hasStartRegionInfo = true;
				}
				if(drawObject is FramedRegionEndText)
				{
					hasEndRegionInfo = true;
				}
			}

			CSSObjectClass rval = CSSObjectClass.normalBarline;
			if(hasStartRegionInfo)
			{
				if(hasEndRegionInfo)
				{
					rval = CSSObjectClass.endAndStartRegionBarline;
				}
				else
				{
					rval = CSSObjectClass.startRegionBarline;
				}
			}
			else if(hasEndRegionInfo && !isLastBarlineInScore)
			{
				rval = CSSObjectClass.endRegionBarline;
			}
			else if(isLastBarlineInScore) // can, but need not have EndRegionInfo
			{
				rval = CSSObjectClass.endOfScoreBarline;
			}

			return rval;
		}

		private void SetSystemAbsEndMsPositions()
        {
            int totalMsDuration = 0;
            foreach(SvgSystem system in Systems)
            {
                List<NoteObject> noteObjects = system.Staves[0].Voices[0].NoteObjects;
                foreach(NoteObject noteObject in noteObjects)
                {
                    if(noteObject is DurationSymbol ds)
                    {
                        totalMsDuration += ds.MsDuration;
                    }
                }
            }
            int endMsPosition = totalMsDuration;
            for(int i = Systems.Count - 1; i >= 0; --i)
            {
                Systems[i].AbsEndMsPosition = endMsPosition;
                endMsPosition = Systems[i].AbsStartMsPosition;
            }
        }

		/// <summary>
		/// If a SmallClefs is followed by a rest, it is moved after the rest.
		/// Then if a SmallClef is followed by another SmallClef:
		///    in top voices, an alert message is displayed,
		///    in lower voices, the first SmallClef is silently removed.
		/// </summary>
		private void NormalizeSmallClefs()
		{
			for(int i = 0; i < Systems.Count; ++i)
			{
				SvgSystem system = Systems[i];
				for(int j = 0; j < system.Staves.Count; ++j)
				{
					Staff staff = system.Staves[j];
					for(int k = 0; k < staff.Voices.Count; ++k)
					{
						Voice voice = staff.Voices[k];
						MoveSmallClefsToFollowRests(voice, i, j, k);
						RemoveFirstOfConsecutiveSmallClefs(voice, i, j, k);
					}
				}
			}
		}

		/// <summary>
		/// If a SmallClef is followed by a rest, it is moved after that rest.
		/// This function is called recursively in case there are consecutive rests.
		/// Any consecutive SmallClefs are left untouched.
		/// </summary>
		private void MoveSmallClefsToFollowRests(Voice voice, int systemIndex, int staffIndex, int voiceIndex)
		{
			List<NoteObject> noteObjects = voice.NoteObjects;
			//M.Assert(noteObjects[0] is Clef);
			M.Assert(noteObjects[noteObjects.Count - 1] is Barline);

			for(int i = noteObjects.Count - 1; i > 0; --i)
            {
                if(noteObjects[i] is RestSymbol && noteObjects[i-1] is SmallClef smallClef)
                {
					if(voiceIndex == 0)
					{
						MessageBox.Show($"A SmallClef (type:{smallClef.ClefType}) has been moved to follow a rest.\n" +
									$"   systemIndex:{systemIndex} staffIndex:{staffIndex} voiceIndex:{voiceIndex}", "Warning");
					}
					noteObjects.Insert(i + 1, smallClef); 
					noteObjects.RemoveAt(i - 1);
					MoveSmallClefsToFollowRests(voice, systemIndex, staffIndex, voiceIndex); // recursive function
				}
            }
        }

		/// <summary>
		/// The first of consecutive SmallClefs is removed.
		/// </summary>
		/// <param name="voice"></param>
		private void RemoveFirstOfConsecutiveSmallClefs(Voice voice, int systemIndex, int staffIndex, int voiceIndex)
		{
			List<SmallClef> clefsToRemove = new List<SmallClef>();
			List<NoteObject> noteObjects = voice.NoteObjects;
			if(noteObjects.Count > 1)
			{
				for(int i = 0; i < noteObjects.Count - 2; ++i)
				{
					if(noteObjects[i] is SmallClef smallClef && noteObjects[i + 1] is SmallClef)
					{
						if(voiceIndex == 0)
						{
							MessageBox.Show($"Removing a redundant SmallClef (type:{smallClef.ClefType}).\n" +
										$"   systemIndex:{systemIndex} staffIndex:{staffIndex} voiceIndex:{voiceIndex}", "Warning");
						}
						clefsToRemove.Add(smallClef);
					}
				}
			}
			if(clefsToRemove.Count > 0)
			{
				foreach(SmallClef smallClef in clefsToRemove)
				{
					noteObjects.Remove(smallClef);
				}
			}
		}

        protected SvgPage NewSvgPage(string filePath, int pageNumber, ref int systemIndex, bool graphicsOnly)
        {
            TextInfo infoTextInfo = GetInfoTextAtTopOfPage(filePath);

            List<SvgSystem> systemsOnPage = new List<SvgSystem>();
            bool lastPage = true;
            double systemHeight = 0;
            double frameHeight;
            if(pageNumber == 1)
                frameHeight = M.PageFormat.FirstPageFrameHeight;
            else
                frameHeight = M.PageFormat.OtherPagesFrameHeight;

            double systemHeightsTotal = 0;
            while(systemIndex < Systems.Count)
            {
                M.Assert(Systems[systemIndex].Metrics != null);
                M.Assert(Systems[systemIndex].Metrics.StafflinesTop == 0);

                systemHeight = Systems[systemIndex].Metrics.NotesBottom - Systems[systemIndex].Metrics.NotesTop;

                systemHeightsTotal += systemHeight;
                if(systemHeightsTotal > frameHeight)
                {
                    lastPage = false;
                    break;
                }

                systemHeightsTotal += M.PageFormat.DefaultDistanceBetweenSystemsVBPX;

                systemsOnPage.Add(Systems[systemIndex]);

                systemIndex++;
            }

            return new SvgPage(this, M.PageFormat, pageNumber, infoTextInfo, systemsOnPage, lastPage);
        }

        private TextInfo GetInfoTextAtTopOfPage(string filePath)
        {
            string infoString = Path.GetFileName(filePath);				

            if(MetadataWithDate != null)
                infoString += (", " + MetadataWithDate.Date);

            return new TextInfo(infoString, "Arial", M.PageFormat.TimeStampFontHeight, TextHorizAlign.left);
        }


        /// <summary>
        /// The first duration symbol in the staff.
        /// </summary>
        /// <returns></returns>
        protected DurationSymbol FirstDurationSymbol(Staff staff)
        {
            DurationSymbol firstDurationSymbol = null;
            Voice voice = staff.Voices[0];
            foreach(NoteObject noteObject in voice.NoteObjects)
            {
                firstDurationSymbol = noteObject as DurationSymbol;
                if(firstDurationSymbol != null)
                    break;
            }
            return firstDurationSymbol;
        }

        #endregion protected functions

        /// <summary>
        /// Adds a bar number to the first Barline in the top voice of each system except the first.
        /// </summary>
        private void AddBarNumbers()
        {
            int barNumber = 1;
            foreach(SvgSystem system in Systems)
            {
                Voice topVoice = system.Staves[0].Voices[0];
                bool isFirstBarline = true;
                for(int i = 0; i < topVoice.NoteObjects.Count - 1; i++)
                {
                    if(topVoice.NoteObjects[i] is Barline barline)
                    {
                        if(isFirstBarline && system != Systems[0])
                        {
                            FramedBarNumberText framedBarNumber = new FramedBarNumberText(this, barNumber.ToString(), M.PageFormat.GapVBPX, M.PageFormat.StafflineStemStrokeWidthVBPX);

                            barline.DrawObjects.Add(framedBarNumber);
                            isFirstBarline = false;
                        }
                        barNumber++;
                    }
                }
            }
        }

		/// <summary>
		/// Adds a FramedRegionStartText to each Barline that is the start of one or more (repeat) regions.
		/// Such regionStart info is left-aligned to the barline, so is never added to the *final* barline on a system
		/// </summary>
		private void AddRegionStartInfo()
		{
			SortedDictionary<int,List<string>> regionStartData = ScoreData.RegionSequence.barlineStartRegionsDict;

			var regionStartDataBarIndices = new List<int>(regionStartData.Keys);
			int lastRegionStartBarIndex = regionStartDataBarIndices[regionStartDataBarIndices.Count - 1];

			int barlineIndex = 0;
			List<Barline> barlines = new List<Barline>();
			foreach(SvgSystem system in Systems)
			{
				Voice topVoice = system.Staves[0].Voices[0];
				barlines.Clear();
				for(int i = 0; i < topVoice.NoteObjects.Count; i++)
				{					
					if(topVoice.NoteObjects[i] is NormalBarline barline)
					{
						barlines.Add(barline);
					}
				}
				barlines.RemoveAt(barlines.Count - 1); // ignore the final barline on the voice (system)

				foreach(Barline barline in barlines)
				{
					if(regionStartDataBarIndices.Contains(barlineIndex))
					{
						FramedRegionStartText frst = new FramedRegionStartText(this, regionStartData[barlineIndex], M.PageFormat.GapVBPX, M.PageFormat.StafflineStemStrokeWidthVBPX);
						barline.DrawObjects.Add(frst);
					}
					barlineIndex++;
					if(barlineIndex > lastRegionStartBarIndex)
					{
						break;
					}
				}

				if(barlineIndex > lastRegionStartBarIndex)
				{
					break;
				}
			}
		}

		/// <summary>
		/// Adds a FramedRegionEndText to each Barline that is the end of one or more regions.
		/// Such regionEnd info is right-aligned to the barline, so is never added to the *first* barline on a system
		/// </summary>
		private void AddRegionEndInfo()
		{
			SortedDictionary<int, List<string>> regionEndData = ScoreData.RegionSequence.barlineRegionLinksDict;

			var regionEndDataBarIndices = new List<int>(regionEndData.Keys);
			int lastRegionEndBarIndex = regionEndDataBarIndices[regionEndDataBarIndices.Count - 1];

			int barlineIndex = 1; // the first barline is going to be ignored. 
			List<Barline> barlines = new List<Barline>();
			foreach(SvgSystem system in Systems)
			{
				Voice topVoice = system.Staves[0].Voices[0];
				barlines.Clear();
				for(int i = 0; i < topVoice.NoteObjects.Count; i++)
				{
					if(topVoice.NoteObjects[i] is Barline barline)
					{
						barlines.Add(barline);
					}
				}

				barlines.RemoveAt(0); // ignore the first barline on the voice (system)

				foreach(Barline barline in barlines)
				{
					if(regionEndDataBarIndices.Contains(barlineIndex))
					{
						FramedRegionEndText fret = new FramedRegionEndText(this, regionEndData[barlineIndex], M.PageFormat.GapVBPX, M.PageFormat.StafflineStemStrokeWidthVBPX);
						barline.DrawObjects.Add(fret);
					}
					barlineIndex++;
					if(barlineIndex > lastRegionEndBarIndex)
					{
						break;
					}
				}

				if(barlineIndex > lastRegionEndBarIndex)
				{
					break;
				}
			}
		}

        #endregion functions

    }
}

