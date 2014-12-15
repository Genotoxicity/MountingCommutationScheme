﻿using System.Collections.Generic;
using System.Linq;

namespace MountingCommutationScheme
{
    public class ElementPin
    {
        private int equivalentElectricSchemePinId;
        private int panelPinId;

        public string Signal { get; private set; }

        public int PanelPinId
        {
            get
            {
                return panelPinId;
            }
        }

        public List<string> Addresses { get; private set; }

        public List<int> ElectricPinIds
        {
            get
            {
                if (equivalentElectricSchemePinId == 0)
                    return new List<int>(1) { panelPinId };
                else
                    return new List<int>(2) { panelPinId, equivalentElectricSchemePinId };
            }
        }

        public ElementPin(int panelPinId, string signal)
        {
            this.panelPinId = panelPinId;
            Signal = signal;
        }

        public void SetEquivalentElectricSchemePinIds(int electricSchemePinId)
        {
            equivalentElectricSchemePinId = electricSchemePinId;
        }

        public void SetMateAdresses(MateAddresses mateAddresses)
        {
            List<string> addresses = mateAddresses.GetMateAdressesByPinId(panelPinId);
            if (equivalentElectricSchemePinId != 0)
            {
                addresses.AddRange(mateAddresses.GetMateAdressesByPinId(equivalentElectricSchemePinId));
                addresses = addresses.Distinct().ToList();
            }
            Addresses = addresses;
        }
    }
}