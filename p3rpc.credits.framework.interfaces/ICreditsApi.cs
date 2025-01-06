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
        public string? ModID { get; set; }
        public required string FirstColumnName { get; set; }
        public string? SecondColumnName { get; set; } = "";
        public string? ThirdColumnName { get; set; } = "";
        public string? FourthColumnName { get; set; } = "";
        public bool? LineCommand { get; set; } = false;
        public int? LineCount { get; set; } = 0;
        public int? EmptyCount { get; set; } = 8;
        public byte? FirstCommand { get; set; } = 5;
        public byte? SecondCommand { get; set; } = 5;
        public byte? ThirdCommand { get; set; } = 5;
        public byte? FourthCommand { get; set; } = 182;
        public FColor? FirstColor { get; set; } = new FColor { R = 0, G = 0, B = 0, A = 255 };
        public FColor? SecondColor { get; set; } = new FColor { R = 0, G = 0, B = 0, A = 255 };
        public FColor? ThirdColor { get; set; } = new FColor { R = 0, G = 0, B = 0, A = 255 };
        public FColor? FourthColor { get; set; } = new FColor { R = 0, G = 0, B = 0, A = 255 };
        public float? FinishSeconds { get; set; } = 0.0f;
        public float? StartWaitSeconds { get; set; } = 8.0f;
        public float? LastSeconds { get; set; } = 0.0f;
        public int? TableIndex { get; set; }
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