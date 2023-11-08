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


// base LR impl
public abstract class LRBase
{
    enum LrState
    {
        PENDING,
        ACTIVE,
    }

    public LRBase(LastRequestManager lr_manager,int lr_slot,int t, int ct)
    {
        t_slot = t;
        ct_slot = ct;
        state = LrState.PENDING;
        slot = lr_slot;

        // while lr is pending damage is off
        restrict_damage = true;
        manager = lr_manager;
    }


    public virtual void start_lr()
    {
        var t_player = Utilities.GetPlayerFromSlot(t_slot);

        if(t_player == null || !t_player.is_valid_alive())
        {
            manager.end_lr(slot);
            return;
        }

        init_player(t_player);

        var ct_player = Utilities.GetPlayerFromSlot(ct_slot);

        if(ct_player == null || !ct_player.is_valid_alive())
        {
            manager.end_lr(slot);
            return;
        }

        init_player(ct_player);
    }

    public void activate_lr()
    {
        // renable damage
        // NOTE: start_lr can override this if it so pleases
        restrict_damage = false;

        start_lr();

        state = LrState.ACTIVE;
    }
    
    // player setup -> NOTE: hp and gun stripping is done for us
    abstract public void init_player(CCSPlayerController player);

    // what events might we want access to?
    public virtual void weapon_fire(CCSPlayerController player, String name) {}

    public virtual void weapon_drop(CCSPlayerController player, String name) {}

    public virtual void ent_created() {}

    // lr partners
    readonly int t_slot;
    readonly int ct_slot;
    readonly int slot;

    LastRequestManager manager;

    // what weapon are we allowed to use?
    public String weapon_restrict = "";

    public bool restrict_damage = true;

    LrState state;
};