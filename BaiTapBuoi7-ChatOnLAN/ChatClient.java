import java.io.IOException;
import java.net.InetSocketAddress;
import java.net.Socket;

public class ChatClient {
    public static void main(String[] args) {
        String serverAddress = args.length > 0 ? args[0] : "127.0.0.1";

        try (Socket socket = new Socket()) {
            socket.connect(new InetSocketAddress(serverAddress, ChatSession.PORT), 5000);
            ChatSession.run(socket, "Client", "Server", true);
        } catch (IOException error) {
            ChatSession.showError("Client", error);
        }
    }
}
