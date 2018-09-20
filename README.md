# MulticastRelay
Multicast UDP relay command-line program in VB.NET (.net core). Used for relaying multicast UDP packets to a (unicast) host/port of choice.
My main use case is relaying multicast packets from an Xiaomi Hub to a docker host.

### Arguments
The arguments are:

| Arg short | Arg long | Required? | Description |
| - |--|--| -----|
| -m | --multicast-address | Required | The from multicast address in the form of host:port. |
| -d | --destination-address | Required | The to address in the form of host:port. |
| -b | --bind-to | Optional | Bind to specific interface (All if not specified) |

### Compile
Make sure to have .Net Core SDK 2.1.4 or higher installed. 
Run the command: `dotnet build` in the diretory to create a Debug build. 

### Run precompiled (Portable) build
Make sure to have the .Net Core Runtime installed.
Run the command: `dotnet MultiCastRelay.dll -m <address> -d <address>` to start the program

### Example 
This is the command i use to relay packets from a Xiaomi Hub to a docker host:
`dotnet MultiCastRelay.dll -m 224.0.0.50:9898 -d 192.168.2.100:9899 -b 192.168.2.100`


