# mayLCU

`mayLCU` is a C# library that provides a convenient way to interact with the League of Legends Client (LCU) through its HTTP API. It allows you to perform various actions such as making requests, retrieving data, and sending commands to the League Client.

## Usage

### Creating an instance of LCU

To create an instance of `LCU` and connect to the League of Legends Client, you can use the provided factory methods:

- `HookRiotClient()`: Connects to the Riot Client.
- `HookLeagueClient()`: Connects to the League Client.
- `HookLeagueStore(LCU leagueClient)`: Connects to the League Store.

Example:

```csharp
LCU lcu = LCU.HookLeagueClient();
```

### Making Requests

`mayLCU` provides methods to make HTTP requests to the League of Legends Client API. You can use the following methods:

- `RequestAsync(string uri)`: Sends an asynchronous GET request to the specified URI and returns the response as a string.
- `RequestAsync(RequestMethod requestMethod, string uri, string payload = "")`: Sends an asynchronous HTTP request with the specified method (GET, POST, PUT, DELETE, etc.), URI, and payload. Returns the response as a string.

Example:

```csharp
string response = await lcu.RequestAsync("/lol-summoner/v1/current-summoner");
```

### Handling Responses

You can also make requests that return dynamic objects instead of strings. The library provides methods for that purpose:

- `RequestDynamicAsync(string uri)`: Sends an asynchronous GET request to the specified URI and returns the response as a dynamic object.
- `RequestDynamicAsync(RequestMethod requestMethod, string uri, string payload = "")`: Sends an asynchronous HTTP request with the specified method (GET, POST, PUT, DELETE, etc.), URI, and payload. Returns the response as a dynamic object.

Example:

```csharp
dynamic data = await lcu.RequestDynamicAsync("/lol-summoner/v1/current-summoner");
string summonerName = data.displayName;
```

### Synchronous Requests

If you prefer to make synchronous requests instead of asynchronous ones, `mayLCU` provides equivalent synchronous methods:

- `Request(string uri)`: Sends a synchronous GET request to the specified URI and returns the response as a string.
- `Request(RequestMethod requestMethod, string uri, string payload = "")`: Sends a synchronous HTTP request with the specified method (GET, POST, PUT, DELETE, etc.), URI, and payload. Returns the response as a string.

Example:

```csharp
string response = lcu.Request("/lol-summoner/v1/current-summoner");
```

### Additional Information

- `IsConnected`: Gets a value indicating whether the connection to the League Client is established.
- `Target`: Gets the targeted process name (without the "Ux" suffix).

### Examples

Here are some examples of how you can use `mayLCU`:

```csharp
// Hook League Client
LCU lcu = LCU.HookLeagueClient();

// Get the current summoner's name
dynamic data = await lcu.RequestDynamicAsync("/lol-summoner/v1/current-summoner");
string summonerName = data.displayName;
Console.WriteLine($"Summoner Name: {summonerName}");

```
```csharp
// Hook League Client
LCU leagueClient = LCU.HookLeagueClient();

// Hook League Store using the leagueClient instance
LCU leagueStoreClient = LCU.HookLeagueStore(leagueClient);

// Example: Make a purchase request
var httpPayload = $"{{\"accountId\":{accountId},\"items\":[{{\"inventoryType\":\"{type}\",\"itemId\":{itemId},\"ipCost\":null,\"rpCost\":{rpPrice},\"quantity\":1}}]}}"
dynamic data = await leageuStoreClient.RequestDynamicAsync(RequestMethod.POST, "/storefront/v3/purchase?language=en_US", httpPayload)
```

## Disclaimer

This project is not affiliated with or endorsed by Riot Games.
