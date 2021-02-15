import { Socket } from "net";
import { DataType } from "./models/data";
import { IEvent } from "./models/event";
import { IMessage } from "./models/message";

export class Chat {
    private clients: Map<string, Socket>;

    constructor() {
        this.clients = new Map<string, Socket>();
    }

    public getClientId(socket: Socket): string {
        let id: string = "notFound";
        this.clients.forEach((clientSocket: Socket, clientId: string) => {
            if (this.isClientSocket(socket, clientSocket)) {
                id = clientId;
            }
        });

        return id;
    }

    private isClientSocket(socket: Socket, clientSocket: Socket): boolean {
        return socket.remoteAddress === clientSocket.remoteAddress && socket.remotePort === clientSocket.remotePort;
    }

    public addClient(clientId: string, clientSocket: Socket): boolean {
        if (!this.clients.has(clientId)) {
            this.clients.set(clientId, clientSocket);

            return true;
        }

        return false;
    }

    public removeClient(socket: Socket): boolean {
        const clientId: string | undefined = this.getClientId(socket);
        if (clientId) {
            this.clients.delete(clientId);

            return true;
        }

        return false;
    }

    public broadcast(data: string, senderId?: string): void {
        this.clients.forEach((clientSocket: Socket, clientId: string) => {
            if (senderId && clientId !== senderId) {
                clientSocket.write(data);
            }
        });
    }

    public broadcastMessage(message: IMessage): void {
        message.created = new Date();
        const jsonMessage: string = JSON.stringify(message);
        this.clients.forEach((clientSocket: Socket, clientId: string) => {
            clientSocket.write(jsonMessage);            
        });
    }

    public broadcastEvent(clientId: string, eventName: string): void {
        const connection: IEvent = {
            type: DataType.EVENT,
            name: eventName,
            context: "chat-0",
            source: clientId,
            timestamp: new Date(),
        };

        const jsonConnection = JSON.stringify(connection);

        this.broadcast(jsonConnection, clientId);
    }
}
