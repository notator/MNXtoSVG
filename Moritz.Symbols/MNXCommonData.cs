﻿
using Moritz.Spec;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Moritz.Symbols
{
    /// <summary>
    /// Information returned from a parsed MNXCommon file.
    /// </summary>
    public class MNXCommonData
    {
        public IReadOnlyList<VoiceDef> VoiceDefs;
        public IReadOnlyList<IReadOnlyList<int>> MidiChannelsPerStaff;
        public IReadOnlyList<int> MsPositionPerBar;
    }
}
