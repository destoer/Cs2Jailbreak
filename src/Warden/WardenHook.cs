
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
    void setup_cvar()
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

    public void round_start()
    {
        setup_cvar();

        PurgeRound();

        // handle submodules
        mute.round_start();
        block.round_start();
        warday.round_start();

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

    public void take_damage(CCSPlayerController? victim,CCSPlayerController? attacker, ref float damage)
    {
        // TODO: cant figure out how to get current player weapon
    /*
        if(!victim.is_valid_alive() && !attacker.is_valid_alive())
        {
            String weapon = 

            // if ct handicap is active rescale knife and awp damage to be unaffected
            if(ctHandicap && victim.IsCt() && attacker.IsT() && !in_lr(attacker) && (weapon.Contains("knife") || weapon.Contains("awp")))
            {
                damage = damage * 1.3;
            }
        }
    */
    }

    public void round_end()
    {
        mute.round_end();
        warday.round_end();
        PurgeRound();
    }


    public void connect(CCSPlayerController? player)
    {
        if(player != null)
        {
            jailPlayers[player.Slot].reset();
        }

        mute.connect(player);
    }

    public void disconnect(CCSPlayerController? player)
    {
        RemoveIfWarden(player);
    }


    public void map_start()
    {
        setup_cvar();
        warday.map_start();
    }

    public void voice(CCSPlayerController? player)
    {
        if(!player.is_valid_alive())
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

    public void spawn(CCSPlayerController? player)
    {
        if(!player.is_valid_alive())
        {
            return;
        }

        if(player.IsCt() && ctHandicap)
        {
            player.set_health(130);
        }

        SetupPlayerGuns(player);

        mute.spawn(player);
    }   

    public void switch_team(CCSPlayerController? player,int new_team)
    {
        RemoveIfWarden(player);
        mute.switch_team(player,new_team);
    }

    public void death(CCSPlayerController? player, CCSPlayerController? killer)
    {
        // player is no longer on server
        if(!player.is_valid())
        {
            return;
        }

        if(Config.wardenForceRemoval)
        {
            // handle warden death
            RemoveIfWarden(player);
        }

        // mute player
        mute.death(player);

        var jailPlayer = JailPlayerFromPlayer(player);

        if(jailPlayer != null)
        {
            jailPlayer.rebel_death(player,killer);
        }

        // if a t dies we dont need to regive the warden
        if(player.IsCt())
        {
            SetWardenIfLast(true);
        }
    }

    public void player_hurt(CCSPlayerController? player, CCSPlayerController? attacker, int damage,int health)
    {
        var jailPlayer = JailPlayerFromPlayer(player);

        if(jailPlayer != null)
        {  
            jailPlayer.player_hurt(player,attacker,damage, health);
        }  
    }

    public void weapon_fire(CCSPlayerController? player, String name)
    {
        // attempt to set rebel
        var jailPlayer = JailPlayerFromPlayer(player);

        if(jailPlayer != null)
        {
            jailPlayer.rebel_weapon_fire(player,name);
        }
    }

}