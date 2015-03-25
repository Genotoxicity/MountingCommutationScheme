using System.Collections.Generic;
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
        public HashSet<int> ElectricSheetIds { get; private set; }

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
            ElectricSheetIds = GetElectricSheetIds(project.SheetIds);
        }

        private HashSet<int> GetElectricSheetIds(List<int> sheetIds)
        {
            int electricSchemeTypeCode = Settings.ElectricSchemeTypeCode;
            HashSet<int> electricSchemeSheetIds = new HashSet<int>();
            foreach (int sheetId in sheetIds)
            {
                Sheet.Id = sheetId;
                if (Sheet.IsSchematicTypeOf(electricSchemeTypeCode))
                    electricSchemeSheetIds.Add(sheetId);
            }
            return electricSchemeSheetIds;
        }
    }
}
