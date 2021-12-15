using MNX.Globals;

using System.Xml;

namespace MNX.Common
{
    public class TextBlock : IDirectionsComponent
    {
        public readonly string Text;
        public readonly int TicksPosInScore = -1; // set in ctor

        public TextBlock(XmlReader r, int ticksPosInScore)
        {
            M.Assert(r.Name == "text-block");

            XmlReaderSettings localSettings = new XmlReaderSettings();
            localSettings.DtdProcessing = DtdProcessing.Parse;

            TicksPosInScore = ticksPosInScore;

            using(XmlReader htmlReader = XmlReader.Create(r, localSettings))
            {
                htmlReader.Read();
                Text = htmlReader.Value;
            }

            //r.Read();
            //Text = r.Value;

            M.ReadToXmlElementTag(r, "text-block");


            M.Assert(r.Name == "text-block"); // end of "directions"
        }
    }
}