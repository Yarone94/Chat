class Message {
    constructor(text, connectionId, date) {
        this.text = text;
        this.connectionId = connectionId;
        this.date = date;
    }
}

class InfoMessage {
    constructor(text) {
        this.text = text;
    }
}

var app = new Vue({
    el: "#main-block",
    data: {
        signalR: {
            connection: null
        },
        chat: {
            messages: [],
            newMessageText: ''
        }
    },
    mounted: function () {
        this.signalR.connection = new signalR.HubConnectionBuilder()
            .withUrl('/chatHub')
            .configureLogging(signalR.LogLevel.Trace)
            .build();

        this.signalR.connection.start()
            .catch(error => this.onConnectionError(error));

        this.signalR.connection.on("OnMessageReceived", this.onMessageReceived);
        this.signalR.connection.on("OnNewClientConnected", this.onNewClientConnected);
        this.signalR.connection.on("OnClientDisconnected", this.onClientDisconnected);
    },
    methods: {
        onConnectionError: function (error) {
            console.log(error);
        },
        getMessageStyle: function (msg) {
            return {
                media: true,
                'media-meta-day': this.isInfo(msg),
                'media-chat': this.isMessage(msg),
                'media-chat-reverse': this.isMessage(msg) && msg.connectionId == this.signalR.connection.connectionId
            };
        },
        onMessageSend: function () {
            this.signalR.connection.invoke("SendMessage", this.chat.newMessageText);
            this.chat.messages.push(new Message(this.chat.newMessageText, this.signalR.connection.connectionId, new Date()));
            this.chat.newMessageText = '';
        },
        onMessageReceived: function (data) {
            this.chat.messages.push(new Message(data.text, data.connectionId, new Date(data.date)));
        },
        onNewClientConnected: function (clientName) {
            this.chat.messages.push(new InfoMessage(`${clientName} joined chat.`));
        },
        onClientDisconnected: function (clientName) {
            this.chat.messages.push(new InfoMessage(`${clientName} has left chat.`));
        },
        isInfo: function (msg) {
            return msg instanceof InfoMessage;
        },
        isMessage: function (msg) {
            return msg instanceof Message;
        }
    }
});