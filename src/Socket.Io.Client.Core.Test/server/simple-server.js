const server = require("http").createServer();
const io = require("socket.io")(server, {
  path: "/",
  serveClient: false,
  pingInterval: 50,
  pingTimeout: 400,
  cookie: false
});

io.on("connection", client => {
  console.log("client connected", client.id);
  console.log("query", client.handshake.query);
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

  client.on("disconnect", () => {
    console.log("client disconnected", client.id);
  });
  client.on("ack-message", callback => {
    console.log("received ack-message, responding ack-response");
    if (callback) {
      callback("ack-response");
    }
  });
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
