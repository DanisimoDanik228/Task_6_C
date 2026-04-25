import { NetworkManager } from './NetworkManager.js'
import { DrawingEvent } from './DrawingEvent.js'

export class DrawingApp {
    constructor() {
        this.currentGroup = "Home";
        this.currentMode = "pen";
        this.previewStore = {};

        this.canvas = document.getElementById('canvas1');
        this.previewCanvas = document.getElementById('canvas2');

        this.network = new NetworkManager(
            this.AllGroupIds.bind(this),
            this.UpdateHome.bind(this),
            this.UpdateMain.bind(this),
            this.ReceiveHistory.bind(this)
        );
        this.drawingEvent = new DrawingEvent(this);

        this.groupPage = document.getElementById("groupPage");
        this.drawPage = document.getElementById("drawPage");
        this.groupList = document.getElementById("groupList");

        window.setMode = (mode) => { this.currentMode = mode; };
        window.loadMain = () => { this.loadMainPage(); };
        window.createGroup = (groupId) => { this.createGroup(groupId); };

        window.addEventListener('resize', () => {
            if (this.drawPage.classList.contains('visible')) {
                this.resizeCanvas();
                this.network.getHistory(this.currentGroup);
            }
        });
    }

    async init() {
        await this.network.start();
        this.network.createGroup("Home");
        this.network.joinGroup("Home");
        this.network.getAllGroupIds();
    }

    createGroup(groupId) {
        this.network.createGroup(groupId);
    }

    resizeCanvas() {
        const container = document.getElementById('canvas-container');
        this.canvas.width = container.clientWidth;
        this.canvas.height = container.clientHeight;
        this.previewCanvas.width = container.clientWidth;
        this.previewCanvas.height = container.clientHeight;
    }

    loadMainPage() {
        this.network.leaveGroup("Home");
        this.network.joinGroup(this.currentGroup);
        this.network.getHistory(this.currentGroup);

        this.groupPage.classList.replace('visible', 'invisible');
        this.drawPage.classList.replace('invisible', 'visible');
        document.getElementById("showGroupId").innerText = "Group: " + this.currentGroup;
        this.resizeCanvas();
    }

    drawMainCanvas(ctx, data) {
        ctx.beginPath();
        ctx.lineWidth = 5;
        ctx.strokeStyle = 'black';
        const w = ctx.canvas.width;
        const h = ctx.canvas.height;

        for (let el of data) {
            const x1 = el.point1.x * w;
            const y1 = el.point1.y * h;
            const x2 = el.point2.x * w;
            const y2 = el.point2.y * h;

            if (el.type === 'line') {
                ctx.moveTo(x1, y1);
                ctx.lineTo(x2, y2);
            } else if (el.type === 'square') {
                ctx.strokeRect(x1, y1, x2 - x1, y2 - y1);
            }
        }
        ctx.stroke();
    }

    drawPreviewCanvas(previewCtx, groupStore) {
        previewCtx.clearRect(0, 0, previewCtx.canvas.width, previewCtx.canvas.height);
        const w = previewCtx.canvas.width;
        const h = previewCtx.canvas.height;

        for (let id in groupStore) {
            const el = groupStore[id];
            previewCtx.beginPath();
            previewCtx.lineWidth = 5;
            previewCtx.strokeStyle = 'black';

            const x1 = el.point1.x * w;
            const y1 = el.point1.y * h;
            const x2 = el.point2.x * w;
            const y2 = el.point2.y * h;

            if (el.type === 'line') {
                previewCtx.moveTo(x1, y1);
                previewCtx.lineTo(x2, y2);
            } else if (el.type === 'square') {
                previewCtx.strokeRect(x1, y1, x2 - x1, y2 - y1);
            }
            previewCtx.stroke();
        }
    }

    processDrawingTwoCanvas(targetCtx, targetPrevCtx, message, clientConnectionId, groupId) {
        if (!this.previewStore[groupId]) {
            this.previewStore[groupId] = {};
        }

        if (message.type === 'line' || message.type === 'square') {
            if (message.isPreview && clientConnectionId != null) {
                this.previewStore[groupId][clientConnectionId] = message;
                this.drawPreviewCanvas(targetPrevCtx, this.previewStore[groupId]);
            } else {
                if (clientConnectionId != null)
                    delete this.previewStore[groupId][clientConnectionId];

                this.drawPreviewCanvas(targetPrevCtx, this.previewStore[groupId]);
                this.drawMainCanvas(targetCtx, [message]);
            }
        }
    }

    UpdateHome(data, clientConnectionId, groupId) {
        console.log("UpdateHome", clientConnectionId, groupId);

        const thumbC = document.getElementById(`${groupId}GroupId`);
        const thumbP = document.getElementById(`${groupId}previewGroupId`);
        if (thumbC && thumbP) {
            this.processDrawingTwoCanvas(thumbC.getContext('2d'), thumbP.getContext('2d'), data, clientConnectionId, groupId);
        }
    }

    UpdateMain(message, clientConnectionId) {
        console.log("UpdateMain", message, clientConnectionId);

        const ctx = document.getElementById('canvas1').getContext('2d');
        const prevCtx = document.getElementById('canvas2').getContext('2d');
        this.processDrawingTwoCanvas(ctx, prevCtx, message, clientConnectionId, this.currentGroup);
    }

    AllGroupIds(data) {
        console.log("AllGroupIds", data);

        this.groupList.innerHTML = "";
        data.forEach(groupId => {
            if (groupId === "Home") return;

            const container = document.createElement("div");
            container.innerHTML = `
                <div style="position:relative; width:100px; height:130px; display:inline-block; margin:10px;">
                    <canvas id="${groupId}GroupId" width="800" height="800" style="width:100px; height:100px; position:absolute; border:1px solid gray; z-index:1"></canvas>
                    <canvas id="${groupId}previewGroupId" width="800" height="800" style="width:100px; height:100px; position:absolute; border:1px solid gray; z-index:2"></canvas>
                    <div style="margin-top:105px">${groupId}</div>
                    <button onclick="joinFromPictureGroup('${groupId}')">Join</button>
                </div>
            `;
            this.groupList.appendChild(container);
            this.network.getHistory(groupId);
        });

        window.joinFromPictureGroup = (id) => {
            this.currentGroup = id;
            this.loadMainPage();
        };
    }

    ReceiveHistory(clasterData, groupId) {
        console.log("ReceiveHistory",clasterData, groupId);

        if (!clasterData) return;
        clasterData.forEach(data => {
            if (this.currentGroup === groupId && this.drawPage.classList.contains('visible')) {
                const ctx = document.getElementById('canvas1').getContext('2d');
                this.drawMainCanvas(ctx, [data]);
            } else {
                this.UpdateHome(data, null, groupId);
            }
        });
    }
}