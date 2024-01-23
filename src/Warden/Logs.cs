using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

public class Logs
{
    private readonly List<string> logs = new List<string>();
    private readonly BasePlugin plugin;
    private long roundStart = -1;

    public Logs(BasePlugin plugin)
    {
        this.plugin = plugin;
        plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (!player.IsValid || player.IsBot || player.IsHLTV) continue;
            printLogs(player);
        }
        return HookResult.Continue;
    }

    private void printLogs(CCSPlayerController player)
    {
        player.PrintToConsole("********************************");
        player.PrintToConsole("***** BEGIN JAILBREAK LOGS *****");
        player.PrintToConsole("********************************");
        foreach (string log in logs)
        {
            player.PrintToConsole(log);
        }
        player.PrintToConsole("********************************");
        player.PrintToConsole("****** END JAILBREAK LOGS ******");
        player.PrintToConsole("********************************");
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        logs.Clear();
        roundStart = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return HookResult.Continue;
    }

    public void Add(string log)
    {
        string format = $"[{DateTimeOffset.UtcNow.ToUnixTimeSeconds() - roundStart}:mm\\:ss] {log}";
        logs.Add(log);
    }

    public void AddLocalized(string key, params object[] args)
    {
        Add(plugin.Localizer[key, args]);
    }
}