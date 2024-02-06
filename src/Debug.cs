
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
    public static void Nuke(CCSPlayerController? invoke, CommandInfo command)
    {
        Chat.announce(DEBUG_PREFIX,"Slaying all players");

        Chat.print_console_all("Nuke!");

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            player.slay();
        }       
    }

    [RequiresPermissions("@jail/debug")]
    public static void TestLaser(CCSPlayerController? invoke, CommandInfo command)
    {
        CCSPlayerPawn? pawn = invoke.pawn();

        if(pawn != null && pawn.AbsOrigin != null)
        {
            Circle marker = new Circle();

            marker.Draw(30.0f,72.0f,pawn.AbsOrigin);
        }
    }
    
    // block   0b000000000001011000011000000010001
    // noblock 0b100000000001011000011000000010001;

    [RequiresPermissions("@jail/debug")]
    public static void TestNoblockCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(!invoke.is_valid())
        {
            return;
        }

        invoke.PrintToChat("changed collision");

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            var pawn = player.pawn();

            if(pawn == null)
            {
                continue;
            }

            var col = pawn.Collision;

            if(col != null)
            {
                pawn.MyCollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS_TRIGGER;
                col.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS_TRIGGER;
            }
        }      
    }

    [RequiresPermissions("@jail/debug")]
    public static void TestStripCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        invoke.StripWeapons(true);
    }

    [RequiresPermissions("@jail/debug")]
    public static void JoinCtCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(invoke != null && invoke.is_valid())
        {
            invoke.SwitchTeam(CsTeam.CounterTerrorist);
        }
    }

    [RequiresPermissions("@jail/debug")]
    public static void HideWeaponCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(invoke != null && invoke.is_valid())
        {
            invoke.PrintToChat("hiding weapons");
        }

        invoke.hide_weapon();
    }

    [RequiresPermissions("@jail/debug")]
    public static void WSDEnableCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(invoke != null && invoke.is_valid())
        {
            invoke.PrintToChat("enable wsd");
            JailPlugin.sd.wsd_round = 0x7000_0000;
        }
    }

    [RequiresPermissions("@jail/debug")]
    public static void IsMutedCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(!invoke.is_valid())
        {
            return;
        }

        invoke.PrintToConsole("Is muted?");

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            invoke.PrintToConsole($"{player.PlayerName} : {player.VoiceFlags.HasFlag(VoiceFlags.Muted)} : {player.VoiceFlags.HasFlag(VoiceFlags.ListenAll)} : {player.VoiceFlags.HasFlag(VoiceFlags.ListenTeam)}");
        } 
    }

    [RequiresPermissions("@jail/debug")]
    public static void TestLRInc(CCSPlayerController? invoke, CommandInfo command)
    {
        JailPlugin.WinLR(invoke, LastRequest.LRType.KNIFE);
    }

    // are these commands allowed or not?
    public static readonly bool enable = true;

    public static readonly String DEBUG_PREFIX = $" {ChatColors.Green}[DEBUG]: {ChatColors.White}";    
}