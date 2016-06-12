# MinecraftServerStatus
Simple Minecraft server pinger library

Usage:

```C#
MCServerStatus.MCPinger pinger = new MCServerStatus.MCPinger("mcserver.example.com", 25565);
var status = await pinger.Ping(); // You can make a 1 sec loop
```

So simple.
