
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Accidental : IWritable
    {
        public readonly MNXCommonAccidental? Type;

        public Accidental(string value)
        {
            switch(value)
            {
                case "auto":
                    Type = MNXCommonAccidental.auto;
                    break;
                case "sharp":
                    Type = MNXCommonAccidental.sharp;
                    break;
                case "natural":
                    Type = MNXCommonAccidental.natural;
                    break;
                case "flat":
                    Type = MNXCommonAccidental.flat;
                    break;
                case "double-sharp":
                    Type = MNXCommonAccidental.doubleSharp;
                    break;
                case "sharp-sharp":
                    Type = MNXCommonAccidental.sharpSharp;
                    break;
                case "flat-flat":
                    Type = MNXCommonAccidental.flatFlat;
                    break;
                case "natural-sharp":
                    Type = MNXCommonAccidental.naturalSharp;
                    break;
                case "natural-flat":
                    Type = MNXCommonAccidental.naturalFlat;
                    break;
                case "quarter-flat":
                    Type = MNXCommonAccidental.quarterFlat;
                    break;
                case "quarter-sharp":
                    Type = MNXCommonAccidental.quarterSharp;
                    break;
                case "three-quarters-flat":
                    Type = MNXCommonAccidental.threeQuartersFlat;
                    break;
                case "three-quarters-sharp":
                    Type = MNXCommonAccidental.threeQuartersSharp;
                    break;
                case "sharp-down":
                    Type = MNXCommonAccidental.sharpDown;
                    break;
                case "sharp-up":
                    Type = MNXCommonAccidental.sharpUp;
                    break;
                case "natural-down":
                    Type = MNXCommonAccidental.naturalDown;
                    break;
                case "natural-up":
                    Type = MNXCommonAccidental.naturalUp;
                    break;
                case "flat-down":
                    Type = MNXCommonAccidental.flatDown;
                    break;
                case "flat-up":
                    Type = MNXCommonAccidental.flatUp;
                    break;
                case "triple-sharp":
                    Type = MNXCommonAccidental.tripleSharp;
                    break;
                case "triple-flat":
                    Type = MNXCommonAccidental.tripleFlat;
                    break;
                default:
                    G.ThrowError("Error: unknown accidental type.");
                    break;
            }
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new System.NotImplementedException();
        }
    }
}