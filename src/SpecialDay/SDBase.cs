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
    public abstract void Setup();

    public abstract void Start();

    public abstract void End();

    public void SetupCommon()
    {
        // no damage before start
        restrictDamage = true;

        // revive all dead players


        state = SDState.STARTED;
        Setup();

        SetupPlayers();
    }

    public void StartCommon()
    {
        restrictDamage = false;

        state = SDState.ACTIVE;
        Entity.ForceOpen();
        Start();
    }

    // NOTE: this will be recalled by the Disconnect function if the boss dc's
    public virtual void MakeBoss(CCSPlayerController? tank, int count)
    {

    }

    public (CCSPlayerController, int) PickBoss()
    {
        // get valid players
        var valid = Lib.GetAlivePlayers();

        CCSPlayerController? rigged = Utilities.GetPlayerFromSlot(riggedSlot);

        // override pick
        if(rigged.IsLegalAlive())
        {
            var player = rigged;
            riggedSlot = -1;
            return (player,valid.Count);
        }

        // pick one back at random
        Random rnd = new Random((int)DateTime.Now.Ticks);

        int boss = rnd.Next(0,valid.Count);

        bossSlot = valid[boss].Slot;

        return (valid[boss],valid.Count);
    }

    public void Disconnect(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return;
        }

        // player has dced re roll the boss if we have one
        if(player.Slot == bossSlot)
        {
            (CCSPlayerController boss, int count) = PickBoss();

            MakeBoss(boss,count);
        }
    }

    public void EndCommon()
    {
        state = SDState.INACTIVE;
        End();

        Lib.DisableFriendlyFire();

        CCSPlayerController? boss = Utilities.GetPlayerFromSlot(bossSlot);

        // reset the boss colour
        if(boss.IsLegalAlive())
        {
            boss.SetVelocity(1.0f);
            boss.SetColour(Color.FromArgb(255, 255, 255, 255));
        }

        CleanupPlayers();
    }

    public bool IsBoss(CCSPlayerController? player)
    {
        if(player == null)
        {
            return false;
        }

        return player.Slot == bossSlot;
    }

    public virtual bool WeaponEquip(CCSPlayerController player, String name) 
    {
        return weaponRestrict == "" || name.Contains(weaponRestrict); 
    }

    public virtual void PlayerHurt(CCSPlayerController? player,CCSPlayerController? attacker,int health,int damage, int hitgroup) {}

    public virtual void EntCreated(CEntityInstance entity) {}
    public virtual void GrenadeThrown(CCSPlayerController? player) {}

    

    public virtual void Death(CCSPlayerController? player, CCSPlayerController? attacker, String weapon) {}

    public abstract void SetupPlayer(CCSPlayerController player);

    public virtual void CleanupPlayer(CCSPlayerController player) {}

    public void SetupPlayers()
    {
        foreach(CCSPlayerController player in Lib.GetAlivePlayers())
        {
            // reset the player colour incase of rebel
            player.SetColour(Player.DEFAULT_COLOUR);

            SetupPlayer(player);
        }       
    }

    public void CleanupPlayers()
    {
        foreach(CCSPlayerController player in Lib.GetAlivePlayers())
        {
            CleanupPlayer(player);
        }       
    }

    public void LocalizeAnnounce(String name, params Object[] args)
    {
        Chat.LocalizeAnnounce(SpecialDay.SPECIALDAY_PREFIX,name,args);
    }

    public void ResurectPlayer(CCSPlayerController player,float delay)
    {
        int victimSlot = player.Slot;

        JailPlugin.globalCtx.AddTimer(delay, () =>
        {
            CCSPlayerController? target = Utilities.GetPlayerFromSlot(victimSlot);
            target.Respawn();

        },CSTimer.TimerFlags.STOP_ON_MAPCHANGE);

        JailPlugin.globalCtx.AddTimer(delay + 0.5f,() =>
        {
            CCSPlayerController? target = Utilities.GetPlayerFromSlot(victimSlot);

            if(state == SDState.ACTIVE && target.IsLegalAlive())
            {
                SetupPlayer(target);
            }

        },CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
    }


    public int bossSlot = -1;
    public int riggedSlot = -1;

    public bool restrictDamage = false;
    public String weaponRestrict = "";
    public SDState state = SDState.INACTIVE;

    public int delay = 15;
}