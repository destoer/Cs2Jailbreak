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

using CSTimer = CounterStrikeSharp.API.Modules.Timers;

public partial class LastRequest
{
    public void PlayerHurt(CCSPlayerController? player, CCSPlayerController? attacker, int damage,int health, int hitgroup)
    {
        // check no damage restrict
        LRBase? lr = find_lr(player);

        // no lr
        if(lr == null)
        {
            return;
        }
        
        // not a pair
        if(!is_pair(player,attacker))
        {
            return;
        }

        lr.PlayerHurt(damage,health,hitgroup);
    }

    public void TakeDamage(CCSPlayerController? player, CCSPlayerController? attacker, ref float damage)
    {
        // neither player is in lr we dont care
        if(!InLR(player) && !InLR(attacker))
        {
            return;
        }

        // not a pair restore hp
        if(!is_pair(player,attacker))
        {
            damage = 0.0f;
            return;
        }

        // check no damage restrict
        LRBase? lr = find_lr(player);

        if(lr == null)
        {
            return;
        }

        if(!lr.TakeDamage())
        {
            damage = 0.0f;
        }   
    }

    public void WeaponEquip(CCSPlayerController? player,String name) 
    {
        if(!player.IsLegalAlive())
        {
            return;
        }

        if(rebel_type == RebelType.KNIFE && !name.Contains("knife"))
        {
            player.StripWeapons();
            return;
        }

        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            CCSPlayerPawn? pawn = player.Pawn();

            if(pawn == null)
            {
                return;
            }

            // strip all weapons that aint the restricted one
            var weapons = pawn.WeaponServices?.MyWeapons;

            if(weapons == null)
            {
                return;
            }

            foreach (var weapon_opt in weapons)
            {
                CBasePlayerWeapon? weapon = weapon_opt.Value;

                if (weapon == null)
                { 
                    continue;
                }
                
                var weapon_name = weapon.DesignerName;

                // TODO: Ideally we should just deny the equip all together but this works well enough
                if(!lr.WeaponEquip(weapon_name))
                {
                    //Server.PrintToChatAll($"drop player gun: {player.PlayerName} : {weapon_name}");
                    player.DropActiveWeapon();
                }
            }    
        }
    }

    // couldnt get pulling the owner from the projectile ent working
    // so instead we opt for this
    public void WeaponZoom(CCSPlayerController? player)
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            lr.WeaponZoom();
        }       
    }

    // couldnt get pulling the owner from the projectile ent working
    // so instead we opt for this
    public void GrenadeThrown(CCSPlayerController? player)
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            lr.GrenadeThrown();
        }       
    }

    public void EntCreated(CEntityInstance entity)
    {
        for(int l = 0; l < active_lr.Length; l++)
        {
            LRBase? lr = active_lr[l];

            if(lr != null && entity.IsValid)
            {
                lr.EntCreated(entity);
            }
        }
    }

    public void RoundStart()
    {
        start_timestamp = Lib.CurTimestamp();

        purge_lr();
    }

    public void RoundEnd()
    {
        purge_lr();
    }

    public void Disconnect(CCSPlayerController? player)
    {
        JailPlugin.PurgePlayerStats(player);

        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            Chat.Announce(LR_PREFIX,"Player Disconnection cancelling LR");
            end_lr(lr.slot);
        }
    }

    public bool weapon_drop(CCSPlayerController? player,String name) 
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            return lr.weapon_drop(name);
        }

        return true;
    }

    public void WeaponFire(CCSPlayerController? player,String name) 
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            lr.WeaponFire(name);
        }
    }

    public void Death(CCSPlayerController? player)
    {
        if(Lib.AliveTCount() == Config.lrCount && player.IsT())
        {
            Chat.LocalizeAnnounce(LR_PREFIX,"lr.ready");
        }


        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            lr.lose();
        }
    }

}