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
            List<Cabinet> cabinets = GetCabinets(projectObjects, project.TreeSelectedSheetIds);
            List<int> connectionIds = project.ConnectionIds;
            cabinets.ForEach(c => c.Place(projectObjects, connectionIds));
            project.Release();
        }

        private static List<Cabinet> GetCabinets(ProjectObjects projectObjects, List<int> sheetIds)
        {
            Sheet sheet = projectObjects.Sheet;
            IEnumerable<int> embeddedSheetIds = sheetIds.SelectMany(id => { sheet.Id = id; return sheet.EmbeddedSheetIds; });
            List<int> panelSheetIds = embeddedSheetIds.Where(id => { sheet.Id = id; return sheet.IsPanel; }).ToList();
            List<Cabinet> cabinets = new List<Cabinet>(panelSheetIds.Count);
            panelSheetIds.ForEach(panelSheetId => cabinets.Add(new Cabinet(projectObjects, panelSheetId)));
            return cabinets;
        }

    }
}