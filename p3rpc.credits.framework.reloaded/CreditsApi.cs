using System.Runtime.InteropServices;
using p3rpc.credits.framework.interfaces;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;
using System.Drawing;

namespace p3rpc.credits.framework.reloaded
{
    internal class CreditsApi : ICreditsApi
    {
        private readonly IUnreal _unreal;
        private readonly IUObjects _uObject;
        SortedDictionary<string, List<(string, string, string, string, byte, byte, byte, byte, bool, int, int, FColor, FColor, FColor, FColor, float, float, float, int?)>> creditsByModID = [];
        
        public CreditsApi(IUObjects uObject, IUnreal unreal)
        {
             _unreal = unreal;
            _uObject = uObject;
            _uObject.FindObject("StaffRollDataAsset_C", this.UpdateCreditsData);
            printMessage("Searching for StaffRollDataAsset_C");
        }
        
        private readonly List<string> _strings = new List<string>();

        // Event to notify when a new string is added
        public event Action<string>? StringAdded;

        public void printMessage(string msg)
        {
            _strings.Add(msg);
            // Trigger the event for the new string
            StringAdded?.Invoke(msg);
        }

        public IEnumerable<string> GetStrings()
        {
            return _strings;
        }


        /// <summary>
        /// Calculates the area of a rectangle.
        /// </summary>
        /// <param name="modID"> The modID refers to a unique identity for your mod which will be used for handling your credits. If you want your credits to be grouped together use the same modID.</param>
        /// <param name="firstColumnName">The name of the first entry in the credit.</param>
        /// <param name = "secondColumnName">The name of the second entry in the credit.</param>
        /// <param name="thirdColumnName">The name of the third entry in the credit.</param>
        /// <param name = "fourthColumnName">The name of the fourth entry in the credit.</param>
        /// <param name="firstColor">The color of the first entry. Leave null for the default value.</param>
        /// <param name="secondColor">The color of the second entry. Leave null for the default value.</param>
        /// <param name="thirdColor">The color of the third entry. Leave null for the default value.</param>
        /// <param name="fourthColor">The color of the fourth entry. Leave null for the default value.</param>
        /// <param name="firstCommand">The command that you want to be executed for the first entry. Leave null for the default value. Check the documentation for more information on commands.</param>
        /// <param name="secondCommand">The command that you want to be executed for the second entry. Leave null for the default value. Check the documentation for more information on commands.</param>
        /// <param name="thirdCommand">The command that you want to be executed for the third entry. Leave null for the default value. Check the documentation for more information on commands.</param>
        /// <param name="fourthCommand">The command that you want to be executed for the fourth entry. Leave null for the default value. Check the documentation for more information on commands.</param>
        /// <param name="lineCommand">Enable it if you want the side by side credits. If enabled the first column name will be used for the left side title. Default value is false. Check the documentation for more information.</param>
        /// <param name="lineCount">The number of (vertical side by side) lines that will appear if line command is enabled. Default value is 0. Check the documentation for more information.</param>
        /// <param name="emptyCount">The number of blank lines between this and the next credit. Default Value is 8.</param>
        /// <param name="finishSeconds">The number of seconds that the credit is on screen. It is recommended that you don't assign this value as there is a lack of documentation on this value.</param>
        /// <param name="startWaitSeconds">The number of seconds that pass after the previous credit for this one to appear. It is recommended that you don't assign this value as there is a lack of documentation on this value.</param>
        /// <param name="lastSeconds">The number of seconds that the credit is on screen. It is recommended that you don't assign this value as there is a lack of documentation on this value.</param>
        /// <param name="tableIndex">It is recommended that you DO NOT ASSIGN this value. Allows you to overwrite an existing data asset value.</param>
        /// <returns>The area of the rectangle in square units.</returns>
        public unsafe void AddCredit(string modID, string firstColumnName="", string secondColumnName="", string thirdColumnName="", string fourthColumnName="", bool lineCommand = false, int lineCount = 0, int emptyCount = 8, byte firstCommand = 5, byte secondCommand = 60, byte thirdCommand = 187, byte fourthCommand = 182, Color? firstColor = null, Color? secondColor = null, Color? thirdColor = null, Color? fourthColor = null, float finishSeconds = 490.00f, float startWaitSeconds = 8.00f, float lastSeconds = 5.00f, int? tableIndex=null)
        {

            FColor intfirstColor = ColorConverter.ToFColor(firstColor.GetValueOrDefault(Color.FromArgb(255, 0, 0, 0)));
            FColor intsecondColor = ColorConverter.ToFColor(secondColor.GetValueOrDefault(Color.FromArgb(255, 0, 0, 0)));
            FColor intthirdColor = ColorConverter.ToFColor(thirdColor.GetValueOrDefault(Color.FromArgb(255, 0, 0, 0)));
            FColor intfourthColor = ColorConverter.ToFColor(fourthColor.GetValueOrDefault(Color.FromArgb(255, 0, 0, 0)));
            if (creditsByModID.ContainsKey(modID))
            {
                creditsByModID[modID].Add((firstColumnName, secondColumnName, thirdColumnName, fourthColumnName, firstCommand, secondCommand, thirdCommand, fourthCommand, lineCommand, lineCount, emptyCount, intfirstColor, intsecondColor, intthirdColor, intfourthColor, finishSeconds, startWaitSeconds, lastSeconds, tableIndex));
            }
            else
            {
                creditsByModID.Add(modID, [(firstColumnName, secondColumnName, thirdColumnName, fourthColumnName, firstCommand, secondCommand, thirdCommand, fourthCommand, lineCommand, lineCount, emptyCount, intfirstColor, intsecondColor, intthirdColor, intfourthColor, finishSeconds, startWaitSeconds, lastSeconds, tableIndex)]);
            }  
        }

        private unsafe void UpdateCreditsData(UnrealObject obj)
        {
            var CreditTable = (UStaffRollDataAsset*)obj.Self;
            int StaffRollIndex = 1410;
            int TableIndex = 749;
            var newItem = &CreditTable->Data.AllocatorInstance[TableIndex];
            foreach (KeyValuePair<string, List<(string, string, string, string, byte, byte, byte, byte, bool, int, int, FColor, FColor, FColor, FColor, float, float, float, int?)>> dictitem in creditsByModID)
            {
                foreach (var item in dictitem.Value)
                {
                    newItem = (item.Item19 == null || item.Item19 > TableIndex) ? &CreditTable->Data.AllocatorInstance[TableIndex] : &CreditTable->Data.AllocatorInstance[item.Item19.GetValueOrDefault(TableIndex)];
                    newItem->StaffRollIndex = (item.Item19 == null || item.Item19 > TableIndex) ? StaffRollIndex : item.Item19.GetValueOrDefault(StaffRollIndex);
                    newItem->FirstColumnName = new FString(_unreal, item.Item1);
                    newItem->SecondColumnName = new FString(_unreal, item.Item2);
                    newItem->ThirdColumnName = new FString(_unreal, item.Item3);
                    newItem->ForthColumnName = new FString(_unreal, item.Item4);
                    newItem->Ficolor = item.Item12;
                    newItem->Scolor = item.Item13;
                    newItem->Tcolor = item.Item14;
                    newItem->Focolor = item.Item15;
                    newItem->Fisize = 1;
                    newItem->Ssize = 1;
                    newItem->Tsize = 1;
                    newItem->Fosize = 1;
                    newItem->Fistyle = 0;
                    newItem->Sstyle = 0;
                    newItem->Tstyle = 0;
                    newItem->Fostyle = 0;
                    newItem->Command = item.Item5;
                    newItem->SecondCommand = item.Item6;
                    newItem->ThirdCommand = item.Item7;
                    newItem->ForthCommand = item.Item8;
                    newItem->LineCommand = false;
                    newItem->LastSeconds = item.Item18;
                    newItem->FinishSeconds = item.Item16;
                    newItem->StartWaitSeconds = item.Item17;
                    newItem->LineCommand = item.Item9;
                    newItem->LineCount = item.Item10;
                    newItem->EmptyCount = item.Item11;
                    StaffRollIndex = (item.Item19 == null || item.Item19 > TableIndex) ? StaffRollIndex + item.Item11 + 1 : StaffRollIndex;
                    TableIndex = (item.Item19 == null || item.Item19 > TableIndex) ? TableIndex+1 : TableIndex;
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

        [StructLayout(LayoutKind.Explicit, Size = 0x10)]
        public unsafe struct FColor
        {
            [FieldOffset(0x0000)] public byte B;
            [FieldOffset(0x0001)] public byte G;
            [FieldOffset(0x0002)] public byte R;
            [FieldOffset(0x0003)] public byte A;
        }

        public static class ColorConverter
        {
            public static FColor ToFColor(Color color)
            {
                return new FColor
                {
                    R = color.R,
                    G = color.G,
                    B = color.B,
                    A = color.A
                };
            }

            public static Color FromFColor(FColor fColor)
            {
                return Color.FromArgb(fColor.A, fColor.R, fColor.G, fColor.B);
            }
        }



    }
}
