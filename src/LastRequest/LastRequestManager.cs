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

    public LastRequestManager()
    {
        for(int c = 0; c < lr_choice.Length; c++)
        {
            lr_choice[c] = new LrChoice();
        }
    }

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


    // lr_command

    // init_lr
    void init_lr(LrChoice setup)
    {
        CCSPlayerController? t_player = Utilities.GetPlayerFromSlot(setup.t_slot);
        CCSPlayerController? ct_player = Utilities.GetPlayerFromSlot(setup.ct_slot);

        // check we still actually have all the players
        // our handlers only check once we have actually triggered the LR
        if(t_player == null || ct_player == null || !t_player.is_valid_alive() || ct_player.is_valid_alive())
        {
            Server.PrintToChatAll($"{LR_PREFIX}disconnection during lr setup");
            return;
        }

        Server.PrintToChatAll($"setup {setup.choice}: {t_player.PlayerName} {ct_player.PlayerName}");


        // create the LR
        LRBase? t_lr = null;
        LRBase? ct_lr = null;

        int slot = -1;

        // find a slot to install the lr
        for(int lr = 0; lr < active_lr.Length; lr++)
        {
            if(active_lr[lr] == null)
            {
                slot = lr;
                break;
            }
        }

        // This should not happen
        if(slot == -1 || t_lr == null || ct_lr == null)
        {
            Lib.announce(LR_PREFIX,$"Internal LR error in init_lr");
            return;
        }

        // do common player setup
        init_player_common(t_player);
        init_player_common(ct_player); 

        // bind lr pair
        t_lr.partner = ct_lr;
        ct_lr.partner = t_lr;

        active_lr[slot] = t_lr;
        
        // Finally setup final timer for start!
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
        LRBase? lr = active_lr[slot];

        if(lr == null)
        {
            return;
        }

        // cleanup each lr
        lr.cleanup();

        if(lr.partner != null)
        {
            lr.partner.cleanup();
        }

        // Remove lookup

        // remove the slot
        active_lr[slot] = null;
    }

    // our current LR's we use as an event dispatch
    // NOTE: each one of these is the T lr and each holds the other pair
    LRBase?[] active_lr = new LRBase[2];

    // lookup to each LR to see if we need a trigger on a event!
    internal class LrChoice
    {
        public int choice = -1;
        public int t_slot = -1;
        public int ct_slot = -1;
    } 

    LrChoice[] lr_choice = new LrChoice[64];

    const String LR_PREFIX = "[LR]: ";
}