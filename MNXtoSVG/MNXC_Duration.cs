namespace MNXtoSVG
{
    public class MNXC_Duration
    {
        private string value; // the MNX duration symbol string ("/2", "/4" etc.)

        public MNXC_Duration(string value)
        {
            // https://w3c.github.io/mnx/specification/common/#note-value
            // https://w3c.github.io/mnx/specification/common/#base-note-values
            this.value = value;
        }
    }
}