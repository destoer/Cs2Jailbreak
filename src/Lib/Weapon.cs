using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Menu;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;

public static class Weapon
{
    static public bool IsLegal(this CBasePlayerWeapon? weapon)
    {
        return weapon != null && weapon.IsValid;
    }

    static public CBasePlayerWeapon? FindWeapon(this CCSPlayerController? player, String name)
    {
        // only care if player is alive
        if(!player.IsLegalAlive())
        {
            return null;
        }

        CCSPlayerPawn? pawn = player.Pawn();

        if(pawn == null)
        {
            return null;
        }

        var weapons = pawn.WeaponServices?.MyWeapons;

        if(weapons == null)
        {
            return null;
        }

        foreach (var weaponOpt in weapons)
        {
            CBasePlayerWeapon? weapon = weaponOpt.Value;

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



    static public void SetAmmo(this CBasePlayerWeapon? weapon, int clip, int reserve)
    {
        if(weapon == null || !weapon.IsLegal())
        {
            return;
        }

        // overide reserve max so it doesn't get clipped when
        // setting "infinite ammo"
        // thanks 1Mack
        CCSWeaponBaseVData? weaponData = weapon.As<CCSWeaponBase>().VData;

        if(weaponData != null)
        {
            if(clip > weaponData.MaxClip1)
            {
                weaponData.MaxClip1 = clip;
                weaponData.DefaultClip1 = clip;
            }

            if(reserve > weaponData.PrimaryReserveAmmoMax)
            {
                weaponData.PrimaryReserveAmmoMax = reserve;
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

    public static void EventGunMenuCallback(CCSPlayerController player, ChatMenuOption option)
    {
        // Event has been cancelled in the mean time dont give any guns
        if(!JailPlugin.EventActive())
        {
            return;
        }

        GunMenuGive(player,option);
    }

    static public void EventGunMenu(this CCSPlayerController? player)
    {
        // Event has been cancelled in the mean time dont give any guns
        if(!JailPlugin.EventActive())
        {
            return;
        }

        player.GunMenuInternal(false,EventGunMenuCallback);
    }

    public static void GunMenuGive(this CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.IsLegalAlive())
        {
            return;
        }

        player.StripWeapons();

        player.GiveMenuWeapon(option.Text);
        player.GiveWeapon("deagle");

        player.GiveArmour();

        CBasePlayerWeapon? primary = player.FindWeapon(GUN_LIST[option.Text]);
        primary.SetAmmo(-1,999);

        CBasePlayerWeapon? secondary = player.FindWeapon("deagle");
        secondary.SetAmmo(-1,999);
    }

    static void GiveMenuWeaponCallback(this CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.IsLegal())
        {
            return;
        }

        GunMenuGive(player,option);
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
    
    public static String GunGiveName(String name)
    {
        return "weapon_" + GUN_LIST[name];
    }

    static public void GiveWeapon(this CCSPlayerController? player,String name)
    {
        if(player.IsLegalAlive())
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
        if(!player.IsLegalAlive())
        {
            return;
        } 

    
        var gunMenu = new ChatMenu("Gun Menu");

        foreach(var weapon_pair in GUN_LIST)
        {
            var weapon_name = weapon_pair.Key;

            if(no_awp && weapon_name == "awp")
            {
                continue;
            }

            gunMenu.AddMenuOption(weapon_name, callback);
        }

        ChatMenus.OpenMenu(player, gunMenu);
    }

    static public void GunMenu(this CCSPlayerController? player, bool no_awp)
    {
        // give bots some test guns
        if(player.IsLegalAlive() && player.IsBot)
        {
            player.GiveWeapon("ak47");
            player.GiveWeapon("deagle");
        }

        GunMenuInternal(player,no_awp,GiveMenuWeaponCallback);
    }    
}