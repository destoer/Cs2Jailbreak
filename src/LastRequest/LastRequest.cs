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

/*
    these should be doable

    rebel,
    knife_rebel

    russian_roulette,

*/


public class LastRequest
{
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
        player.set_health(100);
        player.set_armour(100);
        player.strip_weapons();
        player.GiveNamedItem("item_assaultsuit");
    }

    bool lr_exists(LRBase lr)
    {
        for(int l = 0; l < active_lr.Count(); l++)
        {
            if(active_lr[l] == lr)
            {
                return true;
            }
        }

        return false;
    }

    void activate_lr(LRBase lr)
    {
        if(lr_exists(lr))
        {
            // call the final LR init function and mark it as truly active
            lr.activate();
            lr.pair_activate();
        }
    }

    public void death(CCSPlayerController? player)
    {
        // TODO: add auto menu open

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

            case LRType.DODGEBALL:
            {
                t_lr = new LRDodgeball(this,slot,choice.t_slot,choice.option);
                ct_lr = new LRDodgeball(this,slot,choice.ct_slot,choice.option);
                break;              
            }

            case LRType.GRENADE:
            {
                t_lr = new LRGrenade(this,slot,choice.t_slot,choice.option);
                ct_lr = new LRGrenade(this,slot,choice.ct_slot,choice.option);
                break;              
            }

            case LRType.SHOTGUN_WAR:
            {
                t_lr = new LRShotgunWar(this,slot,choice.t_slot,choice.option);
                ct_lr = new LRShotgunWar(this,slot,choice.ct_slot,choice.option);
                break;              
            }
    
            case LRType.SCOUT_KNIFE:
            {
                t_lr = new LRScoutKnife(this,slot,choice.t_slot,choice.option);
                ct_lr = new LRScoutKnife(this,slot,choice.ct_slot,choice.option);
                break;              
            }

            case LRType.SHOT_FOR_SHOT:
            {
                t_lr = new LRShotForShot(this,slot,choice.t_slot,choice.option);
                ct_lr = new LRShotForShot(this,slot,choice.ct_slot,choice.option);
                break;              
            }

            case LRType.MAG_FOR_MAG:
            {
                t_lr = new LRShotForShot(this,slot,choice.t_slot,choice.option,true);
                ct_lr = new LRShotForShot(this,slot,choice.ct_slot,choice.option,true);
                break;              
            }

            case LRType.HEADSHOT_ONLY:
            {
                t_lr = new LRHeadshotOnly(this,slot,choice.t_slot,choice.option);
                ct_lr = new LRHeadshotOnly(this,slot,choice.ct_slot,choice.option);
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
            JailPlugin.global_ctx.AddTimer(5.0f,() => activate_lr(t_lr),CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
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

    public void weapon_fire(CCSPlayerController? player,String name) 
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            lr.weapon_fire(name);
        }
    }

    bool is_pair(CCSPlayerController? v1, CCSPlayerController? v2)
    {
        LRBase? lr1 = find_lr(v1);
        LRBase? lr2 = find_lr(v2);

        // if either aint in lr they aernt a pair
        if(lr1 == null || lr2 == null)
        {
            return false;
        }

        // same slot must be a pair!
        return lr1.slot == lr2.slot;
    }

    void restore_hp(CCSPlayerController? player, int damage, int health)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        // TODO: why does this sometimes mess up?
        if(health < 100)
        {
            player.set_health(Math.Min(health + damage,100));
        }

        else
        {
            player.set_health(health + damage);
        }
    }

    public void take_damage(CCSPlayerController? player, CCSPlayerController? attacker, int damage,int health, int hitgroup)
    {
        // neither player is in lr we dont care
        if(!in_lr(player) && !in_lr(attacker))
        {
            return;
        }

        // not a pair restore hp
        if(!is_pair(player,attacker))
        {
            restore_hp(player,damage,health);
            return;
        }

        // check no damage restrict
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            if(!lr.take_damage(damage,health,hitgroup))
            {
                restore_hp(player,damage,health);
            }
        }
    }

    public void weapon_equip(CCSPlayerController? player,String name) 
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            if(player != null && player.is_valid_alive())
            {
                // strip all weapons that aint the restricted one
                var weapons = player.Pawn.Value.WeaponServices?.MyWeapons;

                if(weapons == null)
                {
                    return;
                }

                foreach (var weapon in weapons)
                {
                    if (!weapon.is_valid())
                    { 
                        continue;
                    }
                    
                    var weapon_name = weapon.Value.DesignerName;

                    if(!lr.weapon_equip(weapon_name) && !weapon_name.Contains("knife"))
                    {
                        weapon.Value.Remove();
                    }
                }       
            }   
        }
    }

    public void ent_created(CEntityInstance entity)
    {
        for(int l = 0; l < active_lr.Length; l++)
        {
            LRBase? lr = active_lr[l];

            if(lr != null && entity.IsValid)
            {
                lr.ent_created(entity);
            }
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

            if(lr.partner != null && lr.partner.player_slot == slot)
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

    public void picked_option(CCSPlayerController? player, ChatMenuOption option)
    {
        pick_partner_internal(player,option.Text);
    }

    public void pick_option(CCSPlayerController? player, ChatMenuOption option)
    {
        // called from lr_type selection
        // save type
        LrChoice? choice = choice_from_player(player);

        if(choice == null || player == null)
        {
            return;
        }

        choice.type = type_from_name(option.Text);



        // now select option
        switch(choice.type)
        {
            case LRType.KNIFE:
            {
                var lr_menu = new ChatMenu("Choice Menu");

                lr_menu.AddMenuOption("Vanilla", picked_option);
                lr_menu.AddMenuOption("Low gravity", picked_option);
                lr_menu.AddMenuOption("High speed", picked_option);
                lr_menu.AddMenuOption("One hit", picked_option);
                
                ChatMenus.OpenMenu(player, lr_menu);                
                break;
            }

            case LRType.DODGEBALL:
            {
                var lr_menu = new ChatMenu("Choice Menu");

                lr_menu.AddMenuOption("Vanilla", picked_option);
                lr_menu.AddMenuOption("Low gravity", picked_option);

                ChatMenus.OpenMenu(player, lr_menu);
                break;
            }

            case LRType.GRENADE:
            {
                var lr_menu = new ChatMenu("Choice Menu");

                lr_menu.AddMenuOption("Vanilla", picked_option);
                lr_menu.AddMenuOption("Low gravity", picked_option);

                ChatMenus.OpenMenu(player, lr_menu);
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

        // Debugging pick t's
        if(choice.bypass && player.is_ct())
        {

            // scan for avaiable CT's that are alive and add as choice
            var alive_t = Lib.get_alive_t();

            var lr_menu = new ChatMenu("Partner Menu");

            foreach(var t in alive_t)
            {
                if(!t.is_valid())
                {
                    continue;
                }

                lr_menu.AddMenuOption(t.PlayerName, finialise_choice);
            }

            ChatMenus.OpenMenu(player, lr_menu);
        }

        else
        {

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
        DODGEBALL,
        GRENADE,
        SHOTGUN_WAR,
        SCOUT_KNIFE,
        HEADSHOT_ONLY,
        SHOT_FOR_SHOT,
        MAG_FOR_MAG,
        NONE,
    };

    static String[] LR_NAME = {
        "Knife Fight",
        "Dodgeball",
        "Grenade",
        "Shotgun war",
        "Scout knife",
        "Headshot only",
        "Shot for shot",
        "Mag for mag",
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