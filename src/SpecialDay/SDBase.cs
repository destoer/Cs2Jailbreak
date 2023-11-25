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


        state = SDState.INACTIVE;
        setup();

        setup_players();
    }

    public void start_common()
    {
        restrict_damage = false;

        state = SDState.ACTIVE;
        start();
    }

    public void end_common()
    {
        state = SDState.INACTIVE;
        end();

        Lib.disable_friendly_fire();

        cleanup_players();
    }

    public virtual bool weapon_equip(String name) 
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

    public void announce(String str)
    {
        Lib.announce(SpecialDay.SPECIALDAY_PREFIX,str);
    }

    public enum SDState
    {
        INACTIVE,
        STARTED,
        ACTIVE
    };

    public bool restrict_damage = false;
    public String weapon_restrict = "";
    public SDState state = SDState.INACTIVE;
}