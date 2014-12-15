using System.Collections.Generic;
using System.Linq;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    class Script
    {
        public Script()
        {
        }

        public static void Main(int processId)
        {
            E3Project project = new E3Project(processId);
            ProjectObjects projectObjects = new ProjectObjects(project);
            Settings settings = new Settings();
            HashSet<int> electricSchemeSheetIds = GetElectricSchemeSheetIds(project.SheetIds, projectObjects, settings);
            List<Cabinet> cabinets = GetCabinets(projectObjects, project.TreeSelectedSheetIds, settings, electricSchemeSheetIds);
            SetMateAddresses(projectObjects, project.ConnectionIds, electricSchemeSheetIds, cabinets);
            cabinets.ForEach(c => c.Place(projectObjects, settings));
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

        private static void SetMateAddresses(ProjectObjects projectObjects, List<int> connectionIds, HashSet<int> electricSchemeSheetIds, List<Cabinet> cabinets)
        {
            List<Element> elements = cabinets.SelectMany(c => c.Elements).ToList();
            IEnumerable<int> electricSchemePinIdsEquivalentToPanelPins = elements.SelectMany(e=>e.ElectricSchemePinIds);
            MateAddresses mateAddresses = new MateAddresses(projectObjects, connectionIds, electricSchemePinIdsEquivalentToPanelPins, electricSchemeSheetIds);
            elements.ForEach(e => e.SetAddresses(mateAddresses));
        }

    }
}