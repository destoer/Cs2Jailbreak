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
        Lib.force_open();
        start();
    }

    // NOTE: this will be recalled by the disconnect function if the boss dc's
    public virtual void make_boss(CCSPlayerController? tank, int count)
    {

    }

    public (CCSPlayerController, int) pick_boss()
    {
        // get valid players
        List<CCSPlayerController> players = Utilities.GetPlayers();
        var valid = players.FindAll(player => player.is_valid_alive());

        // override pick
        if(rigged != null)
        {
            var player = rigged;
            rigged = null;
            return (player,valid.Count);
        }

        // pick one back at random
        Random rnd = new Random((int)DateTime.Now.Ticks);

        int boss = rnd.Next(0,valid.Count);

        return (valid[boss],valid.Count);
    }

    
    public void disconnect(CCSPlayerController? player)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        // player has dced re roll the boss if we have one
        if(player.slot() == boss.slot())
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

        // reset the boss colour
        if(boss != null && boss.is_valid_alive())
        {
            boss.set_velocity(1.0f);
            boss.set_colour(Color.FromArgb(255, 255, 255, 255));
        }

        cleanup_players();
    }

    public bool is_boss(CCSPlayerController? player)
    {
        return boss.slot() != null && player.slot() == boss.slot();
    }

    public virtual bool weapon_equip(CCSPlayerController player, String name) 
    {
        return weapon_restrict == "" || name.Contains(weapon_restrict); 
    }

    public virtual void player_hurt(CCSPlayerController? player,int health,int damage, int hitgroup) {}

    public virtual void ent_created(CEntityInstance entity) {}
    public virtual void grenade_thrown(CCSPlayerController? player) {}

    

    public virtual void death(CCSPlayerController? player, CCSPlayerController? attacker) {}

    public abstract void setup_player(CCSPlayerController player);

    public virtual void cleanup_player(CCSPlayerController player) {}

    public void setup_players()
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.is_valid_alive())
            {
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

    public void localise_announce(String name, params Object[] args)
    {
        Lib.localise_announce(SpecialDay.SPECIALDAY_PREFIX,name,args);
    }

    public CCSPlayerController? boss = null;
    public CCSPlayerController? rigged = null;

    public bool restrict_damage = false;
    public String weapon_restrict = "";
    public SDState state = SDState.INACTIVE;

    public int delay = 15;
}