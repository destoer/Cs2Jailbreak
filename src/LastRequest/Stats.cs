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

public class LRStats
{
    public LRStats()
    {
        for(int i = 0; i < 64; i++)
        {
            lr_players[i] = new PlayerStat();
        }
    }

    public void win(CCSPlayerController? player, LastRequest.LRType type)
    {
        var lr_player = lr_player_from_player(player);

        if(lr_player != null && type != LastRequest.LRType.NONE)
        {
            int idx = (int)type;
            lr_player.win[idx] += 1;
        }
    }

    public void loss(CCSPlayerController? player, LastRequest.LRType type)
    {
        var lr_player = lr_player_from_player(player);

        if(lr_player != null && type != LastRequest.LRType.NONE)
        {
            int idx = (int)type;
            lr_player.loss[idx] += 1;
        }        
    }

    PlayerStat? lr_player_from_player(CCSPlayerController? player)
    {
        if(player == null || !player.is_valid())
        {
            return null;
        }

        var slot = player.slot();

        if(slot == null)
        {
            return null;
        }

        return  lr_players[slot.Value];        
    }

    class PlayerStat
    {
        public int[] win = new int[LastRequest.LR_SIZE];
        public int[] loss = new int[LastRequest.LR_SIZE]; 
    }

    PlayerStat[] lr_players = new PlayerStat[64];
}