const server = require("http").createServer();
const io = require("socket.io")(server, {
  path: "/",
  serveClient: false,
  pingInterval: 50,
  pingTimeout: 200,
  cookie: false
});

io.on("connection", client => {
  console.log("client connected", client.id);
  client.join("some-room");
  client.on("disconnect", () => {
    console.log("client disconnected", client.id);
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
