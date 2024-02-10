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
    bool CanStartLR(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return false;
        }

        // prevent starts are round begin to stop lr activations on map joins
        if(Lib.CurTimestamp() - startTimestamp < 15)
        {
            player.LocalisePrefix(LR_PREFIX,"lr.wait");
            return false;
        }

        if(!IsValidT(player))
        {
            return false;
        } 

        if(JailPlugin.warden.IsAliveRebel(player) && Config.rebelCantLr)
        {
            player.LocalisePrefix(LR_PREFIX,"lr.rebel_cant_lr");
            return false;
        }

        
        if(Lib.AliveTCount() > activeLR.Length)
        {
            player.LocalisePrefix(LR_PREFIX,"lr.too_many",activeLR.Length);
            return false;
        }

        return true;
    }

    public void FinaliseChoice(CCSPlayerController? player, ChatMenuOption option)
    {
        // called from pick_parter -> finalise the type struct
        LRChoice? choice = ChoiceFromPlayer(player);

        if(choice == null)
        {
            return;
        }
        
        String name = option.Text;

        choice.ctSlot = Player.SlotFromName(name);

        // finally setup the lr
        InitLR(choice);
    }

    public void PickedOption(CCSPlayerController? player, ChatMenuOption option)
    {
        PickPartnerInternal(player,option.Text);
    }

    public void PickOption(CCSPlayerController? player, ChatMenuOption option)
    {
        // called from lr_type selection
        // save type
        LRChoice? choice = ChoiceFromPlayer(player);

        if(choice == null || !player.IsLegal())
        {
            return;
        }

        choice.type = TypeFromName(option.Text);

        String lrName = LR_NAME[(int)choice.type];

        // now select option
        switch(choice.type)
        {
            case LRType.KNIFE:
            {
                var lrMenu = new ChatMenu($"Choice Menu ({lrName})");

                lrMenu.AddMenuOption("Vanilla", PickedOption);
                lrMenu.AddMenuOption("Low gravity", PickedOption);
                lrMenu.AddMenuOption("High speed", PickedOption);
                lrMenu.AddMenuOption("One hit", PickedOption);
                
                MenuManager.OpenChatMenu(player, lrMenu);                
                break;
            }

            case LRType.DODGEBALL:
            {
                var lrMenu = new ChatMenu($"Choice Menu ({lrName})");

                lrMenu.AddMenuOption("Vanilla", PickedOption);
                lrMenu.AddMenuOption("Low gravity", PickedOption);

                MenuManager.OpenChatMenu(player, lrMenu);
                break;
            }

            case LRType.WAR:
            {
                var lrMenu = new ChatMenu($"Choice Menu ({lrName})");

                lrMenu.AddMenuOption("XM1014", PickedOption);
                lrMenu.AddMenuOption("M249", PickedOption);
                lrMenu.AddMenuOption("P90", PickedOption);
                lrMenu.AddMenuOption("MP5", PickedOption);

                MenuManager.OpenChatMenu(player, lrMenu);
                break;
            }

            case LRType.NO_SCOPE:
            {
                var lrMenu = new ChatMenu($"Choice Menu ({lrName})");

                lrMenu.AddMenuOption("Awp", PickedOption);
                lrMenu.AddMenuOption("Scout", PickedOption);

                MenuManager.OpenChatMenu(player, lrMenu);
                break;                
            }

            case LRType.GRENADE:
            {
                var lrMenu = new ChatMenu($"Choice Menu ({lrName})");

                lrMenu.AddMenuOption("Vanilla", PickedOption);
                lrMenu.AddMenuOption("Low gravity", PickedOption);

                MenuManager.OpenChatMenu(player, lrMenu);
                break;
            }

            case LRType.SHOT_FOR_SHOT:
            case LRType.MAG_FOR_MAG:
            {
                var lrMenu = new ChatMenu($"Choice Menu ({lrName})");

                lrMenu.AddMenuOption("Deagle",PickedOption);
                //lrMenu.AddMenuOption("Usp",PickedOption);
                lrMenu.AddMenuOption("Glock",PickedOption);
                lrMenu.AddMenuOption("Five seven",PickedOption);
                lrMenu.AddMenuOption("Dual Elite",PickedOption);

                MenuManager.OpenChatMenu(player, lrMenu);
                break;
            }

            // no choices just pick a partner
            default:
            {
                PickPartnerInternal(player,"");
                break;
            }
        }
    }

    void PickPartnerInternal(CCSPlayerController? player, String name)
    {
        // called from pick_choice -> pick partner
        LRChoice? choice = ChoiceFromPlayer(player);

        if(choice == null || !player.IsLegal())
        {
            return;
        }

        choice.option = name;

        String lrName = LR_NAME[(int)choice.type];
        String menuName = $"Partner Menu ({lrName})";

        // Debugging pick t's
        if(choice.bypass && player.IsCt())
        {
            Lib.InvokePlayerMenu(player,menuName,FinaliseChoice,Player.IsLegalAliveT);
        }

        else
        {
            Lib.InvokePlayerMenu(player,menuName,FinaliseChoice,Player.IsLegalAliveCT);
        }   
    }

}