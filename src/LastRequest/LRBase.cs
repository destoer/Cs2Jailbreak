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

    

    protected LRBase(LastRequest lr_manager,LastRequest.LRType lr_type,int lr_slot,int actor_slot, String lr_choice)
    {
        state = LrState.PENDING;
        slot = lr_slot;
        player_slot = actor_slot;
        choice = lr_choice;
        lr_name = LastRequest.LR_NAME[(int)lr_type];
        type = lr_type;

        // while lr is pending damage is off
        restrictDamage = true;
        manager = lr_manager;

        // make sure we cant get guns during startup
        weaponRestrict = "knife";
    }


    public virtual void Start()
    {
        var player = Utilities.GetPlayerFromSlot(player_slot);

        // player is not alive cancel the lr
        if(!player.IsLegalAlive())
        {
            manager.end_lr(slot);
            return;
        }

        init_player(player);
    }

    public void cleanup()
    {
        // clean up timer
        Lib.KillTimer(ref timer);

        // clean up laser
        Lib.KillTimer(ref laser_timer);

        laser.Destroy();

        countdown.Kill();

        // reset alive player
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);

        if(!player.IsLegalAlive())
        {
            return;
        }


        // make sure our weapons dont get taken off 
        weaponRestrict = "";

        // restore hp
        player.SetHealth(100);

        // restore weapons
        player.StripWeapons();

        // reset gravity
        player.SetGravity(1.0f);

        player.SetVelocity(1.0f);

        if(player.IsCt())
        {
            player.GiveArmour();
            player.GiveWeapon("deagle");
            player.GiveWeapon("m4a1");           
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

        if(!player.IsLegal() || winner == null || !winner.IsLegal())
        {
            manager.end_lr(slot);
            return;
        }

        JailPlugin.WinLR(winner,type);
        JailPlugin.LoseLR(player,type);

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

        if(player.IsLegalAlive())
        {
            player.Announce(LastRequest.LR_PREFIX,"Fight!");
        }

        // renable damage
        // NOTE: start_lr can override this if it so pleases
        restrictDamage = false;

        Start();

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
    public virtual void WeaponFire(String name) {}

    public virtual void EntCreated(CEntityInstance entity) {}
    
    public virtual bool TakeDamage()
    {
        return !restrictDamage;
    }

    public virtual void PlayerHurt(int health,int damage, int hitgroup) 
    {
       
    }

    public virtual bool weapon_drop(String name) 
    {
        return !restrict_drop;
    }

    public virtual bool WeaponEquip(String name) 
    {
        //Server.PrintToChatAll($"{name} : {weaponRestrict}");
        return weaponRestrict == "" || name.Contains(weaponRestrict); 
    }

    public (CCSPlayerController? winner, CCSPlayerController? loser, LRBase? winner_lr) pick_rand_player()
    {
        Random rnd = new Random((int)DateTime.Now.Ticks);

        CCSPlayerController? winner = null;
        CCSPlayerController? loser = null;
        LRBase? winner_lr = null;

        if(rnd.Next(0,2) == 0)
        {
            if(partner != null)
            {
                winner = Utilities.GetPlayerFromSlot(player_slot);
                loser =  Utilities.GetPlayerFromSlot(partner.player_slot);
                winner_lr = this;
            }
        }

        else
        {
            if(partner != null)
            {
                winner =  Utilities.GetPlayerFromSlot(partner.player_slot);
                loser =  Utilities.GetPlayerFromSlot(player_slot);
                winner_lr = partner;
            }
        }


        return (winner,loser,winner_lr);
    }


    public void give_lr_nade_delay(float delay, String name)
    {
        if(JailPlugin.globalCtx == null)
        {
            return;
        }

        JailPlugin.globalCtx.AddTimer(delay,() => 
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);

            // need to double check LR is actually still active...
            if(player.IsLegalAlive() && manager.InLR(player))
            {
                //Server.PrintToChatAll("give nade");
                player.StripWeapons(true);
                player.GiveNamedItem(name);
            }
        });
    }

    static public void prinTCountdown(LRBase lr, int delay)
    {
        if(lr.partner == null)
        {
            return;
        }

        CCSPlayerController? t_player = Utilities.GetPlayerFromSlot(lr.player_slot);
        CCSPlayerController? ct_player = Utilities.GetPlayerFromSlot(lr.partner.player_slot);

        if(!t_player.IsLegal() || !ct_player.IsLegal())
        {
            return;
        }

        t_player.PrintToCenter($"Starting {lr.lr_name} against {ct_player.PlayerName} in {delay} seconds");
        ct_player.PrintToCenter($"Starting {lr.lr_name} against {t_player.PlayerName} in {delay} seconds");
    }

    public void countdown_Start()
    {
        if(laser_timer == null)
        {
            // create the laser timer
            if(JailPlugin.globalCtx != null)
            {
                laser_timer = JailPlugin.globalCtx.AddTimer(1.0f / 25.0f,LaserTick,CSTimer.TimerFlags.STOP_ON_MAPCHANGE | CSTimer.TimerFlags.REPEAT);
            }
        }

        countdown.Start(lr_name,5,this,prinTCountdown,manager.activate_lr);
    }

    public void LaserTick()
    {
        // get both players and check they are valid
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(player_slot);
        CCSPlayerController? other = null;

        if(partner != null)
        {
            other = Utilities.GetPlayerFromSlot(partner.player_slot);
        }

        if(!player.IsLegalAlive() || other == null || !other.IsLegalAlive())
        {
            return;
        }

        CCSPlayerPawn? v1 = player.Pawn();
        CCSPlayerPawn? v2 = other.Pawn();

        // check we can get origin!
        if(v1 == null || v2 == null || v1.AbsOrigin == null || v2.AbsOrigin == null)
        {
            return;
        }


        Vector start = v1.AbsOrigin;
        Vector end = v2.AbsOrigin;

        // make sure it doesn't clip into the ground!
        start.Z += 3.0f;
        end.Z += 3.0f;

        laser.Move(start,end);
    }

    public virtual void EntCreated(String name) {}

    public virtual void GrenadeThrown() {}

    public virtual void WeaponZoom() {}

    public String lr_name = "";

    // player and lr info
    public readonly int player_slot;
    public readonly int slot;

    LastRequest manager;

    // what weapon are we allowed to use?
    public String weaponRestrict = "";

    public bool restrictDamage = true;

    public bool restrict_drop = true;

    LrState state;

    LastRequest.LRType type;

    // who are we playing against, set up in create_pair
    public LRBase? partner;

    // custom choice
    protected String choice = "";

    // countdown for start
    public Countdown<LRBase> countdown = new Countdown<LRBase>();

    Line laser = new Line();

    CSTimer.Timer? laser_timer = null;

    // managed timer
    CSTimer.Timer? timer = null;
};