using System;
using System.Text;

namespace MNXtoSVG
{
    /// <summary>
    /// The first part of these names were derived from the SMuFL names at 
    /// https://w3c.github.io/smufl/gitbook/tables/individual-notes.html
    /// (SMuFL defines these 12 symbols)
    /// </summary>
    public enum MNXC_DurationSymbolType
    {
        undefined,
        noteDoubleWhole_breve,
        noteWhole_semibreve,
        noteHalf_minim,
        noteQuarter_crotchet,
        note8th_1flag_quaver,
        note16th_2flags_semiquaver,
        note32nd_3flags_demisemiquaver,
        note64th_4flags,
        note128th_5flags,
        note256th_6flags,
        note512th_7flags,
        note1024th_8flags
    }

    public class MNXC_Duration
    {
        public readonly int Multiple;
        public readonly MNXC_DurationSymbolType DurationSymbolType = MNXC_DurationSymbolType.undefined;
        public readonly int NumberOfDots;        

        /// <summary>
        /// The value argument is the MNX duration symbol string ("/2", "/4", "/8d" etc.)
        /// I assume that this string can have an arbitrary number of 'd's corresponding to the number of dots.
        /// </summary>
        public MNXC_Duration(string value)
        {
        // https://w3c.github.io/mnx/specification/common/#note-value
        // https://w3c.github.io/mnx/specification/common/#base-note-values
        // https://w3c.github.io/mnx/specification/common/#ref-for-note-value%E2%91%A0

            Tuple<int, MNXC_DurationSymbolType, int>  analysis = StringAnalysis(value);
            Multiple = analysis.Item1;
            DurationSymbolType = analysis.Item2;
            NumberOfDots = analysis.Item3;
        }

        /// <summary>
        /// returns Item 1:multiple, Item2:durationSymbolType, Item3: numberOfDots
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private Tuple<int, MNXC_DurationSymbolType, int> StringAnalysis(string value)
        {
            /**********************************************/
            /*  local functions ***************************/

            // removes the trailing 'd' characters (if any) from sbDots
            int GetNDots(StringBuilder sbDots)
            {
                int dots = 0;                

                for(int i = value.Length - 1; i >= 0; i--)
                {
                    if(value[i] == 'd')
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

                MNXC_DurationSymbolType s = MNXC_DurationSymbolType.undefined;
                switch(symb.ToString())
                {
                    case "2":
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

            StringBuilder sb = new StringBuilder(value);

            int nDots = GetNDots(sb);

            MNXC_DurationSymbolType symbol = GetSymbol(sb);

            int multiple = GetMultiple(sb);

            Tuple<int, MNXC_DurationSymbolType, int> rval = new Tuple<int, MNXC_DurationSymbolType, int>(multiple, symbol, nDots);

            return rval;
        }


    }
}