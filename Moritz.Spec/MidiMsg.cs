﻿using System;
using System.Xml;
using System.Diagnostics;

using Moritz.Globals;

namespace Moritz.Spec
{
    public class MidiMsg
    {
        internal MidiMsg(int status, int data1, int? data2)
        {
            A.Assert(status >= 0 && status <= 255); // 0x0..0xFF
            A.Assert(data1 >= 0 && data1 <= 127);
            A.Assert(data2 == null || (data2 >= 0 && data2 <= 127));

            _status = status;
            _data1 = data1;
            _data2 = data2;
        }

        internal void WriteSVG(XmlWriter w)
        {
            string mString = null;
            if(_data2 == null)
            {
                mString = $"0x{_status.ToString("X")} {_data1.ToString()}";
            }
            else
            {
                mString = $"0x{_status.ToString("X")} {_data1.ToString()} {_data2.ToString()}";
            }
            w.WriteStartElement("msg");
            w.WriteAttributeString("m", mString );
            w.WriteEndElement(); // end of msg
        }

        public override string ToString() => $"MidiMsg: Status={"0x" + _status.ToString("X")} Data1={_data1.ToString()} Data2={_data2.ToString()}";

        public int Status { get { return _status; } }
        public int Channel { get { return _status & 0xF; } }
        public int Data1 { get { return _data1; } }
        public int? Data2 { get { return _data2; } }

        private int _status;
        private int _data1; 
        private int? _data2;
    }
}