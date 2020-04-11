namespace MNXtoSVG
{
    public class MNXC_PositionInMeasure
    {
        private string value;

        public MNXC_PositionInMeasure(string value)
        {
            // I assume that the argument value can be "incoming" and "outgoing"
            // in addition to the values described at
            // https://w3c.github.io/mnx/specification/common/#measure-location
            // See https://w3c.github.io/mnx/specification/common/#the-tied-element

            this.value = value;
        }
    }
}