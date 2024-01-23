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
    public LRShotForShot(LastRequest manager,LastRequest.LRType type,int lr_slot, int player_slot, String choice, bool mag = false) : base(manager,type,lr_slot,player_slot,choice)
    {
        mag_for_mag = mag;
    }

    public override void init_player(CCSPlayerController player)
    {   
        // NOTE: clip size assumes mag for mag
        switch(choice)
        {
            case "Deagle":
            {
                weapon_restrict = "deagle";
                clip_size = 7;
                break;
            }

            // this crashes because?
        /*
            case "Usp":
            {
                weapon_restrict = "usp_silencer";
                clip_size = 12;
                break;
            }
        */
            case "Glock":
            {
                weapon_restrict = "glock";
                clip_size = 20;
                break;
            }

            case "Five seven":
            {
                weapon_restrict = "fiveseven";
                clip_size = 20;
                break;
            }

            case "Dual Elite":
            {
                weapon_restrict = "elite";
                clip_size = 30;
                break;
            }

        }
        
        // override to 1 if mag for mag
        if(!mag_for_mag)
        {
            clip_size = 1;
        }

        player.GiveNamedItem("weapon_" + weapon_restrict);



        var deagle = Lib.find_weapon(player,"weapon_" + weapon_restrict);

        if(deagle != null)
        {
            deagle.set_ammo(0,0);
        } 

    }

    void pick_clip()
    {
        (CCSPlayerController? winner,CCSPlayerController? loser,LRBase? winner_lr_base) = pick_rand_player();

        LRShotForShot? winner_lr = (LRShotForShot?)winner_lr_base;


        // Give the lucky player the first shot
        if(winner != null && loser != null && winner_lr != null)
        {
            winner.announce(LastRequest.LR_PREFIX,$"Randomly chose {winner.PlayerName} to shoot first");
            loser.announce(LastRequest.LR_PREFIX,$"Randomly chose {winner.PlayerName} to shoot first");

            winner_lr.reload_clip();
        }
    }

    public override void pair_activate()
    {
        pick_clip();
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

        if(player.is_valid_alive())
        {     
            player.PrintToChat($"{LastRequest.LR_PREFIX} Reload!");

            var deagle = Lib.find_weapon(player,"weapon_" + weapon_restrict);

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

        //Server.PrintToChatAll($"Fired {cur_clip}");

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