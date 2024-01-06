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
    }

    public void reset()
    {
        purge_round();

        // TODO: reset client specific settings
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
            if(config.colour_rebel)
            {
                player.set_colour(Lib.RED);
            }
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
            Lib.localise_announce($" {ChatColors.Green}[REBEL]: {ChatColors.White}","rebel.kill",killer.PlayerName,player.PlayerName);
        }
    }

    public void rebel_weapon_fire(CCSPlayerController? player, String weapon)
    {
        if(config.rebel_requirehit)
        {
            return;
        }
        
        // ignore weapons players are meant to have
        if(!weapon.Contains("knife") && !weapon.Contains("c4"))
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
    public static JailConfig config = new JailConfig();

    public bool is_rebel = false;
};
