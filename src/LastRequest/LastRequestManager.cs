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

public class LastRequestManager
{
    // TODO: need lookup from player id
    // to a LR!

    void init_player_common(CCSPlayerController? player)
    {
        if(!player.is_valid_alive() || player == null)
        {
            return;
        }

        // strip weapons restore hp
        player.PlayerPawn.Value.Health = 100;
        player.strip_weapons();
    }

    void start_lr(LRBase lr)
    {
        // call the final LR init function and mark it as truly active
    }

    internal class LrSetup
    {
        int choice = -1;
        int t_slot = -1;
        int ct_slot = -1;
    } 

    // lr_command

    // init_lr
    void init_lr(LrSetup setup)
    {
        // start the timer for getting the lr ready

        // setup a pending lr

        // init both common players so each indivdual lr doesnt need too
    }
    
    public void purge_lr()
    {
        for(int l = 0; l < active_lr.Length; l++)
        {
            end_lr(l);
        }
    }

    // end an lr
    public void end_lr(int slot)
    {
        if(active_lr[slot] == null)
        {
            return;
        }

        // Remove lookup

        // Restore the still alive participants


        // mark lr null
    }

    // our current LR's we use as an event dispatch
    // NOTE: each one of these is the T lr and each holds the other pair
    LRBase?[] active_lr = new LRBase[2];

    // lookup to each LR to see if we need a trigger on a event!
}