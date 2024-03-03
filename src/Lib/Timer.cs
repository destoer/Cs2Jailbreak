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
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Admin;
using System.Drawing;
using System.Text;
using System.Diagnostics.CodeAnalysis;



// dump these in their own class so that people can copy the other library
// files without getting compilation errors


// NOTE: this is a timer wrapper, and should be owned the class
// wanting to use the timer
public class Countdown<T>
{
    public void Start(String countdownName, int countdownDelay,
        T countdownData,Action<T,int>? countdownPrintFunc, Action <T>? countdownCallback)
    {
        this.delay = countdownDelay;
        this.callback = countdownCallback;
        this.name = countdownName;
        this.data = countdownData;
        this.printFunc = countdownPrintFunc;

        this.handle = JailPlugin.globalCtx.AddTimer(1.0f,Tick,CSTimer.TimerFlags.STOP_ON_MAPCHANGE | CSTimer.TimerFlags.REPEAT);
    }

    public void Kill()
    {
       Lib.KillTimer(ref handle);
    }

    void Tick()
    {
        delay -= 1;

        // countdown over
        if(delay <= 0)
        {
            // kill the timer
            // and then call the callback
            Kill();

            if(callback != null && data != null)
            {
                callback(data);
            }

            else
            {
                Chat.PrintCentreAll("Countdown over");
            }
        }

        // countdown still active
        else
        {
            // custom print
            if(printFunc != null && data != null)
            {
                printFunc(data,delay);
            }

            // default print
            else
            {
                Chat.PrintCentreAll($"{name}: {delay}");
            }
        }
    }

    public int delay = 0;
    public Action<T>? callback = null;
    public String name = "";
    public Action<T,int>? printFunc = null;
    CSTimer.Timer? handle = null;

    // callback data
    T? data = default(T);
}


static public class Timer
{
    // player
    static public void RespawnDelay(this CCSPlayerController? player, float delay)
    {
        if(!player.IsLegal())
        {
            return;
        }

        JailPlugin.globalCtx.AddTimer(delay,() => Player.RespawnCallback(player.Slot),CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
    }

    // ent
    static public void RemoveDelay(this CEntityInstance entity, float delay, String name)
    {
        // remove projectile
        if(entity.DesignerName == name)
        {
            int index = (int)entity.Index;

            JailPlugin.globalCtx.AddTimer(delay,() => 
            {
                Entity.Remove(index,name);
            });
        }
    }


    static public void GiveEventNadeDelay(this CCSPlayerController? target,float delay, String name)
    {
        if(!target.IsLegalAlive())
        {
            return;
        }

        int slot = target.Slot;

        JailPlugin.globalCtx.AddTimer(delay,() => 
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);

            if(player.IsLegalAlive())
            {
                //Server.PrintToChatAll("give nade");
                player.StripWeapons(true);
                player.GiveNamedItem(name);
            }
        });
    }
}