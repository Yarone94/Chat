using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RandomNameGeneratorLibrary;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Hubs
{
    public class ChatHub: Hub
    {
        private PersonNameGenerator _nameGenerator;
        private readonly ILogger<ChatHub> _logger;
        private static IDictionary<string, string> _connectedClientNames = new Dictionary<string, string>();

        public const string OnNewClientConnected = nameof(OnNewClientConnected);
        public const string OnClientDisconnected = nameof(OnClientDisconnected);
        public const string OnMessageReceived = nameof(OnMessageReceived);

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
            _nameGenerator = new PersonNameGenerator();
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            _connectedClientNames.Add(Context.ConnectionId, _nameGenerator.GenerateRandomFirstName());
            await Clients.AllExcept(Context.ConnectionId).SendAsync(OnNewClientConnected, _connectedClientNames[Context.ConnectionId]);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.AllExcept(Context.ConnectionId).SendAsync(OnClientDisconnected, _connectedClientNames[Context.ConnectionId]);
            _connectedClientNames.Remove(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            _logger.LogTrace("ConnectionId: {connectionId}. Message: {message}", Context.ConnectionId, message);
            
            await Clients.AllExcept(Context.ConnectionId).SendAsync(OnMessageReceived,
                new { text = message, connectionId = Context.ConnectionId, date = DateTime.Now });
        }
    }
}
