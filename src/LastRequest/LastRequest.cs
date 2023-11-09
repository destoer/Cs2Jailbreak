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

// base LR impl
public abstract class LRBase
{
    enum LrState
    {
        PENDING,
        ACTIVE,
    }

    protected LRBase(LastRequestManager lr_manager,int lr_slot,int actor_slot, int lr_choice)
    {
        state = LrState.PENDING;
        slot = lr_slot;
        player_slot = actor_slot;
        choice = lr_choice;

        // while lr is pending damage is off
        restrict_damage = true;
        manager = lr_manager;
    }


    public virtual void start()
    {
        var player = Utilities.GetPlayerFromSlot(player_slot);

        // player is not alive cancel the lr
        if(player == null || !player.is_valid_alive())
        {
            manager.end_lr(slot);
            return;
        }

        init_player(player);
    }

    public void cleanup()
    {
        // clean up timer
        Lib.kill_timer(ref timer);

        // reset alive player
    }


    public void activate()
    {
        // check this was built correctly
        // TODO: is there a static way to ensure this is made properly or no?
        if(partner == null)
        {
            manager.end_lr(slot);
            return;         
        }

        // renable damage
        // NOTE: start_lr can override this if it so pleases
        restrict_damage = false;

        start();

        state = LrState.ACTIVE;

        // make partner lr active if pending
        if(partner.state == LrState.PENDING)
        {
            partner.activate();
        }
    }
    
    // player setup -> NOTE: hp and gun stripping is done for us
    abstract public void init_player(CCSPlayerController player);

    // what events might we want access to?
    public virtual void weapon_fire(String name) {}

    public virtual bool weapon_drop(String name) 
    {
        return !restrict_drop;
    }

    public virtual bool weapon_pickup(String name) 
    {
        return weapon_restrict == name;
    }

    public virtual void ent_created(String name) {}

    // player and lr info
    readonly int player_slot;
    readonly int slot;

    LastRequestManager manager;

    // what weapon are we allowed to use?
    public String weapon_restrict = "";

    public bool restrict_damage = true;

    public bool restrict_drop = false;

    LrState state;

    // who are we playing against, set up in create_pair
    public LRBase? partner;

    // custom choice
    int choice = -1;

    // managed timer
    CSTimer.Timer? timer = null;
};