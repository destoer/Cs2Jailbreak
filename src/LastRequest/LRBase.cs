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

    protected LRBase(LastRequest lr_manager,String name,int lr_slot,int actor_slot, String lr_choice)
    {
        state = LrState.PENDING;
        slot = lr_slot;
        player_slot = actor_slot;
        choice = lr_choice;
        lr_name = name;

        // while lr is pending damage is off
        restrict_damage = true;
        manager = lr_manager;

        // make sure we cant get guns during startup
        weapon_restrict = "knife";
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
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);

        if(player == null || !player.is_valid_alive())
        {
            return;
        }


        // restore hp
        player.set_health(100);

        // restore weapons
        player.strip_weapons();

        // reset gravity
        player.set_gravity(1.0f);

        player.set_velocity(1.0f);

        if(player.is_ct())
        {
            player.GiveNamedItem("item_assaultsuit");
            player.GiveNamedItem("weapon_deagle");
            player.GiveNamedItem("weapon_m4a1");           
        }
    }

    public void lose()
    {
        if(partner == null)
        {
            return;
        }

        CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);
        CCSPlayerController? winner = Utilities.GetPlayerFromSlot(partner.player_slot);

        if(player == null || !player.is_valid_alive() || winner == null || !winner.is_valid_alive())
        {
            manager.end_lr(slot);
            return;
        }

        Lib.announce(LastRequest.LR_PREFIX,$"{player.PlayerName} lost {lr_name}, {winner.PlayerName} won!");
        manager.end_lr(slot);
    }

    // NOTE: this is called once for a pair on the starting slot
    public virtual void pair_activate()
    {

    }

    public void activate()
    {
        // this is a timer callback set it to null
        timer = null;

        // check this was built correctly
        // TODO: is there a static way to ensure this is made properly or no?
        if(partner == null)
        {
            manager.end_lr(slot);
            return;         
        }

        CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);

        if(player != null && player.is_valid_alive())
        {
            player.announce(LastRequest.LR_PREFIX,"Fight!");
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
    
    protected void pick_clip(int clip_size)
    {
        Random rnd = new Random((int)DateTime.Now.Ticks);

        CCSPlayerController? winner = null;
        CCSPlayerController? loser = null;

        if(rnd.Next(0,2) == 0)
        {
            if(partner != null)
            {
                winner = Utilities.GetPlayerFromSlot(player_slot);
                loser =  Utilities.GetPlayerFromSlot(partner.player_slot);
            }
        }

        else
        {
            if(partner != null)
            {
                winner =  Utilities.GetPlayerFromSlot(partner.player_slot);
                loser =  Utilities.GetPlayerFromSlot(player_slot);
            }
        }


        // Give the lucky player the first shot
        if(winner != null && loser != null)
        {
            winner.announce(LastRequest.LR_PREFIX,$"Randomly chose {winner.PlayerName} to shoot first");
            loser.announce(LastRequest.LR_PREFIX,$"Randomly chose {winner.PlayerName} to shoot first");

            var deagle = Lib.find_weapon(winner,weapon_restrict);

            if(deagle != null)
            {
                deagle.set_ammo(clip_size,0);
            }
        }
    }

    // player setup -> NOTE: hp and gun stripping is done for us
    abstract public void init_player(CCSPlayerController player);

    // what events might we want access to?
    public virtual void weapon_fire(String name) {}

    public virtual void ent_created(CEntityInstance entity) {}

    public virtual bool take_damage(int health,int damage, int hitgroup) 
    {
        return !restrict_damage;
    }

    public virtual bool weapon_drop(String name) 
    {
        return !restrict_drop;
    }

    public virtual bool weapon_equip(String name) 
    {
        //Server.PrintToChatAll($"{name} : {weapon_restrict}");
        return weapon_restrict == "" || name.Contains(weapon_restrict); 
    }

    public virtual void ent_created(String name) {}

    public String lr_name = "";

    // player and lr info
    public readonly int player_slot;
    public readonly int slot;

    LastRequest manager;

    // what weapon are we allowed to use?
    public String weapon_restrict = "";

    public bool restrict_damage = true;

    public bool restrict_drop = true;

    LrState state;

    // who are we playing against, set up in create_pair
    public LRBase? partner;

    // custom choice
    protected String choice = "";

    // managed timer
    CSTimer.Timer? timer = null;
};