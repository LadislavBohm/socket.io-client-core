const io = require("socket.io")();
io.on("connection", client => {
  console.log("client connected", client.id);
});

setInterval(() => {
  io.emit("broadcast", "broadcast message");
}, 25);

io.listen(3000);
