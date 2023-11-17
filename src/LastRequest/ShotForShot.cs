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

// NOTE: this also implements Mag for Mag

public class LRShotForShot : LRBase
{
    public LRShotForShot(LastRequest manager,int lr_slot, int player_slot, String choice, bool mag = false) : base(manager,"Shot for shot",lr_slot,player_slot,choice)
    {
        mag_for_mag = mag;
    }

    public override void init_player(CCSPlayerController player)
    {    
        weapon_restrict = "deagle";

        player.GiveNamedItem("weapon_deagle");

        var deagle = Lib.find_weapon(player,"weapon_deagle");

        if(deagle != null)
        {
            deagle.set_ammo(0,0);
        }

        if(mag_for_mag)
        {
            clip_size = 7;
        }

        else
        {
            clip_size = 1;
        }
    }

    public override void pair_activate()
    {
        pick_clip(clip_size);
    }

    public override void weapon_fire(String name)
    {
        if(name.Contains(weapon_restrict))
        {
            fire_clip();
        }
    }

    void reload_clip()
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);

        if(player != null && player.is_valid_alive())
        {     
            player.PrintToChat($"{LastRequest.LR_PREFIX} Reload!");

            var deagle = Lib.find_weapon(player,weapon_restrict);

            if(deagle != null)
            {
                deagle.set_ammo(clip_size,0);
            } 

            cur_clip = clip_size;
        }          
    }

    void fire_clip()
    {
        if(cur_clip <= 0)
        {
            return;
        }

        cur_clip -= 1;

        if(cur_clip <= 0 && partner != null)
        {
            var lr_shot = (LRShotForShot)partner; 
            lr_shot.reload_clip();
        }
    }

    int clip_size = 1;
    int cur_clip = 0;

    bool mag_for_mag = false;
}