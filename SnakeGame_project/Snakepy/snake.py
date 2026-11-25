import tkinter as tk
import random
import os
import threading

try:
    import winsound
    HAS_SOUND = True
except:
    HAS_SOUND = False

WIDTH = 40
HEIGHT = 20
CELL = 20
HIGH_FILE = "highscore.txt"

def load_high():
    if os.path.exists(HIGH_FILE):
        try:
            with open(HIGH_FILE) as f:
                return int(f.read().strip())
        except:
            return 0
    return 0

def save_high(s):
    with open(HIGH_FILE, "w") as f:
        f.write(str(s))

def play_sound(freq, duration):
    if not HAS_SOUND:
        return
    def _beep():
        try:
            winsound.Beep(freq, duration)
        except:
            pass
    threading.Thread(target=_beep, daemon=True).start()

class SnakeGame:
    def __init__(self, root):
        self.root = root
        self.root.title("Nokia Snake - Python - CLASSIC")
        self.root.resizable(False, False)
        self.high = load_high()
        self.next_direction = "RIGHT"
        self.setup()
        self.canvas = tk.Canvas(root, width=WIDTH*CELL, height=HEIGHT*CELL, bg="#a8d0a8")
        self.canvas.pack()
        self.label = tk.Label(root, text="", font=("Arial", 12, "bold"))
        self.label.pack()
        self.info = tk.Label(root, text="Arrows to move | P = Pause | M = Mute | R = Restart | ESC = Quit | Hit Wall = GAME OVER", font=("Arial", 10))
        self.info.pack()
        self.root.bind_all("<Key>", self.on_key)
        self.root.focus_set()
        self.draw()
        self.loop()

    def setup(self):
        self.snake = [[WIDTH//2, HEIGHT//2]]
        self.direction = "RIGHT"
        self.next_direction = "RIGHT"
        self.score = 0
        self.level = 1
        self.speed = 200
        self.paused = False
        self.muted = False
        self.game_over = False
        self.create_fruit()

    def create_fruit(self):
        free = [[x, y] for x in range(WIDTH) for y in range(HEIGHT) if [x, y] not in self.snake]
        if free:
            self.fruit = random.choice(free)
        else:
            self.fruit = [0, 0]

    def do_sound(self, freq, dur):
        if not self.muted:
            play_sound(freq, dur)

    def on_key(self, e):
        key = e.keysym
        if key == "Up" and self.direction != "DOWN" and self.next_direction != "DOWN":
            self.next_direction = "UP"
        elif key == "Down" and self.direction != "UP" and self.next_direction != "UP":
            self.next_direction = "DOWN"
        elif key == "Left" and self.direction != "RIGHT" and self.next_direction != "RIGHT":
            self.next_direction = "LEFT"
        elif key == "Right" and self.direction != "LEFT" and self.next_direction != "LEFT":
            self.next_direction = "RIGHT"
        elif key == "Escape":
            self.root.quit()
        elif key.lower() == "r":
            self.setup()
        elif key.lower() == "p" or key == "space":
            if not self.game_over:
                self.paused = not self.paused
                self.do_sound(600 if self.paused else 800, 50)
        elif key.lower() == "m":
            self.muted = not self.muted

    def draw(self):
        self.canvas.delete("all")
        fx, fy = self.fruit
        self.canvas.create_text(fx*CELL+10, fy*CELL+10, text="🍎", font=("Arial", 16))

        if len(self.snake) > 1:
            points = []
            for x, y in self.snake:
                points.append(x*CELL+10)
                points.append(y*CELL+10)
            # FIXED: jointype -> joinstyle (Python name)
            # This was the crash after eating 1 apple!
            self.canvas.create_line(points, fill="#2e9e3a", width=14, capstyle="round", joinstyle="round", smooth=False)
            self.canvas.create_line(points, fill="#4caf50", width=6, capstyle="round", joinstyle="round", smooth=False)

        hx, hy = self.snake[0]
        cx, cy = hx*CELL+10, hy*CELL+10
        self.canvas.create_oval(cx-10, cy-10, cx+10, cy+10, fill="#2e9e3a", outline="#1b5e20", width=2)

        if self.direction == "RIGHT":
            self.canvas.create_line(cx+10, cy, cx+18, cy-3, fill="red", width=2)
            self.canvas.create_line(cx+10, cy, cx+18, cy+3, fill="red", width=2)
            ex1, ey1, ex2, ey2 = cx+4, cy-5, cx+4, cy+5
        elif self.direction == "LEFT":
            self.canvas.create_line(cx-10, cy, cx-18, cy-3, fill="red", width=2)
            self.canvas.create_line(cx-10, cy, cx-18, cy+3, fill="red", width=2)
            ex1, ey1, ex2, ey2 = cx-4, cy-5, cx-4, cy+5
        elif self.direction == "UP":
            self.canvas.create_line(cx, cy-10, cx-3, cy-18, fill="red", width=2)
            self.canvas.create_line(cx, cy-10, cx+3, cy-18, fill="red", width=2)
            ex1, ey1, ex2, ey2 = cx-5, cy-4, cx+5, cy-4
        else:
            self.canvas.create_line(cx, cy+10, cx-3, cy+18, fill="red", width=2)
            self.canvas.create_line(cx, cy+10, cx+3, cy+18, fill="red", width=2)
            ex1, ey1, ex2, ey2 = cx-5, cy+4, cx+5, cy+4

        self.canvas.create_oval(ex1-4, ey1-4, ex1+4, ey1+4, fill="white", outline="")
        self.canvas.create_oval(ex2-4, ey2-4, ex2+4, ey2+4, fill="white", outline="")
        self.canvas.create_oval(ex1-2, ey1-2, ex1+2, ey1+2, fill="black", outline="")
        self.canvas.create_oval(ex2-2, ey2-2, ex2+2, ey2+2, fill="black", outline="")

        # PAUSED - CENTRED
        if self.paused and not self.game_over:
            self.canvas.create_rectangle(0, 0, WIDTH*CELL, HEIGHT*CELL, fill="black", stipple="gray50")
            self.canvas.create_text(WIDTH*CELL//2, HEIGHT*CELL//2, text="PAUSED", font=("Arial", 36, "bold"), fill="white")
            self.canvas.create_text(WIDTH*CELL//2, HEIGHT*CELL//2+35, text="Press P to Resume", font=("Arial", 16), fill="white")

        # GAME OVER - CENTRED
        if self.game_over:
            self.canvas.create_rectangle(0, 0, WIDTH*CELL, HEIGHT*CELL, fill="black", stipple="gray50")
            self.canvas.create_text(WIDTH*CELL//2, HEIGHT*CELL//2-20, text="GAME OVER", font=("Arial", 36, "bold"), fill="white")
            self.canvas.create_text(WIDTH*CELL//2, HEIGHT*CELL//2+15, text=f"Score: {self.score}  |  High: {self.high}", font=("Arial", 16), fill="white")
            self.canvas.create_text(WIDTH*CELL//2, HEIGHT*CELL//2+45, text="Press R to Play Again", font=("Arial", 16), fill="white")

        self.label.config(text=f"Score: {self.score}  |  High: {self.high}  |  Level: {self.level}  |  Sound: {'🔇 MUTED' if self.muted else '🔊 ON'}{'  |  GAME OVER - Press R' if self.game_over else ''}")

    def loop(self):
        try:
            if not self.paused and not self.game_over:
                self.direction = self.next_direction
                head = self.snake[0].copy()
                if self.direction == "UP": head[1] -= 1
                if self.direction == "DOWN": head[1] += 1
                if self.direction == "LEFT": head[0] -= 1
                if self.direction == "RIGHT": head[0] += 1

                # CLASSIC: HIT WALL = GAME OVER
                hit_wall = (head[0] < 0 or head[0] >= WIDTH or head[1] < 0 or head[1] >= HEIGHT)
                hit_self = head in self.snake

                if hit_wall or hit_self:
                    self.game_over = True
                    self.do_sound(400, 150)
                    if self.score > self.high:
                        self.high = self.score
                        save_high(self.high)
                else:
                    will_eat = (head == self.fruit)
                    self.snake.insert(0, head)
                    if will_eat:
                        self.score += 10
                        print("ATE APPLE! Score:", self.score)
                        self.do_sound(1000, 30)
                        if self.score > self.high:
                            self.high = self.score
                            save_high(self.high)
                        if self.score % 50 == 0:
                            self.level += 1
                            if self.speed > 140:
                                self.speed -= 5
                        self.create_fruit()
                    else:
                        self.snake.pop()
            self.draw()
        except Exception as e:
            print("ERROR:", e)
            import traceback
            traceback.print_exc()
        self.root.after(self.speed, self.loop)

if __name__ == "__main__":
    root = tk.Tk()
    game = SnakeGame(root)
    root.mainloop()