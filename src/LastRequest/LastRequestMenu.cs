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
    bool can_start_lr(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return false;
        }

        // prevent starts are round begin to stop lr activations on map joins
        if(Lib.CurTimestamp() - start_timestamp < 15)
        {
            player.LocalisePrefix(LR_PREFIX,"lr.wait");
            return false;
        }

        if(!is_valid_t(player))
        {
            return false;
        } 

        if(JailPlugin.warden.IsAliveRebel(player) && Config.rebelCantLr)
        {
            player.LocalisePrefix(LR_PREFIX,"lr.rebel_cant_lr");
            return false;
        }

        
        if(Lib.AliveTCount() > active_lr.Length)
        {
            player.LocalisePrefix(LR_PREFIX,"lr.too_many",active_lr.Length);
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
        
        String name = option.Text;

        choice.ct_slot = Player.SlotFromName(name);

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

        if(choice == null || !player.IsLegal())
        {
            return;
        }

        choice.type = type_from_name(option.Text);

        String lr_name = LR_NAME[(int)choice.type];

        // now select option
        switch(choice.type)
        {
            case LRType.KNIFE:
            {
                var lr_menu = new ChatMenu($"Choice Menu ({lr_name})");

                lr_menu.AddMenuOption("Vanilla", picked_option);
                lr_menu.AddMenuOption("Low gravity", picked_option);
                lr_menu.AddMenuOption("High speed", picked_option);
                lr_menu.AddMenuOption("One hit", picked_option);
                
                ChatMenus.OpenMenu(player, lr_menu);                
                break;
            }

            case LRType.DODGEBALL:
            {
                var lr_menu = new ChatMenu($"Choice Menu ({lr_name})");

                lr_menu.AddMenuOption("Vanilla", picked_option);
                lr_menu.AddMenuOption("Low gravity", picked_option);

                ChatMenus.OpenMenu(player, lr_menu);
                break;
            }

            case LRType.NO_SCOPE:
            {
                var lr_menu = new ChatMenu($"Choice Menu ({lr_name})");

                lr_menu.AddMenuOption("Awp", picked_option);
                lr_menu.AddMenuOption("Scout", picked_option);

                ChatMenus.OpenMenu(player, lr_menu);
                break;                
            }

            case LRType.GRENADE:
            {
                var lr_menu = new ChatMenu($"Choice Menu ({lr_name})");

                lr_menu.AddMenuOption("Vanilla", picked_option);
                lr_menu.AddMenuOption("Low gravity", picked_option);

                ChatMenus.OpenMenu(player, lr_menu);
                break;
            }

            case LRType.SHOT_FOR_SHOT:
            case LRType.MAG_FOR_MAG:
            {
                var lr_menu = new ChatMenu($"Choice Menu ({lr_name})");

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

        if(choice == null || !player.IsLegal())
        {
            return;
        }

        choice.option = name;

        String lr_name = LR_NAME[(int)choice.type];
        String menu_name = $"Partner Menu ({lr_name})";

        // Debugging pick t's
        if(choice.bypass && player.IsCt())
        {
            Lib.InvokePlayerMenu(player,menu_name,finialise_choice,Player.IsLegalAliveT);
        }

        else
        {
            Lib.InvokePlayerMenu(player,menu_name,finialise_choice,Player.IsLegalAliveCT);
        }   
    }

}