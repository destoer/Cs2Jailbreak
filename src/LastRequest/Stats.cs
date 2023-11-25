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

        if(lr_player != null && type != LastRequest.LRType.NONE && player != null && player.is_valid())
        {
            int idx = (int)type;
            lr_player.win[idx] += 1;
            Lib.announce(LastRequest.LR_PREFIX,$"{player.PlayerName} won {LastRequest.LR_NAME[idx]} win {lr_player.win[idx]} : loss {lr_player.loss[idx]}");
        }
    }

    public void loss(CCSPlayerController? player, LastRequest.LRType type)
    {
        var lr_player = lr_player_from_player(player);

        if(lr_player != null && type != LastRequest.LRType.NONE && player != null && player.is_valid())
        {
            int idx = (int)type;
            lr_player.loss[idx] += 1;
            Lib.announce(LastRequest.LR_PREFIX,$"{player.PlayerName} lost {LastRequest.LR_NAME[idx]} win {lr_player.win[idx]} : loss {lr_player.loss[idx]}");
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



    void print_stats(CCSPlayerController? invoke, CCSPlayerController? player)
    {
        if(invoke == null || !invoke.is_valid())
        {
            return;
        }

        var lr_player = lr_player_from_player(player);

        if(lr_player != null && player != null && player.is_valid())
        {
            invoke.PrintToChat($"{LastRequest.LR_PREFIX} lr stats for {player.PlayerName}");

            for(int i = 0; i < LastRequest.LR_SIZE; i++)
            {
                invoke.PrintToChat($"{LastRequest.LR_PREFIX} {LastRequest.LR_NAME[i]} win {lr_player.win[i]} : loss {lr_player.loss[i]}");
            }
        }
    }

    public void lr_stats_cmd(CCSPlayerController? player, CommandInfo command)
    {
        // just do own player for now
        print_stats(player,player);
    }

    public void purge_player(CCSPlayerController? player)
    {
        var lr_player = lr_player_from_player(player);

        if(lr_player != null)
        {
            for(int i = 0; i < LastRequest.LR_SIZE; i++)
            {
                lr_player.win[i] = 0;
                lr_player.loss[i] = 0;
            }
        }
    }

    class PlayerStat
    {
        public int[] win = new int[LastRequest.LR_SIZE];
        public int[] loss = new int[LastRequest.LR_SIZE]; 
    }

    PlayerStat[] lr_players = new PlayerStat[64];
}