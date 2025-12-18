using System.Runtime.InteropServices;
using p3rpc.credits.framework.interfaces;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;
using UnrealEssentials.Interfaces;

namespace p3rpc.credits.framework.reloaded
{
    /// <summary>
    /// Implementation of the Credits API for managing credits in Persona 3 Reload.
    /// This class handles the modification of the game's staff roll data asset.
    /// </summary>
    internal class CreditsApi : ICreditsApi
    {
        private readonly IUnreal _unreal;
        private readonly IUObjects _uObject;
        private readonly IUnrealEssentials _unrealEssentials;

        /// <summary>
        /// Gets the dictionary of credits organized by mod ID.
        /// </summary>
        public SortedDictionary<string, List<CreditEntry>> creditsByModID = [];

        /// <summary>
        /// Gets the dictionary of configuration settings organized by mod ID.
        /// </summary>
        public SortedDictionary<string, Dictionary<string, bool>> configByModID = [];

        /// <summary>
        /// Gets the dictionary mapping mod IDs to their display names.
        /// </summary>
        public SortedDictionary<string, string> ModNameByModID = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditsApi"/> class.
        /// </summary>
        /// <param name="uObject">The Unreal Objects service for finding and modifying game objects.</param>
        /// <param name="unreal">The Unreal service for memory allocation and string creation.</param>
        /// <param name="unrealEssentials">The Unreal Essentials service for memory management.</param>
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

        /// <summary>
        /// Adds a credit entry to the credits list. Credits with the same ModID will be grouped together.
        /// </summary>
        /// <param name="credit">The credit entry to add.</param>
        public void AddManualCredit(CreditEntry credit)
        {
            if (creditsByModID.TryGetValue(credit.ModID!, out List<CreditEntry>? value))
            {
                value.Add(credit);         
            }
            else
            {
                creditsByModID.Add(credit.ModID!, [credit]);
            }
        }

        /// <summary>
        /// Deletes all credits associated with a specific mod ID.
        /// </summary>
        /// <param name="modID">The mod ID whose credits should be removed.</param>
        public void DeleteCredit(string modID)
        {
            creditsByModID.Remove(modID);
        }

        /// <summary>
        /// Toggles a configuration setting for a specific mod.
        /// </summary>
        /// <param name="modID">The mod ID for which to set the configuration.</param>
        /// <param name="ModName">The display name of the mod.</param>
        /// <param name="config">The configuration key to set.</param>
        /// <param name="configval">The value to set for the configuration.</param>
        public void ToggleConfigbyModID(string modID, string ModName, string config, bool configval)
        {
            if (configByModID.TryGetValue(modID, out Dictionary<string, bool>? value))
            {
                value.Add(config, configval);    
            }
            else
            {
                configByModID.Add(modID, new Dictionary<string, bool> { { config, true } });
                ModNameByModID.Add(modID, ModName);
            }
        }

        /// <summary>
        /// Updates the staff roll data asset with custom credits.
        /// This method allocates new memory for the credits table, copies existing credits,
        /// adds custom credits, and adds a final butterfly icon entry.
        /// </summary>
        /// <param name="obj">Pointer to the UStaffRollDataAsset to update.</param>
        /// <returns>Pointer to the updated UStaffRollDataAsset.</returns>
        private unsafe UStaffRollDataAsset* UpdateCreditsData(UStaffRollDataAsset* obj)
        {
            //Original UStaffRollDataAssetInfo (Number of elements: 750, max index: 749)

            //Check how many elements we need to add
            int elementsnum = 750;
            foreach (KeyValuePair<string, List<CreditEntry>> dictitem in creditsByModID)
            {
                if(configByModID.TryGetValue(dictitem.Key, out Dictionary<string, bool>? value) && (value.TryGetValue("autoheader", out bool headerConfig) && headerConfig))
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
                if (configByModID.TryGetValue(dictitem.Key, out Dictionary<string, bool>? value) && value.TryGetValue("autoheader", out bool headerConfig) && headerConfig)
                {
                    var headerItem = &obj->Data.AllocatorInstance[TableIndex];
                    headerItem->FirstColumnName = _unreal.FString(ModNameByModID[dictitem.Key]);
                    headerItem->Command = 1;
                    headerItem->Fisize = 1;
                    headerItem->Fistyle = 0;
                    headerItem->Ficolor = new FColor { R = 0, G = 0, B = 0, A = 255 };
                    headerItem->EmptyCount = 0;
                    headerItem->StartWaitSeconds = 8.0f;
                    headerItem->LastSeconds = 0.0f;
                    headerItem->FinishSeconds = 0.0f;
                    headerItem->StaffRollIndex = StaffRollIndex;
                    TableIndex++;
                    StaffRollIndex++;
                    Console.WriteLine($"Auto Header Added: {ModNameByModID[dictitem.Key]}");
                }
                foreach (var item in dictitem.Value)
                {
                    var newItem = (item.TableIndex == null || item.TableIndex > TableIndex)
                        ? &obj->Data.AllocatorInstance[TableIndex]
                        : &obj->Data.AllocatorInstance[item.TableIndex.GetValueOrDefault(TableIndex)];
                    newItem->StaffRollIndex = (item.TableIndex == null || item.TableIndex > TableIndex) ? StaffRollIndex : item.TableIndex.GetValueOrDefault(StaffRollIndex);
                    newItem->FirstColumnName = _unreal.FString(item.FirstColumnName ?? "");
                    newItem->SecondColumnName = _unreal.FString(item.SecondColumnName ?? "");
                    newItem->ThirdColumnName = _unreal.FString(item.ThirdColumnName ?? "");
                    newItem->ForthColumnName = _unreal.FString(item.FourthColumnName ?? "s");
                    newItem->Ficolor = item.FirstColor ?? new FColor { R = 0, G = 0, B = 0, A = 255 };
                    newItem->Scolor = item.SecondColor ?? new FColor { R = 0, G = 0, B = 0, A = 255 };
                    newItem->Tcolor = item.ThirdColor ?? new FColor { R = 0, G = 0, B = 0, A = 255 };
                    newItem->Focolor = item.FourthColor ?? new FColor { R = 0, G = 0, B = 0, A = 255 };
                    newItem->Fisize = 1;
                    newItem->Ssize = 1;
                    newItem->Tsize = 1;
                    newItem->Fosize = 1;
                    newItem->Fistyle = 0;
                    newItem->Sstyle = 0;
                    newItem->Tstyle = 0;
                    newItem->Fostyle = 0;
                    newItem->Command = item.FirstCommand ?? 5;
                    newItem->SecondCommand = item.SecondCommand ?? 5;
                    newItem->ThirdCommand = item.ThirdCommand ?? 5;
                    newItem->ForthCommand = item.FourthCommand ?? 182;
                    newItem->LineCommand = item.LineCommand ?? false;
                    newItem->LastSeconds = item.LastSeconds ?? 0.0f;
                    newItem->FinishSeconds = item.FinishSeconds ?? 0.0f;
                    newItem->StartWaitSeconds = item.StartWaitSeconds ?? 8.0f;
                    newItem->LineCount = item.LineCount ?? 0;
                    newItem->EmptyCount = item.EmptyCount ?? 8;
                    StaffRollIndex = (item.TableIndex == null || item.TableIndex > TableIndex) ? StaffRollIndex + (item.EmptyCount ?? 8) + 1 : StaffRollIndex;
                    TableIndex = (item.TableIndex == null || item.TableIndex > TableIndex) ? TableIndex + 1 : TableIndex;
                }
            }

            //Add a cute butterfly icon at the end and move the custom timings from the atlus logo to the butterfly icon
            var newitem = &obj->Data.AllocatorInstance[749];
            obj->Data.AllocatorInstance[749].FinishSeconds = 0.0f;
            obj->Data.AllocatorInstance[749].LastSeconds = 0.0f;
            obj->Data.AllocatorInstance[749].EmptyCount = 10;
            newitem = &obj->Data.AllocatorInstance[TableIndex-1];
            newitem->EmptyCount = 15;
            newitem = &obj->Data.AllocatorInstance[TableIndex];
            newitem->FinishSeconds = 490.0f;
            newitem->LastSeconds = 5.0f;
            newitem->FirstColumnName = _unreal.FString("5");
            newitem->Command = 3;
            obj->Data.Num = TableIndex + 1;
            
            //Finally return the object to replace the ingame credits
            return obj;
        }

        /// <summary>
        /// Represents a single entry in the staff roll table data.
        /// This structure matches the Unreal Engine layout for staff roll entries.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 0x80)]
        public unsafe struct FStaffRollTableData
        {
            /// <summary>
            /// Gets or sets the zero-based index of this entry in the staff roll sequence.
            /// </summary>
            [FieldOffset(0x0000)] public int StaffRollIndex;

            /// <summary>
            /// Gets or sets the text for the first column.
            /// </summary>
            [FieldOffset(0x0008)] public FString FirstColumnName;

            /// <summary>
            /// Gets or sets the text for the second column.
            /// </summary>
            [FieldOffset(0x0018)] public FString SecondColumnName;

            /// <summary>
            /// Gets or sets the text for the third column.
            /// </summary>
            [FieldOffset(0x0028)] public FString ThirdColumnName;

            /// <summary>
            /// Gets or sets the text for the fourth column.
            /// </summary>
            [FieldOffset(0x0038)] public FString ForthColumnName;

            /// <summary>
            /// Gets or sets the color of the first column text.
            /// </summary>
            [FieldOffset(0x0048)] public FColor Ficolor;

            /// <summary>
            /// Gets or sets the color of the second column text.
            /// </summary>
            [FieldOffset(0x004C)] public FColor Scolor;

            /// <summary>
            /// Gets or sets the color of the third column text.
            /// </summary>
            [FieldOffset(0x0050)] public FColor Tcolor;

            /// <summary>
            /// Gets or sets the color of the fourth column text.
            /// </summary>
            [FieldOffset(0x0054)] public FColor Focolor;

            /// <summary>
            /// Gets or sets the style for the first column text.
            /// </summary>
            [FieldOffset(0x0058)] public byte Fistyle;

            /// <summary>
            /// Gets or sets the style for the second column text.
            /// </summary>
            [FieldOffset(0x0059)] public byte Sstyle;

            /// <summary>
            /// Gets or sets the style for the third column text.
            /// </summary>
            [FieldOffset(0x005A)] public byte Tstyle;

            /// <summary>
            /// Gets or sets the style for the fourth column text.
            /// </summary>
            [FieldOffset(0x005B)] public byte Fostyle;

            /// <summary>
            /// Gets or sets the size for the first column text.
            /// </summary>
            [FieldOffset(0x005C)] public byte Fisize;

            /// <summary>
            /// Gets or sets the size for the second column text.
            /// </summary>
            [FieldOffset(0x005D)] public byte Ssize;

            /// <summary>
            /// Gets or sets the size for the third column text.
            /// </summary>
            [FieldOffset(0x005E)] public byte Tsize;

            /// <summary>
            /// Gets or sets the size for the fourth column text.
            /// </summary>
            [FieldOffset(0x005F)] public byte Fosize;

            /// <summary>
            /// Gets or sets the primary command that controls how this credit entry is displayed and formatted.
            /// This corresponds to the FirstCommand property in the API.
            /// </summary>
            [FieldOffset(0x0060)] public byte Command;

            /// <summary>
            /// Gets or sets the command for the second column (determines formatting/layout behavior).
            /// </summary>
            [FieldOffset(0x0061)] public byte SecondCommand;

            /// <summary>
            /// Gets or sets the command for the third column (determines formatting/layout behavior).
            /// </summary>
            [FieldOffset(0x0062)] public byte ThirdCommand;

            /// <summary>
            /// Gets or sets the command for the fourth column (determines formatting/layout behavior).
            /// </summary>
            [FieldOffset(0x0063)] public byte ForthCommand;

            /// <summary>
            /// Gets or sets the number of lines to display when LineCommand is enabled.
            /// </summary>
            [FieldOffset(0x0064)] public int LineCount;

            /// <summary>
            /// Gets or sets whether to use side-by-side layout mode.
            /// </summary>
            [FieldOffset(0x0068)] public bool LineCommand;

            /// <summary>
            /// Gets or sets the number of empty lines to display after this entry.
            /// </summary>
            [FieldOffset(0x006C)] public int EmptyCount;

            /// <summary>
            /// Gets or sets the number of seconds to wait before displaying this entry.
            /// </summary>
            [FieldOffset(0x0070)] public float StartWaitSeconds;

            /// <summary>
            /// Gets or sets the number of seconds this entry is displayed (finish timing).
            /// </summary>
            [FieldOffset(0x0074)] public float FinishSeconds;

            /// <summary>
            /// Gets or sets the number of seconds this entry is displayed (last timing).
            /// </summary>
            [FieldOffset(0x0078)] public float LastSeconds;
        }

        /// <summary>
        /// Represents the Unreal Engine data asset containing the staff roll table.
        /// This structure maps to the UStaffRollDataAsset_C class in the game.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 0x50)]
        public unsafe struct UStaffRollDataAsset
        {
            /// <summary>
            /// Gets or sets the array containing all staff roll table entries.
            /// </summary>
            [FieldOffset(0x0030)] public TArray<FStaffRollTableData> Data;
            
        }

    }
}
