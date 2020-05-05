using System;
using System.Collections.Generic;
using System.Windows.Forms; // MessageBox
using System.Diagnostics;
using System.Xml;
using System.Text;

using MNX.Globals;
using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
    public class SvgPage
    {
        /// <summary>
        /// The systems contain Metrics info, but their top staffline is at 0.
        /// The systems are moved to their correct vertical positions on the page here.
		/// If pageNumber is set to 0, all the systems in pageSystems will be printed
		/// in a single .svg file, whose page height has been changed accordingly.
        /// </summary>
        /// <param name="Systems"></param>
        public SvgPage(SvgScore containerScore, PageFormat pageFormat, int pageNumber, TextInfo infoTextInfo, List<SvgSystem> pageSystems, bool lastPage)
        {
            _score = containerScore;
            _pageFormat = pageFormat;
            _pageNumber = pageNumber;
            _infoTextInfo = infoTextInfo;

            Systems = pageSystems;

            MoveSystemsVertically(pageFormat, pageSystems, (pageNumber == 1 || pageNumber == 0), lastPage);
        }



        /// <summary>
        /// Moves the systems to their correct vertical position. Justifies on all but the last page.
        /// On the first page use pageFormat.FirstPageFrameHeight.
        /// On the last page (which may also be the first), the systems are separated by 
        /// pageFormat.MinimumDistanceBetweenSystems.
        /// </summary>
        private void MoveSystemsVertically(PageFormat pageFormat, List<SvgSystem> pageSystems, bool firstPage, bool lastPage)
        {
            double frameTop;
            double frameHeight;
            if(firstPage)
            {
                frameTop = pageFormat.TopMarginPage1VBPX;
				frameHeight = pageFormat.FirstPageFrameHeight; // property uses BottomMarginPos
            }
            else
            {
                frameTop = pageFormat.TopMarginOtherPagesVBPX;
                frameHeight = pageFormat.OtherPagesFrameHeight;
            }

            MoveSystemsVertically(pageSystems, frameTop, frameHeight, pageFormat.DefaultDistanceBetweenSystemsVBPX, lastPage);
        }

        private void MoveSystemsVertically(List<SvgSystem> pageSystems, double frameTop, double frameHeight, double defaultSystemSeparation, bool lastPage)
        {
            double systemSeparation = 0;
            if(lastPage) // dont justify
            {
                systemSeparation = defaultSystemSeparation;
            }
            else
            {                
                if(pageSystems.Count >= 1)
                {
                    double totalSystemHeight = 0;
                    foreach(SvgSystem system in pageSystems)
                    {
                        M.Assert(system.Metrics != null);
                        totalSystemHeight += (system.Metrics.NotesBottom - system.Metrics.NotesTop);
                    }
                    systemSeparation = (frameHeight - totalSystemHeight) / (pageSystems.Count - 1);
                }
            }

            double top = frameTop;
            foreach(SvgSystem system in pageSystems)
            {
                if(system.Metrics != null)
                {
                    double deltaY = top - system.Metrics.NotesTop;
                    // Limit stafflineHeight to multiples of _pageMetrics.Gap
                    // so that stafflines are not displayed as thick grey lines.
                    // The following works, because the top staffline of each system is currently at 0.
                    deltaY -= (deltaY % _pageFormat.GapVBPX);
                    system.Metrics.Move(0, deltaY);
                    top = system.Metrics.NotesBottom + systemSeparation;
                }
            }
        }

        /// <summary>
        /// Writes this page.
        /// </summary>
        /// <param name="w"></param>
        public void WriteSVG(SvgWriter w, MetadataWithDate metadataWithDate, bool isSinglePageScore, bool graphicsOnly, bool printTitleAndAuthorOnScorePage1)
        {
			int nOutputVoices = 0;
			GetNumberOfVoices(Systems[0], ref nOutputVoices);

            w.WriteStartDocument(); // standalone="no"
            //<?xml-stylesheet href="../../fontsStyleSheet.css" type="text/css"?>
            w.WriteProcessingInstruction("xml-stylesheet", "href=\"../../fontsStyleSheet.css\" type=\"text/css\"");
            w.WriteStartElement("svg", "http://www.w3.org/2000/svg");

            WriteSvgHeader(w, graphicsOnly);

			if(!graphicsOnly)
			{
				metadataWithDate.WriteSVG(w, _pageNumber, _score.PageCount, nOutputVoices);
			}

            _score.WriteDefs(w, _pageNumber);

            if(isSinglePageScore && (!graphicsOnly))
			{
				_score.WriteScoreData(w);
			}

            #region layers

            WriteBackgroundLayer(w, _pageFormat.RightVBPX, _pageFormat.BottomVBPX);

            if(_pageNumber > 0)
			{ 
				WriteFrameLayer(w, _pageFormat.RightVBPX, _pageFormat.BottomVBPX);
			}

			WriteSystemsLayer(w, _pageNumber, metadataWithDate, graphicsOnly, printTitleAndAuthorOnScorePage1);

            w.WriteComment(@" Annotations that are added here will be ignored by the AssistantPerformer. ");

            #endregion layers

            w.WriteEndElement(); // close the svg element
            w.WriteEndDocument();
        }

        private void GetNumberOfVoices(SvgSystem svgSystem, ref int nOutputVoices)
		{
			nOutputVoices = 0;
            foreach(Staff staff in svgSystem.Staves)
			{
				foreach(Voice voice in staff.Voices)
				{
					if(voice is OutputVoice)
					{
						nOutputVoices++;
					}
				}
			}
		}

        private void WriteBackgroundLayer(SvgWriter w, double width, double height)
        {
            w.SvgRect(CSSObjectClass.backgroundFill, 0, 0, width, height);
        }

        private void WriteFrameLayer(SvgWriter w, double width, double height)
		{
            w.SvgRect(CSSObjectClass.frame, 0, 0, width, height);
        }

		private void WriteSystemsLayer(SvgWriter w, int pageNumber, MetadataWithDate metadataWithDate, bool graphicsOnly, bool printTitleAndAuthorOnScorePage1)
		{
            w.SvgStartGroup(CSSObjectClass.systems.ToString());

			w.SvgText(CSSObjectClass.timeStamp, _infoTextInfo.Text, 32, _infoTextInfo.FontHeight);

			if((pageNumber == 1 || pageNumber == 0) && printTitleAndAuthorOnScorePage1)
            {
				WritePage1TitleAndAuthor(w, metadataWithDate);
			}

            List<CarryMsgs> carryMsgsPerChannel = new List<CarryMsgs>();
            foreach(Staff staff in Systems[0].Staves)
            {
                foreach(Voice voice in staff.Voices)
                {
                    carryMsgsPerChannel.Add(new CarryMsgs());
                }
            }

			int systemNumber = 1;
			foreach(SvgSystem system in Systems)
			{
				system.WriteSVG(w, systemNumber++, _pageFormat, carryMsgsPerChannel, graphicsOnly);
			}

			w.WriteEndElement(); // end layer
		}

        private void WriteSvgHeader(SvgWriter w, bool graphicsOnly)
        {
			w.WriteAttributeString("xmlns", "http://www.w3.org/2000/svg");
			// I think the following is redundant...
			//w.WriteAttributeString("xmlns", "svg", null, "http://www.w3.org/2000/svg");
			// Deleted the following, since it is only advisory, and I think the latest version is 2. See deprecated xlink below.
			//w.WriteAttributeString("version", "1.1");

			if(!graphicsOnly)
			{
				// Namespaces used for standard metadata
				w.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
				w.WriteAttributeString("xmlns", "cc", null, "http://creativecommons.org/ns#");
				w.WriteAttributeString("xmlns", "rdf", null, "http://www.w3.org/1999/02/22-rdf-syntax-ns#");


				// Namespace used for linking to svg defs (defined objects)
				// N.B.: xlink is deprecated in SVG 2 
				// w.WriteAttributeString("xmlns", "xlink", null, "http://www.w3.org/1999/xlink");

				// Standard definition of the "score" namespace.
				// The file documents the additional attributes and elements available in the "score" namespace.
				w.WriteAttributeString("xmlns", "score", null, "https://www.james-ingram-act-two.de/open-source/svgScoreExtensions.html");

				// The file defines and documents all the element classes used in this particular scoreType.
				// The definitions include information as to how the classes nest, and the directions in which they are read.
				// For example:
				// 1) in cmn_core files, systems are read from top to bottom on a page, and contain
				//    staves that are read in parallel, left to right.
				// 2) cmn_1950.html files might include elements having class="tupletBracket", but
				//    cmn_core files don't. As with the score namespace, the file does not actually
				//    need to be read by the client code in order to discover the scoreType. 
				w.WriteAttributeString("data-scoreType", null, "https://www.james-ingram-act-two.de/open-source/cmn_core.html");
			}

            w.WriteAttributeString("width", M.DoubleToShortString(_pageFormat.RightVBPX / _pageFormat.ViewBoxMagnification)); // the intended screen display size (100%)
            w.WriteAttributeString("height", M.DoubleToShortString(_pageFormat.BottomVBPX / _pageFormat.ViewBoxMagnification)); // the intended screen display size (100%)
            string viewBox = "0 0 " + _pageFormat.RightVBPX.ToString() + " " + _pageFormat.BottomVBPX.ToString();
            w.WriteAttributeString("viewBox", viewBox); // the size of SVG's internal drawing surface (10 x the width and height -- see)            
        }

		/// <summary>
		/// Adds the main title and the author to the first page.
		/// </summary>
		protected void WritePage1TitleAndAuthor(SvgWriter w, MetadataWithDate metadataWithDate)
		{
			string titlesFontFamily = "Open Sans";

			TextInfo titleInfo =
				new TextInfo(metadataWithDate.Title, titlesFontFamily, _pageFormat.Page1TitleHeightVBPX,
					null, TextHorizAlign.center);
			TextInfo authorInfo =
			  new TextInfo(metadataWithDate.Author, titlesFontFamily, _pageFormat.Page1AuthorHeightVBPX,
				  null, TextHorizAlign.right);
			w.WriteStartElement("g");
			w.WriteAttributeString("class", CSSObjectClass.titles.ToString());
			w.SvgText(CSSObjectClass.mainTitle, titleInfo.Text, _pageFormat.RightVBPX / 2, _pageFormat.Page1TitleYVBPX);
			w.SvgText(CSSObjectClass.author, authorInfo.Text, _pageFormat.RightMarginPosVBPX, _pageFormat.Page1TitleYVBPX);
			w.WriteEndElement(); // group
		}

        #region used when creating graphic score
        private readonly SvgScore _score;
        private PageFormat _pageFormat;
        private readonly int _pageNumber;
        private TextInfo _infoTextInfo;
        #endregion

        public List<SvgSystem> Systems = new List<SvgSystem>();
    }
}
