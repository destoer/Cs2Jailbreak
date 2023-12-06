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
            lr_choice[c] = new LRChoice();
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
        player.strip_weapons(true);
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

    // called back by the lr countdown function
    public void activate_lr(LRBase lr)
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
        if(Lib.alive_t_count() == 2 && player.is_t())
        {
            Lib.announce(LR_PREFIX,"Last request is available type !lr");
        }


        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            lr.lose();
        }
    }


    // init_lr
    void init_lr(LRChoice choice)
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
                t_lr = new LRKnife(this,choice.type,slot,choice.t_slot,choice.option);
                ct_lr = new LRKnife(this,choice.type,slot,choice.ct_slot,choice.option);
                break;
            }

            case LRType.GUN_TOSS:
            {
                t_lr = new LRGunToss(this,choice.type,slot,choice.t_slot,choice.option);
                ct_lr = new LRGunToss(this,choice.type,slot,choice.ct_slot,choice.option);
                break;
            }

            case LRType.DODGEBALL:
            {
                t_lr = new LRDodgeball(this,choice.type,slot,choice.t_slot,choice.option);
                ct_lr = new LRDodgeball(this,choice.type,slot,choice.ct_slot,choice.option);
                break;              
            }

            case LRType.GRENADE:
            {
                t_lr = new LRGrenade(this,choice.type,slot,choice.t_slot,choice.option);
                ct_lr = new LRGrenade(this,choice.type,slot,choice.ct_slot,choice.option);
                break;              
            }

            case LRType.SHOTGUN_WAR:
            {
                t_lr = new LRShotgunWar(this,choice.type,slot,choice.t_slot,choice.option);
                ct_lr = new LRShotgunWar(this,choice.type,slot,choice.ct_slot,choice.option);
                break;              
            }
    
            case LRType.SCOUT_KNIFE:
            {
                t_lr = new LRScoutKnife(this,choice.type,slot,choice.t_slot,choice.option);
                ct_lr = new LRScoutKnife(this,choice.type,slot,choice.ct_slot,choice.option);
                break;              
            }

            case LRType.SHOT_FOR_SHOT:
            {
                t_lr = new LRShotForShot(this,choice.type,slot,choice.t_slot,choice.option);
                ct_lr = new LRShotForShot(this,choice.type,slot,choice.ct_slot,choice.option);
                break;              
            }

            case LRType.MAG_FOR_MAG:
            {
                t_lr = new LRShotForShot(this,choice.type,slot,choice.t_slot,choice.option,true);
                ct_lr = new LRShotForShot(this,choice.type,slot,choice.ct_slot,choice.option,true);
                break;              
            }

            case LRType.HEADSHOT_ONLY:
            {
                t_lr = new LRHeadshotOnly(this,choice.type,slot,choice.t_slot,choice.option);
                ct_lr = new LRHeadshotOnly(this,choice.type,slot,choice.ct_slot,choice.option);
                break;              
            }

            case LRType.RUSSIAN_ROULETTE:
            {
                t_lr = new LRRussianRoulette(this,choice.type,slot,choice.t_slot,choice.option);
                ct_lr = new LRRussianRoulette(this,choice.type,slot,choice.ct_slot,choice.option);
                break;              
            }

            case LRType.NO_SCOPE:
            {
                t_lr = new LRNoScope(this,choice.type,slot,choice.t_slot,choice.option);
                ct_lr = new LRNoScope(this,choice.type,slot,choice.ct_slot,choice.option);
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
        
        // begin counting down the lr
        t_lr.countdown_start();
    }
    

    public void purge_lr()
    {
        for(int l = 0; l < active_lr.Length; l++)
        {
            end_lr(l);
        }

        rebel_type = RebelType.NONE;
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
        lr_stats.purge_player(player);

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



    public void player_hurt(CCSPlayerController? player, CCSPlayerController? attacker, int damage,int health, int hitgroup)
    {
        // check no damage restrict
        LRBase? lr = find_lr(player);

        // no lr
        if(lr == null)
        {
            return;
        }
        
        // not a pair
        if(!is_pair(player,attacker))
        {
            return;
        }

        lr.player_hurt(damage,health,hitgroup);
    }

    public void take_damage(CCSPlayerController? player, CCSPlayerController? attacker, ref float damage)
    {
        // neither player is in lr we dont care
        if(!in_lr(player) && !in_lr(attacker))
        {
            return;
        }

        // not a pair restore hp
        if(!is_pair(player,attacker))
        {
            damage = 0.0f;
            return;
        }

        // check no damage restrict
        LRBase? lr = find_lr(player);

        if(lr == null)
        {
            return;
        }

        if(!lr.take_damage())
        {
            damage = 0.0f;
        }   
    }

    public void weapon_equip(CCSPlayerController? player,String name) 
    {
        if(player == null || !player.is_valid_alive())
        {
            return;
        }

        if(rebel_type == RebelType.KNIFE && !name.Contains("knife"))
        {
            player.strip_weapons();
            return;
        }

        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            CCSPlayerPawn? pawn = player.pawn();

            if(pawn == null)
            {
                return;
            }

            // strip all weapons that aint the restricted one
            var weapons = pawn.WeaponServices?.MyWeapons;

            if(weapons == null)
            {
                return;
            }

            foreach (var weapon_opt in weapons)
            {
                CBasePlayerWeapon? weapon = weapon_opt.Value;

                if (weapon == null)
                { 
                    continue;
                }
                
                var weapon_name = weapon.DesignerName;

                // TODO: Ideally we should just deny the equip all together but this works well enough
                if(!lr.weapon_equip(weapon_name))
                {
                    //Server.PrintToChatAll($"drop player gun: {player.PlayerName} : {weapon_name}");
                    player.DropActiveWeapon();
                }
            }    
        }
    }

    // couldnt get pulling the owner from the projectile ent working
    // so instead we opt for this
    public void weapon_zoom(CCSPlayerController? player)
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            lr.weapon_zoom();
        }       
    }

    // couldnt get pulling the owner from the projectile ent working
    // so instead we opt for this
    public void grenade_thrown(CCSPlayerController? player)
    {
        LRBase? lr = find_lr(player);

        if(lr != null)
        {
            lr.grenade_thrown();
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

    public bool in_lr(CCSPlayerController? player)
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
        LRChoice? choice = choice_from_player(player);

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
        LRChoice? choice = choice_from_player(player);

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

            case LRType.NO_SCOPE:
            {
                var lr_menu = new ChatMenu("Choice Menu");

                lr_menu.AddMenuOption("Awp", picked_option);
                lr_menu.AddMenuOption("Scout", picked_option);

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

            case LRType.SHOT_FOR_SHOT:
            case LRType.MAG_FOR_MAG:
            {
                var lr_menu = new ChatMenu("Choice Menu");

                lr_menu.AddMenuOption("Deagle",picked_option);
                //lr_menu.AddMenuOption("Usp",picked_option);
                lr_menu.AddMenuOption("Glock",picked_option);
                lr_menu.AddMenuOption("Five seven",picked_option);
                lr_menu.AddMenuOption("Dual Elite",picked_option);

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
        LRChoice? choice = choice_from_player(player);

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
                if(!t.is_valid() || in_lr(t))
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
                if(!ct.is_valid() || in_lr(ct))
                {
                    continue;
                }

                lr_menu.AddMenuOption(ct.PlayerName, finialise_choice);
            }

            ChatMenus.OpenMenu(player, lr_menu);
        }
    }

    bool can_rebel()
    {
        return Lib.alive_t_count() == 1;
    }

    public void rebel_guns(CCSPlayerController player, ChatMenuOption option)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        if(!can_rebel() || rebel_type != RebelType.KNIFE)
        {
            player.PrintToChat($"{LR_PREFIX} You must be the last player alive to rebel");
            return;
        }

        player.strip_weapons();

        player.GiveNamedItem("weapon_" + option.Text);
        player.GiveNamedItem("weapon_deagle");

        player.GiveNamedItem("item_assaultsuit");
    
        player.set_health(Lib.alive_ct_count() * 100);

        rebel_type = RebelType.REBEL;

        Lib.announce(LR_PREFIX,$"{player.PlayerName} is a rebel!");
    }

    public void start_rebel(CCSPlayerController? player, ChatMenuOption option)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.gun_menu_internal(false,rebel_guns);
    }

    public void start_knife_rebel(CCSPlayerController? rebel, ChatMenuOption option)
    {
        if(rebel == null || !rebel.is_valid())
        {
            return;
        }

        if(!can_rebel())
        {
            rebel.PrintToChat($"{LR_PREFIX} You must be the last player alive to rebel");
            return;
        }

        rebel_type = RebelType.KNIFE;

        Lib.announce(LR_PREFIX,$"{rebel.PlayerName} is knife a rebel!");
        rebel.set_health(Lib.alive_ct_count() * 100);

        foreach(CCSPlayerController? player in Utilities.GetPlayers())
        {
            if(player != null && player.is_valid_alive())
            {
                player.strip_weapons();
            }
        }
    }

    public void riot_respawn()
    {
        // riot cancelled in mean time
        if(rebel_type != RebelType.RIOT)
        {
            return;
        }


        Lib.announce(LR_PREFIX,"Riot active");

        foreach(CCSPlayerController? player in Utilities.GetPlayers())
        {
            if(player != null && player.is_valid() && !player.is_valid_alive())
            {
                //Server.PrintToChatAll($"Respawn {player.PlayerName}");
                player.Respawn();
            }
        }
    }


    public void start_riot(CCSPlayerController? rebel, ChatMenuOption option)
    {
        if(rebel == null || !rebel.is_valid())
        {
            return;
        }

        if(!can_rebel())
        {
            rebel.PrintToChat($"{LR_PREFIX} You must be the last player alive to rebel");
            return;
        }


        rebel_type = RebelType.RIOT;

        Lib.announce(LR_PREFIX,"A riot has started CT's have 15 seconds to hide");

        if(JailPlugin.global_ctx != null)
        {
            JailPlugin.global_ctx.AddTimer(15.0f,riot_respawn,CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
        }
    }


    public void lr_cmd_internal(CCSPlayerController? player,bool bypass, CommandInfo command)
    {
        int? player_slot_opt = player.slot();

        // check player can start lr
        // NOTE: higher level function checks its valid to start an lr
        // so we can do a bypass for debugging
        if(player == null  || !player.is_valid() || player_slot_opt == null || rebel_type != RebelType.NONE)
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
        
        // rebel
        if(can_rebel())
        {
            lr_menu.AddMenuOption("Knife rebel",start_knife_rebel);
            lr_menu.AddMenuOption("Rebel",start_rebel);

            if(config.riot_enable)
            {
                lr_menu.AddMenuOption("Riot",start_riot);
            }
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

    LRChoice? choice_from_player(CCSPlayerController? player)
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
        GUN_TOSS,
        DODGEBALL,
        NO_SCOPE,
        GRENADE,
        SHOTGUN_WAR,
        RUSSIAN_ROULETTE,
        SCOUT_KNIFE,
        HEADSHOT_ONLY,
        SHOT_FOR_SHOT,
        MAG_FOR_MAG,
        NONE,
    };

    public static String[] LR_NAME = {
        "Knife Fight",
        "Gun toss",
        "Dodgeball",
        "No Scope",
        "Grenade",
        "Shotgun war",
        "Russian roulette",
        "Scout knife",
        "Headshot only",
        "Shot for shot",
        "Mag for mag",
        "None",
    };

    static public readonly int LR_SIZE = 10;

    // Selection for LR
    internal class LRChoice
    {
        public LRType type = LRType.NONE;
        public String option = "";
        public int t_slot = -1;
        public int ct_slot = -1;
        public bool bypass = false;
    } 

    enum RebelType
    {
        NONE,
        REBEL,
        KNIFE,
        RIOT,
    };

    RebelType rebel_type = RebelType.NONE;

    public JailConfig config = new JailConfig();

    LRChoice[] lr_choice = new LRChoice[64];
    public LRStats lr_stats = new LRStats();


    public static readonly String LR_PREFIX = $" {ChatColors.Green}[LR]: {ChatColors.White}";
}