import * as signalR from 'https://esm.sh/@microsoft/signalr@6.0.1';

export class NetworkManager {
    constructor(
        SERVER_ADRESS,
        AllGroupIds,
        UpdateHome,
        UpdateMain,
        ReceiveHistory,
        DeleteMainGroup,
        SetStatus,
        AllUsers,
        SetName) {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${SERVER_ADRESS}/hub`)
            .build();

        this.connection.on("UpdateHome", UpdateHome);
        this.connection.on("UpdateMain", UpdateMain);
        this.connection.on("AllGroupIds", AllGroupIds);
        this.connection.on("ReceiveHistory", ReceiveHistory);
        this.connection.on("DeleteMainGroup", DeleteMainGroup);
        this.connection.on("SetStatus", SetStatus);
        this.connection.on("AllUsers", AllUsers);
        this.connection.on("SetName", SetName);
    }

    async start() {
        await this.connection.start();
    }

    addUser() {
        this.connection.invoke("AddUser");
    }

    setName(name) {
        this.connection.invoke("SetName",name);
    }

    getAllGroupIds() {
        this.connection.invoke("GetAllGroupIds");
    }

    getAllUsers() {
        this.connection.invoke("GetAllUsers");
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

    deleteGroup(groupId) {
        this.connection.invoke("DeleteGroup", groupId);
    }

    createHomeGroup() {
        this.connection.invoke("CreateHomeGroup");
    }

    deleteMainGroup() {
        this.connection.invoke("DeleteMainGroup");
    }

    setStatus(users,status) {
        this.connection.invoke("SetStatus",users,status);
    }
}