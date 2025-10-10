using CloudServer;

var tcpChat = new TcpServer("192.168.0.101", 9999);

tcpChat.OnClientConnected += () => { Console.WriteLine($"Client connected!"); };

tcpChat.OnClientDisconnected += () => { Console.WriteLine("Client disconnected!"); };

tcpChat.OnStartedListening += () => { Console.WriteLine("Started listening!"); };

tcpChat.OnClientRegistered += () => { Console.WriteLine("Client registered!"); };

await tcpChat.Start();