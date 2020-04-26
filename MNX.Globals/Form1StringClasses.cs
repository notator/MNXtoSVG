namespace MNX.Globals
{
    public class Form1PageStrings
    {
        public Form1PageStrings()
        {
        }

        public string Width { get; set; }
        public string Height { get; set; }
        public string MarginTopPage1 { get; set; }
        public string MarginTopOther { get; set; }
        public string MarginRight { get; set; }
        public string MarginBottom { get; set; }
        public string MarginLeft { get; set; }
    }

    public class Form1NotationStrings
    {
        public string stafflineStemStrokeWidth { get; set; }
        public string gapSize { get; set; }
        public string minGapsBetweenStaves { get; set; }
        public string minGapsBetweenSystems { get; set; }
        public string systemStartBars { get; set; }
        public string crotchetsPerMinute { get; set; }
    }

    public class Form1MetadataStrings
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Keywords { get; set; } = "";
        public string Comment { get; set; } = "";
    }

    public class Form1OptionsStrings
    {
        public string WritePage1Titles { get; set; }
        public string IncludeMIDIData { get; set; }
        public string WriteScrollScore { get; set; }
    }
}