using MNX.Common;
using MNX.Globals;
using Moritz.Xml;
using System;
using System.Text;

namespace Moritz.Symbols
{
	/// <summary>
	/// DurationSymbols are NoteObjects which have a logical width (and symbolize a duration)
	/// </summary>
	public abstract class DurationSymbol : AnchorageSymbol 
    {
        /// <summary>
        /// Used by MNX.Common
        /// </summary>
        public DurationSymbol(Voice voice, int msDuration, int absMsPosition, MNXDurationSymbol mnxDurationSymbol, double fontHeight)
            : base(voice, fontHeight)
        {
            _msDuration = msDuration;
            AbsMsPosition = absMsPosition;
            this.SetDurationClass((DurationSymbolType)mnxDurationSymbol.DurationSymbolTyp);
            _nAugmentationDots = mnxDurationSymbol.NAugmentationDots ?? 0;
        }

        /// <summary>
        /// note that Moritz' DurationClass.cautionary type is never set here
        /// </summary>
        /// <param name="durationSymbolType"></param>
        private void SetDurationClass(MNX.Common.DurationSymbolType durationSymbolType)
        {
            switch (durationSymbolType)
            {
                case DurationSymbolType.noteDoubleWhole_breve:
                    _durationClass = DurationClass.breve;
                    break;
                case DurationSymbolType.noteWhole_semibreve:
                    _durationClass = DurationClass.semibreve;
                    break;
                case DurationSymbolType.noteHalf_minim:
                    _durationClass = DurationClass.minim;
                    break;
                case DurationSymbolType.noteQuarter_crotchet:
                    _durationClass = DurationClass.crotchet;
                    break;
                case DurationSymbolType.note8th_1flag_quaver:
                    _durationClass = DurationClass.quaver;
                    break;
                case DurationSymbolType.note16th_2flags_semiquaver:
                    _durationClass = DurationClass.semiquaver;
                    break;
                case DurationSymbolType.note32nd_3flags_demisemiquaver:
                    _durationClass = DurationClass.threeFlags;
                    break;
                case DurationSymbolType.note64th_4flags:
                    _durationClass = DurationClass.fourFlags;
                    break;
                case DurationSymbolType.note128th_5flags:
                    _durationClass = DurationClass.fiveFlags;
                    break;
                case DurationSymbolType.note256th_6flags:
                    _durationClass = DurationClass.sixFlags;
                    break;
                case DurationSymbolType.note512th_7flags:
                    _durationClass = DurationClass.sevenFlags;
                    break;
                case DurationSymbolType.note1024th_8flags:
                    _durationClass = DurationClass.eightFlags;
                    break;
            }
        }

        /// <summary>
        /// Used by Moritz' Assistant Composer
        /// </summary>
        public DurationSymbol(Voice voice, int msDuration, int absMsPosition, int minimumCrotchetDuration, double fontHeight)
            : base(voice, fontHeight)
        {
            _msDuration = msDuration;
            AbsMsPosition = absMsPosition;
            this.SetDurationClass(MsDuration, minimumCrotchetDuration);
        }

        /// <summary>
        /// The duration class is DurationClass.cautionary if the duration is zero
        /// The duration class is DurationClass.breve if the duration is >= (minimumCrotchetDuration * 8).
        /// The minimumCrotchetDuration will usually be set to something like 1200ms.
        /// </summary>
        private void SetDurationClass(int msDuration, int minimumCrotchetDuration)
        {
            //_msDuration = durationMS;
            MinimumCrotchetDuration = minimumCrotchetDuration;
            if(msDuration == 0)
                _durationClass = DurationClass.cautionary;
            else if(msDuration < (MinimumCrotchetDuration / 16))
                _durationClass = DurationClass.fiveFlags;
            else if(msDuration < (MinimumCrotchetDuration / 8))
                _durationClass = DurationClass.fourFlags;
            else if(msDuration < (MinimumCrotchetDuration / 4))
                _durationClass = DurationClass.threeFlags;
            else if(msDuration < (MinimumCrotchetDuration / 2))
                _durationClass = DurationClass.semiquaver;
            else if(msDuration < MinimumCrotchetDuration)
                _durationClass = DurationClass.quaver;
            else if(msDuration < (MinimumCrotchetDuration * 2))
                _durationClass = DurationClass.crotchet;
            else if(msDuration < (MinimumCrotchetDuration * 4))
                _durationClass = DurationClass.minim;
            else if(msDuration < (MinimumCrotchetDuration * 8))
                _durationClass = DurationClass.semibreve;
            else _durationClass = DurationClass.breve;
        }

        //public virtual void WriteSVG(SvgWriter w, int msPos) {}

        protected string InfoString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("staff=" + Voice.Staff.Staffname + " ");
                sb.Append("absMsPos=" + this.AbsMsPosition.ToString() + " ");
                sb.Append("msDur=" + this.MsDuration.ToString());
                return sb.ToString();
            }
        }

        /// <summary>
        /// The position from the beginning of the piece.
        /// </summary>
        public int AbsMsPosition = 0;

        public virtual int MsDuration 
        { 
            get { return _msDuration; }
            set { M.Assert(false, "This property should only be set when agglomerating RestSymbols."); } 
        }
        protected int _msDuration = 0;

        // these fields are readonly
        public DurationClass DurationClass { get { return _durationClass; } }
        public int NAugmentationDots { get { return _nAugmentationDots; } }
        public int MinimumCrotchetDuration { get; private set; }

        protected DurationClass _durationClass = DurationClass.none;
        protected int _nAugmentationDots;
    }
}
