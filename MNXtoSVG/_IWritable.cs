
using System.Xml;

namespace MNXtoSVG
{
    interface IWritable
    {
        void WriteSVG(XmlWriter w);
    }
}
