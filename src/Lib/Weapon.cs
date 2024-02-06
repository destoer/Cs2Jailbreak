using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Menu;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;

public static class Weapon
{
    static public bool is_valid(this CBasePlayerWeapon? weapon)
    {
        return weapon != null && weapon.IsValid;
    }

    static public CBasePlayerWeapon? find_weapon(this CCSPlayerController? player, String name)
    {
        // only care if player is alive
        if(!player.is_valid_alive())
        {
            return null;
        }

        CCSPlayerPawn? pawn = player.pawn();

        if(pawn == null)
        {
            return null;
        }

        var weapons = pawn.WeaponServices?.MyWeapons;

        if(weapons == null)
        {
            return null;
        }

        foreach (var weapon_opt in weapons)
        {
            CBasePlayerWeapon? weapon = weapon_opt.Value;

            if(weapon == null)
            {
                continue;
            }
         
            if(weapon.DesignerName.Contains(name))
            {
                return weapon;
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

        // overide reserve max so it doesn't get clipped when
        // setting "infinite ammo"
        // thanks 1Mack
        CCSWeaponBaseVData? weapon_data = weapon.As<CCSWeaponBase>().VData;

        if(weapon_data != null)
        {
            if(clip > weapon_data.MaxClip1)
            {
                weapon_data.MaxClip1 = clip;
                weapon_data.DefaultClip1 = clip;
            }

            if(reserve > weapon_data.PrimaryReserveAmmoMax)
            {
                weapon_data.PrimaryReserveAmmoMax = reserve;
            }
        }

        if(clip != -1)
        {
            weapon.Clip1 = clip;
            Utilities.SetStateChanged(weapon,"CBasePlayerWeapon","m_iClip1");
        }

        if(reserve != -1)
        {
            weapon.ReserveAmmo[0] = reserve;
            Utilities.SetStateChanged(weapon,"CBasePlayerWeapon","m_pReserveAmmo");
        }
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

    static void GiveMenuWeaponCallback(this CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.is_valid())
        {
            return;
        }

        // strip guns so the new ones don't just drop to the ground
        player.StripWeapons();

        // give their desired guns with lots of reserve ammo
        player.GiveNamedItem(gun_give_name(option.Text));
        player.GiveWeapon("deagle");

        CBasePlayerWeapon? primary = player.find_weapon(GUN_LIST[option.Text]);
        primary.set_ammo(-1,999);

        CBasePlayerWeapon? secondary = player.find_weapon("deagle");
        secondary.set_ammo(-1,999);
        
        player.GiveArmour();
    }

    static Dictionary<String,String> GUN_LIST = new Dictionary<String,String>()
    {
        {"AK47","ak47"},
        {"M4","m4a1_silencer"},
        {"M3","nova"},
        {"P90","p90"},
        {"M249","m249"},
        {"MP5","mp5sd"},
        {"FAL","galilar"},
        {"SG556","sg556"},
        {"BIZON","bizon"},
        {"AUG","aug"},
        {"FAMAS","famas"},
        {"XM1014","xm1014"},
        {"SCOUT","ssg08"},
        {"AWP", "awp"},
    };
    
    public static String gun_give_name(String name)
    {
        return "weapon_" + GUN_LIST[name];
    }

    static public void GiveWeapon(this CCSPlayerController? player,String name)
    {
        if(player.is_valid_alive())
        {
            player.GiveNamedItem("weapon_" + name);
        }
    }


    static public void GiveMenuWeapon(this CCSPlayerController? player,String name)
    {
        player.GiveWeapon(GUN_LIST[name]);
    }

    static public void GunMenuInternal(this CCSPlayerController? player, bool no_awp, Action<CCSPlayerController, ChatMenuOption> callback)
    {
        // player must be alive and active!
        if(!player.is_valid_alive())
        {
            return;
        } 

    
        var gun_menu = new ChatMenu("Gun Menu");

        foreach(var weapon_pair in GUN_LIST)
        {
            var weapon_name = weapon_pair.Key;

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
        // give bots some test guns
        if(player.is_valid_alive() && player.IsBot)
        {
            player.GiveWeapon("ak47");
            player.GiveWeapon("deagle");
        }

        GunMenuInternal(player,no_awp,GiveMenuWeaponCallback);
    }    
}