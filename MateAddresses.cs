using System;
using System.Collections.Generic;
using System.Linq;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class MateAddresses
    {
        private Dictionary<int, List<int>> mateIdsById;
        private Dictionary<int, string> addressById;
        private Dictionary<int, string> assignmentById;
        private DevicePin pin;

        public MateAddresses(ProjectObjects projectObjects, List<int> connectionIds, IEnumerable<int> electricSchemePinsEquivalentToCabinetPins, HashSet<int> electricSchemeSheetIds)
        {
            pin = projectObjects.Pin;
            mateIdsById = GetMateIdsById(projectObjects, electricSchemePinsEquivalentToCabinetPins, connectionIds, electricSchemeSheetIds);
            IEnumerable<int> pinIds = mateIdsById.Values.SelectMany(v=>v).Distinct();
            NormalDevice device = projectObjects.Device;
            int pinCount = pinIds.Count();
            addressById = new Dictionary<int, string>(pinCount);
            assignmentById = new Dictionary<int, string>(pinCount);
            foreach (int pinId in pinIds)
            {
                pin.Id = pinId;
                device.Id = pinId;
                addressById.Add(pinId, device.Location + device.Name + ":" + pin.Name);
                assignmentById.Add(pinId, device.Assignment);
            }
        }

        private static Dictionary<int, List<int>> GetMateIdsById(ProjectObjects projectObjects, IEnumerable<int> electricSchemePinsEquivalentToCabinetPins, List<int> connectionIds, HashSet<int> electricSchemeSheetIds)
        {
            Connection connection = projectObjects.Connection;
            DevicePin pin = projectObjects.Pin;
            Dictionary<int, List<int>>  localMateIdsById = new Dictionary<int, List<int>>(electricSchemePinsEquivalentToCabinetPins.Count());
            foreach (int connectionId in connectionIds)
            {
                connection.Id = connectionId;
                List<int> connectionPinIds = connection.PinIds.Where(pId => { pin.Id = pId; return electricSchemeSheetIds.Contains(pin.SheetId); }).ToList();
                IEnumerable<int> commonPinIds = electricSchemePinsEquivalentToCabinetPins.Intersect(connectionPinIds);
                Dictionary<int, string> signalById = GetSignalById(pin, connectionPinIds);
                foreach (int commonPinId in commonPinIds)
                {
                    List<int> mateIds = GetMateIds(pin, connectionPinIds, signalById, commonPinId);
                    if (mateIds.Count() > 0)
                        if (localMateIdsById.ContainsKey(commonPinId))
                            localMateIdsById[commonPinId].AddRange(mateIds);
                        else
                            localMateIdsById.Add(commonPinId, mateIds);
                }
            }
            localMateIdsById.Keys.ToList().ForEach(id => localMateIdsById[id] = localMateIdsById[id].Distinct().ToList());
            return localMateIdsById;
        }

        private static Dictionary<int, string> GetSignalById(DevicePin pin, List<int> connectionPinIds)
        {
            Dictionary<int, string> signalById = new Dictionary<int, string>(connectionPinIds.Count);
            connectionPinIds.ForEach(conPId => { pin.Id = conPId; signalById.Add(conPId, pin.SignalName); });
            return signalById;
        }

        private static List<int> GetMateIds(DevicePin pin, List<int> connectionPinIds, Dictionary<int, string> signalById, int commonPinId)
        {
            string commonPinSignal = signalById[commonPinId];
            pin.Id = commonPinId;
            List<int> mateIds;
            mateIds = connectionPinIds.Where(conPId => (conPId != commonPinId && commonPinSignal.Equals(signalById[conPId]))).ToList();
            return mateIds;
        }

        public List<string> GetMateAdressesByPinId(int pinId)
        {
            if (!mateIdsById.ContainsKey(pinId))
                return new List<string>(0);
            List<int> mateIds = mateIdsById[pinId];
            List<string> mateAddresses = new List<string>(mateIds.Count);
             string assignment;
             if (assignmentById.ContainsKey(pinId))
                 assignment = assignmentById[pinId];
             else
                 assignment = String.Empty;
            foreach (int mateId in mateIds)
            {
                string mateAssignment = assignmentById[mateId];
                if (mateAssignment.Equals(assignment))
                    mateAddresses.Add(addressById[mateId]);
                else
                    mateAddresses.Add(mateAssignment + addressById[mateId]);
            }
            return mateAddresses;
        }
    }
}
