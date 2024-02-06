
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

    public void RoundEnd()
    {
        end_sd();
    }

    public void RoundStart()
    {
        // increment our round counter
        wsd_round += 1;
        end_sd();
    }

    public void WeaponEquip(CCSPlayerController? player,String name) 
    {
        if(!player.is_valid_alive())
        {
            return;
        }

        if(active_sd != null)
        {
            // weapon equip not valid drop the weapons
            if(!active_sd.WeaponEquip(player,name))
            {
                active_sd.setup_player(player);
            }
        }
    }

    public void Disconnect(CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return;
        }

        if(active_sd != null)
        {
            active_sd.Disconnect(player);
        }
    }


    public void GrenadeThrown(CCSPlayerController? player)
    {
        if(active_sd != null)
        {
            active_sd.GrenadeThrown(player);
        }       
    }

    public void ent_created(CEntityInstance entity)
    {
        if(active_sd != null)
        {
            active_sd.ent_created(entity);
        }
    }
        

    public void Death(CCSPlayerController? player, CCSPlayerController? attacker)
    {
        if(active_sd != null)
        {
            active_sd.Death(player,attacker);
        }
    }

    public void PlayerHurt(CCSPlayerController? player, CCSPlayerController? attacker, int damage,int health, int hitgroup)
    {
        if(active_sd != null && player.is_valid())
        {
            active_sd.PlayerHurt(player,damage,health,hitgroup);
        }
    }

    public void TakeDamage(CCSPlayerController? player, CCSPlayerController? attacker, ref float damage)
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