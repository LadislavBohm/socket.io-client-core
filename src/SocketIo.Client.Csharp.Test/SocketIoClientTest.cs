using System;
using System.Buffers;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoCompare.Streamer.Utils;
using Microsoft.Extensions.Logging;
using PipelineClientWebSocket;
using SocketIo.Client.Csharp.EventArguments;
using SocketIo.Client.Csharp.Test.Helpers;
using Xunit;

namespace SocketIo.Client.Csharp.Test
{
    public class SocketIoClientTest
    {
        public class Connect
        {
            [Fact]
            public async Task OpenConnection_ShouldOpen()
            {
                var loggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
                var socket = new SocketIoClient(logger: loggerFactory.CreateLogger<SocketIoClient>());
                socket.On(SocketIoEvent.Error, (Func<ErrorEventArgs, ValueTask>)((args) =>
               {
                   Debug.WriteLine($"Error from socket: {args.Description} {args.Exception}");
                   return default;
               }));
                socket.On(SocketIoEvent.Open,
                     () =>
                    {
                        return socket.EmitAsync("SubAdd",
                            new
                            {
                                subs = new[]
                                {
                                    "0~Abucoins~BTC~USD", "0~BTCAlpha~BTC~USD", "0~BTCChina~BTC~USD", "0~BTCE~BTC~USD", "0~BitBay~BTC~USD",
                                    "0~BitFlip~BTC~USD", "0~BitSquare~BTC~USD", "0~BitTrex~BTC~USD"
                                }
                            });
                    });

                socket.On<MessageEventArgs>("m", message =>
                {
                    Debug.WriteLine($"Message: {message.Data?.FirstOrDefault()}");
                    return default;
                });
                await socket.OpenAsync(CryptoCompareUtils.CreateWssUri(new Uri("https://streamer.cryptocompare.com/")));
                await Task.Delay(2000);
                await socket.CloseAsync();
                await Task.Delay(5000);
            }

            [Fact]
            public async Task Test()
            {
                var socket = new PipelineWebSocket();
                socket.OnMessage += (sender, args) =>
                {
                    Debug.WriteLine("Message: " + Encoding.UTF8.GetString(args.Memory.Span));
                    return default;
                };
                await socket.StartAsync(CryptoCompareUtils.CreateWssUri(new Uri("https://streamer.cryptocompare.com/")));

                await Task.Delay(2000);
                var message = "2probe";
                Memory<byte> buffer = ((Memory<byte>) ArrayPool<byte>.Shared.Rent(message.Length)).Slice(0, message.Length);
                Encoding.UTF8.GetBytes(message, buffer.Span);

                await socket.SendAsync(buffer);

                await Task.Delay(2000);
                message = "42[\"SubAdd\", {\"subs\": [\"0~Abucoins~BTC~USD\", \"0~BitTrex~BTC~USD\", \"0~BTCChina~BTC~USD\"]}]";

                buffer = ((Memory<byte>) ArrayPool<byte>.Shared.Rent(message.Length)).Slice(0, message.Length);
                Encoding.UTF8.GetBytes(message, buffer.Span);

                await socket.SendAsync(buffer);
                await Task.Delay(100000);
            }
        }
    }
}