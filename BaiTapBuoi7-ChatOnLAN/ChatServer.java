import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;

public class ChatServer {
    public static void main(String[] args) {
        try (ServerSocket server = new ServerSocket(ChatSession.PORT)) {
            System.out.println("Đang chờ Client kết nối qua TCP cổng " + ChatSession.PORT + "...");

            try (Socket socket = server.accept()) {
                ChatSession.run(socket, "Server", "Client", false);
            }
        } catch (IOException error) {
            ChatSession.showError("Server", error);
        }
    }
}
