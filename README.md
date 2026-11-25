# Nokia Snake - C++, Java, Python and Web

A classic Nokia-style Snake game built five times in five languages: a Windows console game in C++, desktop games in Java (Swing), C# (Windows Forms) and Python (Tkinter), plus a browser version in HTML, CSS and JavaScript. All five share the same rules, board size and scoring, so you can compare how the same program looks in each language.

## Screenshots

| C++ (Console) | Java (Swing) |
| :---: | :---: |
| ![C++ version](screenshots/cpp.png) | ![Java version](screenshots/java.png) |

| Python (Tkinter) | Web (Browser) |
| :---: | :---: |
| ![Python version](screenshots/python.png) | ![Web version](screenshots/web.png) |

## Game Rules

- The board is 40 x 20 cells.
- Eat the fruit to grow the snake and earn 10 points.
- Every 50 points you move up a level and the snake speeds up (it starts at 200 ms per move and stops getting faster at 140 ms).
- The game ends if you hit a wall or run into your own body.
- You cannot reverse straight into yourself.
- Your high score is saved between sessions.

## Controls

| Action | Desktop versions | Web version |
| --- | --- | --- |
| Move | Arrow keys | Arrow keys, on-screen D-pad, or swipe |
| Pause / Resume | P or Space | P or Space, or the Pause button |
| Mute | M | M or the Mute button |
| Restart | R | R or the Restart button |
| Quit | ESC | Close the tab |

## Features

- Five complete versions of the same game
- Level system with increasing speed
- Persistent high score
- Sound effects with a mute toggle
- Pause screen and game over screen with the final score
- Responsive web version: the canvas resizes to the screen, with touch swipe, an on-screen D-pad for phones, and Web Audio sound effects

## Project Structure

```
SnakeGame_project/
├── Snakecpp/
│   ├── snake.cpp          # C++ console version (Windows)
│   ├── snake.exe          # Prebuilt Windows executable
│   └── highscore.txt
├── SnakeJava/
│   └── SnakeGame.java     # Java Swing version
├── SnakeCSharp/
│   ├── SnakeGame.cs       # C# Windows Forms version
│   └── SnakeGame.csproj   # C# project file
├── Snakepy/
│   └── snake.py           # Python Tkinter version
├── SnakeWeb/
│   ├── Snake.html
│   ├── Snake.css
│   └── Snake.js           # Browser version
├── screenshots/
└── highscore.txt
```

## How to Run

### C++ (Windows only)

The console version uses `conio.h` and `windows.h`, so it needs Windows.

```
cd Snakecpp
g++ snake.cpp -o snake.exe
snake.exe
```

You can also run the prebuilt `snake.exe` directly. Run it from the `Snakecpp` folder so it finds `highscore.txt`.

### Java

Requires a JDK (Java 8 or newer).

```
cd SnakeJava
javac SnakeGame.java
java SnakeGame
```

### C# (Windows only)

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download). The game uses Windows Forms, so it runs on Windows.

```
cd SnakeCSharp
dotnet run
```

You can also open `SnakeGame.csproj` in Visual Studio and press F5. Run from the `SnakeCSharp` folder so the high score is saved there.

### Python

Requires Python 3 with Tkinter, which comes with the standard Python installer. Sound uses `winsound` (Windows only); on other systems the game still runs, just without sound.

```
cd Snakepy
python snake.py
```

### Web

No build step or dependencies. Open `SnakeWeb/Snake.html` in any modern browser, or serve the folder with a local server such as XAMPP or the VS Code Live Server extension.

## High Scores

- The C++, Java, C# and Python versions save the high score to a `highscore.txt` file in the folder you run the game from.
- The web version saves it in the browser's `localStorage`, so it is stored per browser and per device.

## Tech Stack

| Version | Language | Library / API |
| --- | --- | --- |
| Console | C++ | Windows console API (`conio.h`, `windows.h`) |
| Desktop | Java | Swing, AWT, `javax.sound.sampled` |
| Desktop | C# | .NET 8, Windows Forms, GDI+ (`System.Drawing`) |
| Desktop | Python | Tkinter, `winsound` |
| Browser | JavaScript | HTML5 Canvas, Web Audio API |

## Possible Improvements

- Wall wrapping mode
- Difficulty selection
- Cross-platform sound for the C++ and Python versions
- Online leaderboard for the web version

## Author

Built by Destin.

- GitHub: [magumodestin](https://github.com/magumodestin)
- Email: magumodestiny@gmail.com
