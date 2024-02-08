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
        for(int c = 0; c < lrChoice.Length; c++)
        {
            lrChoice[c] = new LRChoice();
        }

        for(int lr = 0; lr < activeLR.Length; lr++)
        {
            activeLR[lr] = null;
        }
    }

    public void LRConfigReload()
    {
        CreateLRSlots(Config.lrCount);
    }

    void CreateLRSlots(uint slots)
    {
        activeLR = new LRBase[slots];

        for(int lr = 0; lr < activeLR.Length; lr++)
        {
            activeLR[lr] = null;
        }
    }

    void InitPlayerCommon(CCSPlayerController? player, String lrName)
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

        player.Announce(LR_PREFIX,$"{lrName} is starting\n");
    }

    bool LRExists(LRBase lr)
    {
        for(int l = 0; l < activeLR.Count(); l++)
        {
            if(activeLR[l] == lr)
            {
                return true;
            }
        }

        return false;
    }

    // called back by the lr countdown function
    public void ActivateLR(LRBase lr)
    {
        if(LRExists(lr))
        {
            // call the final LR init function and mark it as truly active
            lr.Activate();
            lr.PairActivate();
        }
    }

    void InitLR(LRChoice choice)
    {
        // Okay type, choice, partner selected
        // now we have all the info we need to setup the lr

        CCSPlayerController? tPlayer = Utilities.GetPlayerFromSlot(choice.tSlot);
        CCSPlayerController? ctPlayer = Utilities.GetPlayerFromSlot(choice.ctSlot);

        // Double check we can still do an LR before we trigger!
        if(!choice.bypass)
        {
            if(!CanStartLR(tPlayer) || !CanStartLR(ctPlayer))
            {
                return;
            }
        }

        // check we still actually have all the players
        // our handlers only check once we have actually triggered the LR
        if(!tPlayer.IsLegalAlive() || !ctPlayer.IsLegalAlive())
        {
            Server.PrintToChatAll($"{LR_PREFIX}Disconnection during lr setup");
            return;
        }

        int slot = -1;

        // find a slot to install the lr
        for(int lr = 0; lr < activeLR.Length; lr++)
        {
            if(activeLR[lr] == null)
            {
                slot = lr;
                break;
            }
        }

        if(slot == -1)
        {
            Chat.Announce(LR_PREFIX,"error Could not find empty lr slot");  
            return;
        }

        // create the LR
        LRBase? tLR = null;
        LRBase? ctLR = null;

        switch(choice.type)
        {
            case LRType.KNIFE:
            {
                tLR = new LRKnife(this,choice.type,slot,choice.tSlot,choice.option);
                ctLR = new LRKnife(this,choice.type,slot,choice.ctSlot,choice.option);
                break;
            }

            case LRType.GUN_TOSS:
            {
                tLR = new LRGunToss(this,choice.type,slot,choice.tSlot,choice.option);
                ctLR = new LRGunToss(this,choice.type,slot,choice.ctSlot,choice.option);
                break;
            }

            case LRType.DODGEBALL:
            {
                tLR = new LRDodgeball(this,choice.type,slot,choice.tSlot,choice.option);
                ctLR = new LRDodgeball(this,choice.type,slot,choice.ctSlot,choice.option);
                break;              
            }

            case LRType.GRENADE:
            {
                tLR = new LRGrenade(this,choice.type,slot,choice.tSlot,choice.option);
                ctLR = new LRGrenade(this,choice.type,slot,choice.ctSlot,choice.option);
                break;              
            }

            case LRType.SHOTGUN_WAR:
            {
                tLR = new LRShotgunWar(this,choice.type,slot,choice.tSlot,choice.option);
                ctLR = new LRShotgunWar(this,choice.type,slot,choice.ctSlot,choice.option);
                break;              
            }
    
            case LRType.SCOUT_KNIFE:
            {
                tLR = new LRScoutKnife(this,choice.type,slot,choice.tSlot,choice.option);
                ctLR = new LRScoutKnife(this,choice.type,slot,choice.ctSlot,choice.option);
                break;              
            }

            case LRType.SHOT_FOR_SHOT:
            {
                tLR = new LRShotForShot(this,choice.type,slot,choice.tSlot,choice.option);
                ctLR = new LRShotForShot(this,choice.type,slot,choice.ctSlot,choice.option);
                break;              
            }

            case LRType.MAG_FOR_MAG:
            {
                tLR = new LRShotForShot(this,choice.type,slot,choice.tSlot,choice.option,true);
                ctLR = new LRShotForShot(this,choice.type,slot,choice.ctSlot,choice.option,true);
                break;              
            }

            case LRType.HEADSHOT_ONLY:
            {
                tLR = new LRHeadshotOnly(this,choice.type,slot,choice.tSlot,choice.option);
                ctLR = new LRHeadshotOnly(this,choice.type,slot,choice.ctSlot,choice.option);
                break;              
            }

            case LRType.RUSSIAN_ROULETTE:
            {
                tLR = new LRRussianRoulette(this,choice.type,slot,choice.tSlot,choice.option);
                ctLR = new LRRussianRoulette(this,choice.type,slot,choice.ctSlot,choice.option);
                break;              
            }

            case LRType.NO_SCOPE:
            {
                tLR = new LRNoScope(this,choice.type,slot,choice.tSlot,choice.option);
                ctLR = new LRNoScope(this,choice.type,slot,choice.ctSlot,choice.option);
                break;                 
            }

            case LRType.NONE:
            {
                return;
            }
        }


        // This should not happen
        if(slot == -1 || tLR == null || ctLR == null)
        {
            Chat.Announce(LR_PREFIX,$"Internal LR error in init_lr {slot} {tLR} {ctLR}");
            return;
        }

        // do common player setup
        InitPlayerCommon(tPlayer,tLR.lrName);
        InitPlayerCommon(ctPlayer,ctLR.lrName); 

        // bind lr pair
        tLR.partner = ctLR;
        ctLR.partner = tLR;

        activeLR[slot] = tLR;

        // begin counting down the lr
        tLR.CountdownStart();
    }
    

    public void PurgeLR()
    {
        for(int l = 0; l < activeLR.Length; l++)
        {
            EndLR(l);
        }

        rebelType = RebelType.NONE;
    }

    bool IsPair(CCSPlayerController? v1, CCSPlayerController? v2)
    {
        LRBase? lr1 = FindLR(v1);
        LRBase? lr2 = FindLR(v2);

        // if either aint in lr they aernt a pair
        if(lr1 == null || lr2 == null)
        {
            return false;
        }

        // same slot must be a pair!
        return lr1.slot == lr2.slot;
    }



    // end an lr
    public void EndLR(int slot)
    {
        LRBase? lr = activeLR[slot];

        if(lr == null)
        {
            return;
        }

        // cleanup each lr
        lr.Cleanup();

        if(lr.partner != null)
        {
            lr.partner.Cleanup();
        }

        // Remove lookup

        // remove the slot
        activeLR[slot] = null;
    }

    bool IsValidT(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return false;
        }

        if(!player.PawnIsAlive)
        {
            player.LocalizeAnnounce(LR_PREFIX,"lr.alive");
            return false;
        }

        if(InLR(player))
        {
            player.LocalizeAnnounce(LR_PREFIX,"lr.in_lr");
            return false;            
        }

        if(!player.IsT())
        {
            player.LocalizeAnnounce(LR_PREFIX,"lr.req_t");
            return false;        
        }

        return true;
    }

    LRBase? FindLR(CCSPlayerController? player)
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
        foreach(LRBase? lr in activeLR)
        {
            if(lr == null)
            {
                continue;
            }

            if(lr.playerSlot == slot)
            {
                return lr;
            }

            if(lr.partner != null && lr.partner.playerSlot == slot)
            {
                return lr.partner;
            }
        }

        // could not find
        return null;
    }

    public bool InLR(CCSPlayerController? player)
    {
        return FindLR(player) != null;        
    }


    public void AddLR(ChatMenu menu, bool cond, LRType type)
    {
        if(cond)
        {
            menu.AddMenuOption(LR_NAME[(int)type],PickOption);
        }
    }

    public void LRCmdInternal(CCSPlayerController? player,bool bypass, CommandInfo command)
    {
        // check player can start lr
        // NOTE: higher level function checks its valid to start an lr
        // so we can do a bypass for debugging
        if(!player.IsLegal() || rebelType != RebelType.NONE || JailPlugin.EventActive())
        {
            return;
        }

        int playerSlot = player.Slot;
        lrChoice[playerSlot].tSlot = playerSlot;
        lrChoice[playerSlot].bypass = bypass;

        var lrMenu = new ChatMenu("LR Menu");

        AddLR(lrMenu,Config.lrKnife,LRType.KNIFE);
        AddLR(lrMenu,Config.lrGunToss,LRType.GUN_TOSS);
        AddLR(lrMenu,Config.lrDodgeball,LRType.DODGEBALL);
        AddLR(lrMenu,Config.lrNoScope,LRType.NO_SCOPE);
        AddLR(lrMenu,Config.lrGrenade,LRType.GRENADE);
        AddLR(lrMenu,Config.lrShotgunWar,LRType.SHOTGUN_WAR);
        AddLR(lrMenu,Config.lrRussianRoulette,LRType.RUSSIAN_ROULETTE);
        AddLR(lrMenu,Config.lrScoutKnife,LRType.SCOUT_KNIFE);
        AddLR(lrMenu,Config.lrHeadshotOnly,LRType.HEADSHOT_ONLY);
        AddLR(lrMenu,Config.lrShotForShot,LRType.SHOT_FOR_SHOT);
        AddLR(lrMenu,Config.lrMagForMag,LRType.MAG_FOR_MAG);


        // rebel
        if(CanRebel())
        {
            lrMenu.AddMenuOption("Knife rebel",StartKnifeRebel);
            lrMenu.AddMenuOption("Rebel",StartRebel);
        
            if(Config.riotEnable)
            {
                lrMenu.AddMenuOption("Riot",StartRiot);
            }
        }

        MenuManager.OpenChatMenu(player, lrMenu);
    }

    public void LRCmd(CCSPlayerController? player, CommandInfo command)
    {   
        if(!CanStartLR(player))
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
            player.LocalisePrefix(LR_PREFIX,"lr.cancel_admin");
            return;
        }

        Chat.LocalizeAnnounce(LR_PREFIX,"lr.cancel");
        PurgeLR();
    }

    // TODO: when we can pass extra data in menus this should not be needed
    LRType TypeFromName(String name)
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

    LRChoice? ChoiceFromPlayer(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return null;
        }

        return lrChoice[player.Slot];
    }

    // our current LR's we use as an event dispatch
    // NOTE: each one of these is the T lr and each holds the other pair
    LRBase?[] activeLR = new LRBase[2];

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
        public int tSlot = -1;
        public int ctSlot = -1;
        public bool bypass = false;
    } 


    public JailConfig Config = new JailConfig();

    LRChoice[] lrChoice = new LRChoice[64];
    
    long startTimestamp = 0;

    public static String LR_PREFIX = $" {ChatColors.Green}[LR]: {ChatColors.White}";
}