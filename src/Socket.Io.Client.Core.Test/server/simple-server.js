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
});

setInterval(() => {
  io.emit("broadcast", "broadcast message");
}, 25);

server.listen(3000);
