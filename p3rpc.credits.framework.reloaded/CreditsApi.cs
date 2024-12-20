using System.Runtime.InteropServices;
using p3rpc.credits.framework.interfaces;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace p3rpc.credits.framework.reloaded
{
    internal class CreditsApi : ICreditsApi
    {
        private readonly IUnreal _unreal;
        private readonly IUObjects _uObject;
        SortedDictionary<string, List<CreditEntry>> creditsByModID = [];
        
        public CreditsApi(IUObjects uObject, IUnreal unreal)
        {
             _unreal = unreal;
            _uObject = uObject;
            _uObject.FindObject("StaffRollDataAsset_C", this.UpdateCreditsData);
        }

        public unsafe void AddManualCredit(CreditEntry credit)
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

        private unsafe void UpdateCreditsData(UnrealObject obj)
        {
            var CreditTable = (UStaffRollDataAsset*)obj.Self;
            int StaffRollIndex = 1410;
            int TableIndex = 749;
            var newItem = &CreditTable->Data.AllocatorInstance[TableIndex];
            foreach (KeyValuePair<string, List<CreditEntry>> dictitem in creditsByModID)
            {
                foreach (var item in dictitem.Value)
                {
                    newItem = (item.TableIndex == null || item.TableIndex > TableIndex) ? &CreditTable->Data.AllocatorInstance[TableIndex] : &CreditTable->Data.AllocatorInstance[item.TableIndex.GetValueOrDefault(TableIndex)];
                    newItem->StaffRollIndex = (item.TableIndex == null || item.TableIndex > TableIndex) ? StaffRollIndex : item.TableIndex.GetValueOrDefault(StaffRollIndex);
                    newItem->FirstColumnName = new FString(_unreal, item.FirstColumnName);
                    newItem->SecondColumnName = new FString(_unreal, item.SecondColumnName);
                    newItem->ThirdColumnName = new FString(_unreal, item.ThirdColumnName);
                    newItem->ForthColumnName = new FString(_unreal, item.FourthColumnName);
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
                    TableIndex = (item.TableIndex == null || item.TableIndex > TableIndex) ? TableIndex+1 : TableIndex;
                }
            }
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
