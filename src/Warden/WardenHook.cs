
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
using CounterStrikeSharp.API.Core.Translations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using System.Drawing;


public partial class Warden
{
    void SetupCvar()
    {
        Server.ExecuteCommand("mp_force_pick_time 3000");
        Server.ExecuteCommand("mp_autoteambalance 0");
        Server.ExecuteCommand("sv_human_autojoin_team 2");

        if(Config.stripSpawnWeapons)
        {
            Server.ExecuteCommand("mp_equipment_reset_rounds 1");
            Server.ExecuteCommand("mp_t_default_secondary \"\" ");
            Server.ExecuteCommand("mp_ct_default_secondary \"\" ");
        }
    }

    public void RoundStart()
    {
        SetupCvar();

        PurgeRound();

        // handle submodules
        mute.RoundStart();
        block.RoundStart();
        warday.RoundStart();

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            player.SetColour(Color.FromArgb(255, 255, 255, 255));
        }

        SetWardenIfLast();
    /*
        ctHandicap = ((Lib.CtCount() * 3) <= Lib.TCount()) && Config.ctHandicap;

        if(ctHandicap)
        {
            Chat.announce(WARDEN_PREFIX,"CT ratio is too low, handicap enabled for this round");
        }
    */
    }

    public void TakeDamage(CCSPlayerController? victim,CCSPlayerController? attacker, ref float damage)
    {
        // TODO: cant figure out how to get current player weapon
    /*
        if(!victim.IsLegalAlive() && !attacker.IsLegalAlive())
        {
            String weapon = 

            // if ct handicap is active rescale knife and awp damage to be unaffected
            if(ctHandicap && victim.IsCt() && attacker.IsT() && !InLR(attacker) && (weapon.Contains("knife") || weapon.Contains("awp")))
            {
                damage = damage * 1.3;
            }
        }
    */
    }

    public void RoundEnd()
    {
        mute.RoundEnd();
        warday.RoundEnd();
        PurgeRound();
    }


    public void Connect(CCSPlayerController? player)
    {
        if(player != null)
        {
            jailPlayers[player.Slot].Reset();
        }

        mute.Connect(player);
    }

    public void Disconnect(CCSPlayerController? player)
    {
        RemoveIfWarden(player);
    }


    public void MapStart()
    {
        SetupCvar();
        warday.MapStart();
    }

    public void Voice(CCSPlayerController? player)
    {
        if(!player.IsLegalAlive())
        {
            return;
        }

        if(!Config.wardenOnVoice)
        {
            return;
        }

        if(wardenSlot == INAVLID_SLOT && player.IsCt())
        {
            SetWarden(player.Slot);
        }
    }

    public void Spawn(CCSPlayerController? player)
    {
        if(!player.IsLegalAlive())
        {
            return;
        }

        if(player.IsCt() && ctHandicap)
        {
            player.SetHealth(130);
        }

        SetupPlayerGuns(player);

        mute.Spawn(player);
    }   

    public void SwitchTeam(CCSPlayerController? player,int new_team)
    {
        RemoveIfWarden(player);
        mute.SwitchTeam(player,new_team);
    }

    public void Death(CCSPlayerController? player, CCSPlayerController? killer)
    {
        // player is no longer on server
        if(!player.IsLegal())
        {
            return;
        }

        if(Config.wardenForceRemoval)
        {
            // handle warden death
            RemoveIfWarden(player);
        }

        // mute player
        mute.Death(player);

        var jailPlayer = JailPlayerFromPlayer(player);

        if(jailPlayer != null)
        {
            jailPlayer.RebelDeath(player,killer);
        }

        // if a t dies we dont need to regive the warden
        if(player.IsCt())
        {
            SetWardenIfLast(true);
        }
    }

    public void PlayerHurt(CCSPlayerController? player, CCSPlayerController? attacker, int damage,int health)
    {
        var jailPlayer = JailPlayerFromPlayer(player);

        if(jailPlayer != null)
        {  
            jailPlayer.PlayerHurt(player,attacker,damage, health);
        }  
    }

    public void WeaponFire(CCSPlayerController? player, String name)
    {
        // attempt to set rebel
        var jailPlayer = JailPlayerFromPlayer(player);

        if(jailPlayer != null)
        {
            jailPlayer.RebelWeaponFire(player,name);
        }
    }

}