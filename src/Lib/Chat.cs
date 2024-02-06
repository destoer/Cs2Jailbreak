using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;


public static class Chat
{
    // chat + centre text print
    static public void announce(String prefix,String str)
    {
        Server.PrintToChatAll(prefix + str);
        print_centre_all(str);
    }

    static public void print_prefix(this CCSPlayerController? player, String prefix, String str)
    {
        if(player.IsLegal() && player.IsConnected() && !player.IsBot)
        {
            player.PrintToChat(prefix + str);
        }
    }

    static public void announce(this CCSPlayerController? player,String prefix,String str)
    {
        if(player.IsLegal() && player.IsConnected() && !player.IsBot)
        {
            player.print_prefix(prefix,str);
            player.PrintToCenter(str);
        }
    }

    // TODO: i dont think there is a builtin func for this...
    static public void print_centre_all(String str)
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.IsLegal())
            {
                continue;
            }

            player.PrintToCenter(str);
        }
    }

    static public void print_console_all(String str, bool admin_only = false)
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.IsLegal() || !player.IsConnected() || player.IsBot)
            {
                continue;
            }

            if(admin_only && !player.IsGenericAdmin())
            {
                return;
            }

            player.PrintToConsole(str);
        }
    }


    static public void localize_announce(this CCSPlayerController? player,String prefix, String name, params Object[] args)
    {
        player.announce(prefix,localize(name,args));
    }

    static public void localize_announce(String prefix, String name, params Object[] args)
    {
        String str = localize(name,args);

        Server.PrintToChatAll(prefix + str);
        print_centre_all(str);
    }

    public static String localize(String name, params Object[] args)
    {
        return JailPlugin.localize(name,args);
    }

    static public void localize(this CCSPlayerController? player,String name, params Object[] args)
    {
        if(player.IsLegal())
        {
            player.PrintToChat(localize(name,args));
        }    
    }

    static public void localise_prefix(this CCSPlayerController? player,String prefix, String name, params Object[] args)
    {
        if(player.IsLegal())
        {
            player.PrintToChat(prefix + localize(name,args));
        }    
    }
}