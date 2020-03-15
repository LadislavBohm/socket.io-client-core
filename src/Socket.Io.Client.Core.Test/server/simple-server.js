const server = require("http").createServer();
const io = require("socket.io")(server, {
  path: "/some-path",
  serveClient: false,
  pingInterval: 50,
  pingTimeout: 400,
  cookie: false
});

io.on("connection", client => {
  console.log("client connected", client.id);

  //handle 'roomId' parameter and join user to room if it's present
  let roomId = client.handshake.query["roomId"];
  if (roomId) {
    console.log("joining user to room from query", roomId);
    client.join(roomId, (data, callback) => {
      client.on(data, message => {
        console.log(`sending message to room ${roomId}`);
        io.to(roomId).emit(data, message);
      });
      io.to(roomId).emit(roomId, "welcome");
    });
  }

  //log users that disconnect
  client.on("disconnect", () => {
    console.log("client disconnected", client.id);
  });

  //handle ACK message and send user response via ACK callback
  client.on("ack-message", callback => {
    console.log("received ack-message, responding ack-response");
    if (callback) {
      callback("ack-response");
    }
  });

  //handle join emit and join user to a room
  const rooms = {};
  client.on("join", (data, callback) => {
    if (!rooms[data]) {
      rooms[data] = data;
      client.on(data, message => {
        console.log(`sending message to room ${data}`);
        io.to(data).emit(data, message);
      });
    }
    console.log("joining room", data);
    client.join(data, () => {
      io.to(data).emit(data, "welcome");
      callback("joined");
    });
  });

  //handle 'disconnect-me' message and send disconnect packet to user
  client.on("disconnect-me", () => {
    console.log(`disconnecting ${client.id} based on 'disconnect-me' emit.`);
    client.disconnect();
  });

  //handle 'emit-error' to simulate error packet to client
  client.on("emit-error", callback => {
    console.log(`emitting error-message error packet to client ${client.id}`);
    client.error({ message: "error-message" });
    callback("error ack");
  });
});

const namespace = io.of("some-namespace");
namespace.on("connection", client => {
  console.log(`client connected ${client.id}`);
});

setInterval(() => {
  io.emit("broadcast-message", "broadcast-message");
}, 25);

setInterval(() => {
  namespace.emit("namespace-message", "namespace-message");
}, 25);

server.listen(3000);
