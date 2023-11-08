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

public static class Lib
{
    // TODO: i dont think there is a builtin func for this...
    static public void print_centre_all(String str)
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid())
            {
                continue;
            }

            player.PrintToCenter(str);
        }
    }

    // Cheers Kill for suggesting method extenstions
    static public bool is_valid(this CCSPlayerController? player)
    {
        return player != null && player.IsValid &&  player.PlayerPawn.IsValid;
    }

    // yes i know the null check is redundant but C# is dumb
    static public bool is_valid_alive(this CCSPlayerController? player)
    {
        return player != null && player.is_valid() && player.PawnIsAlive;
    }

    static public void strip_weapons(this CCSPlayerController? player)
    {
        // only care if player is valid
        if(player == null || !player.is_valid())
        {
            return;
        }

        var weapons = player.Pawn.Value.WeaponServices?.MyWeapons;

        if(weapons == null)
        {
            return;
        }

        foreach (var weapon in weapons)
        {
            if (!weapon.IsValid) 
            { 
                continue;
            }
            
            weapon.Value.Remove();
        }
    }

    static public void mute(this CCSPlayerController? player)
    {

    }

    // TODO: this needs to be hooked into the ban system that becomes used
    static public void unmute(this CCSPlayerController? player)
    {

    }

    static public void mute_all()
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.is_valid())
            {
                player.mute();
            }
        }
    }

    static public void unmute_all()
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.is_valid())
            {
                player.unmute();
            }
        }
    }

    // TODO: for now this is just a give guns
    // because menus dont work
    static public void gun_menu(this CCSPlayerController? player)
    {
        // player must be alive and active!
        if(player == null || !player.is_valid_alive())
        {
            return;
        }

        strip_weapons(player);

        player.GiveNamedItem("weapon_deagle");
        player.GiveNamedItem("weapon_ak47");
    }

    // chat + centre text print
    static public void announce(String prefix,String str)
    {
        Server.PrintToChatAll(prefix + str);
        print_centre_all(str);
    }

    static public void block_all()
    {
        if(block_cvar != null)
        {
            block_cvar.SetValue(1);
        }
    }

    static public void unblock_all()
    {
        if(block_cvar != null)
        {
            block_cvar.SetValue(0);
        }
    }

    public static int? to_slot(int? user_id)
    {
        if(user_id == null)
        {
            return null;
        }

        return user_id & 0xff;
    }

    public static int? slot(this CCSPlayerController? player)
    {
        if(player == null)
        {
            return null;
        }

        return to_slot(player.UserId);
    }

    static ConVar? block_cvar = ConVar.Find("mp_solid_teammates");

    // CONST DEFS
    public const int TEAM_T = 2;
    public const int TEAM_CT = 3;
}