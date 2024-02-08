using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;

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

    [RequiresPermissions("@css/generic")]
    public void LogsCommand(CCSPlayerController? executor, CommandInfo info)
    {
        printLogs(executor);
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        foreach (CCSPlayerController player in Lib.GetPlayers())
        {
            if (!player.IsBot)
            {
                printLogs(player);
            }
        }
        logs.Clear();
        return HookResult.Continue;
    }

    private void printLogs(CCSPlayerController? player)
    {
        if (!player.IsLegal())
        {
            printLogs(Server.PrintToConsole);
        }
        else
        {
            printLogs(player.PrintToConsole);
        }
    }

    private void printLogs(Delegate printFunction)
    {
        printFunction.DynamicInvoke("********************************");
        printFunction.DynamicInvoke("***** BEGIN JAILBREAK LOGS *****");
        printFunction.DynamicInvoke("********************************");
        foreach (string log in logs)
        {
            printFunction.DynamicInvoke(log);
        }
        printFunction.DynamicInvoke("********************************");
        printFunction.DynamicInvoke("****** END JAILBREAK LOGS ******");
        printFunction.DynamicInvoke("********************************");
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        roundStart = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return HookResult.Continue;
    }

    public void Add(string log)
    {
        TimeSpan span = TimeSpan.FromSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - roundStart);
        string format = $"[{span:mm':'ss}] {log}";
        logs.Add(format);
    }

    public void AddLocalized(string key, params object[] args)
    {
        Add(plugin.Localizer[key, args]);
    }

    public void AddLocalized(CCSPlayerController source, string key, params object[] args)
    {
        args = args.Prepend(GetPlayerRole(source)).Prepend(source.PlayerName).ToArray();
        Add(plugin.Localizer[key, args]);
    }

    public void AddLocalized(CCSPlayerController source, CCSPlayerController target, string key, params object[] args)
    {
        args = args.Prepend(GetPlayerRole(target)).Prepend(target.PlayerName).Prepend(GetPlayerRole(source)).Prepend(source.PlayerName).ToArray();
        Add(plugin.Localizer[key, args]);
    }


    public String GetPlayerRole(CCSPlayerController player)
    {
        switch ((CsTeam)player.TeamNum)
        {
            case CsTeam.Spectator:
                return plugin.Localizer["role.spectator"];
            case CsTeam.CounterTerrorist:
                return plugin.Localizer[JailPlugin.warden.IsWarden(player) ? "role.warden" : "role.guard"];
            case CsTeam.Terrorist:
                return plugin.Localizer[JailPlugin.warden.jailPlayers[player.Slot].IsRebel ? "role.rebel" : "role.prisoner"];
            default:
                return plugin.Localizer["role.unknown"];
        }
    }
}