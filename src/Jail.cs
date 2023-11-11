
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

// main plugin file, controls central hooking
// defers to warden, lr and sd
public class JailPlugin : BasePlugin
{
    // workaround to query global state!
    public static JailPlugin? global_ctx;

    // Global event settings, used to filter plugin activits
    // during warday and SD
    bool is_event_active = false;

    public static bool is_warden(CCSPlayerController? player)
    {
        if(global_ctx == null)
        {
            return false;
        }
     
        return warden.is_warden(player);
    }

    public static bool event_active()
    {
        if(global_ctx == null)
        {
            return false;
        }

        return global_ctx.is_event_active;
    }

    public static void start_event()
    {
        if(global_ctx != null)
        {
            global_ctx.is_event_active = true;
        }
    }

    public static void end_event()
    {
        if(global_ctx != null)
        {
            global_ctx.is_event_active = false;
        }
    }

    public override string ModuleName => "CS2 Jailbreak - destoer";

    public override string ModuleVersion => "0.0.1";

    public override void Load(bool hotReload)
    {
        global_ctx = this;

        register_commands();
        
        register_hook();

        Console.WriteLine("Sucessfully started JB");
    }

    void register_commands()
    {
        // reg warden comamnds
        AddCommand("w", "take warden", warden.take_warden_cmd);
        AddCommand("uw", "leave warden", warden.leave_warden_cmd);
        AddCommand("rw", "remove warden", warden.remove_warden_cmd);

        AddCommand("wub","warden : disable block",warden.wub_cmd);
        AddCommand("wb","warden : enable block",warden.wb_cmd);

        AddCommand("wd","warden : start warday",warden.warday_cmd);

        // reg lr commands
        AddCommand("lr","start an lr",lr.lr_cmd);
        AddCommand("cancel_lr","admin : cancel lr",lr.cancel_lr_cmd);

        // debug 
        if(Debug.enable)
        {
            AddCommand("nuke","debug : kill every player",Debug.nuke);
            AddCommand("is_rebel","debug : print rebel state to console",warden.is_rebel_cmd);
            AddCommand("lr_debug","debug : start an lr without restriction",lr.lr_debug_cmd);
        }
    }


    void register_hook()
    {
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventSwitchTeam>(OnSwitchTeam);
        RegisterEventHandler<EventMapTransition>(OnMapChange);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
        RegisterEventHandler<EventItemEquip>(OnItemEquip);

        // TODO: need to hook weapon drop
    }

    HookResult OnItemEquip(EventItemEquip @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if(player != null && player.is_valid())
        {
            lr.weapon_equip(player,@event.Item);
        }

        return HookResult.Continue;
    }

    HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        CCSPlayerController? attacker = @event.Attacker;

        int damage = @event.DmgHealth;
        int health = @event.Health;

        if(player != null && player.is_valid())
        {
            lr.take_damage(player,attacker,damage,health);
            warden.take_damage(player,attacker,damage,health);
        }

        return HookResult.Continue;
    }

    HookResult OnMapChange(EventMapTransition @event, GameEventInfo info)
    {
        warden.map_start();

        return HookResult.Continue;
    }

    HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        warden.round_start();
        lr.round_start();

        return HookResult.Continue;
    }

    HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        CCSPlayerController? killer = @event.Attacker;

        if(player != null && player.is_valid())
        {
            warden.death(player,killer);
            lr.death(player);
        }

        return HookResult.Continue;
    }

    HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if(player != null && player.is_valid())
        {
            warden.spawn(player);
        }

        return HookResult.Continue;
    }

    // TODO: how do we hook this?
    HookResult OnSwitchTeam(EventSwitchTeam @event, GameEventInfo info)
    {
    /*
        CCSPlayerController? player = @event.Userid;

        if(player != null && player.is_valid())
        {
            warden.switch_team(player,player.TeamNum);
        }
    */
        return HookResult.Continue;
    }

    HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if(player != null && player.is_valid())
        {
            warden.disconnect(player);
            lr.disconnect(player);
        }

        return HookResult.Continue;
    }

    HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        warden.round_end();
        lr.round_end();

        return HookResult.Continue;
    }

    HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
    {
        warden.weapon_fire(@event,info);

        return HookResult.Continue;
    }


    static Warden warden = new Warden();
    static LastRequest lr = new LastRequest();
}