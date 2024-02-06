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
    public LRShotForShot(LastRequest manager,LastRequest.LRType type,int LRSlot, int playerSlot, String choice, bool mag = false) : base(manager,type,LRSlot,playerSlot,choice)
    {
        magForMag = mag;
    }

    public override void InitPlayer(CCSPlayerController player)
    {   
        // NOTE: clip size assumes mag for mag
        switch(choice)
        {
            case "Deagle":
            {
                weaponRestrict = "deagle";
                clipSize = 7;
                break;
            }

            // this crashes because?
        /*
            case "Usp":
            {
                weaponRestrict = "usp_silencer";
                clipSize = 12;
                break;
            }
        */
            case "Glock":
            {
                weaponRestrict = "glock";
                clipSize = 20;
                break;
            }

            case "Five seven":
            {
                weaponRestrict = "fiveseven";
                clipSize = 20;
                break;
            }

            case "Dual Elite":
            {
                weaponRestrict = "elite";
                clipSize = 30;
                break;
            }

        }
        
        // override to 1 if mag for mag
        if(magForMag)
        {
            clipSize = 1;
        }

        player.GiveWeapon("" + weaponRestrict);



        var deagle = player.FindWeapon("weapon_" + weaponRestrict);

        if(deagle != null)
        {
            deagle.SetAmmo(0,0);
        } 

    }

    void PickClip()
    {
        (CCSPlayerController? winner,CCSPlayerController? loser,LRBase? winnerLRBase) = pick_rand_player();

        LRShotForShot? winnerLR = (LRShotForShot?)winnerLRBase;


        // Give the lucky player the first shot
        if(winner != null && loser != null && winnerLR != null)
        {
            winner.Announce(LastRequest.LR_PREFIX,$"Randomly chose {winner.PlayerName} to shoot first");
            loser.Announce(LastRequest.LR_PREFIX,$"Randomly chose {winner.PlayerName} to shoot first");

            winnerLR.ReloadClip();
        }
    }

    public override void PairActivate()
    {
        PickClip();
    }

    public override void WeaponFire(String name)
    {
        if(name.Contains(weaponRestrict))
        {
            fire_clip();
        }
    }

    void ReloadClip()
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

        if(player.IsLegalAlive())
        {     
            player.PrintToChat($"{LastRequest.LR_PREFIX} Reload!");

            var deagle = player.FindWeapon("weapon_" + weaponRestrict);

            if(deagle != null)
            {
                deagle.SetAmmo(clipSize,0);
            } 

            curClip = clipSize;
        }          
    }

    void fire_clip()
    {
        if(curClip <= 0)
        {
            return;
        }

        curClip -= 1;

        //Server.PrintToChatAll($"Fired {curClip}");

        if(curClip <= 0 && partner != null)
        {
            var lrShot = (LRShotForShot)partner; 
            lrShot.ReloadClip();
        }
    }

    int clipSize = 1;
    int curClip = 0;

    bool magForMag = false;
}