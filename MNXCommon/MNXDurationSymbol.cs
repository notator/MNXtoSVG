using System;
using System.Text;
using MNX.Globals;

namespace MNX.Common
{
    /// <summary>
    /// The duration of DurationSymbols is measured in ticks.
    /// The default number of ticks per (undotted, single) MNXDurationSymbol are:
    ///          noteDoubleWhole_breve: 8192 ticks
    ///            noteWhole_semibreve: 4096 ticks
    ///                 noteHalf_minim: 2048 ticks
    ///           noteQuarter_crotchet: 1024 ticks
    ///           note8th_1flag_quaver: 512 ticks
    ///     note16th_2flags_semiquaver: 256 ticks
    /// note32nd_3flags_demisemiquaver: 128 ticks
    ///                note64th_4flags: 64 ticks
    ///               note128th_5flags: 32 ticks
    ///               note256th_6flags: 16 ticks
    ///               note512th_7flags: 8 ticks
    ///              note1024th_8flags: 4 ticks
    /// Adding augmentation dots adds the corresponding number of ticks
    /// (A dotted crotchet has 1024 + 512 = 1536 ticks,
    /// a double-dotted crotchet has 1024 + 512 + 256 = 1792 ticks, etc.) 
    /// The actual number of ticks per duration symbol is also affected
    /// if it is part of a Tuplet or Grace.
    /// Ticks can be converted to milliseconds when a tempo is provided.
    /// </summary>
    public class MNXDurationSymbol // N.B. This is not an ITicks. (See Ticks below)
    {
        public readonly int? Multiple = null;
        public readonly DurationSymbolType? DurationSymbolTyp = null;
        public readonly int? NAugmentationDots = null;

        // MNXDurationSymbol does not implement TicksPositionInScore.
        public int TicksDuration { get; set; }

        /// <summary>
        /// The value argument is the MNXCommon duration symbol string ("/2", "/4", "/8d" etc.)
        /// It can also be a floating point number (the fraction of a wholeNote).
        /// I assume that this string can have an arbitrary number of 'd's corresponding to the number of dots.
        /// </summary>
        public MNXDurationSymbol(string value)
        {
            // https://w3c.github.io/mnx/specification/common/#note-value
            // https://w3c.github.io/mnx/specification/common/#base-note-values
            // https://w3c.github.io/mnx/specification/common/#ref-for-note-value%E2%91%A0

            Tuple<int, DurationSymbolType, int> analysis = StringAnalysis(value);
            Multiple = analysis.Item1;
            DurationSymbolTyp = analysis.Item2;
            NAugmentationDots = analysis.Item3;

            TicksDuration = GetDefaultTicks(); // can be 0!
        }

        /// <summary>
        /// returns Item 1:multiple, Item2:durationSymbolType, Item3: numberOfDots
        /// </summary>
        private Tuple<int, DurationSymbolType, int> StringAnalysis(string ctorArg)
        {
            /**********************************************/
            /*  local functions ***************************/

            // removes the trailing 'd' characters (if any) from sbDots
            int GetNDots(StringBuilder sbDots)
            {
                int dots = 0;

                for(int i = ctorArg.Length - 1; i >= 0; i--)
                {
                    if(ctorArg[i] == 'd')
                    {
                        dots++;
                        sbDots.Length--;
                    }
                    else break;
                }
                return dots;
            }

            // removes the trailing symbol type characters from sbType
            DurationSymbolType GetSymbol(StringBuilder sbType)
            {
                StringBuilder symb = new StringBuilder();
                while(sbType.Length > 0)
                {
                    char c = sbType[sbType.Length - 1];
                    symb.Insert(0, c);
                    sbType.Length--;
                    if(c == '/')
                        break;
                }

                DurationSymbolType s = DurationSymbolType.noteQuarter_crotchet;
                switch(symb.ToString())
                {
                    case "*2":
                        s = DurationSymbolType.noteDoubleWhole_breve;
                        break;
                    case "1":
                    case "/1":
                        s = DurationSymbolType.noteWhole_semibreve;
                        break;
                    case "/2":
                        s = DurationSymbolType.noteHalf_minim;
                        break;
                    case "/4":
                        s = DurationSymbolType.noteQuarter_crotchet;
                        break;
                    case "/8":
                        s = DurationSymbolType.note8th_1flag_quaver;
                        break;
                    case "/16":
                        s = DurationSymbolType.note16th_2flags_semiquaver;
                        break;
                    case "/32":
                        s = DurationSymbolType.note32nd_3flags_demisemiquaver;
                        break;
                    case "/64":
                        s = DurationSymbolType.note64th_4flags;
                        break;
                    case "/128":
                        s = DurationSymbolType.note128th_5flags;
                        break;
                    case "/256":
                        s = DurationSymbolType.note256th_6flags;
                        break;
                    case "/512":
                        s = DurationSymbolType.note512th_7flags;
                        break;
                    case "/1024":
                        s = DurationSymbolType.note1024th_8flags;
                        break;
                    default:
                        M.ThrowError("Error: unknown duration symbol");
                        break;
                }

                return s;
            }

            // returns 1 if sbMult is empty
            int GetMultiple(StringBuilder sbMult)
            {
                int mult = 1;
                if(sbMult.Length > 0)
                {
                    int.TryParse(sbMult.ToString(), out mult);
                }
                return mult;
            }

            int multiple = 0;
            DurationSymbolType symbol = DurationSymbolType.noteQuarter_crotchet;
            int nDots = 0;

            if(string.Compare(ctorArg, "0") != 0)
            {
                StringBuilder sb = new StringBuilder(ctorArg);

                nDots = GetNDots(sb);

                symbol = GetSymbol(sb);

                multiple = GetMultiple(sb);
            }

            Tuple<int, DurationSymbolType, int> rval = new Tuple<int, DurationSymbolType, int>(multiple, symbol, nDots);

            return rval;
        }

        /// <summary>
        /// Returns the default number of ticks associated with Multiple, DurationSymbolType and NumberOfDots.
        /// (taking no account of tuplets, grace notes etc.)
        /// </summary>
        public int GetDefaultTicks()
        {
            int dots = (int)NAugmentationDots;
            int baseTicks = M.DurationSymbolTicks[(int)DurationSymbolTyp];
            int extraTicks = baseTicks / 2;
            int rval = baseTicks;
            while(dots > 0 && extraTicks > 0)
            {
                dots--;
                rval += extraTicks;
                extraTicks /= 2;
            }

            rval *= (int)Multiple;

            return rval;
        }
    }
}