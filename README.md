![](socket-io-logo.png)

# socket.io client written in C&#35;

High-performance C# client for socket.io. Flexibility achieved via [Reactive Extensions](https://github.com/dotnet/reactive#reactive-extensions) and optional configurations. Built on top of .NET Standard 2.1.

## Installation

```
PM> Install-Package Socket.Io.Client.Core
```

## Features

- Fully async and non-blocking flexible API
- Emit events and receive optional callbacks
- Subscribe/Unsubscribe to events
- Monitor socket state
  - React to various socket high/low-level events

## Examples

For more examples see [test project](https://github.com/LadislavBohm/socket.io-client-core/tree/master/src/Socket.Io.Client.Core.Test).

Real-world example can be found in my other repository - Crypto Compare streamer API client:
https://github.com/LadislavBohm/cryptocompare-streamer

### Open/Close Connection

```csharp
//socket is disposed using new "using" syntax, don't forget to dispose it!
using var client = new SocketIoClient();
//optionally supply additional parameters using OpenOptions
var options = new SocketIoOpenOptions("custom-path");

await client.OpenAsync(new Uri("http://localhost:3000"), options);
await client.CloseAsync();
```

### Subscribe to built-in events

Events are implemented using Reactive Extensions, for more information see [here](https://github.com/dotnet/reactive#reactive-extensions) or a very nice [tutorial](http://introtorx.com/). However a very basic usage will be shown here.

#### Basic subscriptions

```csharp
using var client = new SocketIoClient();

//no need to hold reference to subscription as it
//will be automatically unsubscribed once client is disposed
client.Events.OnOpen.Subscribe((_) =>
{
    Console.WriteLine("Socket has been opened");
});

//subscribe to event with data
client.Events.OnPacket.Subscribe(packet =>
{
    Console.WriteLine($"Received packet: {packet}");
});

await client.OpenAsync(new Uri("http://localhost:3000"));
```

#### Time-based subscriptions

```csharp
using var client = new SocketIoClient();

//in reality you shouldn't need to work directly with packets
var subscription = client.Events.OnPacket.Subscribe(packet =>
{
    Console.WriteLine($"Received packet: {packet}");
});

await client.OpenAsync(new Uri("http://localhost:3000"));
//let the subscription live for 500 milliseconds
await Task.Delay(500);
//unsubscribe from this event (socket and other subscriptions are still running)
subscription.Dispose();
```

### Subscribe/Unsubscribe to custom events

Event data from socket.io are received as an array. In event data object you can access the whole array or for convenience just the first item (if available).

```csharp
using var client = new SocketIoClient();

//in this example we throttle event messages to 1 second
var someEventSubscription = client.On("some-event")
    .Throttle(TimeSpan.FromSeconds(1)) //optional
    .Subscribe(message =>
{
    Console.WriteLine($"Received event: {message.EventName}. Data: {message.FirstData}");
});

await client.OpenAsync(new Uri("http://localhost:3000"));

//optionally unsubscribe (equivalent to off() from socket.io)
someEventSubscription.Dispose();
```

### Emit message to server

All emitted messages have an optional callback (acknowledgement) possible via subscribing to the result of Emit method.
All operations are non-blocking and asynchronously handled by underlying Channel library.

```csharp
using var client = new SocketIoClient();

client.Emit("some-event"); //no data emitted
client.Emit("some-event", "some-data"); //string data
client.Emit("some-event", new {data = "some-data"}); //object data

//with callback (acknowledgement)
//it is always called only once, no need to unsubscribe/dispose
client.Emit("some-event", "some-data").Subscribe(ack =>
{
    Console.WriteLine($"Callback with data: {ack.FirstData}.");
});
```

### Configuration

```csharp
//optionally supply your own implementation of ILogger (default is NullLogger)
var options = new SocketIoClientOptions()
    .With(logger: new NullLogger<SocketIoClient>());

using var client = new SocketIoClient(options);
```

## To-do

- Binary data (no support yet)
- Implement automatic reconnection (you can do it by yourself using currently available events)
- Document source code

## Inspiration and Thanks

Thanks to following people and libraries that I used as an inspiration or help during development.

- https://github.com/Marfusios/websocket-client
- https://github.com/socketio/socket.io-client-java
- https://github.com/doghappy/socket.io-client-csharp
