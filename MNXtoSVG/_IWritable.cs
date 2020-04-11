
using System.Xml;

namespace MNXtoSVG
{
    public interface IWritable
    {
        void WriteSVG(XmlWriter w);
    }
}
