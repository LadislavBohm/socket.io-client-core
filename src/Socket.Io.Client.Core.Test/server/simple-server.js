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
  client.join("some-room");
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
  io.to("some-room").emit("room-message", "room-message");
}, 25);

setInterval(() => {
  namespace.emit("namespace-message", "namespace-message");
}, 25);

server.listen(3000);
