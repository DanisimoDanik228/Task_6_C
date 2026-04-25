import * as signalR from 'https://esm.sh/@microsoft/signalr@6.0.1';

export class NetworkManager {
    constructor(AllGroupIds, UpdateHome, UpdateMain, ReceiveHistory) {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5123/hub")
            .build();

        this.connection.on("UpdateHome", UpdateHome);
        this.connection.on("UpdateMain", UpdateMain);
        this.connection.on("AllGroupIds", AllGroupIds);
        this.connection.on("ReceiveHistory", ReceiveHistory);
    }

    async start() {
        try {
            await this.connection.start();
            console.log("SignalR Connected");
        } catch (err) {
            console.error("SignalR Connection Error: ", err);
        }
    }

    getAllGroupIds() {
        this.connection.invoke("GetAllGroupIds");
    }

    joinGroup(groupId) {
        this.connection.invoke("JoinGroup", groupId);
    }

    leaveGroup(groupId) {
        this.connection.invoke("LeaveGroup", groupId);
    }

    sendDraw(data, groupId) {
        this.connection.invoke("SendData", data, groupId);
    }

    getHistory(groupId) {
        this.connection.invoke("GetHistory", groupId);
    }

    createGroup(groupId) {
        this.connection.invoke("CreateGroup", groupId);
    }
}