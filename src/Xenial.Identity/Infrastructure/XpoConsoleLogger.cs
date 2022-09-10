using DevExpress.Xpo.Logger;

using Spectre.Console;

namespace Xenial.Identity.Infrastructure;

public class XpoConsoleLogger : DevExpress.Xpo.Logger.ILogger
{
    public int Count => 0;
    public int LostMessageCount => 0;
    public bool IsServerActive => true;
    public bool Enabled { get; set; } = true;
    public int Capacity => 0;
    public void ClearLog() { }

    public void Log(LogMessage message)
    {
        if (Enabled)
        {
            LogConsole.MarkupLineInterpolated($"[green]{message.Date} {message.Duration}[/]");
            LogConsole.MarkupLineInterpolated($"[gray][[{message.MessageType}]][/]");
            if (message.MessageType == LogMessageType.DbCommand)
            {
                ConsoleHelper.PrintSource(message.MessageText, "sql");
            }
            else
            {
                LogConsole.MarkupLineInterpolated($"\t[gray][[{message.MessageText}]][/]");
            }
            AnsiConsole.WriteLine();
        }
    }

    public void Log(LogMessage[] messages)
    {
        foreach (var msg in messages)
        {
            Log(msg);
        }
    }
}
