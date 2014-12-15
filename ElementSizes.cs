namespace MountingCommutationScheme
{
    public struct ElementSizes
    {
        private Margins margins;
        private double signalLineLength;

        public Margins Margins
        {
            get
            {
                return margins;
            }
        }

        public double SignalLineLength
        {
            get
            {
                return signalLineLength;
            }
        }

        public ElementSizes(Margins margins, double signalLineLength)
        {
            this.margins = margins;
            this.signalLineLength = signalLineLength;
        }

    }
}
