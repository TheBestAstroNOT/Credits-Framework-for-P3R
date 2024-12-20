using System.Drawing;

namespace p3rpc.credits.framework.interfaces
{
    public interface ICreditsApi
    {
        void AddCredit(string modID, string firstColumnName, string secondColumnName, string thirdColumnName, string fourthColumnName, bool lineCommand = false, int lineCount = 0, int emptyCount = 8, byte firstCommand = 5, byte secondCommand = 60, byte thirdCommand = 187, byte fourthCommand = 182, Color? firstColor = null, Color? secondColor = null, Color? thirdColor = null, Color? fourthColor = null, float finishSeconds = 490.00f, float startWaitSeconds = 8.00f, float lastSeconds = 5.00f, int? tableIndex=null);

        void printMessage(string msg);
    }
}