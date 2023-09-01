function drawTicks(ctx, step, tickHeight) {
    const rulerWidth = ctx.canvas.width;
    const ticksPerUnit = rulerWidth / 5;

    for (let i = 0; i <= rulerWidth; i += step * ticksPerUnit) {
        ctx.beginPath();
        ctx.moveTo(i, 0);
        ctx.lineTo(i, tickHeight);
        ctx.stroke();
    }
}

function drawNumbers(ctx, step) {
    ctx.font = "15px sans-serif";
    ctx.fontWeight = "bold";
    ctx.textBaseline = "top";
    const rulerWidth = ctx.canvas.width;
    const numbersPerUnit = rulerWidth / 5;

    for (let i = 0; i <= rulerWidth; i += step * numbersPerUnit) {
        ctx.fillText(i / numbersPerUnit, i - 3, 15);
    }
}

function drawMiddleNumbers(ctx, step) {
    ctx.font = "12px sans-serif";
    ctx.fontWeight = "bold";
    ctx.textBaseline = "top";
    const rulerWidth = ctx.canvas.width;
    const numbersPerUnit = rulerWidth / 5;

    for (let i = 0; i <= rulerWidth; i += step * numbersPerUnit) {
        const number = i / numbersPerUnit;
        if (number % 1 !== 0) {
            ctx.fillText(number.toFixed(1), i - 3, 15);
        }
    }
}

export function createRuler() {
    const ruler = document.getElementById("ruler");
    const ctx = ruler.getContext("2d");

    ctx.strokeStyle = "black";
    ctx.lineWidth = 1;

    // Draw main ticks
    drawTicks(ctx, 1, 12);

    // Draw middle ticks
    drawTicks(ctx, 0.5, 8);

    // Draw sub-ticks
    drawTicks(ctx, 0.1, 5);

}

export function createNumber() {
    const ruler = document.getElementById("number");
    const ctx = ruler.getContext("2d");

    // Draw numbers
    drawNumbers(ctx, 1);
}

export function createMiddleNumber() {
    const middle_number = document.getElementById("middle-number");
    const ctx_middle = middle_number.getContext("2d");
    
    // Draw middle_numbers
    drawMiddleNumbers(ctx_middle, 0.5);

}
