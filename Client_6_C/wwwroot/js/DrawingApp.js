import { NetworkManager } from './NetworkManager.js'
import { DrawingEvent } from './DrawingEvent.js'

export class DrawingApp {
    constructor() {
        this.currentGroup = "Home";
        this.currentStatus = "Senior";
        this.currentMode = "pen";
        this.currentId = "currentId";
        this.currentName = "currentName";
        this.previewStore = {};

        this.canvas = document.getElementById('canvas1');
        this.previewCanvas = document.getElementById('canvas2');
        this.textToDraw = document.getElementById('textToDraw');

        this.network = new NetworkManager(
            this.AllGroupIds.bind(this),
            this.UpdateHome.bind(this),
            this.UpdateMain.bind(this),
            this.ReceiveHistory.bind(this),
            this.DeleteMainGroup.bind(this),
            this.SetStatus.bind(this),
            this.AllUsers.bind(this),
            this.SetName.bind(this)
        );
        this.drawingEvent = new DrawingEvent(this);

        this.groupPage = document.getElementById("groupPage");
        this.drawPage = document.getElementById("drawPage");
        this.groupList = document.getElementById("groupList");
        this.userList = document.getElementById("userList");
        this.btnStatus = document.getElementById("btnStatus");
        this.btnName = document.getElementById("btnName");
        this.statusSelect = document.getElementById("statusSelect");
        this.currentStatusHome = document.getElementById("currentStatusHome");
        this.currentStatusMain = document.getElementById("currentStatusMain");
        this.currentNameHome = document.getElementById("currentNameHome");
        this.currentNameMain = document.getElementById("currentNameMain");

        window.setMode = (mode) => {
            this.currentMode = mode;
        };
        window.createGroup = (groupId) => {
            if (groupId.length > 0) {
                this.network.createGroup(groupId);
            }
        };

        window.loadMain = () => {
            this.network.leaveGroup("Home");
            this.network.joinGroup(this.currentGroup);
            this.network.getHistory(this.currentGroup);
            this.loadMainPage();
        };
        window.loadHome = () => {
            this.network.leaveGroup(this.currentGroup);
            this.network.joinGroup("Home");
            this.loadHomePage();
            this.network.getAllGroupIds();
        };
        window.addEventListener('resize', () => {
            if (this.drawPage.classList.contains('visible')) {
                this.resizeCanvas();
                this.network.getHistory(this.currentGroup);
            }
        });
        btnName.addEventListener('click', () => {
            this.network.setName(this.currentNameHome.value);
        });
        btnStatus.addEventListener('click', () => {
            const checkboxes = document.querySelectorAll('.user-checkbox');
            const selectedUsers = [];
            checkboxes.forEach(s => {
                if (s.checked) {
                    selectedUsers.push(s.getAttribute('data-id'));
                }
            });

            this.network.setStatus(selectedUsers, statusSelect.value);
        });
    }

    async init() {
        await this.network.start();
        this.currentId = this.network.connection.connectionId;
        this.currentNameHome.placeholder = `Name: ${this.currentName}`;
        this.network.createHomeGroup();
        this.network.addUser();
        this.network.joinGroup("Home");
        this.network.getAllGroupIds();
        this.network.getAllUsers();
    }

    resizeCanvas() {
        const container = document.getElementById('canvas-container');
        this.canvas.width = container.clientWidth;
        this.canvas.height = container.clientHeight;
        this.previewCanvas.width = container.clientWidth;
        this.previewCanvas.height = container.clientHeight;
    }

    loadHomePage() {
        this.groupPage.classList.replace('invisible', 'visible');
        this.drawPage.classList.replace('visible', 'invisible');
        document.getElementById("currentStatusHome").innerText = "Status: " + this.currentStatus;
    }

    loadMainPage() {
        this.groupPage.classList.replace('visible', 'invisible');
        this.drawPage.classList.replace('invisible', 'visible');
        document.getElementById("showGroupId").innerText = "Group: " + this.currentGroup;
        document.getElementById("currentStatusMain").innerText = "Status: " + this.currentStatus;
        this.resizeCanvas();
        this.currentNameMain.innerText = `${this.currentName}`;
    }

    drawElOnCanvas(ctx, el, w, h) {
        const x1 = el.point1.x * w;
        const y1 = el.point1.y * h;
        const x2 = el.point2.x * w;
        const y2 = el.point2.y * h;

        if (el.type === 'text') {
            this.drawText(ctx, el.otherData, x1, y1);
        }

        if (el.type === 'line') {
            ctx.moveTo(x1, y1);
            ctx.lineTo(x2, y2);
        } else if (el.type === 'square') {
            ctx.strokeRect(x1, y1, x2 - x1, y2 - y1);
        } else if (el.type === 'circle') {
            ctx.arc(x1, y1, Math.abs(x2-x1), 0, Math.PI * 2);
            ctx.stroke();
            ctx.closePath();
        }
    }

    drawMainCanvas(ctx, data) {
        ctx.beginPath();
        ctx.lineWidth = 5;
        ctx.strokeStyle = 'black';
        const w = ctx.canvas.width;
        const h = ctx.canvas.height;

        for (let el of data) {
            this.drawElOnCanvas(ctx,el,w,h);
        }
        ctx.stroke();
    }

    drawPreviewCanvas(previewCtx, groupStore) {
        previewCtx.clearRect(0, 0, previewCtx.canvas.width, previewCtx.canvas.height);
        const w = previewCtx.canvas.width;
        const h = previewCtx.canvas.height;

        previewCtx.beginPath();
        previewCtx.lineWidth = 5;
        previewCtx.strokeStyle = 'black';


        for (let id in groupStore) {
            const el = groupStore[id];
            this.drawElOnCanvas(previewCtx, el, w, h);
            previewCtx.stroke();
        }
    }

    processDrawingTwoCanvas(targetCtx, targetPrevCtx, message, clientConnectionId, groupId) {
        if (!this.previewStore[groupId]) {
            this.previewStore[groupId] = {};
        }

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

    drawText(ctx, text, x, y) {
        console.log(ctx, text, x, y);
        const fontFace = "Arial";
        ctx.fillStyle = "black";
        ctx.font = `25px ${fontFace}`;

        ctx.textBaseline = "top";

        ctx.fillText(text, x, y);
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
                    <button onclick="deleteFromPictureGroup('${groupId}')">Delete</button>
                </div>
            `;
            this.groupList.appendChild(container);
            this.network.getHistory(groupId);
        });

        window.joinFromPictureGroup = (id) => {
            this.currentGroup = id;
            this.network.joinGroup(id);
            this.loadMainPage();
            this.network.getHistory(id);
        };
        window.deleteFromPictureGroup = (id) => {
            this.currentGroup = "Main";
            this.network.deleteGroup(id);
        };
    }

    AllUsers(users) {
        console.log("AllUsers", users);

        this.userList.innerHTML = "";
        users.forEach(userData => {
            let showName = userData.name;
            if (userData.connectionId === this.currentId) {
                this.currentName = userData.name;
                this.currentNameHome.placeholder = `${this.currentName}`;
                showName = `(Me) ${showName}`;
            }

            const container = document.createElement("div");
            container.innerHTML = `
                <div>${showName} - ${userData.status}<div>
                <input type="checkbox" class="user-checkbox" data-id="${userData.user}">`;
            this.userList.appendChild(container);
        });
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

    DeleteMainGroup() {
        console.log("DeleteMainGroup");

        this.currentGroup = "Home";
        this.network.joinGroup("Home");
        this.loadHomePage();
    }

    SetStatus(status) {
        console.log("SetStatus",status);

        this.currentStatusHome.innerText = `Status: ${status}`;
        this.currentStatusMain.innerText = `Status: ${status}`;
        this.currentStatus = status;
    }

    SetName(res) {
        console.log("SetName", res);

        if (res) {
            this.currentNameMain.innerText = `${this.currentNameHome.value}`;
            this.currentName = this.currentNameHome.value;
        }
        else {
            alert("This name exist");
        }
    }
}