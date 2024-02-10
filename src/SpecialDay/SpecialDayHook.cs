
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
        EndSD();
    }

    public void RoundStart()
    {
        // increment our round counter
        wsdRound += 1;
        EndSD();
    }

    public void WeaponEquip(CCSPlayerController? player,String name) 
    {
        if(!player.IsLegalAlive())
        {
            return;
        }

        if(activeSD != null)
        {
            // weapon equip not valid drop the weapons
            if(!activeSD.WeaponEquip(player,name))
            {
                activeSD.SetupPlayer(player);
            }
        }
    }

    public void Disconnect(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return;
        }

        if(activeSD != null)
        {
            activeSD.Disconnect(player);
        }
    }


    public void GrenadeThrown(CCSPlayerController? player)
    {
        if(activeSD != null)
        {
            activeSD.GrenadeThrown(player);
        }       
    }

    public void EntCreated(CEntityInstance entity)
    {
        if(activeSD != null)
        {
            activeSD.EntCreated(entity);
        }
    }
        

    public void Death(CCSPlayerController? player, CCSPlayerController? attacker, String weapon)
    {
        if(activeSD != null)
        {
            activeSD.Death(player,attacker, weapon);
        }
    }

    public void PlayerHurt(CCSPlayerController? player, CCSPlayerController? attacker, int damage,int health, int hitgroup)
    {
        if(activeSD != null && player.IsLegal())
        {
            activeSD.PlayerHurt(player,damage,health,hitgroup);
        }
    }

    public void TakeDamage(CCSPlayerController? player, CCSPlayerController? attacker, ref float damage)
    {
        if(activeSD == null || !player.IsLegal())
        {
            return;
        }

        if(activeSD.restrictDamage)
        {
            damage = 0.0f;
        }
    }
}