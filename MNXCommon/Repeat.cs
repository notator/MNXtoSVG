using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-time-element
    public class Repeat : IUniqueDef, IDirectionsComponent
    {
        public bool IsBegin { get; private set; } = false;
        public string Times { get; private set; } = null;
        // when null, this defaults to 0 for RepeatBegin, and measure duration (= current time signature) for RepeatEnd.
        public PositionInMeasure PositionInMeasure { get; private set; } = null;

        #region IUniqueDef
        public override string ToString()
        {
            string times = (Times == null) ? "null" : Times;
            return $"Repeat: IsBegin={IsBegin} Times={times} TickPositionInMeasure={PositionInMeasure.TickPositionInMeasure}";
        }
        /// <summary>
        /// (?) See IUniqueDef Interface definition. (?)
        /// </summary>
        public object Clone()
        {
            return this;
        }
        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = 0;
        }

        public int MsDuration { get { return 0; } set { M.Assert(false, "Application Error."); } }

        public int MsPositionReFirstUD
        {
            get
            {
                M.Assert(_msPositionReFirstIUD >= 0);
                return _msPositionReFirstIUD;
            }
            set
            {
                M.Assert(value >= 0);
                _msPositionReFirstIUD = value;
            }
        }
        private int _msPositionReFirstIUD = 0;

        #endregion IUniqueDef

        public Repeat(XmlReader r)
        {
            M.Assert(r.Name == "repeat");
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "type":
                    {
                        switch(r.Value)
                        {
                            case "start":
                                IsBegin = true;
                                break;
                            case "end":
                                IsBegin = false;
                                break;
                            default:
                                M.ThrowError("Unknown repeat type.");
                                break;

                        }
                        break;
                    }
                    case "times":
                    {
                        M.Assert(int.TryParse(r.Value, out _));
                        Times = r.Value;
                        break;
                    }
                    case "location":
                    {
                        PositionInMeasure = new PositionInMeasure(r.Value);
                        break;
                    }
                    default:
                        M.ThrowError("Unknown repeat attribute.");
                        break;
                }
            }
            // r.Name is now the name of the last repeat attribute that has been read.
        }
    }
}