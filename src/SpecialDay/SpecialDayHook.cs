
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


public partial class SpecialDay
{

    public void round_end()
    {
        end_sd();
    }

    public void round_start()
    {
        // increment our round counter
        wsd_round += 1;
        end_sd();
    }

    public void weapon_equip(CCSPlayerController? player,String name) 
    {
        if(!player.is_valid_alive())
        {
            return;
        }

        if(active_sd != null)
        {
            // weapon equip not valid drop the weapons
            if(!active_sd.weapon_equip(player,name))
            {
                active_sd.setup_player(player);
            }
        }
    }

    public void disconnect(CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return;
        }

        if(active_sd != null)
        {
            active_sd.disconnect(player);
        }
    }


    public void grenade_thrown(CCSPlayerController? player)
    {
        if(active_sd != null)
        {
            active_sd.grenade_thrown(player);
        }       
    }

    public void ent_created(CEntityInstance entity)
    {
        if(active_sd != null)
        {
            active_sd.ent_created(entity);
        }
    }
        

    public void death(CCSPlayerController? player, CCSPlayerController? attacker)
    {
        if(active_sd != null)
        {
            active_sd.death(player,attacker);
        }
    }

    public void player_hurt(CCSPlayerController? player, CCSPlayerController? attacker, int damage,int health, int hitgroup)
    {
        if(active_sd != null && player.is_valid())
        {
            active_sd.player_hurt(player,damage,health,hitgroup);
        }
    }

    public void take_damage(CCSPlayerController? player, CCSPlayerController? attacker, ref float damage)
    {
        if(active_sd == null || !player.is_valid())
        {
            return;
        }

        if(active_sd.restrict_damage)
        {
            damage = 0.0f;
        }
    }
}