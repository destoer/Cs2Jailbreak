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


public partial class LastRequest
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

    public void lr_config_reload()
    {
        create_lr_slots(Config.lrCount);
    }

    void create_lr_slots(uint slots)
    {
        active_lr = new LRBase[slots];

        for(int lr = 0; lr < active_lr.Length; lr++)
        {
            active_lr[lr] = null;
        }
    }

    void init_player_common(CCSPlayerController? player, String lr_name)
    {
        if(!player.IsLegalAlive())
        {
            return;
        }

        // strip weapons restore hp
        player.SetHealth(100);
        player.SetArmour(100);
        player.StripWeapons(true);
        player.GiveArmour();

        player.announce(LR_PREFIX,$"{lr_name} is starting\n");
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
        if(!t_player.IsLegalAlive() || !ct_player.IsLegalAlive())
        {
            Server.PrintToChatAll($"{LR_PREFIX}Disconnection during lr setup");
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
            Chat.announce(LR_PREFIX,$"Internal LR error in init_lr {slot} {t_lr} {ct_lr}");
            return;
        }

        // do common player setup
        init_player_common(t_player,t_lr.lr_name);
        init_player_common(ct_player,ct_lr.lr_name); 

        // bind lr pair
        t_lr.partner = ct_lr;
        ct_lr.partner = t_lr;

        active_lr[slot] = t_lr;

        // begin counting down the lr
        t_lr.countdown_Start();
    }
    

    public void purge_lr()
    {
        for(int l = 0; l < active_lr.Length; l++)
        {
            end_lr(l);
        }

        rebel_type = RebelType.NONE;
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
        if(!player.IsLegal())
        {
            return false;
        }

        if(!player.PawnIsAlive)
        {
            player.localize_announce(LR_PREFIX,"lr.alive");
            return false;
        }

        if(InLR(player))
        {
            player.localize_announce(LR_PREFIX,"lr.in_lr");
            return false;            
        }

        if(!player.IsT())
        {
            player.localize_announce(LR_PREFIX,"lr.req_t");
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

        int slot = player.Slot;

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

    public bool InLR(CCSPlayerController? player)
    {
        return find_lr(player) != null;        
    }


    public void add_lr(ChatMenu menu, bool cond, LRType type)
    {
        if(cond)
        {
            menu.AddMenuOption(LR_NAME[(int)type],pick_option);
        }
    }

    public void LRCmdInternal(CCSPlayerController? player,bool bypass, CommandInfo command)
    {
        // check player can start lr
        // NOTE: higher level function checks its valid to start an lr
        // so we can do a bypass for debugging
        if(!player.IsLegal() || rebel_type != RebelType.NONE || JailPlugin.EventActive())
        {
            return;
        }

        int player_slot = player.Slot;
        lr_choice[player_slot].t_slot = player_slot;
        lr_choice[player_slot].bypass = bypass;

        var lr_menu = new ChatMenu("LR Menu");

        add_lr(lr_menu,Config.lrKnife,LRType.KNIFE);
        add_lr(lr_menu,Config.lrGunToss,LRType.GUN_TOSS);
        add_lr(lr_menu,Config.lrDodgeball,LRType.DODGEBALL);
        add_lr(lr_menu,Config.lrNoScope,LRType.NO_SCOPE);
        add_lr(lr_menu,Config.lrGrenade,LRType.GRENADE);
        add_lr(lr_menu,Config.lrShotgunWar,LRType.SHOTGUN_WAR);
        add_lr(lr_menu,Config.lrRussianRoulette,LRType.RUSSIAN_ROULETTE);
        add_lr(lr_menu,Config.lrScoutKnife,LRType.SCOUT_KNIFE);
        add_lr(lr_menu,Config.lrHeadshotOnly,LRType.HEADSHOT_ONLY);
        add_lr(lr_menu,Config.lrShotForShot,LRType.SHOT_FOR_SHOT);
        add_lr(lr_menu,Config.lrMagForMag,LRType.MAG_FOR_MAG);


        // rebel
        if(can_rebel())
        {
            lr_menu.AddMenuOption("Knife rebel",start_knife_rebel);
            lr_menu.AddMenuOption("Rebel",start_rebel);
        /*
            if(Config.riotEnable)
            {
                lr_menu.AddMenuOption("Riot",start_riot);
            }
        */
        }

        ChatMenus.OpenMenu(player, lr_menu);
    }

    public void LRCmd(CCSPlayerController? player, CommandInfo command)
    {   
        if(!can_start_lr(player))
        {
            return;
        }

        LRCmdInternal(player,false,command);
    }

    // bypasses validity checks
    [RequiresPermissions("@jail/debug")]
    public void LRDebugCmd(CCSPlayerController? player, CommandInfo command)
    {
        LRCmdInternal(player,true,command);
    }

    public void CancelLRCmd(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.IsLegal())
        {
            return;
        }

        // must be admin or warden
        if(!player.IsGenericAdmin() && !JailPlugin.IsWarden(player))
        {
            player.localise_prefix(LR_PREFIX,"lr.cancel_admin");
            return;
        }

        Chat.localize_announce(LR_PREFIX,"lr.cancel");
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
        if(!player.IsLegal())
        {
            return null;
        }

        return lr_choice[player.Slot];
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


    public JailConfig Config = new JailConfig();

    LRChoice[] lr_choice = new LRChoice[64];
    
    long start_timestamp = 0;

    public static readonly String LR_PREFIX = $" {ChatColors.Green}[LR]: {ChatColors.White}";
}