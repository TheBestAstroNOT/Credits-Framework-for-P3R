using System.Drawing;
using System.Runtime.InteropServices;
namespace p3rpc.credits.framework.interfaces
{
    public interface ICreditsApi
    {
        void AddManualCredit(CreditEntry credit);
    }

    public class CreditEntry
    {
        public string ModID { get; set; }
        public string FirstColumnName { get; set; } = string.Empty;
        public string SecondColumnName { get; set; } = string.Empty;
        public string ThirdColumnName { get; set; } = string.Empty;
        public string FourthColumnName { get; set; } = string.Empty;
        public bool LineCommand { get; set; } = false;
        public int LineCount { get; set; } = 0;
        public int EmptyCount { get; set; } = 8;
        public byte FirstCommand { get; set; } = 5;
        public byte SecondCommand { get; set; } = 60;
        public byte ThirdCommand { get; set; } = 187;
        public byte FourthCommand { get; set; } = 182;
        public FColor FirstColor { get; set; } 
        public FColor SecondColor { get; set; }
        public FColor ThirdColor { get; set; }
        public FColor FourthColor { get; set; }
        public float FinishSeconds { get; set; } = 490.0f;
        public float StartWaitSeconds { get; set; } = 8.0f;
        public float LastSeconds { get; set; } = 5.0f;
        public int? TableIndex { get; set; }

        // Constructor to handle initialization and default color conversion
        public CreditEntry(string modID, string firstColumnName = "", string secondColumnName = "", string thirdColumnName = "", string fourthColumnName = "", bool lineCommand = false, int lineCount = 0, int emptyCount = 8, byte firstCommand = 5, byte secondCommand = 60, byte thirdCommand = 187, byte fourthCommand = 182, Color? firstColor = null, Color? secondColor = null, Color? thirdColor = null, Color? fourthColor = null, float finishSeconds = 490.0f, float startWaitSeconds = 8.0f, float lastSeconds = 5.0f, int? tableIndex = null)
        {
            ModID = modID;
            FirstColumnName = firstColumnName;
            SecondColumnName = secondColumnName;
            ThirdColumnName = thirdColumnName;
            FourthColumnName = fourthColumnName;
            LineCommand = lineCommand;
            LineCount = lineCount;
            EmptyCount = emptyCount;
            FirstCommand = firstCommand;
            SecondCommand = secondCommand;
            ThirdCommand = thirdCommand;
            FourthCommand = fourthCommand;
            FinishSeconds = finishSeconds;
            StartWaitSeconds = startWaitSeconds;
            LastSeconds = lastSeconds;
            TableIndex = tableIndex;
            FirstColor = ColorConverter.ToFColor(firstColor.GetValueOrDefault(Color.FromArgb(255, 0, 0, 0)));
            SecondColor = ColorConverter.ToFColor(secondColor.GetValueOrDefault(Color.FromArgb(255, 0, 0, 0)));
            ThirdColor = ColorConverter.ToFColor(thirdColor.GetValueOrDefault(Color.FromArgb(255, 0, 0, 0)));
            FourthColor = ColorConverter.ToFColor(fourthColor.GetValueOrDefault(Color.FromArgb(255, 0, 0, 0)));
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public struct FColor
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