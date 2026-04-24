export class DrawingEvent {
    constructor(app) {
        this.app = app;
        this.isDrawing = false;
        this.lastX = 0;
        this.lastY = 0;

        this.canvas = document.getElementById('canvas1');
        this.previewCanvas = document.getElementById('canvas2');

        this.mouseEvent();
    }

    mouseEvent() {
        this.previewCanvas.onmousedown = (e) => {
            this.StartDrawing(e.offsetX, e.offsetY);
        };

        this.previewCanvas.onmouseup = (e) => {
            this.StopDrawing(e.offsetX, e.offsetY);
        };

        this.previewCanvas.onmouseleave = (e) => {
            this.StopDrawing(e.offsetX, e.offsetY);
        };

        this.previewCanvas.onmousemove = (e) => {
            this.ProcessDrawing(e.offsetX, e.offsetY);
        };
    }

    StopDrawing(x, y) {
        if (!this.isDrawing) return;
        this.isDrawing = false;

        const sendingMode = (this.app.currentMode === 'pen') ? 'line' : this.app.currentMode;

        const data = {
            point1: { x: this.lastX / this.canvas.width, y: this.lastY / this.canvas.height },
            point2: { x: x / this.canvas.width, y: y / this.canvas.height },
            type: sendingMode,
            isPreview: false
        };

        this.app.network.sendDraw(data, this.app.currentGroup);
    }

    StartDrawing(x, y) {
        this.isDrawing = true;
        this.lastX = x;
        this.lastY = y;
    }

    ProcessDrawing(x, y) {
        if (!this.isDrawing) return;

        const sendingMode = (this.app.currentMode === 'pen') ? 'line' : this.app.currentMode;

        if (this.app.currentMode === 'pen') {
            const data = {
                point1: { x: this.lastX / this.canvas.width, y: this.lastY / this.canvas.height },
                point2: { x: x / this.canvas.width, y: y / this.canvas.height },
                type: sendingMode,
                isPreview: false
            };
            this.lastX = x;
            this.lastY = y;
            this.app.network.sendDraw(data, this.app.currentGroup);
        } else if (this.app.currentMode === 'line' || this.app.currentMode === 'square') {
            const data = {
                point1: { x: this.lastX / this.canvas.width, y: this.lastY / this.canvas.height },
                point2: { x: x / this.canvas.width, y: y / this.canvas.height },
                type: sendingMode,
                isPreview: true
            };
            this.app.network.sendDraw(data, this.app.currentGroup);
        }
    }
}