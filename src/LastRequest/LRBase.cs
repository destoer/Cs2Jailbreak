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

    

    protected LRBase(LastRequest lrManager,LastRequest.LRType lrType,int LRSlot,int actorSlot, String lrChoice)
    {
        state = LrState.PENDING;
        slot = LRSlot;
        playerSlot = actorSlot;
        choice = lrChoice;
        lrName = LastRequest.LR_NAME[(int)lrType];
        type = lrType;

        // while lr is pending damage is off
        restrictDamage = true;
        manager = lrManager;

        // make sure we cant get guns during startup
        weaponRestrict = "knife";
    }


    public virtual void Start()
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);

        // player is not alive cancel the lr
        if(!player.IsLegalAlive())
        {
            manager.EndLR(slot);
            return;
        }

        InitPlayer(player);
    }

    public void Cleanup()
    {
        // clean up timer
        Lib.KillTimer(ref timer);

        // clean up laser
        Lib.KillTimer(ref laserTimer);

        // killthe fail safe timer
        Lib.KillTimer(ref failsafeTimer);

        laser.Destroy();

        countdown.Kill();

        // reset alive player
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

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

    public void Lose()
    {
        if(partner == null)
        {
            return;
        }

        CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);
        CCSPlayerController? winner = Utilities.GetPlayerFromSlot(partner.playerSlot);

        if(!player.IsLegal() || winner == null || !winner.IsLegal())
        {
            manager.EndLR(slot);
            return;
        }

        JailPlugin.WinLR(winner,type);
        JailPlugin.LoseLR(player,type);

        manager.EndLR(slot);
    }

    // NOTE: this is called once for a pair on the starting slot
    public virtual void PairActivate()
    {

    }

    public void Activate()
    {
        // this is a timer callback set it to null
        timer = null;

        // check this was built correctly
        // TODO: is there a static way to ensure this is made properly or no?
        if(partner == null)
        {
            manager.EndLR(slot);
            return;         
        }

        CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

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
            partner.Activate();
        }
    }
    
    // player setup -> NOTE: hp and gun stripping is done for us
    abstract public void InitPlayer(CCSPlayerController player);

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

    public virtual bool WeaponDrop(String name) 
    {
        return !restrictDrop;
    }

    public virtual bool WeaponEquip(String name) 
    {
        //Server.PrintToChatAll($"{name} : {weaponRestrict}");
        return weaponRestrict == "" || name.Contains(weaponRestrict); 
    }

    public (CCSPlayerController? winner, CCSPlayerController? loser, LRBase? winnerLR) pick_rand_player()
    {
        Random rnd = new Random((int)DateTime.Now.Ticks);

        CCSPlayerController? winner = null;
        CCSPlayerController? loser = null;
        LRBase? winnerLR = null;

        if(rnd.Next(0,2) == 0)
        {
            if(partner != null)
            {
                winner = Utilities.GetPlayerFromSlot(playerSlot);
                loser =  Utilities.GetPlayerFromSlot(partner.playerSlot);
                winnerLR = this;
            }
        }

        else
        {
            if(partner != null)
            {
                winner =  Utilities.GetPlayerFromSlot(partner.playerSlot);
                loser =  Utilities.GetPlayerFromSlot(playerSlot);
                winnerLR = partner;
            }
        }


        return (winner,loser,winnerLR);
    }


    public void GiveLRNadeDelay(float delay, String name)
    {
        if(JailPlugin.globalCtx == null)
        {
            return;
        }

        JailPlugin.globalCtx.AddTimer(delay,() => 
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

            // need to double check LR is actually still active...
            if(player.IsLegalAlive() && manager.InLR(player))
            {
                //Server.PrintToChatAll("give nade");
                player.StripWeapons(true);
                player.GiveNamedItem(name);
            }
        });
    }

    static public void PrintCountdown(LRBase lr, int delay)
    {
        if(lr.partner == null)
        {
            return;
        }

        CCSPlayerController? tPlayer = Utilities.GetPlayerFromSlot(lr.playerSlot);
        CCSPlayerController? ctPlayer = Utilities.GetPlayerFromSlot(lr.partner.playerSlot);

        if(!tPlayer.IsLegal() || !ctPlayer.IsLegal())
        {
            return;
        }

        if(lr.choice == "")
        {
            tPlayer.PrintToCenter($"Starting {lr.lrName} against {ctPlayer.PlayerName} in {delay} seconds");
            ctPlayer.PrintToCenter($"Starting {lr.lrName} against {tPlayer.PlayerName} in {delay} seconds");
        }

        else
        {
            tPlayer.PrintToCenter($"Starting {lr.lrName} ({lr.choice}) against {ctPlayer.PlayerName} in {delay} seconds");
            ctPlayer.PrintToCenter($"Starting {lr.lrName} ({lr.choice}) against {tPlayer.PlayerName} in {delay} seconds");       
        }
    }

    public void CountdownStart()
    {
        if(laserTimer == null)
        {
            // create the laser timer
            if(JailPlugin.globalCtx != null)
            {
                laserTimer = JailPlugin.globalCtx.AddTimer(1.0f / 25.0f,LaserTick,CSTimer.TimerFlags.STOP_ON_MAPCHANGE | CSTimer.TimerFlags.REPEAT);
            }
        }

        countdown.Start($"{lrName} starts in",5,this,PrintCountdown,manager.ActivateLR);
    }

    public void FailSafeActivate()
    {
        // clean up timer
        failsafeTimer = null;

        failSafe = true;
        Chat.Announce(LastRequest.LR_PREFIX,$"{lrName} fail-safe active");
    }

    public void DelayFailSafe(float delay)
    {
        Chat.Announce(LastRequest.LR_PREFIX,$"fail-safe active in {delay} seconds");
        failsafeTimer = JailPlugin.globalCtx.AddTimer(delay,FailSafeActivate,CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
    }

    public void LaserTick()
    {
        // get both players and check they are valid
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);
        CCSPlayerController? other = null;

        if(partner != null)
        {
            other = Utilities.GetPlayerFromSlot(partner.playerSlot);
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

    public String lrName = "";

    // player and lr info
    public readonly int playerSlot;
    public readonly int slot;

    LastRequest manager;

    // what weapon are we allowed to use?
    public String weaponRestrict = "";

    public bool restrictDamage = true;

    public bool restrictDrop = true;

    LrState state;

    LastRequest.LRType type;

    // who are we playing against, set up in create_pair
    public LRBase? partner;

    // custom choice
    protected String choice = "";

    // countdown for start
    public Countdown<LRBase> countdown = new Countdown<LRBase>();

    Line laser = new Line();

    CSTimer.Timer? failsafeTimer = null;
    public bool failSafe = false;


    CSTimer.Timer? laserTimer = null;

    // managed timer
    CSTimer.Timer? timer = null;
};