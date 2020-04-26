using System.Collections.Generic;

namespace MNX.Globals
{
    public class Form1PageData
    {
        public Form1PageData()
        {
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public int MarginTopPage1 { get; set; }
        public int MarginTopOther { get; set; }
        public int MarginRight { get; set; }
        public int MarginBottom { get; set; }
        public int MarginLeft { get; set; }
    }

    public class Form1NotationData
    {
        public double StafflineStemStrokeWidth { get; set; }
        public double Gap { get; set; }
        public int MinGapsBetweenStaves { get; set; }
        public int MinGapsBetweenSystems { get; set; }
        public List<int> SystemStartBars { get; set; }
        public double CrotchetsPerMinute { get; set; }
    }

    public class Form1MetadataData
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Keywords { get; set; } = "";
        public string Comment { get; set; } = "";
    }

    public class Form1OptionsData
    {
        public bool WritePage1Titles { get; set; }
        public bool IncludeMIDIData { get; set; }
        public bool WriteScrollScore { get; set; }
    }
}