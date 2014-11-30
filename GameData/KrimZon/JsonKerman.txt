-------------------------------------------------------------------------------

                            J s o n   K e r m a n
                                                    KrimZon <krimzon@gmail.com>
-------------------------------------------------------------------------------

Json Kerman is an external web interface to Kerbal Space program, allowing
(among other things) players to use other displays and devices to show extra
information relating to their current game.

The end goal is an interactive display kind of like RasterPropMonitor but
running on a tablet.


--------------------
Installation And Use
--------------------

1. Extract the zip file into your GameData folder.

2. Note your computer's _LAN_ IP address. see:
     http://www.howtofindmyipaddress.com

3. Run Kerbal Space Program.

4. At some point during the loading screen, the web server will start
   on port 7001.

   TODO: Document how to deal with Windows firewall. (I don't have Windows.)

5. Browse to your computer on port 7001 from a device on your network.
   e.g. If your ip address was 192.168.1.100
   then browse http://192.168.1.100:7001/

6. If the server isn't running yet, hit F5 until it works.
   It should definitely be running by the time you get to the main menu.

7. If you use NoScript, you'll need to whitelist this page
   and ajax.googleapis.com in order for it to update in realtime.


--------------
Technical Info
--------------

JsonKerman blocks access from non-lan IP addresses by default.
These address ranges should work fine:
  192.168.0.0 to 192.168.255.255
  127.0.0.0 to 127.255.255.255
  10.0.0.0 to 10.255.255.255
  172.16.0.0 to 172.32.255.255
(Except that some addresses like x.x.x.0 and x.x.x.255 might be reserved as
the network's broadcast address)

Regular files are served from PluginData/JsonKerman.

The raw data is served from /jebservice.ksp (i.e. Kerbal Server Pages.)


------------
Things To Do
------------

Allow the server port to be read from configuration.

Allow the non-lan IP limit to be disabled in config.

Display more data, especially for the other gamestates.

Make some of the display widgets more interactive.
(e.g. buttons to toggle options.)


-------------------------------------------------------------------------------
