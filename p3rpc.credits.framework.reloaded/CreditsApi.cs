using System.Runtime.InteropServices;
using p3rpc.credits.framework.interfaces;
using p3rpc.credits.framework.reloaded.Configuration;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;
using UnrealEssentials.Interfaces;

namespace p3rpc.credits.framework.reloaded
{
    internal class CreditsApi : ICreditsApi
    {
        private readonly IUnreal _unreal;
        private readonly IUObjects _uObject;
        private readonly IUnrealEssentials _unrealEssentials;
        public SortedDictionary<string, List<CreditEntry>> creditsByModID = [];
        public SortedDictionary<string, Dictionary<string, bool>> configByModID = [];
        public SortedDictionary<string, string> ModNameByModID = [];

        public CreditsApi(IUObjects uObject, IUnreal unreal, IUnrealEssentials unrealEssentials)
        {
            _unreal = unreal;
            _unrealEssentials = unrealEssentials;
            _uObject = uObject;
            unsafe
            {
                _uObject.FindObject("StaffRollDataAsset_C", obj =>
                {
                    UStaffRollDataAsset* CreditTable = (UStaffRollDataAsset*)obj.Self;
                    obj.Self = (UObject*)UpdateCreditsData(CreditTable);
                });
            }
        }

        public void AddManualCredit(CreditEntry credit)
        {
            if (creditsByModID.ContainsKey(credit.ModID))
            {
                creditsByModID[credit.ModID].Add(credit);         
            }
            else
            {
                creditsByModID.Add(credit.ModID, new List<CreditEntry> { credit });
            }
        }

        public void DeleteCredit(string modID)
        {
            if (creditsByModID.ContainsKey(modID))
            {
                creditsByModID.Remove(modID);
            }
        }

        public void ToggleConfigbyModID(string modID, string ModName, string config, bool configval)
        {
            if (creditsByModID.ContainsKey(modID))
            {
                configByModID[modID].Add(config, configval);    
            }
            else
            {
                configByModID.Add(modID, new Dictionary<string, bool> { { config, true } });
                ModNameByModID.Add(modID, ModName);
            }
        }

        private unsafe UStaffRollDataAsset* UpdateCreditsData(UStaffRollDataAsset* obj)
        {
            //Original UStaffRollDataAssetInfo (Number of elements: 750, max index: 749)

            //Check how many elements we need to add
            int elementsnum = 750;
            foreach (KeyValuePair<string, List<CreditEntry>> dictitem in creditsByModID)
            {
                if(configByModID.ContainsKey(dictitem.Key) && configByModID[dictitem.Key].ContainsKey("autoheader") && configByModID[dictitem.Key]["autoheader"])
                {
                    elementsnum++;
                }
                foreach (var item in dictitem.Value)
                {
                    elementsnum++;
                }
            }

            //Allocate memory for the new AllocatorInstance
            FStaffRollTableData* AllocatorInstance = (FStaffRollTableData*)_unreal.FMalloc(128 * (elementsnum+18), 0);

            //Get the elements from the original AllocatorInstance
            FStaffRollTableData* elements = obj->Data.AllocatorInstance;

            //Copy over items from original AllocatorInstance
            int count = obj->Data.Num;
            for (int i = 0; i < count; i++)
            {
                AllocatorInstance[i] = elements[i];
            }

            //Attempt to free the old memory
            _unrealEssentials.Free(obj->Data.AllocatorInstance);

            //Reassign the AllocatorInstance
            try
            {
                *(nint*)(&obj->Data.AllocatorInstance) = (nint)AllocatorInstance;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to assign AllocatorInstance: {ex.Message}");
            }
            
            //Set up variables to begin adding new entries to the credits
            obj->Data.Max = elementsnum+18;
            int StaffRollIndex = 1410;
            int TableIndex = 750;

            foreach (KeyValuePair<string, List<CreditEntry>> dictitem in creditsByModID)
            {
                if (configByModID.ContainsKey(dictitem.Key) && configByModID[dictitem.Key].ContainsKey("autoheader") && configByModID[dictitem.Key]["autoheader"])
                {
                    var headerItem = &obj->Data.AllocatorInstance[TableIndex];
                    headerItem->FirstColumnName = _unreal.FString(ModNameByModID[dictitem.Key]);
                    headerItem->Command = 1;
                }
                foreach (var item in dictitem.Value)
                {
                    var newItem = (item.TableIndex == null || item.TableIndex > TableIndex)
                        ? &obj->Data.AllocatorInstance[TableIndex]
                        : &obj->Data.AllocatorInstance[item.TableIndex.GetValueOrDefault(TableIndex)];
                    newItem->StaffRollIndex = (item.TableIndex == null || item.TableIndex > TableIndex) ? StaffRollIndex : item.TableIndex.GetValueOrDefault(StaffRollIndex);
                    newItem->FirstColumnName = _unreal.FString(item.FirstColumnName);
                    newItem->SecondColumnName = _unreal.FString(item.SecondColumnName);
                    newItem->ThirdColumnName = _unreal.FString(item.ThirdColumnName);
                    newItem->ForthColumnName = _unreal.FString(item.FourthColumnName);
                    newItem->Ficolor = item.FirstColor;
                    newItem->Scolor = item.SecondColor;
                    newItem->Tcolor = item.ThirdColor;
                    newItem->Focolor = item.FourthColor;
                    newItem->Fisize = 1;
                    newItem->Ssize = 1;
                    newItem->Tsize = 1;
                    newItem->Fosize = 1;
                    newItem->Fistyle = 0;
                    newItem->Sstyle = 0;
                    newItem->Tstyle = 0;
                    newItem->Fostyle = 0;
                    newItem->Command = item.FirstCommand;
                    newItem->SecondCommand = item.SecondCommand;
                    newItem->ThirdCommand = item.ThirdCommand;
                    newItem->ForthCommand = item.FourthCommand;
                    newItem->LineCommand = item.LineCommand;
                    newItem->LastSeconds = item.LastSeconds;
                    newItem->FinishSeconds = item.FinishSeconds;
                    newItem->StartWaitSeconds = item.StartWaitSeconds;
                    newItem->LineCount = item.LineCount;
                    newItem->EmptyCount = item.EmptyCount;
                    StaffRollIndex = (item.TableIndex == null || item.TableIndex > TableIndex) ? StaffRollIndex + item.EmptyCount + 1 : StaffRollIndex;
                    TableIndex = (item.TableIndex == null || item.TableIndex > TableIndex) ? TableIndex + 1 : TableIndex;
                    obj->Data.Num = (item.TableIndex == null || item.TableIndex > TableIndex) ? obj->Data.Num+1 : obj->Data.Num;
                }
            }

            //Add a cute butterfly icon at the end
            var newitem = &obj->Data.AllocatorInstance[749];
            obj->Data.AllocatorInstance[749].FinishSeconds = 0.0f;
            obj->Data.AllocatorInstance[749].LastSeconds = 0.0f;
            newitem = &obj->Data.AllocatorInstance[TableIndex];
            newitem->FinishSeconds = 490.0f;
            newitem->LastSeconds = 5.0f;
            newitem->FirstColumnName = _unreal.FString("5");
            newitem->Command = 3;

            //Finally return the object to replace the ingame credits
            return obj;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x80)]
        public unsafe struct FStaffRollTableData
        {
            [FieldOffset(0x0000)] public int StaffRollIndex;
            [FieldOffset(0x0008)] public FString FirstColumnName;
            [FieldOffset(0x0018)] public FString SecondColumnName;
            [FieldOffset(0x0028)] public FString ThirdColumnName;
            [FieldOffset(0x0038)] public FString ForthColumnName;
            [FieldOffset(0x0048)] public FColor Ficolor;
            [FieldOffset(0x004C)] public FColor Scolor;
            [FieldOffset(0x0050)] public FColor Tcolor;
            [FieldOffset(0x0054)] public FColor Focolor;
            [FieldOffset(0x0058)] public byte Fistyle;
            [FieldOffset(0x0059)] public byte Sstyle;
            [FieldOffset(0x005A)] public byte Tstyle;
            [FieldOffset(0x005B)] public byte Fostyle;
            [FieldOffset(0x005C)] public byte Fisize;
            [FieldOffset(0x005D)] public byte Ssize;
            [FieldOffset(0x005E)] public byte Tsize;
            [FieldOffset(0x005F)] public byte Fosize;
            [FieldOffset(0x0060)] public byte Command;
            [FieldOffset(0x0061)] public byte SecondCommand;
            [FieldOffset(0x0062)] public byte ThirdCommand;
            [FieldOffset(0x0063)] public byte ForthCommand;
            [FieldOffset(0x0064)] public int LineCount;
            [FieldOffset(0x0068)] public bool LineCommand;
            [FieldOffset(0x006C)] public int EmptyCount;
            [FieldOffset(0x0070)] public float StartWaitSeconds;
            [FieldOffset(0x0074)] public float FinishSeconds;
            [FieldOffset(0x0078)] public float LastSeconds;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x50)]
        public unsafe struct UStaffRollDataAsset
        {
            [FieldOffset(0x0030)] public TArray<FStaffRollTableData> Data;
            
        }

    }
}
