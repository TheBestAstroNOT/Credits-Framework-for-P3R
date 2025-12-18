using System.Drawing;
using System.Runtime.InteropServices;
namespace p3rpc.credits.framework.interfaces
{
    /// <summary>
    /// Provides an API for managing credits in the Persona 3 Reload credits framework.
    /// </summary>
    public interface ICreditsApi
    {
        /// <summary>
        /// Adds a credit entry to the game's credits roll.
        /// </summary>
        /// <param name="credit">The credit entry to add. See <see cref="CreditEntry"/> for details on configuring the credit.</param>
        void AddManualCredit(CreditEntry credit);
    }

    /// <summary>
    /// Represents a single credit entry that will appear in the game's credits roll.
    /// </summary>
    public class CreditEntry
    {
        /// <summary>
        /// Gets or sets the mod ID. This unique identifier for your mod is used for handling your credits.
        /// If you want your credits to be grouped together, use the same ModID for all entries.
        /// </summary>
        public string? ModID { get; set; }

        /// <summary>
        /// Gets or sets the name of the first entry in the credit. This is required.
        /// </summary>
        public required string FirstColumnName { get; set; }

        /// <summary>
        /// Gets or sets the name of the second entry in the credit. Default is empty string.
        /// </summary>
        public string? SecondColumnName { get; set; } = "";

        /// <summary>
        /// Gets or sets the name of the third entry in the credit. Default is empty string.
        /// </summary>
        public string? ThirdColumnName { get; set; } = "";

        /// <summary>
        /// Gets or sets the name of the fourth entry in the credit. Default is empty string.
        /// </summary>
        public string? FourthColumnName { get; set; } = "";

        /// <summary>
        /// Gets or sets whether to enable side-by-side credits layout. 
        /// If enabled, the first column name will be used for the left side title. Default is false.
        /// Check the documentation for more information.
        /// </summary>
        public bool? LineCommand { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of vertical side-by-side lines that will appear if LineCommand is enabled.
        /// Default is 0. Check the documentation for more information.
        /// </summary>
        public int? LineCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of blank lines between this and the next credit. Default is 8.
        /// </summary>
        public int? EmptyCount { get; set; } = 8;

        /// <summary>
        /// Gets or sets the command that you want to be executed for the first entry.
        /// Default is 5. Check the documentation for more information on commands.
        /// </summary>
        public byte? FirstCommand { get; set; } = 5;

        /// <summary>
        /// Gets or sets the command that you want to be executed for the second entry.
        /// Default is 5. Check the documentation for more information on commands.
        /// </summary>
        public byte? SecondCommand { get; set; } = 5;

        /// <summary>
        /// Gets or sets the command that you want to be executed for the third entry.
        /// Default is 5. Check the documentation for more information on commands.
        /// </summary>
        public byte? ThirdCommand { get; set; } = 5;

        /// <summary>
        /// Gets or sets the command that you want to be executed for the fourth entry.
        /// Default is 182. Check the documentation for more information on commands.
        /// </summary>
        public byte? FourthCommand { get; set; } = 182;

        /// <summary>
        /// Gets or sets the color of the first entry. Default is black (R=0, G=0, B=0, A=255).
        /// </summary>
        public FColor? FirstColor { get; set; } = new FColor { R = 0, G = 0, B = 0, A = 255 };

        /// <summary>
        /// Gets or sets the color of the second entry. Default is black (R=0, G=0, B=0, A=255).
        /// </summary>
        public FColor? SecondColor { get; set; } = new FColor { R = 0, G = 0, B = 0, A = 255 };

        /// <summary>
        /// Gets or sets the color of the third entry. Default is black (R=0, G=0, B=0, A=255).
        /// </summary>
        public FColor? ThirdColor { get; set; } = new FColor { R = 0, G = 0, B = 0, A = 255 };

        /// <summary>
        /// Gets or sets the color of the fourth entry. Default is black (R=0, G=0, B=0, A=255).
        /// </summary>
        public FColor? FourthColor { get; set; } = new FColor { R = 0, G = 0, B = 0, A = 255 };

        /// <summary>
        /// Gets or sets the number of seconds that the credit is on screen.
        /// It is recommended that you don't assign this value as there is a lack of documentation on this value.
        /// Default is 0.0f.
        /// </summary>
        public float? FinishSeconds { get; set; } = 0.0f;

        /// <summary>
        /// Gets or sets the number of seconds that pass after the previous credit for this one to appear.
        /// It is recommended that you don't assign this value as there is a lack of documentation on this value.
        /// Default is 8.0f.
        /// </summary>
        public float? StartWaitSeconds { get; set; } = 8.0f;

        /// <summary>
        /// Gets or sets the duration in seconds for the final display phase of the credit.
        /// It is recommended that you don't assign this value as there is a lack of documentation on this value.
        /// Default is 0.0f.
        /// </summary>
        public float? LastSeconds { get; set; } = 0.0f;

        /// <summary>
        /// Gets or sets the table index. It is recommended that you DO NOT ASSIGN this value.
        /// Allows you to overwrite an existing data asset value at the specified zero-based index.
        /// </summary>
        public int? TableIndex { get; set; }
    }


    /// <summary>
    /// Represents a color value in Unreal Engine format with BGRA byte order.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public struct FColor
    {
        /// <summary>
        /// Gets or sets the blue component of the color (0-255).
        /// </summary>
        [FieldOffset(0x0000)] public byte B;

        /// <summary>
        /// Gets or sets the green component of the color (0-255).
        /// </summary>
        [FieldOffset(0x0001)] public byte G;

        /// <summary>
        /// Gets or sets the red component of the color (0-255).
        /// </summary>
        [FieldOffset(0x0002)] public byte R;

        /// <summary>
        /// Gets or sets the alpha (transparency) component of the color (0-255).
        /// </summary>
        [FieldOffset(0x0003)] public byte A;
    }

    /// <summary>
    /// Provides utility methods for converting between System.Drawing.Color and FColor.
    /// </summary>
    public static class ColorConverter
    {
        /// <summary>
        /// Converts a System.Drawing.Color to an Unreal Engine FColor.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>An FColor representation of the input color.</returns>
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

        /// <summary>
        /// Converts an Unreal Engine FColor to a System.Drawing.Color.
        /// </summary>
        /// <param name="fColor">The FColor to convert.</param>
        /// <returns>A System.Drawing.Color representation of the input FColor.</returns>
        public static Color FromFColor(FColor fColor)
        {
            return Color.FromArgb(fColor.A, fColor.R, fColor.G, fColor.B);
        }
        
    }
}