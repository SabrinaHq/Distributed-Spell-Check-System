# PROJECT FOR CSE 5306: Distributed Systems  
**Spring 2021**

## Description
This is a simple client-server application (desktop) programmed in **C#**. The client-server data transfer is implemented using **sockets**, and threading is used to handle multiple clients.

## Features
- **Client-Server Communication**: Socket-based data transfer.
- **Threading**: Supports multiple clients connecting to the server.
- **Spelling Check**: Clients can upload text files for spelling checks.
- **Dynamic Lexicon Management**: 
  - Clients can add new words to their queue.
  - The server polls client queues every 60 seconds to update the lexicon.
- **Backup Server Support**:
  - Primary server connects to a backup server.
  - Lexicon synchronization between servers.

## Requirements
- **Programming Language**: C#
- **IDE**: Visual Studio 2019 (preferably)

## How to Run
1. **Server Setup**:
   - Run the **backup server** first.
   - Start the **primary server**, which will automatically connect to the backup server and sync the lexicon.
   - Select a folder containing the `lexicon.txt` file in the server application.

2. **Client Setup**:
   - Run the client application.
   - Input a unique username to connect to the server. If the username is already in use, retry with a different username.
   - Select an input `.txt` file using the **Browse** button.
   - Send the file to the server for a spelling check.
   - Add new words to the queue using the **Enter** button in the client GUI.
   - Disconnect using the **Disconnect** button when finished.

3. **Shutdown**:
   - After all clients disconnect, the primary server can be disconnected.

## Project Files
- Open the `ServerGUI.sln` solution file in Visual Studio.
- Both the **Client** and **Server** projects are included.
- The executable files (`.exe`) have been removed for safety.

## References
- **Socket Data Transfer**: [YouTube Video](https://www.youtube.com/watch?v=651yVDINPBY&list=PLAC179D21AF94D28F&index=5)
- **Threading**: [Technotif Guide](https://technotif.com/creating-simple-tcpip-server-client-transfer-data-using-c-vb-net/)
- **Socket SendFile API**: [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.sendfile?view=net-5.0)
- **Open File Dialog**: [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.openfiledialog?view=net-5.0)
- **Spelling Check**: [StackOverflow Discussion](https://stackoverflow.com/questions/38416265/c-sharp-checking-if-a-word-is-in-an-english-dictionary)
- **Data Transfer**: [StackOverflow Discussion](https://stackoverflow.com/questions/7906300/sending-multiple-type-of-data-from-a-single-network-stream-in-c-sharp)
- **Timer Function**: [StackOverflow Discussion](https://stackoverflow.com/questions/6169288/execute-specified-function-every-x-seconds)


