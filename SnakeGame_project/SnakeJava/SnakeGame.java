package snakegame;

import javax.swing.*;
import java.awt.*;
import java.awt.event.*;
import java.io.*;
import java.util.ArrayList;
import java.util.Random;
import javax.sound.sampled.*;

public class SnakeGame extends JFrame {
    static final int WIDTH = 40;
    static final int HEIGHT = 20;
    static final int CELL = 20;
    ArrayList<Point> snake = new ArrayList<>();
    Point fruit;
    String direction = "RIGHT";
    String nextDirection = "RIGHT";
    int score = 0, high = 0, level = 1, speed = 200;
    boolean paused = false, muted = false, gameOver = false;
    Timer timer;
    GamePanel panel;
    JLabel label, pausedLabel, gameOverLabel;

    public SnakeGame() {
        setTitle("Nokia Snake - Java - CENTRED");
        setResizable(false);
        setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        setLayout(new BorderLayout());

        JLabel info = new JLabel("Arrows to move | P = Pause | M = Mute | R = Restart | ESC = Quit | Hit Wall = GAME OVER", SwingConstants.CENTER);
        info.setFont(new Font("Arial", Font.BOLD, 14));
        add(info, BorderLayout.NORTH);

        panel = new GamePanel();
        panel.setPreferredSize(new Dimension(WIDTH * CELL, HEIGHT * CELL));
        panel.setLayout(new GridBagLayout());
        panel.setBackground(new Color(168, 208, 168));
        add(panel, BorderLayout.CENTER);

        pausedLabel = new JLabel("<html><center><font size='7'><b>PAUSED</b></font><br><font size='4'>Press P to Resume</font></center></html>", SwingConstants.CENTER);
        pausedLabel.setForeground(Color.WHITE);
        pausedLabel.setBackground(new Color(0, 0, 0, 160));
        pausedLabel.setOpaque(true);
        pausedLabel.setVisible(false);
        pausedLabel.setBorder(BorderFactory.createEmptyBorder(30, 80, 30, 80));
        panel.add(pausedLabel, new GridBagConstraints());

        gameOverLabel = new JLabel("<html><center><font size='7'><b>GAME OVER</b></font><br><font size='4'>Press R to Play Again</font></center></html>", SwingConstants.CENTER);
        gameOverLabel.setForeground(Color.WHITE);
        gameOverLabel.setBackground(new Color(0, 0, 0, 160));
        gameOverLabel.setOpaque(true);
        gameOverLabel.setVisible(false);
        gameOverLabel.setBorder(BorderFactory.createEmptyBorder(30, 80, 30, 80));
        panel.add(gameOverLabel, new GridBagConstraints());

        label = new JLabel("Loading...", SwingConstants.CENTER);
        label.setFont(new Font("Arial", Font.BOLD, 14));
        add(label, BorderLayout.SOUTH);

        loadHigh();
        setup();

        addKeyListener(new KeyAdapter() {
            @Override
            public void keyPressed(KeyEvent e) {
                int key = e.getKeyCode();
                if (key == KeyEvent.VK_UP && !"DOWN".equals(direction) && !"DOWN".equals(nextDirection)) nextDirection = "UP";
                else if (key == KeyEvent.VK_DOWN && !"UP".equals(direction) && !"UP".equals(nextDirection)) nextDirection = "DOWN";
                else if (key == KeyEvent.VK_LEFT && !"RIGHT".equals(direction) && !"RIGHT".equals(nextDirection)) nextDirection = "LEFT";
                else if (key == KeyEvent.VK_RIGHT && !"LEFT".equals(direction) && !"LEFT".equals(nextDirection)) nextDirection = "RIGHT";
                else if (key == KeyEvent.VK_ESCAPE) System.exit(0);
                else if (key == KeyEvent.VK_R) { setup(); restartTimer(); }
                else if (key == KeyEvent.VK_P || key == KeyEvent.VK_SPACE) {
                    if (!gameOver) {
                        paused = !paused;
                        pausedLabel.setVisible(paused);
                        if (!muted) playTone(paused ? 600 : 800, 50);
                    }
                }
                else if (key == KeyEvent.VK_M) muted = !muted;
                panel.repaint();
            }
        });

        setFocusable(true);
        pack();
        setLocationRelativeTo(null);
        setVisible(true);
        updateLabel();
        restartTimer();
    }

    void setup() {
        snake.clear();
        snake.add(new Point(WIDTH / 2, HEIGHT / 2));
        direction = "RIGHT";
        nextDirection = "RIGHT";
        score = 0; level = 1; speed = 200;
        paused = false; gameOver = false;
        pausedLabel.setVisible(false);
        gameOverLabel.setVisible(false);
        createFruit();
        updateLabel();
    }

    void restartTimer() {
        if (timer != null) timer.stop();
        timer = new Timer(speed, e -> gameLoop());
        timer.start();
    }

    void createFruit() {
        Random rand = new Random();
        while (true) {
            Point p = new Point(rand.nextInt(WIDTH), rand.nextInt(HEIGHT));
            if (!snake.contains(p)) { fruit = p; break; }
        }
    }

    void loadHigh() {
        try {
            File f = new File("highscore.txt");
            if (f.exists()) {
                BufferedReader br = new BufferedReader(new FileReader(f));
                high = Integer.parseInt(br.readLine().trim());
                br.close();
            }
        } catch (Exception e) { high = 0; }
    }

    void saveHigh() {
        try {
            FileWriter fw = new FileWriter("highscore.txt");
            fw.write(String.valueOf(high));
            fw.close();
        } catch (Exception e) {}
    }

    void playTone(int freq, int duration) {
        if (muted) return;
        new Thread(() -> {
            try {
                float sampleRate = 44100;
                byte[] buf = new byte[(int)(sampleRate * duration / 1000)];
                for (int i = 0; i < buf.length; i++) {
                    double angle = 2.0 * Math.PI * i * freq / sampleRate;
                    buf[i] = (byte)(Math.sin(angle) * 100);
                }
                AudioFormat format = new AudioFormat(sampleRate, 8, 1, true, false);
                SourceDataLine line = AudioSystem.getSourceDataLine(format);
                line.open(format);
                line.start();
                line.write(buf, 0, buf.length);
                line.drain();
                line.close();
            } catch (Exception e) {}
        }).start();
    }

    void updateLabel() {
        label.setText("Score: " + score + "  |  High: " + high + "  |  Level: " + level + "  |  Sound: " + (muted ? "MUTED" : "ON") + (gameOver ? "  |  GAME OVER - Press R" : ""));
    }

    void gameLoop() {
        if (paused || gameOver) { panel.repaint(); return; }
        direction = nextDirection;
        Point head = new Point(snake.get(0));
        if (direction.equals("UP")) head.y--;
        if (direction.equals("DOWN")) head.y++;
        if (direction.equals("LEFT")) head.x--;
        if (direction.equals("RIGHT")) head.x++;
        boolean hitWall = (head.x < 0 || head.x >= WIDTH || head.y < 0 || head.y >= HEIGHT);
        boolean hitSelf = snake.contains(head);
        if (hitWall || hitSelf) {
            gameOver = true;
            gameOverLabel.setText("<html><center><font size='7'><b>GAME OVER</b></font><br><font size='4'>Score: " + score + " | High: " + Math.max(high, score) + "<br>Press R to Play Again</font></center></html>");
            gameOverLabel.setVisible(true);
            playTone(400, 150);
            if (score > high) { high = score; saveHigh(); }
            updateLabel();
            panel.repaint();
            return;
        }
        boolean willEat = head.equals(fruit);
        snake.add(0, head);
        if (willEat) {
            score += 10;
            playTone(1000, 30);
            if (score > high) { high = score; saveHigh(); }
            if (score % 50 == 0) {
                level++;
                if (speed > 140) { speed -= 5; restartTimer(); }
            }
            createFruit();
        } else {
            snake.remove(snake.size() - 1);
        }
        updateLabel();
        panel.repaint();
    }

    class GamePanel extends JPanel {
        @Override
        protected void paintComponent(Graphics g) {
            super.paintComponent(g);
            Graphics2D g2 = (Graphics2D) g;
            g2.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
            g2.setColor(new Color(168, 208, 168));
            g2.fillRect(0, 0, WIDTH * CELL, HEIGHT * CELL);
            int fcx = fruit.x * CELL + 10;
            int fcy = fruit.y * CELL + 10;
            g2.setColor(new Color(139, 69, 19));
            g2.fillRect(fcx - 1, fcy - 10, 2, 6);
            g2.setColor(new Color(34, 139, 34));
            g2.fillOval(fcx + 1, fcy - 9, 8, 4);
            g2.setColor(new Color(220, 20, 60));
            g2.fillOval(fcx - 8, fcy - 5, 16, 16);
            g2.setColor(new Color(255, 255, 255, 90));
            g2.fillOval(fcx - 5, fcy - 3, 5, 5);
            if (snake.size() > 1) {
                g2.setColor(new Color(46, 158, 58));
                g2.setStroke(new BasicStroke(14, BasicStroke.CAP_ROUND, BasicStroke.JOIN_ROUND));
                for (int i = 0; i < snake.size() - 1; i++) {
                    Point a = snake.get(i);
                    Point b = snake.get(i + 1);
                    g2.drawLine(a.x * CELL + 10, a.y * CELL + 10, b.x * CELL + 10, b.y * CELL + 10);
                }
                g2.setColor(new Color(76, 175, 80));
                g2.setStroke(new BasicStroke(6, BasicStroke.CAP_ROUND, BasicStroke.JOIN_ROUND));
                for (int i = 0; i < snake.size() - 1; i++) {
                    Point a = snake.get(i);
                    Point b = snake.get(i + 1);
                    g2.drawLine(a.x * CELL + 10, a.y * CELL + 10, b.x * CELL + 10, b.y * CELL + 10);
                }
            }
            Point h = snake.get(0);
            int cx = h.x * CELL + 10;
            int cy = h.y * CELL + 10;
            g2.setColor(Color.RED);
            g2.setStroke(new BasicStroke(2));
            if (direction.equals("RIGHT")) {
                g2.drawLine(cx + 10, cy, cx + 18, cy - 3);
                g2.drawLine(cx + 10, cy, cx + 18, cy + 3);
            } else if (direction.equals("LEFT")) {
                g2.drawLine(cx - 10, cy, cx - 18, cy - 3);
                g2.drawLine(cx - 10, cy, cx - 18, cy + 3);
            } else if (direction.equals("UP")) {
                g2.drawLine(cx, cy - 10, cx - 3, cy - 18);
                g2.drawLine(cx, cy - 10, cx + 3, cy - 18);
            } else {
                g2.drawLine(cx, cy + 10, cx - 3, cy + 18);
                g2.drawLine(cx, cy + 10, cx + 3, cy + 18);
            }
            g2.setColor(new Color(46, 158, 58));
            g2.fillOval(cx - 10, cy - 10, 20, 20);
            g2.setColor(new Color(27, 94, 32));
            g2.setStroke(new BasicStroke(2));
            g2.drawOval(cx - 10, cy - 10, 20, 20);
            int ex1, ey1, ex2, ey2;
            if (direction.equals("RIGHT")) { ex1 = cx + 4; ey1 = cy - 5; ex2 = cx + 4; ey2 = cy + 5; }
            else if (direction.equals("LEFT")) { ex1 = cx - 4; ey1 = cy - 5; ex2 = cx - 4; ey2 = cy + 5; }
            else if (direction.equals("UP")) { ex1 = cx - 5; ey1 = cy - 4; ex2 = cx + 5; ey2 = cy - 4; }
            else { ex1 = cx - 5; ey1 = cy + 4; ex2 = cx + 5; ey2 = cy + 4; }
            g2.setColor(Color.WHITE);
            g2.fillOval(ex1 - 4, ey1 - 4, 8, 8);
            g2.fillOval(ex2 - 4, ey2 - 4, 8, 8);
            g2.setColor(Color.BLACK);
            g2.fillOval(ex1 - 2, ey1 - 2, 4, 4);
            g2.fillOval(ex2 - 2, ey2 - 2, 4, 4);
        }
    }

    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> new SnakeGame());
    }
}