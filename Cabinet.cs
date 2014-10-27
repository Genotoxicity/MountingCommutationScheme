using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class Cabinet
    {
        private List<CabinetSide> sides;
        private string name;
        private StampAttributes stampAttributes;

        public List<CabinetSide> Sides
        {
            get
            {
                return sides;
            }
        }

        public Cabinet(ProjectObjects projectObjects, int panelSheetId, Settings settings, HashSet<int> electricSchemeSheetIds)
        {
            Sheet sheet = projectObjects.Sheet;
            sheet.Id = panelSheetId;
            NormalDevice device = projectObjects.Device;
            Outline outline = projectObjects.Outline;
            Symbol symbol = projectObjects.Symbol;
            Dictionary<int, DeviceOutline> deviceOutlineById = new Dictionary<int, DeviceOutline>();
            Dictionary<string, int> assignmentCountByAssignment = new Dictionary<string, int>();
            foreach (int symbolId in sheet.SymbolIds)
            {
                device.Id = symbolId;
                int deviceId = device.Id;
                string assignment = String.Intern(device.Assignment);
                if (!assignmentCountByAssignment.ContainsKey(assignment))
                    assignmentCountByAssignment.Add(assignment, 0);
                assignmentCountByAssignment[assignment]++;
                DeviceOutline deviceOutline = new DeviceOutline(device, outline, deviceId, (symbolIds) => { if (symbolIds.Count == 0) return false; return symbolIds.Any(sId => { symbol.Id = sId; return electricSchemeSheetIds.Contains(symbol.SheetId); }); });
                if (!deviceOutlineById.ContainsKey(deviceId))
                    deviceOutlineById.Add(deviceId, deviceOutline);
            }
            name = GetName(assignmentCountByAssignment);
            sides = GetSides(projectObjects, sheet, settings, deviceOutlineById.Values.ToList(), electricSchemeSheetIds);
            stampAttributes = new StampAttributes(projectObjects, settings);
        }

        private static string GetName(Dictionary<string, int> assignmentCountByAssignment)
        {
            string cabinetName = String.Empty;
            int max = int.MinValue;
            foreach (string assignment in assignmentCountByAssignment.Keys)
            {
                if (max <= assignmentCountByAssignment[assignment])
                {
                    max = assignmentCountByAssignment[assignment];
                    cabinetName = assignment;
                }
            }
            return cabinetName;
        }

        private List<CabinetSide> GetSides(ProjectObjects projectObjects, Sheet sheet, Settings settings, List<DeviceOutline> deviceOutlines, HashSet<int> electricSchemeSheetIds)
        {
            Dictionary<DeviceOutline, List<DeviceOutline>> includedOutlinesByIncluding = GetIncludedOutlinesByIncluding(deviceOutlines);
            List<CabinetSide> localSides = new List<CabinetSide>();
            List<DeviceOutline> sidewallOutlines = new List<DeviceOutline>();
            NormalDevice device = projectObjects.Device;
            foreach (DeviceOutline outline in includedOutlinesByIncluding.Keys)
            {
                List<DeviceOutline> includedOutlines = includedOutlinesByIncluding[outline];
                if (includedOutlines.Select(i => i.DeviceId == 400082).Count() > 0)
                    includedOutlines.ToString();
                includedOutlines.RemoveAll(devOut => !devOut.HasPlacedSymbols);    // удаляем устройства, не имеющие размещенных символов - монтажные рейки, соединители, батареи
                if (includedOutlines.Select(i => i.DeviceId == 400082).Count() > 0)
                    includedOutlines.ToString();
                includedOutlines.ForEach(devOut => deviceOutlines.Remove(devOut));  // удаляем устройства, уже попавшие в какую - либо часть шкафа
                deviceOutlines.Remove(outline); // удаляем включающее устройство - это какой - либо шкаф, стенка и т.п. не отображающиеся на схеме
                device.Id = outline.DeviceId;
                SideType sideType = SideType.Panel;
                string function = device.GetAttributeValue(settings.FunctionAttribute);
                if (settings.SideTypeByFunction.ContainsKey(function))
                    sideType = settings.SideTypeByFunction[function];
                if (sideType == SideType.Panel)
                    localSides.Add(new CabinetSide(projectObjects, settings, name, "Монтажная панель", outline.DeviceId, includedOutlines, sheet, electricSchemeSheetIds));
                if (sideType == SideType.Sidewall)
                    sidewallOutlines.Add(outline);
            }
            deviceOutlines.RemoveAll(devOut => !devOut.HasPlacedSymbols); // оставшиеся устройства относятся к "двери"
            if( deviceOutlines.Count>0)
                localSides.Add(new CabinetSide(projectObjects, settings, name, "Дверь", 0, deviceOutlines, sheet, electricSchemeSheetIds));
            int sidewallCount = sidewallOutlines.Count;
            if (sidewallCount == 1)
                localSides.Add(new CabinetSide(projectObjects, settings, name, "Боковая стенка", sidewallOutlines.First().DeviceId, includedOutlinesByIncluding[sidewallOutlines.First()], sheet, electricSchemeSheetIds));
            if (sidewallCount > 1)
            {
                sidewallOutlines.Sort(new DeviceOutlineOnSheetHorizontalComparer(sheet));
                localSides.Add(new CabinetSide(projectObjects, settings, name, "Левая боковая стенка", sidewallOutlines.First().DeviceId, includedOutlinesByIncluding[sidewallOutlines.First()], sheet, electricSchemeSheetIds));
                for (int i = 1; i < sidewallCount; i++)
                    localSides.Add(new CabinetSide(projectObjects, settings, name, "Правая боковая стенка", sidewallOutlines[i].DeviceId, includedOutlinesByIncluding[sidewallOutlines[i]], sheet, electricSchemeSheetIds));
            }
            return localSides;
        }

        private static Dictionary<DeviceOutline, List<DeviceOutline>> GetIncludedOutlinesByIncluding(List<DeviceOutline> deviceOutlines)
        {
            deviceOutlines.Sort((do1, do2) => -do1.Area.CompareTo(do2.Area));   // сортируем по площади, от больших к меньшим
            HashSet<int> includedIndexes = new HashSet<int>();
            Dictionary<DeviceOutline, List<DeviceOutline>> includedOutlinesByIncluding = new Dictionary<DeviceOutline, List<DeviceOutline>>();
            int outlinesCount = deviceOutlines.Count;
            for (int i = 0; i < outlinesCount - 1; i++)
            {
                if (includedIndexes.Contains(i))
                    continue;
                DeviceOutline first = deviceOutlines[i];
                for (int j = i + 1; j < outlinesCount; j++) // меньшие площади по определению не включают в себя большие
                {
                    if (includedIndexes.Contains(j))
                        continue;
                    DeviceOutline second = deviceOutlines[j];
                    if (first.Contains(second))
                    {
                        if (!includedOutlinesByIncluding.ContainsKey(first))
                            includedOutlinesByIncluding.Add(first, new List<DeviceOutline>());
                        includedOutlinesByIncluding[first].Add(second);
                        includedIndexes.Add(j);
                    }
                }
            }
            return includedOutlinesByIncluding;
        }

        public void Place(ProjectObjects projectObjects, Settings settings, Dictionary<string, ComponentLayout> componentLayoutByName)
        {
            sides.ForEach(s=>s.CalculateRows(settings, componentLayoutByName));
            int sideCount = sides.Count;
            bool isNeedPreview = false;
            isNeedPreview |= !sides[0].IsFitIntoOneSheet(settings, true);
            for (int i = 1; i < sideCount; i++)
                isNeedPreview |= !sides[i].IsFitIntoOneSheet(settings, false);
            sides[0].SetSheetLayout(settings, !isNeedPreview);
            for (int i = 1; i < sideCount; i++)
                sides[i].SetSheetLayout(settings, false);
            if (isNeedPreview)
            {
                List<SidePreview> previews = new List<SidePreview>(sideCount);
                sides.ForEach(s => previews.Add(new SidePreview(settings, s)));
                PreviewAllocator previewAllocator = new PreviewAllocator(previews, settings);
                int previewSheetCount = previewAllocator.PreviewSheetCount;
                int totalSheetCount = previewSheetCount + sides.Sum(s => s.SheetCount);
                stampAttributes.SetSheetCount(totalSheetCount);
                previewAllocator.Place(projectObjects, settings, stampAttributes);
                int sheetNumber = previewSheetCount + 1;
                sides.ForEach(side => side.Place(projectObjects, settings, componentLayoutByName, ref sheetNumber, stampAttributes));
            }
            else
            {
                int sheetNumber = 1;
                stampAttributes.SetSheetCount(sides.Sum(s => s.SheetCount));
                sides.ForEach(side => side.Place(projectObjects, settings, componentLayoutByName, ref sheetNumber, stampAttributes));
            }
        }
    }
}
