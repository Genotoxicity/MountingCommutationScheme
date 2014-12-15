namespace MountingCommutationScheme
{
    public class SheetFormat
    {
        public string Name { get; private set; }

        public double AvailableWidth { get; private set; }

        public double AvailableHeight { get; private set; }

        public double LeftBorder { get; private set; }

        public double TopBorder { get; private set; }

        public double PreviewWidth { get; private set; }

        public double PreviewHeight { get; private set; }

        public SheetFormat(string name, double availableWidth, double availableHeight, double leftBorder, double topBorder, double previewWidth, double previewHeight)
        {
            Name = name;
            AvailableWidth = availableWidth;
            AvailableHeight = availableHeight;
            LeftBorder = leftBorder;
            TopBorder = topBorder;
            PreviewWidth = previewWidth;
            PreviewHeight = previewHeight;
        }

    }
}
