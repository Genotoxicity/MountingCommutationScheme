using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class ElementSequence
    {
        public List<SequenceSymbol> Symbols { get; private set; }

        public Orientation Orientation { get; private set; }

        public int Number { get; private set; }

        public ElementSequence(ProjectObjects projectObjects, OutlineSequence sequence, Dictionary<int, Element> elementById, int number)
        {
            Orientation = sequence.Orientation;
            Symbols = GetSymbols(sequence, elementById, projectObjects.Text);
            Number = number;
        }

        private static List<SequenceSymbol> GetSymbols(OutlineSequence sequence, Dictionary<int, Element> elementById, E3Text text)
        {
            List<SequenceSymbol> symbols = new List<SequenceSymbol>();
            List<TerminalElement> terminalElements = new List<TerminalElement>();
            foreach (DeviceOutline outline in sequence.Outlines)
            {
                Element element = elementById[outline.DeviceId];
                TerminalElement terminalElement = element as TerminalElement;
                if (terminalElement != null)
                {
                    if (terminalElements.Count > 0 && !terminalElements.Last().Name.Equals(terminalElement.Name))
                    {
                        symbols.Add(new StripSymbol(terminalElements, sequence.Orientation, text));
                        terminalElements = new List<TerminalElement>();
                    }
                    terminalElements.Add(terminalElement);
                }
                else
                {
                    if (terminalElements.Count > 0)
                    {
                        symbols.Add(new StripSymbol(terminalElements, sequence.Orientation, text));
                        terminalElements = new List<TerminalElement>();
                    }
                    symbols.Add(new SingleSymbol(element, text));
                }
            }
            if (terminalElements.Count > 0)
                symbols.Add(new StripSymbol(terminalElements, sequence.Orientation, text));
            return symbols;
        }

    }
}
