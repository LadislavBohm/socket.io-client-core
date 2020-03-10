const io = require("socket.io")();
io.on("connection", client => {
  console.log("client connected", client.id);

  client.emit("m", 123);
});
io.listen(3000);
