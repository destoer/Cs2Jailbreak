
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
            player.slay();
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
        CCSPlayerPawn? pawn = invoke.pawn();

        if(pawn != null && pawn.AbsOrigin != null)
        {
            Vector end = new Vector(pawn.AbsOrigin.X + 100.0f,pawn.AbsOrigin.Y + 100.0f,pawn.AbsOrigin.Z + 100.0f);

            Lib.draw_laser(pawn.AbsOrigin,end,2.0f,10.0f,Lib.CYAN);
        }
    }
    
    [RequiresPermissions("@jail/debug")]
    public static void test_strip_cmd(CCSPlayerController? invoke, CommandInfo command)
    {
        invoke.strip_weapons(true);
    }

    // are these commands allowed or not?
    public static readonly bool enable  = true;

    public static readonly String DEBUG_PREFIX = $" {ChatColors.Green}[DEBUG]: {ChatColors.White}";    
}