import java.awt.BasicStroke;
import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Component;
import java.awt.Dimension;
import java.awt.Font;
import java.awt.FontMetrics;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.Insets;
import java.awt.RenderingHints;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.lang.reflect.InvocationTargetException;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.concurrent.atomic.AtomicReference;
import javax.swing.BorderFactory;
import javax.swing.Icon;
import javax.swing.JDialog;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import javax.swing.JTextField;
import javax.swing.SwingUtilities;
import javax.swing.UIManager;

final class ChatSession {
    static final int PORT = 5000;
    private static final Font DIALOG_FONT = new Font(Font.SERIF, Font.BOLD, 15);
    private static final Icon QUESTION_ICON = new DialogIcon("?", false);
    private static final Icon INFORMATION_ICON = new DialogIcon("i", true);

    static {
        UIManager.put("OptionPane.background", Color.WHITE);
        UIManager.put("Panel.background", Color.WHITE);
        UIManager.put("OptionPane.minimumSize", new Dimension(460, 146));
        UIManager.put("OptionPane.border", BorderFactory.createEmptyBorder(20, 20, 12, 20));
        UIManager.put("OptionPane.messageAreaBorder", BorderFactory.createEmptyBorder(0, 0, 12, 0));
        UIManager.put("OptionPane.buttonFont", DIALOG_FONT);
        UIManager.put("OptionPane.buttonMinimumWidth", 92);
        UIManager.put("OptionPane.buttonPadding", 24);
    }

    private ChatSession() {
    }

    static void run(Socket socket, String localName, String remoteName, boolean sendsFirst)
            throws IOException {
        BufferedReader input = new BufferedReader(
                new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
        BufferedWriter output = new BufferedWriter(
                new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8));

        if (sendsFirst && !send(output, localName)) {
            return;
        }

        String message;
        while ((message = input.readLine()) != null) {
            showMessage(localName, message);
            if (!send(output, localName)) {
                return;
            }
        }

        System.out.println(remoteName + " đã kết thúc cuộc trò chuyện.");
    }

    private static boolean send(BufferedWriter output, String localName) throws IOException {
        AtomicReference<String> message = new AtomicReference<>();
        onSwingThread(() -> {
            JTextField field = new JTextField(30);
            field.setMargin(new Insets(4, 6, 4, 6));
            JLabel prompt = new JLabel(localName + " nhập:");
            prompt.setFont(DIALOG_FONT);
            prompt.setLabelFor(field);

            JPanel form = new JPanel(new BorderLayout(0, 5));
            form.add(prompt, BorderLayout.NORTH);
            form.add(field, BorderLayout.CENTER);

            JOptionPane pane = new JOptionPane(form, JOptionPane.QUESTION_MESSAGE,
                    JOptionPane.OK_CANCEL_OPTION, QUESTION_ICON);
            field.addActionListener(event -> pane.setValue(JOptionPane.OK_OPTION));
            showDialog(pane, localName, field);
            if (Integer.valueOf(JOptionPane.OK_OPTION).equals(pane.getValue())) {
                message.set(field.getText());
            }
        });

        String text = message.get();
        if (text == null) {
            // Cancel hoặc đóng hộp thoại. Hàm main sẽ đóng socket để báo EOF cho bên kia.
            return false;
        }

        // Mỗi tin nhắn chiếm một dòng trong luồng TCP.
        output.write(text.replace('\r', ' ').replace('\n', ' '));
        output.newLine();
        output.flush();
        return true;
    }

    private static void showMessage(String localName, String message) {
        onSwingThread(() -> {
            JTextArea text = new JTextArea(message, 2, 30);
            text.setFont(DIALOG_FONT);
            text.setMargin(new Insets(10, 0, 0, 0));
            text.setEditable(false);
            text.setLineWrap(true);
            text.setWrapStyleWord(true);
            text.setCaretPosition(0);
            text.getAccessibleContext().setAccessibleName("Tin nhắn nhận được");

            JScrollPane content = new JScrollPane(text, JScrollPane.VERTICAL_SCROLLBAR_AS_NEEDED,
                    JScrollPane.HORIZONTAL_SCROLLBAR_NEVER);
            content.setBorder(BorderFactory.createEmptyBorder());
            showDialog(new JOptionPane(content, JOptionPane.INFORMATION_MESSAGE,
                    JOptionPane.DEFAULT_OPTION, INFORMATION_ICON), localName, null);
        });
    }

    static void showError(String localName, IOException error) {
        onSwingThread(() -> showDialog(new JOptionPane("Lỗi kết nối TCP: " + error.getMessage(),
                JOptionPane.ERROR_MESSAGE), localName, null));
    }

    private static void showDialog(JOptionPane pane, String localName, JTextField field) {
        JDialog dialog = pane.createDialog(null, localName);
        if (field != null) {
            dialog.addWindowListener(new WindowAdapter() {
                @Override
                public void windowOpened(WindowEvent event) {
                    field.requestFocusInWindow();
                }
            });
        }
        try {
            // Hai tiến trình thay phiên hiện hộp thoại. Giữ hộp thoại đang mở phía trước.
            dialog.setAlwaysOnTop(true);
            dialog.setVisible(true);
        } finally {
            dialog.dispose();
        }
    }

    private static final class DialogIcon implements Icon {
        private static final int SIZE = 44;
        private final String mark;
        private final boolean circle;

        private DialogIcon(String mark, boolean circle) {
            this.mark = mark;
            this.circle = circle;
        }

        @Override
        public int getIconWidth() {
            return SIZE;
        }

        @Override
        public int getIconHeight() {
            return SIZE;
        }

        @Override
        public void paintIcon(Component component, Graphics graphics, int x, int y) {
            Graphics2D icon = (Graphics2D) graphics.create();
            try {
                icon.translate(x, y);
                icon.setColor(Color.BLACK);
                icon.setStroke(new BasicStroke(2));
                icon.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
                if (circle) {
                    icon.drawOval(1, 1, SIZE - 3, SIZE - 3);
                } else {
                    icon.drawRect(1, 1, SIZE - 3, SIZE - 3);
                }
                icon.setFont(DIALOG_FONT);
                FontMetrics metrics = icon.getFontMetrics();
                icon.drawString(mark, (SIZE - metrics.stringWidth(mark)) / 2,
                        (SIZE - metrics.getHeight()) / 2 + metrics.getAscent());
            } finally {
                icon.dispose();
            }
        }
    }

    private static void onSwingThread(Runnable action) {
        try {
            SwingUtilities.invokeAndWait(action);
        } catch (InterruptedException error) {
            Thread.currentThread().interrupt();
            throw new IllegalStateException("Luồng trò chuyện bị gián đoạn.", error);
        } catch (InvocationTargetException error) {
            throw new IllegalStateException("Không thể hiển thị hộp thoại.", error.getCause());
        }
    }
}
