# Inputshare #
Inputshare allows machine to share its keyboard and mouse with any number of client machines

Currently windows only, but will be ported to mac/linux soon.

Inputshare server can be run standalone, but the client will require an install as it requires a windows service. (TODO - create a standalone client)

## Features ##
- Switch clients by either a hotkey or moving mouse to client
- Detect fullscreen games and automatically disable mouse client switching
- Share clipboard between server & clients

### Server ###
![](https://github.com/sbarrac1/Inputshare/blob/master/_img/basicserverimg.PNG)
### Client ###
![](https://github.com/sbarrac1/Inputshare/blob/master/_img/basicclientimg.PNG)


## Default hotkeys ##
- Local input - Alt+Z
- Stop server - Ctrl+Alt+Q
- Send SAS (Alt+ctrl+del) - Alt+Ctrl+P

## TODO ##
- Allow changing of function keys (SAS, Exit)
- fix OutOfMemoryException in anonymousePipes
- Rewrite config manager
- Add function hotkeys to config
- add more functionality to client (cancel connect, disable auto retry, connect at boot etc...)
- Create a trayicon that isn't a red square...
- Reading clipboard text sometimes does not work in some cases
