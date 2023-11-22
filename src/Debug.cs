
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Admin;

// debugging commands:
// TODO: these need to be sealed by admin
public static class Debug
{
    [RequiresPermissions("@jail/debug")]
    public static void nuke(CCSPlayerController? invoke, CommandInfo command)
    {
        Lib.announce(DEBUG_PREFIX,"Slaying all players");

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            player.PlayerPawn.Value.CommitSuicide(true,true);
        }       
    }

    [RequiresPermissions("@jail/debug")]
    public static void force_open_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        Lib.force_open();
    }

    [RequiresPermissions("@jail/debug")]
    public static void test_laser(CCSPlayerController? invoke, CommandInfo command)
    {
        // new Vector(-1181.000000f, -754.000000f, 182.934967f), new Vector(-1181.000000f, -800.000000f, 182.934967f)
        Lib.draw_laser(invoke);
    }
    
    // are these commands allowed or not?
    public static readonly bool enable  = true;

    public static readonly String DEBUG_PREFIX = $" {ChatColors.Green}[DEBUG]: {ChatColors.White}";    
}