#include <iostream>
#include <conio.h>
#include <windows.h>
#include <cstdlib>
#include <ctime>
#include <fstream>
#include <string>

using namespace std;

const int WIDTH = 40;
const int HEIGHT = 20;
const int MAX_SNAKE_LENGTH = 100;

bool gameOver;
bool paused = false;
bool muted = false; // NEW - mute state
int score;
int highScore;
int level;
int speedDelay;
int snakeX[MAX_SNAKE_LENGTH];
int snakeY[MAX_SNAKE_LENGTH];
int snakeLength;
int fruitX;
int fruitY;

enum Direction { STOP = 0, LEFT, RIGHT, UP, DOWN };
Direction direction;

void moveCursor(int x, int y)
{
    COORD position;
    position.X = x;
    position.Y = y;
    SetConsoleCursorPosition(GetStdHandle(STD_OUTPUT_HANDLE), position);
}

void hideCursor()
{
    HANDLE consoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);
    CONSOLE_CURSOR_INFO cursorInfo;
    cursorInfo.dwSize = 100;
    cursorInfo.bVisible = false;
    SetConsoleCursorInfo(consoleHandle, &cursorInfo);
}

// ALL sounds check muted first
void playFruitSound() { if (!muted) Beep(1000, 30); }
void playGameOverSound() { if (muted) return; Beep(400, 150); Beep(250, 200); }
void playPauseSound() { if (!muted) Beep(600, 50); }
void playResumeSound() { if (!muted) Beep(800, 50); }

bool snakeOccupiesPosition(int x, int y)
{
    for (int i = 0; i < snakeLength; i++)
        if (snakeX[i] == x && snakeY[i] == y) return true;
    return false;
}

void createFruit()
{
    do {
        fruitX = rand() % WIDTH;
        fruitY = rand() % HEIGHT;
    } while (snakeOccupiesPosition(fruitX, fruitY));
}

void loadHighScore()
{
    ifstream file("highscore.txt");
    if (file.is_open()) { file >> highScore; file.close(); }
    else highScore = 0;
}

void saveHighScore()
{
    ofstream file("highscore.txt");
    if (file.is_open()) { file << highScore; file.close(); }
}

void setup()
{
    gameOver = false;
    paused = false;
    score = 0;
    level = 1;
    speedDelay = 200;
    direction = STOP;
    snakeLength = 1;
    snakeX[0] = WIDTH / 2;
    snakeY[0] = HEIGHT / 2;
    createFruit();
}

void draw()
{
    moveCursor(0, 0);
    string screen = "";
    for (int y = 0; y < HEIGHT + 2; y++)
    {
        for (int x = 0; x < WIDTH + 2; x++)
        {
            if (y == 0 || y == HEIGHT + 1 || x == 0 || x == WIDTH + 1) screen += "#";
            else if (x - 1 == fruitX && y - 1 == fruitY) screen += "F";
            else
            {
                bool found = false;
                for (int i = 0; i < snakeLength; i++)
                {
                    if (snakeX[i] == x - 1 && snakeY[i] == y - 1)
                    {
                        if (i == 0) screen += "O"; else screen += "o";
                        found = true; break;
                    }
                }
                if (!found) screen += " ";
            }
        }
        screen += "\n";
    }
    cout << screen;
    cout << endl;
    cout << "Score: " << score << endl;
    cout << "High Score: " << highScore << endl;
    cout << "Level: " << level << endl;
    cout << "Sound: " << (muted ? "MUTED (M to Unmute)" : "ON (M to Mute)") << endl;
    if (paused)
    {
        cout << endl;
        cout << "======== PAUSED ========" << endl;
        cout << "Press P to Resume" << endl;
    }
    else
    {
        cout << "Arrows to move. P = Pause. M = Mute. R = Replay. ESC = Quit." << endl;
    }
}

void input()
{
    if (_kbhit())
    {
        int key = _getch();
        if (key == 0 || key == 224)
        {
            key = _getch();
            if (paused) return;
            switch (key)
            {
                case 72: if (direction != DOWN) direction = UP; break;
                case 80: if (direction != UP) direction = DOWN; break;
                case 75: if (direction != RIGHT) direction = LEFT; break;
                case 77: if (direction != LEFT) direction = RIGHT; break;
            }
        }
        else if (key == 27) gameOver = true;
        else if (key == 'r' || key == 'R') setup();
        else if (key == 'p' || key == 'P' || key == ' ')
        {
            paused = !paused;
            if (paused) playPauseSound();
            else playResumeSound();
        }
        // MUTE - M key
        else if (key == 'm' || key == 'M')
        {
            muted = !muted;
        }
    }
}

void logic()
{
    if (paused) return;
    if (direction == STOP) return;
    for (int i = snakeLength - 1; i > 0; i--)
    {
        snakeX[i] = snakeX[i - 1];
        snakeY[i] = snakeY[i - 1];
    }
    switch (direction)
    {
        case LEFT: snakeX[0]--; break;
        case RIGHT: snakeX[0]++; break;
        case UP: snakeY[0]--; break;
        case DOWN: snakeY[0]++; break;
        case STOP: break;
    }
    if (snakeX[0] < 0 || snakeX[0] >= WIDTH || snakeY[0] < 0 || snakeY[0] >= HEIGHT) gameOver = true;
    for (int i = 1; i < snakeLength; i++)
        if (snakeX[0] == snakeX[i] && snakeY[0] == snakeY[i]) gameOver = true;
    if (snakeX[0] == fruitX && snakeY[0] == fruitY)
    {
        score += 10;
        playFruitSound();
        if (snakeLength < MAX_SNAKE_LENGTH) snakeLength++;
        if (score % 50 == 0)
        {
            level++;
            if (speedDelay > 140) speedDelay -= 5;
        }
        createFruit();
    }
}

int main()
{
    srand(static_cast<unsigned int>(time(0)));
    hideCursor();
    loadHighScore();
    bool playAgain = true;
    while (playAgain)
    {
        setup();
        while (!gameOver)
        {
            draw();
            input();
            logic();
            Sleep(speedDelay);
        }
        playGameOverSound();
        if (score > highScore) { highScore = score; saveHighScore(); }
        system("cls");
        cout << "==============================" << endl;
        cout << "          GAME OVER           " << endl;
        cout << "==============================" << endl;
        cout << endl;
        cout << "Your score: " << score << endl;
        cout << "High score: " << highScore << endl;
        cout << "Level reached: " << level << endl;
        cout << endl;
        if (score == highScore && score > 0) cout << "NEW HIGH SCORE!" << endl;
        cout << endl;
        cout << "Press R to Play Again" << endl;
        cout << "Press ESC to Quit" << endl;
        while (true)
        {
            int key = _getch();
            if (key == 'r' || key == 'R') { playAgain = true; break; }
            if (key == 27) { playAgain = false; break; }
        }
    }
    return 0;
}