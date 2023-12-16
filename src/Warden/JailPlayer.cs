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

public class JailPlayer
{
    public void purge_round()
    {
        is_rebel = false;
        init_weapon = false;
    }

    public void reset()
    {
        purge_round();

        // TODO: reset client specific settings
        joined_team = false;
    }

    public void set_rebel(CCSPlayerController? player)
    {
        if(JailPlugin.event_active())
        {
            return;
        }

        // ignore if they are in lr
        if(JailPlugin.global_ctx != null)
        {
            if(JailPlugin.lr.in_lr(player))
            {
                return;
            }
        }

        // dont care if player is invalid
        if(!player.is_valid() || player == null)
        {
            return;
        }

        // on T with no warday or sd active
        if(player.TeamNum == Lib.TEAM_T)
        {
            is_rebel = true;
        }
    }

    public void rebel_death(CCSPlayerController? player,CCSPlayerController? killer)
    {
        // event active dont care
        if(JailPlugin.event_active())
        {
            return;
        }

        // players aernt valid dont care
        if(killer == null || player == null || !player.is_valid() || !killer.is_valid())
        {
            return;
        }

        // print death if player is rebel and killer on CT
        if(is_rebel && killer.TeamNum == Lib.TEAM_CT)
        {
            Server.PrintToChatAll($" {ChatColors.Green}[REBEL]: {ChatColors.White}{killer.PlayerName} killed the rebel {player.PlayerName}");
        }
    }

    public void rebel_weapon_fire(CCSPlayerController? player, String weapon)
    {
        // ignore weapons players are meant to have
        if(weapon != "knife" && weapon != "c4")
        {
            set_rebel(player);
        }
    }

    public void player_hurt(CCSPlayerController? player, CCSPlayerController? attacker, int health, int damage)
    {
        if(player == null || attacker == null || !player.is_valid() || !attacker.is_valid())
        {
            return;
        }

        // ct hit by T they are a rebel
        if(player.is_ct() && attacker.is_t())
        {
            set_rebel(attacker);
        }

        // log any ct damage
        else if(attacker.is_ct())
        {
            //Lib.print_console_all($"CT {attacker.PlayerName} hit {player.PlayerName} for {damage}");
        }
    }

    // TODO: Laser stuff needs to go here!
    // but we dont have access to the necessary primtives yet

    public bool is_rebel = false;
    public bool init_weapon = false;
    public bool joined_team = false;
};
