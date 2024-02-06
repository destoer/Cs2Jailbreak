
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
using CounterStrikeSharp.API.Core.Translations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using System.Drawing;


public partial class Warden
{
    public void LeaveWardenCmd(CCSPlayerController? player, CommandInfo command)
    {
        RemoveIfWarden(player);
    }

    public void RemoveMarkerCmd(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        if(IsWarden(player))
        {
            player.announce(WARDEN_PREFIX,"Marker removed");
            RemoveMarker();
        }
    }

    [RequiresPermissions("@css/generic")]
    public void RemoveWardenCmd(CCSPlayerController? player, CommandInfo command)
    {
        Chat.localize_announce(WARDEN_PREFIX,"warden.remove");
        RemoveWarden();
    }

    [RequiresPermissions("@css/generic")]
    public void ForceOpenCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        Entity.ForceOpen();
    }


    [RequiresPermissions("@css/generic")]
    public void ForceCloseCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        Entity.ForceClose();
    }


    public void WardayCmd(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        // must be warden
        if(!IsWarden(player))
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.warday_restrict");
            return;
        }

        // must specify location
        if(command.ArgCount < 2)
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.warday_usage");
            return;
        }

        // attempt the start the warday
        String location = command.ArgByIndex(1);

        // attempt to parse optional delay
        int delay = 20;

        if(command.ArgCount >= 3)
        {
            if(Int32.TryParse(command.ArgByIndex(2),out int delayOpt))
            {
                delay = delayOpt;
            }       
        }

        if(!warday.StartWarday(location,delay))
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.warday_round_restrict",Warday.ROUND_LIMIT - warday.roundCounter);
        }
    }


    (JailPlayer?, CCSPlayerController?) GiveTInternal(CCSPlayerController? invoke, String name, String playerName)
    {
        if(!IsWarden(invoke))
        {
            invoke.announce(WARDEN_PREFIX,$"You must be the warden to give a {name}");
            return (null,null);
        }

        int slot = Player.SlotFromName(playerName);

        if(slot != -1)
        {
            JailPlayer jailPlayer = jailPlayers[slot];
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);

            return (jailPlayer,player);
        }

        return (null,null);
    }

    public void GiveFreedayCallback(CCSPlayerController? invoke, ChatMenuOption option)
    {
        var (jailPlayer,player) = GiveTInternal(invoke,"freeday",option.Text);

        jailPlayer?.GiveFreeday(player);  
    }

    public void GivePardonCallback(CCSPlayerController? invoke, ChatMenuOption option)
    {
        var (jailPlayer,player) = GiveTInternal(invoke,"pardon",option.Text);

        jailPlayer?.GivePardon(player);  
    }

    public bool IsAliveRebel(CCSPlayerController? player)
    {
        var jailPlayer = JailPlayerFromPlayer(player);

        if(jailPlayer != null)
        {
            return jailPlayer.IsRebel && player.is_valid_alive();
        }

        return false;
    }

    public void GiveT(CCSPlayerController? invoke, String name, Action<CCSPlayerController, ChatMenuOption> callback,Func<CCSPlayerController?,bool> filter)
    {
        if(!IsWarden(invoke))
        {
            invoke.announce(WARDEN_PREFIX,$"Must be warden to give {name}");
            return;
        }

        Lib.InvokePlayerMenu(invoke,name,callback,filter);
    }

    public void ColourCallback(CCSPlayerController? invoke, ChatMenuOption option)
    {
        if(!IsWarden(invoke))
        {
            invoke.announce(WARDEN_PREFIX,$"You must be the warden to colour t's");
            return;        
        }

        CCSPlayerController? player = Utilities.GetPlayerFromSlot(colourSlot);

        Color colour = Lib.COLOUR_CONFIG_MAP[option.Text];

        Chat.announce(WARDEN_PREFIX,$"Setting {player.PlayerName} colour to {option.Text}");
        player.SetColour(colour);
    }

    public void ColourPlayerCallback(CCSPlayerController? invoke, ChatMenuOption option)
    {
        // save this slot for 2nd stage of the command
        colourSlot = Player.SlotFromName(option.Text);

        CCSPlayerController? player = Utilities.GetPlayerFromSlot(colourSlot);

        if(player.is_valid_alive())
        {
            Lib.ColourMenu(invoke,ColourCallback,$"Player colour {player.PlayerName}");
        }

        else
        {
            invoke.announce(WARDEN_PREFIX,$"No such alive player {option.Text} to colour");
        }
    }

    public void ColourCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(!IsWarden(invoke))
        {
            invoke.announce(WARDEN_PREFIX,$"You must be the warden to colour t's");
            return;
        }

        Lib.InvokePlayerMenu(invoke,"Colour",ColourPlayerCallback,Player.is_valid_alive_t);
    }

    public void GiveFreedayCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        GiveT(invoke,"Freeday",GiveFreedayCallback,Player.is_valid_alive_t);
    }

    public void GivePardonCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        GiveT(invoke,"Pardon",GivePardonCallback,IsAliveRebel);
    }
    
    public void WubCmd(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        // must be warden
        if(!IsWarden(player))
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.wub_restrict");
            return;
        }

        block.UnBlockAll();
    }

    public void WbCmd(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        // must be warden
        if(!IsWarden(player))
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.wb_restrict");
            return;
        }

        block.BlockAll();
    }

    // debug command
    [RequiresPermissions("@jail/debug")]
    public void IsRebelCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(!invoke.is_valid())
        {
            return;
        }

        invoke.PrintToConsole("rebels\n");

        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid())
            {
                continue;
            }

            invoke.PrintToConsole($"{jailPlayers[player.Slot].IsRebel} : {player.PlayerName}\n");
        }
    }

    public void WardenTimeCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        if(!invoke.is_valid())
        {
            return;
        }

        if(wardenSlot == INAVLID_SLOT)
        {
            invoke.localise_prefix(WARDEN_PREFIX,"warden.no_warden");
            return;
        }

        long elaspedMin = (Lib.CurTimestamp() - wardenTimestamp) / 60;

        invoke.localise_prefix(WARDEN_PREFIX,"warden.time",elaspedMin);
    }

    public void CmdInfo(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        player.localize("warden.warden_command_desc");
        player.localize("warden.warday_command_desc");
        player.localize("warden.unwarden_command_desc");
        player.localize("warden.block_command_desc");
        player.localize("warden.unblock_command_desc");
        player.localize("warden.remove_warden_command_desc");
        player.localize("warden.laser_colour_command_desc");
        player.localize("warden.marker_colour_command_desc");
        player.localize("warden.wsd_command_desc");
        player.localize("warden.wsd_ff_command_desc");
        player.localize("warden.give_pardon_command_desc");
        player.localize("warden.give_freeday_command_desc");
        player.localize("warden.colour_command_desc");
    }

    public void TakeWardenCmd(CCSPlayerController? player, CommandInfo command)
    {
        // invalid player we dont care
        if(!player.is_valid())
        {
            return;
        }

        // player must be alive
        if(!player.PawnIsAlive)
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.warden_req_alive");
        }        

        // check team is valid
        else if(!player.IsCt())
        {
            player.localise_prefix(WARDEN_PREFIX,"warden.warden_req_ct");
        }

        // check there is no warden
        else if(wardenSlot != INAVLID_SLOT)
        {
            var warden = Utilities.GetPlayerFromSlot(wardenSlot);

            player.localise_prefix(WARDEN_PREFIX,"warden.warden_taken",warden.PlayerName);
        }

        // player is valid to take warden
        else
        {
            SetWarden(player.Slot);
        }
    }


    [RequiresPermissions("@css/generic")]
    public void FireGuardCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        Chat.localize_announce(WARDEN_PREFIX,"warden.fire_guard");

        // swap every guard apart from warden to T
        List<CCSPlayerController> players = Utilities.GetPlayers();
        var valid = players.FindAll(player => player.is_valid() && player.IsCt() && !IsWarden(player));

        foreach(var player in valid)
        {
            player.slay();
            player.SwitchTeam(CsTeam.Terrorist);
        }
    }

    public void CtGuns(CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.is_valid_alive() || !player.IsCt()) 
        {
            return;
        }

        player.StripWeapons();

   
        var jailPlayer = JailPlayerFromPlayer(player);

        if(jailPlayer != null)
        {
            jailPlayer.UpdatePlayer(player, "ct_gun", option.Text);
            jailPlayer.ctGun = option.Text;
        }

        player.GiveMenuWeapon(option.Text);
        player.GiveWeapon("deagle");

        if(Config.ctArmour)
        {
            player.GiveArmour();
        }
    }

    public void CmdCtGuns(CCSPlayerController? player, CommandInfo command)
    {
        if(!player.is_valid())
        {
            return;
        }

        if(!player.IsCt())
        {
            player.localize_announce(WARDEN_PREFIX,"warden.ct_gun_menu");
            return;
        }

        if(!Config.ctGunMenu)
        {
            player.localize_announce(WARDEN_PREFIX,"warden.gun_menu_disabled");
            return;
        }

        player.GunMenuInternal(true,CtGuns);     
    }

}