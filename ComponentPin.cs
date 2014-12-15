namespace MountingCommutationScheme
{
    public class ComponentPin
    {
        public string Name { get; private set; }

        public double Offset { get; private set; }

        public ComponentPin(string name)
        {
            Name = name;
        }

        public void SetOffset(double value)
        {
            Offset = value;
        }

    }
}
