
# TileLords

## Project features
### Game world
This repository contains multiple projects. This repository contains a fully functional GPS-based game core much like Pokemon Go. Movement of the Client(/Player) in the real world (GPS changes) translates to movement in the game world. The game world has several different biomes each roughly equalling to 300x300m of real world size. The biomes can contain different types of resources and those are not shared between players (unlike Pokemon in Pokemon Go) forcing player competition for resources. The resource spawn behavior can be fully customized by providing custom functions. Those can enforce different rarity for different items, checks for biomes (certain resources only spawn in certain biomes) or spawn based on the current player density in the zone. (Balance high population density with low population density areas). The players are also able to see each other which might be a problem in certain countries due to privacy laws. (your exact position is shared with others)
### Quests/scavanger hunt
The current state of the project also has quests fully implemented. The player can pick up a limited amount of quests with exipration times which force the player to go to a randomly picked spot in his area (can be customized in the server for example making the player walk up to kilometers away). This game feature resembles a [Scavenger hunt](https://en.wikipedia.org/wiki/Scavenger_hunt), but the user would be rewarded in the game with coins for reaching a certain spot in his area. However, this game feature relies heavily on randomly generated spots in the area which might not be reachable and a solution for this problem is not yet implemented. (Reroll quest location area, make user confirm it is reachable etc.)
### Missing features:

Privacy mode function to not share position publicly with other players due to concerns with privacy laws in some countries.
The game core currently provides a way to generate resources, clients being able to move and see each other and compete for spawning resources or complete scavanger hunts. The project has currently no resource sinks implemented. Examples would be:

- Player trading
- Player pvp like maintaining player build cities and sieges.
- Player pve like farming.

## Implementation details
### Mapping the earth to a virtual world
The server uses [PlusCode](https://en.wikipedia.org/wiki/Open_Location_Code) to map the earth to an ingame world. The clients use a determenistic procedural algorithm to generate a game world to create an ingame world with an identical look on all devices. This is necessary to reduce bandwidth between server and mobile client. (Mobile Data is limited in some countries)
Only non static data (e.g. other players, spawned resources) is synced between server and clients.

### Databases
The server project requires two external databases. MongoDb for more persistent data (User account data or e.g. User inventory) and Redis (in Memory database) for handling volatile data like spanwed resources or current user GPS positions. IPs for machines hosting the databases are currently hardcoded and can be changed in the source. See [here](https://github.com/Dgitk54/TileLords/blob/master/DataModel.Server/MongoDBFunctions.cs#L28) for the ip of the machine running MongoDB and [here](https://github.com/Dgitk54/TileLords/blob/master/DataModel.Server/RedisDatabaseFunctions.cs#L25) for the ip of the machine running Redis.

### Functional approach
The implementation is heavily using [ReactiveX](https://reactivex.io/) to achieve a more functional programming approach. Both the client and server are build on top of [DotNetty](https://github.com/Azure/DotNetty) (with LZ4 compression/decompression to save bandwidth) and wrap the inbound network traffic as observable streams. The services within the server architecture are input/output functions over the the network traffic, albeit having side effects (=> e.g. database access) reducing the testability of some services. The project heavily follows the design principle of seperation of concerns, thus making it possible to hook the client up to different views, which themselves only interact with the client by passing through user input or requests. The ResourceSpawnService is an example of input/output with filtering on the server. See [ResourceSpawnService](https://github.com/Dgitk54/TileLords/blob/master/DataModel.Server/Services/ResourceSpawnService.cs#L70) as an example for such filtering of the stream.



### Creating a custom view
The repository comes with a few custom views like a console based view for debugging purposes or fake clients for server stress tests. It is possible to import the client in a game engine like [Unity](https://unity.com/) or create a custom renderer with e.g. OpenGL to render the in game world. In case you want to import the client in Unity it is suggested to use [UniRx](https://github.com/neuecc/UniRx) to simplify the process. The client has also been tested in the following scenario: Client was imported into UnityEngine (with the help of use of UniRx) and then build and deployed on an android device (also tested with mobile data).

You can instantiate an experimental [ClientInstanceManager](https://github.com/Dgitk54/TileLords/blob/master/DataModel.Client/ClientInstanceManager.cs) with the ip and port of the machine running the server. It comes with an auto reconnect feature ensuring better support for mobile platforms.
The client currently has an observable stream for the determenistic map, and other information such as players, resources, quests, inventory or picking up resources is handled via [messages](https://github.com/Dgitk54/TileLords/tree/master/DataModel.Common/Messages)


## 3D view example + video
[Lurania](https://github.com/lurania) created a view for the game core with UnityEngine with 3d graphics and full support for registering/logging in, quests, user inventory, gps based movement, visible other players, resource/player interaction UI. This is a demonstration of how a view might look like. A cube in the game world represents roughly a ~10x10m area on the earth. <br />
[3D example view in Unity with a video](https://www.youtube.com/watch?v=Ck_AphVqzBI) <br />
![grafik](https://user-images.githubusercontent.com/68773319/219220935-898548c7-eac1-4636-b551-3dabb47f0df8.png)
![grafik](https://user-images.githubusercontent.com/68773319/219221227-1b5b36aa-d65d-4114-b23f-c65273f15311.png)
![grafik](https://user-images.githubusercontent.com/68773319/219221458-e826b73a-e210-4a08-97e2-83524b228f04.png)


### Big ToDo
- The server has been tested with up to 500 fake users. The fake clients need to be improved to consume less resources or more machines need to be used to benchmark higher player numbers. Check for bottlenecks within ReactiveX or the server/dotnetty. [Read more](https://github.com/Azure/DotNetty/issues/135#issuecomment-227676481)
- Extend the client with more ViewModel functions like filters over the inbound traffic for easier data binding.
- Refactoring the functional code to have less side effects, if possible, for greater testability and thus safety.
- The IP of the machines hosting the databases are currently hardcoded, this needs to be changed. (Service Discovery?) [See](https://github.com/Dgitk54/TileLords/issues/13)
- Add a config for more customizable world generation, currently hardcoded.
- Add a config file for different server settings, simplify the process of modifying spawn functions via config files.
- Add License
