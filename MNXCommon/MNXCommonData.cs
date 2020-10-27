
using Moritz.Spec;
using System.Collections.Generic;

namespace MNX.Common
{
    /// <summary>
    /// Information returned from a parsed MNXCommon file.
    /// </summary>
    public class MNXCommonData
    {
        public int NumberOfMeasures;
        public IReadOnlyList<VoiceDef> VoiceDefs;
        public IReadOnlyList<IReadOnlyList<int>> MidiChannelsPerStaff;
        public IReadOnlyList<int> NumberOfStavesPerPart;
        public IReadOnlyList<int> EndBarlineMsPositionPerBar;
    }
}
