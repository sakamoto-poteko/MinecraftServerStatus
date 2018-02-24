# MinecraftServerStatus
Simple Minecraft server pinger library

Usage:

```C#
IMinecraftPinger pinger = new MinecraftPinger("mcserver.example.com", 25565);
var status = await pinger.PingAsync(); // You can make a 1 sec loop
```

So simple.
