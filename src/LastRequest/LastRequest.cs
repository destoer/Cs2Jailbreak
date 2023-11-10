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
using CounterStrikeSharp.API.Modules.Admin;

using CSTimer = CounterStrikeSharp.API.Modules.Timers;

public class LastRequest
{
    // TODO: need lookup from player id
    // to a LR!

    public LastRequest()
    {
        for(int c = 0; c < lr_choice.Length; c++)
        {
            lr_choice[c] = new LrChoice();
        }

        for(int lr = 0; lr < active_lr.Length; lr++)
        {
            active_lr[lr] = null;
        }
    }

    void init_player_common(CCSPlayerController? player)
    {
        if(!player.is_valid_alive() || player == null)
        {
            return;
        }

        // strip weapons restore hp
        player.PawnHealth = 100;
        player.strip_weapons();
    }

    void activate_lr(LRBase lr)
    {
        // call the final LR init function and mark it as truly active
        lr.activate();
    }

    public void death(CCSPlayerController? player)
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            lr.lose();
        }
    }


    // init_lr
    void init_lr(LrChoice choice)
    {
        // Okay type, choice, partner selected
        // now we have all the info we need to setup the lr

        CCSPlayerController? t_player = Utilities.GetPlayerFromSlot(choice.t_slot);
        CCSPlayerController? ct_player = Utilities.GetPlayerFromSlot(choice.ct_slot);

        // Double check we can still do an LR before we trigger!
        if(!choice.bypass)
        {
            if(!can_start_lr(t_player))
            {
                return;
            }
        }

        // check we still actually have all the players
        // our handlers only check once we have actually triggered the LR
        if(t_player == null || ct_player == null || !t_player.is_valid_alive() || !ct_player.is_valid_alive())
        {
            Server.PrintToChatAll($"{LR_PREFIX}disconnection during lr setup");
            return;
        }

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

        // create the LR
        LRBase? t_lr = null;
        LRBase? ct_lr = null;

        switch(choice.type)
        {
            case LRType.KNIFE:
            {
                t_lr = new LRKnife(this,slot,choice.t_slot,choice.option);
                ct_lr = new LRKnife(this,slot,choice.ct_slot,choice.option);
                break;
            }

            case LRType.NONE:
            {
                return;
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
        if(JailPlugin.global_ctx != null)
        {
            JailPlugin.global_ctx.AddTimer(5.0f,t_lr.activate,CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
        }

        // print init to players
        String lr_name = LR_NAME[(int)choice.type];
        t_player.announce(LR_PREFIX,$"Starting {lr_name} against {ct_player.PlayerName} in 5 seconds");
        ct_player.announce(LR_PREFIX,$"Starting {lr_name} against {t_player.PlayerName} in 5 seconds");
    }
    
    public void purge_lr()
    {
        for(int l = 0; l < active_lr.Length; l++)
        {
            end_lr(l);
        }
    }

    public void round_start()
    {
        purge_lr();
    }

    public void round_end()
    {
        purge_lr();
    }

    public void disconnect(CCSPlayerController? player)
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            Lib.announce(LR_PREFIX,"Player disconnection cancelling LR");
            end_lr(lr.slot);
        }
    }

    public bool weapon_drop(CCSPlayerController? player,String name) 
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            return lr.weapon_drop(name);
        }

        return true;
    }

    bool is_pair(CCSPlayerController? v1, CCSPlayerController? v2)
    {
        LRBase? l1 = find_lr(v1);
        LRBase? l2 = find_lr(v2);

        // if either aint in lr they aernt a pair
        if(l1 == null || l2 == null)
        {
            return false;
        }

        // same slot must be a pair!
        return v1.slot == v2.slot;
    }

    public void take_damage(CCSPlayerController? player, CCSPlayerController? attacker,ref int damage,ref int health)
    {
        // neither player is in lr we dont care
        if(!in_lr(player) && !in_lr(attacker))
        {
            return;
        }

        LRBase? lr = find_lr(player);

        if(lr == null)
        {
            return;
        }

        // lr has restricted damage or player is not in the same lr
        // dont deal any damage
        if(lr.restrict_damage || !is_pair(player,attacker))
        {
            health = health + damage;
            damage = 0;
        }
    }

    public bool weapon_pickup(CCSPlayerController? player,String name) 
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            return lr.weapon_pickup(name);
        }

        return true;
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

    bool is_valid_t(CCSPlayerController? player)
    {
        if(player == null || !player.is_valid())
        {
            return false;
        }

        if(!player.PawnIsAlive)
        {
            player.PrintToChat($"{LR_PREFIX}You must be alive to start an lr");
            return false;
        }

        if(in_lr(player))
        {
            player.PrintToChat($"{LR_PREFIX}You are allready in a lr");
            return false;            
        }

        if(!player.is_t())
        {
            player.PrintToChat($"{LR_PREFIX}You must be on T to start an lr");
            return false;        
        }

        return true;
    }

    LRBase? find_lr(CCSPlayerController? player)
    {
        // NOTE: dont use anything much from player
        // because the pawn is not their as they may be dced
        if(player == null)
        {
            return null;
        }

        int? slot_opt = player.slot();

        if(slot_opt == null)
        {
            return null;
        }

        int slot = slot_opt.Value;

        // scan each active lr for player and partner
        // a HashTable setup is probably not worthwhile here
        foreach(LRBase? lr in active_lr)
        {
            if(lr == null)
            {
                continue;
            }

            if(lr.player_slot == slot)
            {
                return lr;
            }

            else if(lr.partner != null && lr.partner.player_slot == slot)
            {
                return lr.partner;
            }
        }

        // could not find
        return null;
    }

    bool in_lr(CCSPlayerController? player)
    {
        return find_lr(player) != null;        
    }

    bool can_start_lr(CCSPlayerController? player)
    {
        if(player == null || !is_valid_t(player))
        {
            return false;
        } 
        
        if(Lib.alive_t_count() > active_lr.Length)
        {
            player.PrintToChat($"{LR_PREFIX}There are too many t's alive to start an lr");
            return false;
        }

        return true;
    }

    public void finialise_choice(CCSPlayerController? player, ChatMenuOption option)
    {
        // called from pick_parter -> finalise the type struct
        LrChoice? choice = choice_from_player(player);

        if(choice == null)
        {
            return;
        }
        
        // find the slot of our partner with a scan...
        int slot = -1;

        String name = option.Text;

        foreach(CCSPlayerController partner in Utilities.GetPlayers())
        {
            if(!partner.is_valid())
            {
                continue;
            }

            if(partner.PlayerName == name)
            {
                int? slot_opt = partner.slot();

                if(slot_opt == null)
                {
                    return;
                }

                slot = slot_opt.Value;
                break;
            }
        }

        choice.ct_slot = slot;

        // finally setup the lr
        init_lr(choice);
    }

    public void pick_option(CCSPlayerController? player, ChatMenuOption option)
    {
        // called from lr_type selection
        // save type
        LrChoice? choice = choice_from_player(player);

        if(choice == null)
        {
            return;
        }

        choice.type = type_from_name(option.Text);



        // now select option
        switch(choice.type)
        {
            case LRType.KNIFE:
            {
                pick_partner_internal(player,"");
                break;
            }

            // no choices just pick a partner
            default:
            {
                pick_partner_internal(player,"");
                break;
            }
        }
    }

    void pick_partner_internal(CCSPlayerController? player, String name)
    {
        // called from pick_choice -> pick partner
        LrChoice? choice = choice_from_player(player);

        if(choice == null || player == null)
        {
            return;
        }

        choice.option = name;

        // scan for avaiable CT's that are alive and add as choice
        var alive_ct = Lib.get_alive_ct();

        var lr_menu = new ChatMenu("Partner Menu");

        foreach(var ct in alive_ct)
        {
            if(!ct.is_valid())
            {
                continue;
            }

            lr_menu.AddMenuOption(ct.PlayerName, finialise_choice);
        }

        ChatMenus.OpenMenu(player, lr_menu);
    }

    public void lr_cmd_internal(CCSPlayerController? player,bool bypass, CommandInfo command)
    {
        int? player_slot_opt = player.slot();

        // check player can start lr
        // NOTE: higher level function checks its valid to start an lr
        // so we can do a bypass for debugging
        if(player == null  || !player.is_valid() || player_slot_opt == null)
        {
            return;
        }

        int player_slot = player_slot_opt.Value;
        lr_choice[player_slot].t_slot = player_slot;
        lr_choice[player_slot].bypass = bypass;

        var lr_menu = new ChatMenu("LR Menu");

        // Build the basic LR menu
        for(int t = 0; t < LR_NAME.Length - 1; t++)
        {
            lr_menu.AddMenuOption(LR_NAME[t], pick_option);
        }
        
        ChatMenus.OpenMenu(player, lr_menu);
    }

    public void lr_cmd(CCSPlayerController? player, CommandInfo command)
    {   
        if(!can_start_lr(player))
        {
            return;
        }

        lr_cmd_internal(player,false,command);
    }

    // bypasses validity checks
    [RequiresPermissions("@jail/debug")]
    public void lr_debug_cmd(CCSPlayerController? player, CommandInfo command)
    {
        lr_cmd_internal(player,true,command);
    }

    public void cancel_lr_cmd(CCSPlayerController? player, CommandInfo command)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        // must be admin or warden
        if(!player.is_generic_admin() && !JailPlugin.is_warden(player))
        {
            player.PrintToChat($"{LR_PREFIX} You must be an admin or warden to cancel lr");
            return;
        }

        Lib.announce(LR_PREFIX,"LR cancelled");
        purge_lr();
    }

    // TODO: when we can pass extra data in menus this should not be needed
    LRType type_from_name(String name)
    {
        for(int t = 0; t < LR_NAME.Length; t++)
        {
            if(name == LR_NAME[t])
            {
                return (LRType)t;
            }
        }

        return LRType.NONE;
    }

    LrChoice? choice_from_player(CCSPlayerController? player)
    {
        int? player_slot_opt = player.slot();

        if(!player.is_valid() || player_slot_opt == null)
        {
            return null;
        }

        return lr_choice[player_slot_opt.Value];
    }

    // our current LR's we use as an event dispatch
    // NOTE: each one of these is the T lr and each holds the other pair
    LRBase?[] active_lr = new LRBase[2];

    public enum LRType
    {
        KNIFE,
        NONE,
    };

    static String[] LR_NAME = {
        "Knife Fight",
        "None",
    };

    // Selection for LR
    internal class LrChoice
    {
        public LRType type = LRType.NONE;
        public String option = "";
        public int t_slot = -1;
        public int ct_slot = -1;
        public bool bypass = false;
    } 

    LrChoice[] lr_choice = new LrChoice[64];

    public static readonly String LR_PREFIX = $"{ChatColors.Green}[LR]: {ChatColors.White}";
}