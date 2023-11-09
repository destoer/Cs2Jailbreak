
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

// main plugin file, controls central hooking
// defers to warden, lr and sd
public class JailPlugin : BasePlugin
{
    // workaround to query global state!
    public static JailPlugin? global_ctx;

    // Global event settings, used to filter plugin activits
    // during warday and SD
    bool is_event_active = false;

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
        AddCommand("w", "w", warden.take_warden_cmd);
        AddCommand("uw", "uw", warden.leave_warden_cmd);

        AddCommand("wub","wub",warden.wub_cmd);
        AddCommand("wb","wb",warden.wb_cmd);

        AddCommand("wd","wd",warden.warday_cmd);

        // reg lr commands
        AddCommand("lr","lr",lr.lr_cmd);

        // debug 
        if(Debug.enable)
        {
            AddCommand("nuke","nuke",Debug.nuke);
            AddCommand("is_rebel","is_rebel",warden.is_rebel_cmd);
            AddCommand("lr_debug","lr_debug",lr.lr_debug_cmd);
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

    Warden warden = new Warden();
    LastRequest lr = new LastRequest();
}