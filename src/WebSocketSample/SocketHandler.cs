using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket.JogoDaVelha
{
    public class SocketHandler
    {
        public const int BufferSize = 4096;
        private static readonly List<SocketHandler> _observersChat = new List<SocketHandler>();
        private static readonly List<SocketHandler> _observersTab = new List<SocketHandler>();

        private readonly int[] _closedStates =
        {
            (int) WebSocketState.Aborted,
            (int) WebSocketState.Closed,
            (int) WebSocketState.None
        };

        private readonly WebSocket _socket;
        private int _useObserver = 1;

        public SocketHandler(WebSocket socket)
        {
            _socket = socket;
        }

        public static void MapChat(IApplicationBuilder app)
        {
            app.UseWebSockets(new WebSocketOptions());
            app.Use(AcceptorChat);
        }

        public static void MapTab(IApplicationBuilder app)
        {
            app.UseWebSockets(new WebSocketOptions());
            app.Use(AcceptorTab);
        }

        private static async Task AcceptorChat(HttpContext hc, Func<Task> n)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
                return;

            var socket = await hc.WebSockets.AcceptWebSocketAsync();
            var handle = new SocketHandler(socket);
            _observersChat.Add(handle);
            await handle.TakeConnection();
            if (n != null) await n();
        }

        private static async Task AcceptorTab(HttpContext hc, Func<Task> n)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
                return;

            var socket = await hc.WebSockets.AcceptWebSocketAsync();
            var handle = new SocketHandler(socket);
            _observersTab.Add(handle);
            handle._useObserver = 2;
            await handle.TakeConnection();
            if (n != null) await n();
        }

        private List<SocketHandler> GetList()
        {
            List<SocketHandler> collection = null;
            switch (_useObserver)
            {
                case 1:
                    collection = _observersChat;
                    break;

                case 2:
                    collection = _observersTab;
                    break;

                default:
                    collection = _observersChat;
                    break;
            }
            return collection;
        }

        private async Task PublishMessge(WebSocketReceiveResult incoming, byte[] buffer)
        {
            var collection = GetList();

            collection.RemoveAll(s =>
                s._socket == null || _closedStates.Contains((int) s._socket.State));

            foreach (var socketHandler in
                collection.Where(x => x._socket != _socket && x._socket.State == WebSocketState.Open))
            {
                var outgoing = new ArraySegment<byte>(buffer, 0, incoming.Count);
                await socketHandler._socket
                    .SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async Task TakeConnection()
        {
            var buffer = new byte[BufferSize];
            var seg = new ArraySegment<byte>(buffer);
            while (_socket.State == WebSocketState.Open)
            {
                var incoming = await _socket.ReceiveAsync(seg, CancellationToken.None);
                await PublishMessge(incoming, buffer);
            }
        }
    }
}