using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class ProjectObjects
    {
        public Sheet Sheet { get; private set; }
        public NormalDevice Device { get; private set; }
        public DevicePin Pin { get; private set; }
        public Outline Outline { get; private set; }
        public Connection Connection { get; private set; }
        public Core Core { get; private set; }
        public E3Text Text { get; private set; }
        public Graphic Graphic { get; private set; }
        public Group Group { get; private set; }
        public Symbol Symbol { get; private set; }

        public ProjectObjects(E3Project project)
        {
            Sheet = project.GetSheetById(0);
            Device = project.GetNormalDeviceById(0);
            Pin = project.GetDevicePinById(0);
            Outline = project.GetOutlineById(0);
            Connection = project.GetConnectionById(0);
            Core = project.GetCableCoreById(0);
            Text = project.GetTextById(0);
            Graphic = project.GetGraphicById(0);
            Group = project.GetGroupById(0);
            Symbol = project.GetSymbolById(0);
        }
    }
}
