using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSPE3Lib;

namespace MountingCommutationScheme
{
    public class MateAddresses
    {
        private Dictionary<int, List<int>> mateIdsById;
        private Dictionary<int, string> addressById;
        private Dictionary<int, string> assignmentById;
        //private Dictionary<int, string> sheetAddressById;
        //private Dictionary<int, int> sheetIdByPinId;
        private DevicePin pin;
        //private Sheet sheet;
        //private string sheetMarkAttribute;

        public MateAddresses(ProjectObjects projectObjects, Settings settings, List<int> connectionIds, IEnumerable<int> electricSchemePinsEquivalentToCabinetPins, HashSet<int> electricSchemeSheetIds)
        {
            pin = projectObjects.Pin;
            //sheet = projectObjects.Sheet;
            //sheetMarkAttribute = settings.SheetMarkAttribute;
            mateIdsById = GetMateIdsById(projectObjects, electricSchemePinsEquivalentToCabinetPins, connectionIds, electricSchemeSheetIds);
            IEnumerable<int> pinIds = mateIdsById.Values.SelectMany(v=>v).Distinct();
            NormalDevice device = projectObjects.Device;
            int pinCount = pinIds.Count();
            addressById = new Dictionary<int, string>(pinCount);
            assignmentById = new Dictionary<int, string>(pinCount);
            //sheetIdByPinId = new Dictionary<int, int >(pinCount);
            //sheetAddressById = new Dictionary<int, string>();
            foreach (int pinId in pinIds)
            {
                pin.Id = pinId;
                device.Id = pinId;
                //sheetIdByPinId.Add(pinId, pin.SheetId);
                addressById.Add(pinId, device.Location + device.Name + ":" + pin.Name);
                assignmentById.Add(pinId, device.Assignment);
            }
        }

        private static Dictionary<int, List<int>> GetMateIdsById(ProjectObjects projectObjects, IEnumerable<int> electricSchemePinsEquivalentToCabinetPins, List<int> connectionIds, HashSet<int> electricSchemeSheetIds)
        {
            Connection connection = projectObjects.Connection;
            DevicePin pin = projectObjects.Pin;
            //Core core = projectObjects.Core;
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
                    //List<int> mateIds = connectionPinIds.Where(conPId => (conPId != commonPinId /* && commonPinSignal.Equals(signalById[conPId])*/)).ToList();
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
            /*List<int> coreIds = pin.CoreIds;
            if (coreIds.Count > 0)
            {
                List<int> pinIds = coreIds.SelectMany(cId => { core.Id = cId; return core.ConnectedPinIds; }).ToList();
                pinIds.RemoveAll(pId => pId == 0 || pId == commonPinId);
                mateIds = pinIds.Distinct().ToList();
            }
            else
            {*/
            mateIds = connectionPinIds.Where(conPId => (conPId != commonPinId && commonPinSignal.Equals(signalById[conPId]))).ToList();
            //}
            return mateIds;
        }

        /*private Dictionary<int, string> GetAddressById(ProjectObjects projectObjects, Settings settings, IEnumerable<int> pinIds)
        {
            NormalDevice device = projectObjects.Device;
            DevicePin pin = projectObjects.Pin;
            Sheet sheet = projectObjects.Sheet;
            Dictionary<int, string> localAddressById = new Dictionary<int, string>(pinIds.Count());
            Dictionary<int, string> sheetAddressById = new Dictionary<int, string>();
            foreach (int pinId in pinIds)
            {
                pin.Id = pinId;
                device.Id = pinId;
                int sheetId = pin.SheetId;
                if (!sheetAddressById.ContainsKey(sheetId))
                {
                    sheet.Id = sheetId;
                    string mark = sheet.GetAttributeValue(settings.SheetMarkAttribute);
                    string sheetAddress = "." + (String.IsNullOrEmpty(mark) ? String.Empty : mark) + "/" + sheet.Name;
                    sheetAddressById.Add(sheetId, sheetAddress);
                }
                localAddressById.Add(pinId, device.Assignment + device.Location + device.Name + ":" + pin.Name + sheetAddressById[sheetId]);
            }
            return localAddressById;
        }*/

        public List<string> GetMateAdressesByPinId(int pinId)
        {
            if (!mateIdsById.ContainsKey(pinId))
                return new List<string>(0);
            /*if (!sheetIdByPinId.ContainsKey(pinId))
            {
                pin.Id = pinId;
                sheetIdByPinId.Add(pinId, pin.SheetId);
            }
            int pinSheetId = sheetIdByPinId[pinId];*/
            List<int> mateIds = mateIdsById[pinId];
            List<string> mateAddresses = new List<string>(mateIds.Count);
             string assignment;
             if (assignmentById.ContainsKey(pinId))
                 assignment = assignmentById[pinId];
             else
                 assignment = String.Empty;
            foreach (int mateId in mateIds)
            {
                //int mateSheetId = sheetIdByPinId[mateId];
                //if (pinSheetId == mateSheetId)
                string mateAssignment = assignmentById[mateId];
                if (mateAssignment.Equals(assignment))
                    mateAddresses.Add(addressById[mateId]);
                else
                    mateAddresses.Add(mateAssignment + addressById[mateId]);
                /*else
                {
                    if (!sheetAddressById.ContainsKey(mateSheetId))
                    {
                        sheet.Id = mateSheetId;
                        string mark = sheet.GetAttributeValue(sheetMarkAttribute);
                        string sheetAddress = "." + (String.IsNullOrEmpty(mark) ? String.Empty : mark) + "/" + sheet.Name;
                        sheetAddressById.Add(mateSheetId, sheetAddress);
                    }
                    mateAddresses.Add(addressById[mateId] + sheetAddressById[mateSheetId]);
                }*/
            }
            return mateAddresses;
        }
    }
}
