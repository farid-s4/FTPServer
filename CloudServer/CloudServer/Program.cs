using CloudServer.Server;

var tcpChat = new TcpServer("192.168.0.100", 9999);

tcpChat.OnStartedListening += () => { Console.WriteLine("Started listening!"); };


await tcpChat.Start();