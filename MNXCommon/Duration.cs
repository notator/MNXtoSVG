using System;
using System.Text;
using MNX.AGlobals;

namespace MNX.Common
{
    /// <summary>
    /// Duration is measured in ticks.
    /// If there are no tuplets involved, the following correspondences exist:
    ///     noteDoubleWhole_breve, // 8192 ticks
    ///     noteWhole_semibreve,   // 4096 ticks
    ///     noteHalf_minim,        // 2048 ticks
    ///     noteQuarter_crotchet,  // 1024 ticks
    ///     note8th_1flag_quaver,  // 512 ticks
    ///     note16th_2flags_semiquaver, // 256 ticks
    ///     note32nd_3flags_demisemiquaver, // 128 ticks
    ///     note64th_4flags,  // 64 ticks
    ///     note128th_5flags, // 32 ticks
    ///     note256th_6flags, // 16 ticks
    ///     note512th_7flags, // 8 ticks
    ///     note1024th_8flags // 4 ticks
    /// Ticks can be coverted to milliseconds when a tempo is provided.
    /// The symbol-tick correspondence changes when tuplets are involved.
    /// </summary>
    public class Duration
    {
        private readonly int _tupletlevel = -1;
        public readonly int? Multiple = null;
        public readonly DurationSymbolType? DurationSymbolTyp = null;
        public readonly int? NumberOfDots = null;

        private int _ticks = 0;
        public int Ticks
        {
            get
            {
                if(_ticks == 0)
                {
                    throw new ApplicationException("Error: Ticks has not yet been set.");
                }
                return _ticks;
            }
            set
            {
                A.Assert(_ticks == 0, "Error: Ticks has already been set.");
                A.Assert(value > 0, "Error: Ticks cannot be less than 1.");

                _ticks = value;
            }
        } 

        /// <summary>
        /// The value argument is the MNXC duration symbol string ("/2", "/4", "/8d" etc.)
        /// It can also be a floating point number (the fraction of a wholeNote).
        /// I assume that this string can have an arbitrary number of 'd's corresponding to the number of dots.
        /// </summary>
        public Duration(string value, int currentTupletLevel)
        {
            /**********************************************/
            /*  local functions ***************************/

            /// <summary>
            /// returns Item 1:multiple, Item2:durationSymbolType, Item3: numberOfDots
            /// </summary>
            Tuple<int, DurationSymbolType, int> StringAnalysis(string ctorArg)
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
                            A.ThrowError("Error: unknown duration symbol");
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

                /*  end local functions *********************/
                /********************************************/

                StringBuilder sb = new StringBuilder(ctorArg);

                int nDots = GetNDots(sb);

                DurationSymbolType symbol = GetSymbol(sb);

                int multiple = GetMultiple(sb);

                Tuple<int, DurationSymbolType, int> rval = new Tuple<int, DurationSymbolType, int>(multiple, symbol, nDots);

                return rval;
            }

            /*  end local functions *********************/
            /********************************************/

            // https://w3c.github.io/mnx/specification/common/#note-value
            // https://w3c.github.io/mnx/specification/common/#base-note-values
            // https://w3c.github.io/mnx/specification/common/#ref-for-note-value%E2%91%A0

            _tupletlevel = currentTupletLevel;

            if(value.IndexOf('.') >= 0)
            {
                double.TryParse(value, out double factor);
                _ticks = (int) Math.Round(4096 * factor);
            }

            Tuple<int, DurationSymbolType, int> analysis = StringAnalysis(value);
            Multiple = analysis.Item1;
            DurationSymbolTyp = analysis.Item2;
            NumberOfDots = analysis.Item3;

            if(_tupletlevel == 0 && _ticks == 0)
            {
                _ticks = GetBasicTicks();
            }
        }

        /// <summary>
        /// Returns the basic number of ticks associated with Multiple, DurationSymbolType and NumberOfDots.
        /// </summary>
        public int GetBasicTicks()
        {
            int dots = (int)NumberOfDots;
            int baseTicks = B.DurationSymbolTicks[(int)DurationSymbolTyp];
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