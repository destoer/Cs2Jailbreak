// base lr class
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
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using System.Drawing;

public abstract class SDBase
{
    public abstract void setup();

    public abstract void start();

    public abstract void end();

    public void setup_common()
    {
        // no damage before start
        restrict_damage = true;

        // revive all dead players


        state = SDState.STARTED;
        setup();

        setup_players();
    }

    public void start_common()
    {
        restrict_damage = false;

        state = SDState.ACTIVE;
        Entity.ForceOpen();
        start();
    }

    // NOTE: this will be recalled by the Disconnect function if the boss dc's
    public virtual void make_boss(CCSPlayerController? tank, int count)
    {

    }

    public (CCSPlayerController, int) pick_boss()
    {
        // get valid players
        List<CCSPlayerController> players = Utilities.GetPlayers();
        var valid = players.FindAll(player => player.is_valid_alive());

        CCSPlayerController? rigged = Utilities.GetPlayerFromSlot(rigged_slot);

        // override pick
        if(rigged.is_valid_alive())
        {
            var player = rigged;
            rigged_slot = -1;
            return (player,valid.Count);
        }

        // pick one back at random
        Random rnd = new Random((int)DateTime.Now.Ticks);

        int boss = rnd.Next(0,valid.Count);

        boss_slot = valid[boss].Slot;

        return (valid[boss],valid.Count);
    }

    public void Disconnect(CCSPlayerController? player)
    {
        if(!player.is_valid())
        {
            return;
        }

        // player has dced re roll the boss if we have one
        if(player.Slot == boss_slot)
        {
            (CCSPlayerController boss, int count) = pick_boss();

            make_boss(boss,count);
        }
    }

    public void end_common()
    {
        state = SDState.INACTIVE;
        end();

        Lib.disable_friendly_fire();

        CCSPlayerController? boss = Utilities.GetPlayerFromSlot(boss_slot);

        // reset the boss colour
        if(boss.is_valid_alive())
        {
            boss.set_velocity(1.0f);
            boss.SetColour(Color.FromArgb(255, 255, 255, 255));
        }

        cleanup_players();
    }

    public bool is_boss(CCSPlayerController? player)
    {
        if(player == null)
        {
            return false;
        }

        return player.Slot == boss_slot;
    }

    public virtual bool WeaponEquip(CCSPlayerController player, String name) 
    {
        return weapon_restrict == "" || name.Contains(weapon_restrict); 
    }

    public virtual void PlayerHurt(CCSPlayerController? player,int health,int damage, int hitgroup) {}

    public virtual void EntCreated(CEntityInstance entity) {}
    public virtual void GrenadeThrown(CCSPlayerController? player) {}

    

    public virtual void Death(CCSPlayerController? player, CCSPlayerController? attacker) {}

    public abstract void setup_player(CCSPlayerController player);

    public virtual void cleanup_player(CCSPlayerController player) {}

    public void setup_players()
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.is_valid_alive())
            {
                // reset the player colour incase of rebel
                player.SetColour(Player.DEFAULT_COLOUR);

                setup_player(player);
            }
        }       
    }

    public void cleanup_players()
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.is_valid_alive())
            {
                cleanup_player(player);
            }
        }       
    }

    public void localize_announce(String name, params Object[] args)
    {
        Chat.localize_announce(SpecialDay.SPECIALDAY_PREFIX,name,args);
    }


    public int boss_slot = -1;
    public int rigged_slot = -1;

    public bool restrict_damage = false;
    public String weapon_restrict = "";
    public SDState state = SDState.INACTIVE;

    public int delay = 15;
}