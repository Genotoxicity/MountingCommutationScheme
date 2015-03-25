using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MountingCommutationScheme
{
    public class SideSymbolLayout
    {
        List<ElementSequence> rows;
        List<ElementSequence> columns;

        private string name;

        public SideSymbolLayout(ProjectObjects projectObjects, SideOutlineLayout outlineLayout, Dictionary<int, Element> elementById)
        {
            this.name = outlineLayout.Name;
            rows = GetElementSequence(projectObjects, outlineLayout.Rows, elementById);
            columns = GetElementSequence(projectObjects, outlineLayout.Columns, elementById);
        }

        private static List<ElementSequence> GetElementSequence(ProjectObjects projectObjects, List<OutlineSequence> outlineSequences, Dictionary<int, Element> elementById)
        {
            List<ElementSequence> sequences = new List<ElementSequence>();
            int number = 1;
            foreach (OutlineSequence outlineSequence in outlineSequences)
                sequences.Add(new ElementSequence(projectObjects, outlineSequence, elementById, number++));
            return sequences;
        }

        public List<Page> GetPages(ProjectObjects projectObjects)
        {
            List<ElementSequence> sequences = new List<ElementSequence>(rows.Count + columns.Count);
            sequences.AddRange(rows);
            sequences.AddRange(columns);
            List<Page> pages = new List<Page>();
            int startSymbol = 0;
            while (sequences.Count != 0)
            {
                Page A4 = new Page(projectObjects, Settings.A4Subsequent, sequences, startSymbol, name);
                Page A3 = new Page(projectObjects, Settings.A3Subsequent, sequences, startSymbol, name);
                Page page = (A4.Sequences.Count == A3.Sequences.Count && !A4.IsSequenceCarry) ? A4 : A3;
                if (page.IsSequenceCarry)
                    startSymbol = page.LastSymbol;
                else
                {
                    startSymbol = 0;
                    page.Sequences.ForEach(s => sequences.Remove(s));
                }
                pages.Add(page);
            }
            return pages;
        }
    }
}
