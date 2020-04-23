using MNX.Globals;

namespace MNX.Common
{
    public class Accidental
    {
        public readonly AccidentalType? Type;

        public Accidental(string value)
        {
            switch(value)
            {
                case "auto":
                    Type = AccidentalType.auto;
                    break;
                case "sharp":
                    Type = AccidentalType.sharp;
                    break;
                case "natural":
                    Type = AccidentalType.natural;
                    break;
                case "flat":
                    Type = AccidentalType.flat;
                    break;
                case "double-sharp":
                    Type = AccidentalType.doubleSharp;
                    break;
                case "sharp-sharp":
                    Type = AccidentalType.sharpSharp;
                    break;
                case "flat-flat":
                    Type = AccidentalType.flatFlat;
                    break;
                case "natural-sharp":
                    Type = AccidentalType.naturalSharp;
                    break;
                case "natural-flat":
                    Type = AccidentalType.naturalFlat;
                    break;
                case "quarter-flat":
                    Type = AccidentalType.quarterFlat;
                    break;
                case "quarter-sharp":
                    Type = AccidentalType.quarterSharp;
                    break;
                case "three-quarters-flat":
                    Type = AccidentalType.threeQuartersFlat;
                    break;
                case "three-quarters-sharp":
                    Type = AccidentalType.threeQuartersSharp;
                    break;
                case "sharp-down":
                    Type = AccidentalType.sharpDown;
                    break;
                case "sharp-up":
                    Type = AccidentalType.sharpUp;
                    break;
                case "natural-down":
                    Type = AccidentalType.naturalDown;
                    break;
                case "natural-up":
                    Type = AccidentalType.naturalUp;
                    break;
                case "flat-down":
                    Type = AccidentalType.flatDown;
                    break;
                case "flat-up":
                    Type = AccidentalType.flatUp;
                    break;
                case "triple-sharp":
                    Type = AccidentalType.tripleSharp;
                    break;
                case "triple-flat":
                    Type = AccidentalType.tripleFlat;
                    break;
                default:
                    M.ThrowError("Error: unknown accidental type.");
                    break;
            }
        }
    }
}