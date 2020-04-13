using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;
using System.Text;

namespace MNXtoSVG
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
    public class MNXC_Duration
    {
        private readonly int _tupletlevel = -1;
        public readonly int? Multiple = null;
        public readonly MNXC_DurationSymbolType? DurationSymbolType = null;
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
        } 

        /// <summary>
        /// The value argument is the MNXC duration symbol string ("/2", "/4", "/8d" etc.)
        /// It can also be a floating point number (the fraction of a wholeNote).
        /// I assume that this string can have an arbitrary number of 'd's corresponding to the number of dots.
        /// </summary>
        public MNXC_Duration(string value, int currentTupletLevel)
        {
            /**********************************************/
            /*  local functions ***************************/

            /// <summary>
            /// returns Item 1:multiple, Item2:durationSymbolType, Item3: numberOfDots
            /// </summary>
            Tuple<int, MNXC_DurationSymbolType, int> StringAnalysis(string ctorArg)
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
                MNXC_DurationSymbolType GetSymbol(StringBuilder sbType)
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

                    MNXC_DurationSymbolType s = MNXC_DurationSymbolType.noteQuarter_crotchet;
                    switch(symb.ToString())
                    {
                        case "*2":
                            s = MNXC_DurationSymbolType.noteDoubleWhole_breve;
                            break;
                        case "1":
                        case "/1":
                            s = MNXC_DurationSymbolType.noteWhole_semibreve;
                            break;
                        case "/2":
                            s = MNXC_DurationSymbolType.noteHalf_minim;
                            break;
                        case "/4":
                            s = MNXC_DurationSymbolType.noteQuarter_crotchet;
                            break;
                        case "/8":
                            s = MNXC_DurationSymbolType.note8th_1flag_quaver;
                            break;
                        case "/16":
                            s = MNXC_DurationSymbolType.note16th_2flags_semiquaver;
                            break;
                        case "/32":
                            s = MNXC_DurationSymbolType.note32nd_3flags_demisemiquaver;
                            break;
                        case "/64":
                            s = MNXC_DurationSymbolType.note64th_4flags;
                            break;
                        case "/128":
                            s = MNXC_DurationSymbolType.note128th_5flags;
                            break;
                        case "/256":
                            s = MNXC_DurationSymbolType.note256th_6flags;
                            break;
                        case "/512":
                            s = MNXC_DurationSymbolType.note512th_7flags;
                            break;
                        case "/1024":
                            s = MNXC_DurationSymbolType.note1024th_8flags;
                            break;
                        default:
                            G.ThrowError("Error: unknown duration symbol");
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

                MNXC_DurationSymbolType symbol = GetSymbol(sb);

                int multiple = GetMultiple(sb);

                Tuple<int, MNXC_DurationSymbolType, int> rval = new Tuple<int, MNXC_DurationSymbolType, int>(multiple, symbol, nDots);

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

            Tuple<int, MNXC_DurationSymbolType, int> analysis = StringAnalysis(value);
            Multiple = analysis.Item1;
            DurationSymbolType = analysis.Item2;
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
            int baseTicks = G.MNXC_DurationSymbolTicks[(int)DurationSymbolType];
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

        public void SetTicks(int ticks, int tupletLevel)
        {
            G.Assert(_ticks == 0, "Error: Ticks has already been set.");
            G.Assert(ticks > 0, "Error: Ticks cannot be less than 1.");

            if(_tupletlevel == tupletLevel)
            {
                _ticks = ticks;
            }
        }

    }
}