using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NokiaSnake
{
    // The four directions the snake can move in.
    // An enum is the C# way to name a fixed set of options (Java used strings here).
    public enum Direction { Up, Down, Left, Right }

    // A normal Panel flickers when redrawn often. Turning on DoubleBuffered fixes that.
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            DoubleBuffered = true;
        }
    }

    // The game window. "Form" is the C# (Windows Forms) version of Java's JFrame.
    public class SnakeGame : Form
    {
        // Board size: 40 x 20 cells, each cell 20 pixels.
        const int BoardWidth = 40;
        const int BoardHeight = 20;
        const int CellSize = 20;
        const int Half = CellSize / 2;
        const string HighScoreFile = "highscore.txt";

        // Game state
        List<Point> snake = new List<Point>();
        Point fruit;
        Direction direction = Direction.Right;
        Direction nextDirection = Direction.Right;
        int score = 0, high = 0, level = 1, speed = 200;
        bool paused = false, muted = false, gameOver = false;
        Random rand = new Random();

        // Window parts (Timer here is the Windows Forms timer: it fires a Tick event every "Interval" ms)
        Timer timer;
        BufferedPanel panel;
        Label infoLabel, statusLabel;

        public SnakeGame()
        {
            Text = "Nokia Snake - C#";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // Game board
            panel = new BufferedPanel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.FromArgb(168, 208, 168);
            panel.Paint += Panel_Paint;

            // Help text at the top
            infoLabel = new Label();
            infoLabel.Text = "Arrows to move | P = Pause | M = Mute | R = Restart | ESC = Quit | Hit Wall = GAME OVER";
            infoLabel.Dock = DockStyle.Top;
            infoLabel.Height = 28;
            infoLabel.TextAlign = ContentAlignment.MiddleCenter;
            infoLabel.Font = new Font("Arial", 10, FontStyle.Bold);

            // Score text at the bottom
            statusLabel = new Label();
            statusLabel.Dock = DockStyle.Bottom;
            statusLabel.Height = 28;
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            statusLabel.Font = new Font("Arial", 10, FontStyle.Bold);

            Controls.Add(panel);
            Controls.Add(infoLabel);
            Controls.Add(statusLabel);
            ClientSize = new Size(BoardWidth * CellSize, BoardHeight * CellSize + 56);

            LoadHigh();
            Setup();

            // The game loop: every time the timer ticks, the snake moves one step.
            timer = new Timer();
            timer.Interval = speed;
            timer.Tick += (sender, e) => GameLoop();
            timer.Start();
        }

        // Resets everything for a new game.
        void Setup()
        {
            snake.Clear();
            snake.Add(new Point(BoardWidth / 2, BoardHeight / 2));
            direction = Direction.Right;
            nextDirection = Direction.Right;
            score = 0;
            level = 1;
            speed = 200;
            paused = false;
            gameOver = false;
            if (timer != null) timer.Interval = speed;
            CreateFruit();
            UpdateLabel();
        }

        // Puts the fruit on a random cell that the snake is not on.
        void CreateFruit()
        {
            while (true)
            {
                Point p = new Point(rand.Next(BoardWidth), rand.Next(BoardHeight));
                if (!snake.Contains(p))
                {
                    fruit = p;
                    break;
                }
            }
        }

        void LoadHigh()
        {
            try
            {
                if (File.Exists(HighScoreFile))
                    high = int.Parse(File.ReadAllText(HighScoreFile).Trim());
            }
            catch
            {
                high = 0;
            }
        }

        void SaveHigh()
        {
            try
            {
                File.WriteAllText(HighScoreFile, high.ToString());
            }
            catch
            {
                // If the file can't be saved, just carry on playing.
            }
        }

        // Plays a beep on a background task so the game doesn't freeze while it sounds.
        void PlayTone(int frequency, int duration)
        {
            if (muted) return;
            Task.Run(() =>
            {
                try { Console.Beep(frequency, duration); }
                catch { }
            });
        }

        void UpdateLabel()
        {
            statusLabel.Text = "Score: " + score + "  |  High: " + high + "  |  Level: " + level
                + "  |  Sound: " + (muted ? "MUTED" : "ON")
                + (gameOver ? "  |  GAME OVER - Press R" : "");
        }

        // Runs on every timer tick: moves the snake and checks what happened.
        void GameLoop()
        {
            if (paused || gameOver)
            {
                panel.Invalidate();
                return;
            }

            direction = nextDirection;

            // Work out where the head will be after this step.
            Point head = snake[0];
            if (direction == Direction.Up) head.Y--;
            if (direction == Direction.Down) head.Y++;
            if (direction == Direction.Left) head.X--;
            if (direction == Direction.Right) head.X++;

            bool hitWall = head.X < 0 || head.X >= BoardWidth || head.Y < 0 || head.Y >= BoardHeight;
            bool hitSelf = snake.Contains(head);

            if (hitWall || hitSelf)
            {
                gameOver = true;
                PlayTone(400, 150);
                if (score > high)
                {
                    high = score;
                    SaveHigh();
                }
                UpdateLabel();
                panel.Invalidate();
                return;
            }

            bool willEat = head == fruit;
            snake.Insert(0, head); // add the new head at the front

            if (willEat)
            {
                score += 10;
                PlayTone(1000, 30);
                if (score > high)
                {
                    high = score;
                    SaveHigh();
                }
                // Level up every 50 points and speed up until the limit of 140 ms.
                if (score % 50 == 0)
                {
                    level++;
                    if (speed > 140)
                    {
                        speed -= 5;
                        timer.Interval = speed;
                    }
                }
                CreateFruit();
            }
            else
            {
                snake.RemoveAt(snake.Count - 1); // not eating: remove the tail so the length stays the same
            }

            UpdateLabel();
            panel.Invalidate(); // ask Windows to redraw the board
        }

        // Handles the keyboard. ProcessCmdKey also catches the arrow keys,
        // which Windows would otherwise use to move focus between controls.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (HandleKey(keyData)) return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Returns true if the key was one the game uses.
        bool HandleKey(Keys key)
        {
            switch (key)
            {
                // The snake cannot turn straight back into itself.
                case Keys.Up:
                    if (direction != Direction.Down && nextDirection != Direction.Down) nextDirection = Direction.Up;
                    break;
                case Keys.Down:
                    if (direction != Direction.Up && nextDirection != Direction.Up) nextDirection = Direction.Down;
                    break;
                case Keys.Left:
                    if (direction != Direction.Right && nextDirection != Direction.Right) nextDirection = Direction.Left;
                    break;
                case Keys.Right:
                    if (direction != Direction.Left && nextDirection != Direction.Left) nextDirection = Direction.Right;
                    break;
                case Keys.Escape:
                    Close();
                    return true;
                case Keys.R:
                    Setup();
                    break;
                case Keys.P:
                case Keys.Space:
                    if (!gameOver)
                    {
                        paused = !paused;
                        PlayTone(paused ? 600 : 800, 50);
                    }
                    break;
                case Keys.M:
                    muted = !muted;
                    UpdateLabel();
                    break;
                default:
                    return false;
            }

            panel.Invalidate();
            return true;
        }

        // ----- Drawing -----

        void Panel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(168, 208, 168));

            DrawFruit(g);
            DrawSnake(g);

            if (gameOver)
                DrawOverlay(g, "GAME OVER", "Score: " + score + " | High: " + Math.Max(high, score), "Press R to Play Again");
            else if (paused)
                DrawOverlay(g, "PAUSED", "Press P to Resume");
        }

        void FillOval(Graphics g, Color color, int x, int y, int w, int h)
        {
            using (SolidBrush brush = new SolidBrush(color))
                g.FillEllipse(brush, x, y, w, h);
        }

        void FillRect(Graphics g, Color color, int x, int y, int w, int h)
        {
            using (SolidBrush brush = new SolidBrush(color))
                g.FillRectangle(brush, x, y, w, h);
        }

        Pen RoundPen(Color color, float width)
        {
            Pen pen = new Pen(color, width);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            pen.LineJoin = LineJoin.Round;
            return pen;
        }

        void DrawFruit(Graphics g)
        {
            int fx = fruit.X * CellSize + Half;
            int fy = fruit.Y * CellSize + Half;
            FillRect(g, Color.FromArgb(139, 69, 19), fx - 1, fy - 10, 2, 6);     // stem
            FillOval(g, Color.FromArgb(34, 139, 34), fx + 1, fy - 9, 8, 4);      // leaf
            FillOval(g, Color.FromArgb(220, 20, 60), fx - 8, fy - 5, 16, 16);    // apple
            FillOval(g, Color.FromArgb(90, 255, 255, 255), fx - 5, fy - 3, 5, 5); // shine
        }

        void DrawSnake(Graphics g)
        {
            // Body: a wide dark line with a thinner light line on top.
            if (snake.Count > 1)
            {
                using (Pen wide = RoundPen(Color.FromArgb(46, 158, 58), 14))
                using (Pen thin = RoundPen(Color.FromArgb(76, 175, 80), 6))
                {
                    for (int i = 0; i < snake.Count - 1; i++)
                    {
                        Point a = snake[i];
                        Point b = snake[i + 1];
                        g.DrawLine(wide, a.X * CellSize + Half, a.Y * CellSize + Half, b.X * CellSize + Half, b.Y * CellSize + Half);
                    }
                    for (int i = 0; i < snake.Count - 1; i++)
                    {
                        Point a = snake[i];
                        Point b = snake[i + 1];
                        g.DrawLine(thin, a.X * CellSize + Half, a.Y * CellSize + Half, b.X * CellSize + Half, b.Y * CellSize + Half);
                    }
                }
            }

            // Head
            Point h = snake[0];
            int cx = h.X * CellSize + Half;
            int cy = h.Y * CellSize + Half;

            // Tongue (red lines in front of the head)
            using (Pen tongue = new Pen(Color.Red, 2))
            {
                if (direction == Direction.Right)
                {
                    g.DrawLine(tongue, cx + 10, cy, cx + 18, cy - 3);
                    g.DrawLine(tongue, cx + 10, cy, cx + 18, cy + 3);
                }
                else if (direction == Direction.Left)
                {
                    g.DrawLine(tongue, cx - 10, cy, cx - 18, cy - 3);
                    g.DrawLine(tongue, cx - 10, cy, cx - 18, cy + 3);
                }
                else if (direction == Direction.Up)
                {
                    g.DrawLine(tongue, cx, cy - 10, cx - 3, cy - 18);
                    g.DrawLine(tongue, cx, cy - 10, cx + 3, cy - 18);
                }
                else
                {
                    g.DrawLine(tongue, cx, cy + 10, cx - 3, cy + 18);
                    g.DrawLine(tongue, cx, cy + 10, cx + 3, cy + 18);
                }
            }

            FillOval(g, Color.FromArgb(46, 158, 58), cx - 10, cy - 10, 20, 20);
            using (Pen outline = new Pen(Color.FromArgb(27, 94, 32), 2))
                g.DrawEllipse(outline, cx - 10, cy - 10, 20, 20);

            // Eyes: their position depends on the direction
            int ex1, ey1, ex2, ey2;
            if (direction == Direction.Right) { ex1 = cx + 4; ey1 = cy - 5; ex2 = cx + 4; ey2 = cy + 5; }
            else if (direction == Direction.Left) { ex1 = cx - 4; ey1 = cy - 5; ex2 = cx - 4; ey2 = cy + 5; }
            else if (direction == Direction.Up) { ex1 = cx - 5; ey1 = cy - 4; ex2 = cx + 5; ey2 = cy - 4; }
            else { ex1 = cx - 5; ey1 = cy + 4; ex2 = cx + 5; ey2 = cy + 4; }

            FillOval(g, Color.White, ex1 - 4, ey1 - 4, 8, 8);
            FillOval(g, Color.White, ex2 - 4, ey2 - 4, 8, 8);
            FillOval(g, Color.Black, ex1 - 2, ey1 - 2, 4, 4);
            FillOval(g, Color.Black, ex2 - 2, ey2 - 2, 4, 4);
        }

        // Draws the PAUSED / GAME OVER box in the middle of the board.
        void DrawOverlay(Graphics g, string title, params string[] lines)
        {
            int w = 460;
            int h = 80 + lines.Length * 30 + 15;
            int x = (panel.ClientSize.Width - w) / 2;
            int y = (panel.ClientSize.Height - h) / 2;

            FillRect(g, Color.FromArgb(160, 0, 0, 0), x, y, w, h);

            using (StringFormat center = new StringFormat())
            using (Font titleFont = new Font("Arial", 32, FontStyle.Bold))
            using (Font lineFont = new Font("Arial", 14, FontStyle.Regular))
            {
                center.Alignment = StringAlignment.Center;
                center.LineAlignment = StringAlignment.Center;

                g.DrawString(title, titleFont, Brushes.White, new RectangleF(x, y + 15, w, 55), center);
                for (int i = 0; i < lines.Length; i++)
                    g.DrawString(lines[i], lineFont, Brushes.White, new RectangleF(x, y + 80 + i * 30, w, 30), center);
            }
        }

        // The program starts here (like main() in Java).
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SnakeGame());
        }
    }
}
