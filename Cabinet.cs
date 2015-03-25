using System;
using System.Collections.Generic;
using System.Linq;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class Cabinet
    {
        private StampAttributes stampAttributes;
        private int panelSheetId;

        public Cabinet(ProjectObjects projectObjects, int panelSheetId)
        {
            Sheet sheet = projectObjects.Sheet;
            sheet.Id = panelSheetId;
            this.panelSheetId = panelSheetId;
            stampAttributes = new StampAttributes(projectObjects, sheet.ParentSheetId);

        }

        public void Place(ProjectObjects projectObjects, List<int> connectionIds)
        {
            List<SideOutlineLayout> sideOutlineLayouts = SideOutlineLayout.GetSideOutlinesLayout(projectObjects, panelSheetId);
            List<SideSymbolLayout> sideSymbolLayouts = GetSideSymbolLayouts(projectObjects, sideOutlineLayouts, connectionIds);
            CabinetPreview cabinetPreview = new CabinetPreview(sideOutlineLayouts);
            List<Page> pages = new List<Page>();
            sideSymbolLayouts.ForEach(l => pages.AddRange(l.GetPages(projectObjects)));
            cabinetPreview.Place(projectObjects, stampAttributes, pages.Count + 1);
            int number = 2;
            foreach (Page page in pages)
                page.Place(number++, stampAttributes);
        }

        private static Dictionary<int, Orientation> GetMountOrientationById(List<DeviceOutline> deviceOutlines)
        {
            Dictionary<int, Orientation> mountOrientationById = new Dictionary<int, Orientation>();
            foreach (DeviceOutline deviceOutline in deviceOutlines)
                if (deviceOutline.IsMount)
                {
                    Orientation orientation = (deviceOutline.Right - deviceOutline.Left) > (deviceOutline.Top - deviceOutline.Bottom) ? Orientation.Horizontal : Orientation.Vertical;
                    mountOrientationById.Add(deviceOutline.DeviceId, orientation);
                }
            return mountOrientationById;
        }

        private static List<SideSymbolLayout> GetSideSymbolLayouts(ProjectObjects projectObjects, List<SideOutlineLayout> outlineLayouts, List<int> connectionIds)
        {
            List<SideSymbolLayout> symbolLayouts = new List<SideSymbolLayout>(outlineLayouts.Count);
            Dictionary<int, Element> elementById = GetElementById(projectObjects, outlineLayouts, connectionIds);
            foreach (SideOutlineLayout outlineLayout in outlineLayouts)
                symbolLayouts.Add(new SideSymbolLayout(projectObjects, outlineLayout, elementById));
            return symbolLayouts;
        }

        private static Dictionary<int, Element> GetElementById(ProjectObjects projectObjects, List<SideOutlineLayout> outlineLayouts, List<int> connectionIds)
        {
            List<Element> elements = GetElements(projectObjects, outlineLayouts, connectionIds);
            Dictionary<int, Element> elementById = new Dictionary<int, Element>(elements.Count);
            foreach (Element element in elements)
                elementById.Add(element.Id, element);
            return elementById;
        }

        private static List<Element> GetElements(ProjectObjects projectObjects, List<SideOutlineLayout> outlineLayouts, List<int> connectionIds)
        {
            ComponentManager componentManager = new ComponentManager(projectObjects);
            List<OutlineSequence> outlineSequences = new List<OutlineSequence>(outlineLayouts.SelectMany(ol => ol.Rows));
            outlineSequences.AddRange(outlineLayouts.SelectMany(ol => ol.Columns));
            List<Element> elements = new List<Element>();
            foreach (OutlineSequence sequence in outlineSequences)
            {
                Orientation orientation = sequence.Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
                foreach (DeviceOutline outline in sequence.Outlines)
                {
                    Element element;
                    if (outline.IsTerminal)
                        element = new TerminalElement(projectObjects, outline, componentManager, orientation);
                    else
                        element = new DeviceElement(projectObjects, outline, componentManager, orientation);
                    elements.Add(element);
                }
            }
            componentManager.CalculateComponents();
            SetMateAddress(projectObjects, elements, connectionIds);
            return elements;
        }

        private static void SetMateAddress(ProjectObjects projectObjects, List<Element> elements, List<int> connectionIds)
        {
            IEnumerable<int> electricSchemePinIdsEquivalentToPanelPins = elements.SelectMany(e => e.ElectricSchemePinIds);
            MateAddresses mateAddresses = new MateAddresses(projectObjects, connectionIds, electricSchemePinIdsEquivalentToPanelPins);
            elements.ForEach(e => e.SetAddresses(mateAddresses));
        }
    }
}
