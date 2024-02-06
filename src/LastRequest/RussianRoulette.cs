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

public class LRRussianRoulette : LRBase
{
    public LRRussianRoulette(LastRequest manager,LastRequest.LRType type,int LRSlot, int playerSlot, String choice) : base(manager,type,LRSlot,playerSlot,choice)
    {

    }

    public override void InitPlayer(CCSPlayerController player)
    {    
        weaponRestrict = "deagle";

        player.GiveWeapon("deagle");

        var deagle = player.FindWeapon("weapon_" + weaponRestrict);

        if(deagle != null)
        {
            deagle.SetAmmo(0,0);
        } 

        restrictDamage = true;
    }

    public override void PairActivate()
    {
        (CCSPlayerController? winner,CCSPlayerController? loser,LRBase? winnerLRBase) = pick_rand_player();

        LRRussianRoulette? winnerLR = (LRRussianRoulette?)winnerLRBase;


        // Give the lucky player the first shot
        if(winner != null && loser != null && winnerLR != null)
        {
            winner.Announce(LastRequest.LR_PREFIX,$"Randomly chose {winner.PlayerName} to shoot first");
            loser.Announce(LastRequest.LR_PREFIX,$"Randomly chose {winner.PlayerName} to shoot first");

            winnerLR.ReloadClip();
        }   
    }

    public override void WeaponFire(String name)
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

        if(name.Contains(weaponRestrict) && player.IsLegal())
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);

            // Bang!
            if(rnd.Next(0,7) == 6)
            {
                player.Slay();
                Chat.Announce(LastRequest.LR_PREFIX,$"{player.PlayerName} brains splattered against the wall");
            }

            else if(partner != null)
            {
                player.Announce(LastRequest.LR_PREFIX,"Click!");
                var lr_shot = (LRRussianRoulette)partner; 
                lr_shot.ReloadClip();
            }
        }
    }

    void ReloadClip()
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

        if(player.IsLegalAlive())
        {     
            player.PrintToChat($"{LastRequest.LR_PREFIX} Reload!");

            var deagle = player.FindWeapon("weapon_" + weaponRestrict);

            // NOTE: this doesn't update the unload state
            // however giving a new gun doesn't work either because it doesnt register fast enough
            // also taking a gun away too quickly after a shot will cause it not to register
            if(deagle != null)
            {
                deagle.SetAmmo(1,0);
            }
        }     
    }

    
}