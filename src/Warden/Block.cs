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

public class Block
{
    [RequiresPermissions("@jail/debug")]
    public void is_blocked(CCSPlayerController? invoke, CommandInfo command)
    {
        invoke.announce(Debug.DEBUG_PREFIX,$"Block state {block_state} : {Lib.block_enabled()}");
    }

    public void block_all()
    {
        if(!Lib.block_enabled())
        {
            Chat.localize_announce(Warden.WARDEN_PREFIX,"block.enable");
            Lib.block_all();
            block_state = true;
        }
    }

    public void unblock_all()
    {
        if(Lib.block_enabled())
        {
            Chat.localize_announce(Warden.WARDEN_PREFIX,"block.disable");
            Lib.unblock_all();
            block_state = false;
        }
    }

    public void RoundStart()
    {
        // TODO: for now we just assume no block
        // we wont have a cvar
        unblock_all();
    }
 
    bool block_state = false;
}