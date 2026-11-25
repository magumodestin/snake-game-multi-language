const WIDTH = 40,
  HEIGHT = 20;
let CELL = 20;
const canvas = document.getElementById("game");
const ctx = canvas.getContext("2d");
const startScreen = document.getElementById("startScreen");
let snake,
  direction,
  nextDirection,
  score,
  high,
  level,
  speed,
  paused,
  muted,
  gameOver,
  fruit;
let started = false;
let audioCtx = null;

function resizeCanvas() {
  const cssWidth = canvas.clientWidth;
  const cssHeight = cssWidth / 2;
  const dpr = window.devicePixelRatio || 1;
  canvas.width = cssWidth * dpr;
  canvas.height = cssHeight * dpr;
  ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  CELL = cssWidth / WIDTH;
  draw();
}
window.addEventListener("resize", resizeCanvas);
window.addEventListener("orientationchange", () =>
  setTimeout(resizeCanvas, 200),
);

function makeFruit() {
  while (true) {
    let p = {
      x: Math.floor(Math.random() * WIDTH),
      y: Math.floor(Math.random() * HEIGHT),
    };
    if (!snake.some((s) => s.x === p.x && s.y === p.y)) return p;
  }
}
function beep(freq, dur) {
  if (muted) return;
  try {
    if (!audioCtx)
      audioCtx = new (window.AudioContext || window.webkitAudioContext)();
    let o = audioCtx.createOscillator(),
      g = audioCtx.createGain();
    o.connect(g);
    g.connect(audioCtx.destination);
    o.frequency.value = freq;
    g.gain.value = 0.1;
    o.start();
    o.stop(audioCtx.currentTime + dur / 1000);
  } catch (e) {}
}
function startGame() {
  if (gameOver) restart();
  started = true;
  paused = false;
  startScreen.style.display = "none";
  beep(800, 80);
}
function togglePause() {
  if (!started) {
    startGame();
    return;
  }
  if (gameOver) return;
  paused = !paused;
  beep(paused ? 600 : 800, 50);
}
function toggleMute() {
  muted = !muted;
}
function setDir(d) {
  if (d === "UP" && direction !== "DOWN" && nextDirection !== "DOWN")
    nextDirection = "UP";
  if (d === "DOWN" && direction !== "UP" && nextDirection !== "UP")
    nextDirection = "DOWN";
  if (d === "LEFT" && direction !== "RIGHT" && nextDirection !== "RIGHT")
    nextDirection = "LEFT";
  if (d === "RIGHT" && direction !== "LEFT" && nextDirection !== "LEFT")
    nextDirection = "RIGHT";
}
function restart() {
  snake = [{ x: 20, y: 10 }];
  direction = "RIGHT";
  nextDirection = "RIGHT";
  score = 0;
  level = 1;
  speed = 200;
  paused = true;
  gameOver = false;
  started = false;
  fruit = makeFruit();
  startScreen.style.display = "flex";
  draw();
}
function loadHigh() {
  high = parseInt(localStorage.getItem("high") || "0");
}
loadHigh();
snake = [{ x: 20, y: 10 }];
direction = "RIGHT";
nextDirection = "RIGHT";
score = 0;
level = 1;
speed = 200;
paused = true;
gameOver = false;
muted = false;
fruit = makeFruit();

let didSwipe = false;
startScreen.addEventListener("click", (e) => {
  e.stopPropagation();
  startGame();
});

document.addEventListener("keydown", (e) => {
  if (
    e.key === "ArrowUp" ||
    e.key === "ArrowDown" ||
    e.key === "ArrowLeft" ||
    e.key === "ArrowRight" ||
    e.key === " "
  ) {
    e.preventDefault();
  }
  if (!started && e.key.toLowerCase() !== "p" && e.key !== " ") return;
  if (e.key === "ArrowUp" && direction !== "DOWN" && nextDirection !== "DOWN")
    nextDirection = "UP";
  else if (
    e.key === "ArrowDown" &&
    direction !== "UP" &&
    nextDirection !== "UP"
  )
    nextDirection = "DOWN";
  else if (
    e.key === "ArrowLeft" &&
    direction !== "RIGHT" &&
    nextDirection !== "RIGHT"
  )
    nextDirection = "LEFT";
  else if (
    e.key === "ArrowRight" &&
    direction !== "LEFT" &&
    nextDirection !== "LEFT"
  )
    nextDirection = "RIGHT";
  else if (e.key.toLowerCase() === "p" || e.key === " ") {
    togglePause();
  } else if (e.key.toLowerCase() === "m") toggleMute();
  else if (e.key.toLowerCase() === "r") restart();
});

let tsX = 0,
  tsY = 0;
canvas.addEventListener(
  "touchstart",
  (e) => {
    tsX = e.touches[0].clientX;
    tsY = e.touches[0].clientY;
    didSwipe = false;
  },
  { passive: true },
);
canvas.addEventListener(
  "touchmove",
  (e) => {
    e.preventDefault();
    let dx = e.touches[0].clientX - tsX;
    let dy = e.touches[0].clientY - tsY;
    if (Math.abs(dx) < 24 && Math.abs(dy) < 24) return;
    if (Math.abs(dx) > Math.abs(dy)) setDir(dx > 0 ? "RIGHT" : "LEFT");
    else setDir(dy > 0 ? "DOWN" : "UP");
    didSwipe = true;
    tsX = e.touches[0].clientX;
    tsY = e.touches[0].clientY;
  },
  { passive: false },
);
canvas.addEventListener(
  "touchend",
  (e) => {
    if (didSwipe) return;
    if (!started) startGame();
  },
  { passive: true },
);
canvas.addEventListener("click", () => {
  if (didSwipe) {
    didSwipe = false;
    return;
  }
  if (!started) startGame();
  else togglePause();
});

function draw() {
  const W = WIDTH * CELL,
    H = HEIGHT * CELL;
  const s = CELL / 20;
  ctx.fillStyle = "#a8d0a8";
  ctx.fillRect(0, 0, W, H);
  let fcx = fruit.x * CELL + CELL / 2,
    fcy = fruit.y * CELL + CELL / 2;
  ctx.fillStyle = "#8B4513";
  ctx.fillRect(fcx - 1 * s, fcy - 10 * s, 2 * s, 6 * s);
  ctx.fillStyle = "#228B22";
  ctx.beginPath();
  ctx.ellipse(fcx + 5 * s, fcy - 7 * s, 4 * s, 2 * s, 0, 0, Math.PI * 2);
  ctx.fill();
  ctx.fillStyle = "#DC143C";
  ctx.beginPath();
  ctx.arc(fcx, fcy + 3 * s, 8 * s, 0, Math.PI * 2);
  ctx.fill();
  ctx.fillStyle = "rgba(255,255,255,0.5)";
  ctx.beginPath();
  ctx.arc(fcx - 3 * s, fcy, 2.5 * s, 0, Math.PI * 2);
  ctx.fill();
  if (snake.length > 1) {
    ctx.strokeStyle = "#2e9e3a";
    ctx.lineWidth = 14 * s;
    ctx.lineCap = "round";
    ctx.lineJoin = "round";
    ctx.beginPath();
    ctx.moveTo(snake[0].x * CELL + CELL / 2, snake[0].y * CELL + CELL / 2);
    for (let sn of snake)
      ctx.lineTo(sn.x * CELL + CELL / 2, sn.y * CELL + CELL / 2);
    ctx.stroke();
    ctx.strokeStyle = "#4caf50";
    ctx.lineWidth = 6 * s;
    ctx.beginPath();
    ctx.moveTo(snake[0].x * CELL + CELL / 2, snake[0].y * CELL + CELL / 2);
    for (let sn of snake)
      ctx.lineTo(sn.x * CELL + CELL / 2, sn.y * CELL + CELL / 2);
    ctx.stroke();
  }
  let h = snake[0],
    cx = h.x * CELL + CELL / 2,
    cy = h.y * CELL + CELL / 2;
  ctx.strokeStyle = "red";
  ctx.lineWidth = 2 * s;
  ctx.beginPath();
  if (direction === "RIGHT") {
    ctx.moveTo(cx + 10 * s, cy);
    ctx.lineTo(cx + 18 * s, cy - 3 * s);
    ctx.moveTo(cx + 10 * s, cy);
    ctx.lineTo(cx + 18 * s, cy + 3 * s);
  }
  if (direction === "LEFT") {
    ctx.moveTo(cx - 10 * s, cy);
    ctx.lineTo(cx - 18 * s, cy - 3 * s);
    ctx.moveTo(cx - 10 * s, cy);
    ctx.lineTo(cx - 18 * s, cy + 3 * s);
  }
  if (direction === "UP") {
    ctx.moveTo(cx, cy - 10 * s);
    ctx.lineTo(cx - 3 * s, cy - 18 * s);
    ctx.moveTo(cx, cy - 10 * s);
    ctx.lineTo(cx + 3 * s, cy - 18 * s);
  }
  if (direction === "DOWN") {
    ctx.moveTo(cx, cy + 10 * s);
    ctx.lineTo(cx - 3 * s, cy + 18 * s);
    ctx.moveTo(cx, cy + 10 * s);
    ctx.lineTo(cx + 3 * s, cy + 18 * s);
  }
  ctx.stroke();
  ctx.fillStyle = "#2e9e3a";
  ctx.beginPath();
  ctx.arc(cx, cy, 10 * s, 0, Math.PI * 2);
  ctx.fill();
  ctx.strokeStyle = "#1b5e20";
  ctx.lineWidth = 2 * s;
  ctx.stroke();
  let ex1, ey1, ex2, ey2;
  if (direction === "RIGHT") {
    ex1 = cx + 4 * s;
    ey1 = cy - 5 * s;
    ex2 = cx + 4 * s;
    ey2 = cy + 5 * s;
  } else if (direction === "LEFT") {
    ex1 = cx - 4 * s;
    ey1 = cy - 5 * s;
    ex2 = cx - 4 * s;
    ey2 = cy + 5 * s;
  } else if (direction === "UP") {
    ex1 = cx - 5 * s;
    ey1 = cy - 4 * s;
    ex2 = cx + 5 * s;
    ey2 = cy - 4 * s;
  } else {
    ex1 = cx - 5 * s;
    ey1 = cy + 4 * s;
    ex2 = cx + 5 * s;
    ey2 = cy + 4 * s;
  }
  ctx.fillStyle = "white";
  ctx.beginPath();
  ctx.arc(ex1, ey1, 4 * s, 0, Math.PI * 2);
  ctx.arc(ex2, ey2, 4 * s, 0, Math.PI * 2);
  ctx.fill();
  ctx.fillStyle = "black";
  ctx.beginPath();
  ctx.arc(ex1, ey1, 2 * s, 0, Math.PI * 2);
  ctx.arc(ex2, ey2, 2 * s, 0, Math.PI * 2);
  ctx.fill();
  if (gameOver) {
    ctx.fillStyle = "rgba(0,0,0,0.65)";
    ctx.fillRect(0, 0, W, H);
    ctx.fillStyle = "white";
    ctx.font = `bold ${42 * s}px Arial`;
    ctx.textAlign = "center";
    ctx.fillText("GAME OVER", W / 2, H / 2 - 20 * s);
    ctx.font = `${18 * s}px Arial`;
    ctx.fillText(
      "Score: " + score + "  |  High: " + high,
      W / 2,
      H / 2 + 15 * s,
    );
    ctx.fillText("Press R to Play Again", W / 2, H / 2 + 50 * s);
    ctx.textAlign = "left";
  }
  document.getElementById("info").innerText =
    `Score: ${score} | High: ${high} | Level: ${level} | Sound: ${muted ? "MUTED" : "ON"}${!started ? " | Press PLAY" : paused ? " | PAUSED" : ""}${gameOver ? " | GAME OVER - Press R" : ""}`;
}

function loop() {
  if (started && !paused && !gameOver) {
    direction = nextDirection;
    let head = { ...snake[0] };
    if (direction === "UP") head.y--;
    if (direction === "DOWN") head.y++;
    if (direction === "LEFT") head.x--;
    if (direction === "RIGHT") head.x++;
    let hitWall =
      head.x < 0 || head.x >= WIDTH || head.y < 0 || head.y >= HEIGHT;
    let hitSelf = snake.some((s) => s.x === head.x && s.y === head.y);
    if (hitWall || hitSelf) {
      gameOver = true;
      beep(400, 150);
      if (score > high) {
        high = score;
        localStorage.setItem("high", high);
      }
    } else {
      let willEat = head.x === fruit.x && head.y === fruit.y;
      snake.unshift(head);
      if (willEat) {
        score += 10;
        beep(1000, 30);
        if (score % 50 === 0) {
          level++;
          if (speed > 140) speed -= 5;
        }
        fruit = makeFruit();
      } else snake.pop();
    }
  }
  draw();
  setTimeout(loop, speed);
}
resizeCanvas();
loop();
