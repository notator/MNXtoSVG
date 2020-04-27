using MNX.Globals;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-key-element
    internal class KeySignature : IDirectionsComponent
    {
        public readonly int Fifths = 0; // default

        #region IUniqueDef
        public override string ToString() => $"KeySignature: MsPositionReFirstIUD={MsPositionReFirstUD} MsDuration={MsDuration}";

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

        public KeySignature(XmlReader r)
        {
            M.Assert(r.Name == "key");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "fifths":
                        int.TryParse(r.Value, out Fifths);
                        break;
                    default:
                        M.ThrowError("Unknown key attribute.");
                        break;
                }
            }

            // r.Name is now the name of the last key attribute that has been read.
        }
    }


}