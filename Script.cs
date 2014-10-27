using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    class Script
    {
        public Script()
        {
        }

        public void Main(int processId)
        {
            E3Project project = new E3Project(processId);
            ProjectObjects projectObjects = new ProjectObjects(project);
            Settings settings = new Settings();
            HashSet<int> electricSchemeSheetIds = GetElectricSchemeSheetIds(project.SheetIds, projectObjects, settings);
            List<Cabinet> cabinets = GetCabinets(projectObjects, project.TreeSelectedSheetIds, settings, electricSchemeSheetIds);
            List<DeviceSymbol> cabinetDeviceSymbols = cabinets.SelectMany(c => c.Sides).SelectMany(s => s.DeviceRows).SelectMany(dr => dr.RowSymbols).SelectMany(rs=>rs.DeviceSymbols).ToList();
            SetMateAddresses(projectObjects, settings, project.ConnectionIds, cabinetDeviceSymbols, electricSchemeSheetIds);
            Dictionary<string, ComponentLayout> componentLayoutByName = GetComponentLayoutByName(cabinetDeviceSymbols);
            //CalculateLayouts(projectObjects, settings, componentLayoutByName, cabinets);
            componentLayoutByName.Values.ToList().ForEach(cl => cl.Calculate(projectObjects, settings));
            cabinets.ForEach(c => c.Place(projectObjects, settings, componentLayoutByName));
            project.Release();
        }

        private static HashSet<int> GetElectricSchemeSheetIds(List<int> sheetIds, ProjectObjects projectObjects, Settings settings)
        {
            Sheet sheet = projectObjects.Sheet;
            int electricSchemeTypeCode = settings.ElectricSchemeTypeCode;
            HashSet<int> electricSchemeSheetIds = new HashSet<int>();
            foreach (int sheetId in sheetIds)
            {
                sheet.Id = sheetId;
                if (sheet.IsSchematicTypeOf(electricSchemeTypeCode))
                    electricSchemeSheetIds.Add(sheetId);
            }
            return electricSchemeSheetIds;
        }

        private static List<Cabinet> GetCabinets(ProjectObjects projectObjects, List<int> sheetIds, Settings settings, HashSet<int> electricSchemeSheetIds)
        {
            Sheet sheet = projectObjects.Sheet;
            IEnumerable<int> embeddedSheetIds = sheetIds.SelectMany(id => { sheet.Id = id; return sheet.EmbeddedSheetIds; });
            List<int> panelSheetIds = embeddedSheetIds.Where(id => { sheet.Id = id; return sheet.IsPanel; }).ToList();
            List<Cabinet> cabinets = new List<Cabinet>(panelSheetIds.Count);
            panelSheetIds.ForEach(panelSheetId => cabinets.Add(new Cabinet(projectObjects, panelSheetId, settings, electricSchemeSheetIds)));
            return cabinets;
        }

        private static void SetMateAddresses(ProjectObjects projectObjects, Settings settings, List<int> connectionIds, List<DeviceSymbol> cabinetsDeviceSymbols, HashSet<int> electricSchemeSheetIds)
        {
            IEnumerable<int> ElectricSchemePinsEquivalentToCabinetPins = cabinetsDeviceSymbols.SelectMany(ds => ds.PanelPinAndEquivalenceIds);
            MateAddresses mateAddresses = new MateAddresses(projectObjects, settings, connectionIds, ElectricSchemePinsEquivalentToCabinetPins, electricSchemeSheetIds);
            cabinetsDeviceSymbols.ForEach(cds=>cds.PanelPinSymbols.ForEach(ps=>ps.SetMateAddresses(mateAddresses)));
        }

        private static Dictionary<string, ComponentLayout> GetComponentLayoutByName(IEnumerable<DeviceSymbol> cabinetDeviceSymbols)
        {
            Dictionary<string, ComponentLayout> componentLayoutByName = new Dictionary<string, ComponentLayout>();
            foreach (DeviceSymbol deviceSymbol in cabinetDeviceSymbols)
            {
                string component = deviceSymbol.Component;
                if (!componentLayoutByName.ContainsKey(component))
                    componentLayoutByName.Add(component, new ComponentLayout(component));
                componentLayoutByName[component].AddDeviceSymbol(deviceSymbol);
            }
            return componentLayoutByName;
        }

        /*private static void CalculateLayouts(ProjectObjects projectObjects, Settings settings, Dictionary<string, ComponentLayout> componentLayoutByName, List<Cabinet> cabinets)
        {
            componentLayoutByName.Values.ToList().ForEach(cl => cl.Calculate(projectObjects, settings));
            cabinets.ForEach(c => c.CalculateLayout(settings, componentLayoutByName));
        }*/

    }
}