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
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Admin;
using System.Drawing;

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

    static public void print_console_all(String str)
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid())
            {
                continue;
            }

            player.PrintToConsole(str);
        }
    }

    // Cheers Kill for suggesting method extenstions
    static public bool is_valid(this CCSPlayerController? player)
    {
        return player != null && player.IsValid &&  player.PlayerPawn.IsValid;
    }

    static public bool is_t(this CCSPlayerController? player)
    {
        return player != null && is_valid(player) && player.TeamNum == TEAM_T;
    }

    static public bool is_ct(this CCSPlayerController? player)
    {
        return player != null && is_valid(player) && player.TeamNum == TEAM_CT;
    }

    // yes i know the null check is redundant but C# is dumb
    static public bool is_valid_alive(this CCSPlayerController? player)
    {
        return player != null && player.is_valid() && player.PawnIsAlive;
    }

    static public void set_health(this CCSPlayerController? player, int hp)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.PlayerPawn.Value.Health = hp;
    }

    static public void set_movetype(this CCSPlayerController? player, MoveType_t type)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.PlayerPawn.Value.MoveType = type;
    }

    static public void set_gravity(this CCSPlayerController? player, float value)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.PlayerPawn.Value.GravityScale = value;
    }

    static public void set_velocity(this CCSPlayerController? player, float value)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.PlayerPawn.Value.Speed = value;
    }


    static public void set_armour(this CCSPlayerController? player, int hp)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.PlayerPawn.Value.ArmorValue = hp;
    }

    static public void strip_weapons(this CCSPlayerController? player, bool remove_knife = false)
    {
        // only care if player is valid
        if(player == null || !player.is_valid_alive())
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
            if(weapon == null || !is_valid(weapon))
            {
                continue;
            }
            
            weapon.Value.Remove();
        }

        // dont remove knife its buggy
        if(!remove_knife)
        {
            player.GiveNamedItem("weapon_knife");
        }
    }

    static public void set_colour(this CCSPlayerController? player, Color colour)
    {
        if(player == null || !player.is_valid_alive())
        {
            return;
        }

        player.PlayerPawn.Value.RenderMode = RenderMode_t.kRenderTransColor;
        player.PlayerPawn.Value.Render = colour;
    }

    static public bool is_generic_admin(this CCSPlayerController? player)
    {
        if(player == null || !player.is_valid())
        {
            return false;
        }

        return AdminManager.PlayerHasPermissions(player,new String[] {"@css/generic"});
    }

    static public void mute(this CCSPlayerController? player)
    {
        // admins cannot be muted by the plugin
        if(!player.is_generic_admin())
        {

        }
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



    static public void kill_timer(ref CSTimer.Timer? timer)
    {
        if(timer != null)
        {
            timer.Kill();
            timer = null;
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

    static public bool is_valid(this CHandle<CBasePlayerWeapon>? weapon)
    {
        return weapon != null && weapon.IsValid && weapon.Value != null && weapon.Value.IsValid;
    }

    static public bool is_valid(this CBasePlayerWeapon? weapon)
    {
        return weapon != null && weapon.IsValid;
    }

    static public CBasePlayerWeapon? find_weapon(this CCSPlayerController? player, String name)
    {
        // only care if player is valid
        if(player == null || !player.is_valid_alive())
        {
            return null;
        }

        var weapons = player.Pawn.Value.WeaponServices?.MyWeapons;

        if(weapons == null)
        {
            return null;
        }

        foreach (var weapon in weapons)
        {
            if(weapon == null || !is_valid(weapon))
            {
                continue;
            }

            if(weapon.Value.DesignerName.Contains(name))
            {
                return weapon.Value;
            }
        }

        return null;
    }

    static public void set_ammo(this CBasePlayerWeapon? weapon, int clip, int reserve)
    {
        if(weapon == null || !weapon.is_valid())
        {
            return;
        }

        weapon.Clip1 = clip;
        weapon.ReserveAmmo[0] = reserve;
    }

    // TODO: for now this is just a give guns
    // because menus dont work
    static public void event_gun_menu(this CCSPlayerController? player)
    {
        // Event has been cancelled in the mean time dont give any guns
        if(!JailPlugin.event_active())
        {
            return;
        }

        player.gun_menu(false);
    }

    static void give_menu_weapon(CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.is_valid())
        {
            return;
        }

        strip_weapons(player);

        player.GiveNamedItem("weapon_" + option.Text);
        player.GiveNamedItem("weapon_deagle");

        player.GiveNamedItem("item_assaultsuit");
    }

    static String[] GUN_LIST =
    {	
        "ak47", "m4a1_silencer","nova",
        "p90", "m249", "mp5sd",
        "galilar", "sg556","bizon", "aug",
        "famas", "xm1014","ssg08","awp"
        
    };
    static public void gun_menu_internal(this CCSPlayerController? player, bool no_awp, Action<CCSPlayerController, ChatMenuOption> callback)
    {
        // player must be alive and active!
        if(player == null || !player.is_valid_alive())
        {
            return;
        } 

    
        var gun_menu = new ChatMenu("Gun Menu");

        foreach(var weapon_name in GUN_LIST)
        {
            if(no_awp && weapon_name == "awp")
            {
                continue;
            }

            gun_menu.AddMenuOption(weapon_name, callback);
        }

        ChatMenus.OpenMenu(player, gun_menu);
    }

    static public void gun_menu(this CCSPlayerController? player, bool no_awp)
    {
        gun_menu_internal(player,no_awp,give_menu_weapon);
    }

    // chat + centre text print
    static public void announce(String prefix,String str)
    {
        Server.PrintToChatAll(prefix + str);
        print_centre_all(str);
    }

    static public void announce(this CCSPlayerController? player,String prefix,String str)
    {
        if(player != null && player.is_valid())
        {
            player.PrintToChat(prefix + str);
            player.PrintToCenter(str);
        }
    }

    static public List<CCSPlayerController> get_alive_ct()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.is_valid_alive() && player.is_ct());
    }

    static public int alive_ct_count()
    {
        return get_alive_ct().Count;
    }

    static public List<CCSPlayerController> get_alive_t()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.is_valid_alive() && player.is_t());;
    }

    // this doesnt work?
    static public void set_pickup(this CCSPlayerController? player,bool v)
    {
        if(player == null || !player.is_valid())
        {
            return; 
        }

        if(player.Pawn.Value.WeaponServices != null)
        {
            player.Pawn.Value.WeaponServices.PreventWeaponPickup = v;
        }
    }

    static public int alive_t_count()
    {
        return get_alive_t().Count;
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

    // why doesn't this work lol
    static public void set_cvar_str(String name, String value)
    {
        ConVar? cvar = ConVar.Find(name);

        if(cvar != null)
        {
            cvar.StringValue = value;
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

    public const int HITGROUP_HEAD = 0x1;
}