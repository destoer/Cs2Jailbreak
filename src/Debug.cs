
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

// debugging commands:
// TODO: these need to be sealed by admin
public static class Debug
{
    public static void nuke(CCSPlayerController? invoke, CommandInfo command)
    {
        Lib.announce(DEBUG_PREFIX,"Slaying all players");

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            player.PlayerPawn.Value.CommitSuicide(true,true);
        }       
    }

    // are these commands allowed or not?
    public static readonly bool enable  = true;

    const String DEBUG_PREFIX = "[DEBUG]: ";    
}