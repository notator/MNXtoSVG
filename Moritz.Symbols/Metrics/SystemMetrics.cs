using Moritz.Xml;

namespace Moritz.Symbols
{
    internal class SystemMetrics : GroupMetrics
    {
        public SystemMetrics()
            : base(CSSObjectClass.system)
        {

        }

        public override void Move(double dx, double dy)
        {
            base.Move(dx, dy);
            _stafflinesTop += dy;
            _stafflinesBottom += dy;
            _notesTop += dy;
            _notesBottom += dy;
        }

        public override void Add(Metrics metrics)
        {
            base.Add(metrics);
			if(metrics is StaffMetrics staffMetrics)
				SetTopAndBottomMetrics(staffMetrics);
		}

        private void SetTopAndBottomMetrics(StaffMetrics staffMetrics)
        {
            _notesTop =
                _notesTop < staffMetrics.Top ? _notesTop : staffMetrics.Top;

            _stafflinesTop =
                _stafflinesTop < staffMetrics.StafflinesTop ? _stafflinesTop : staffMetrics.StafflinesTop;

            _stafflinesBottom =
                _stafflinesBottom > staffMetrics.StafflinesBottom ? _stafflinesBottom : staffMetrics.StafflinesBottom;

            _notesBottom =
                _notesBottom > staffMetrics.Bottom ? _notesBottom : staffMetrics.Bottom;
        }


        public double StafflinesTop { get { return _stafflinesTop; } }
        private double _stafflinesTop = double.MaxValue;

        public double StafflinesBottom { get { return _stafflinesBottom; } set { _stafflinesBottom = value; } }
        private double _stafflinesBottom = double.MinValue;

        public double NotesTop { get { return _notesTop; } set { _notesTop = value; } }
        private double _notesTop = 0F;
        public double NotesBottom { get { return _notesBottom; } set { _notesBottom = value; } }
        private double _notesBottom = 0F;
    }
}
