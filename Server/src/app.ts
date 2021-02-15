import { createServer, Socket } from "net";
import readline, { ReadLine } from "readline";
import { IAuthRequest } from "./models/authRequest";
import { DataType, IData } from "./models/data";
import { IErrorResponse } from "./models/errorResponse";
import { IMessage } from "./models/message";
import { getPublicIp, isValidIp } from "./network";
import { DEFAULT_IP, DEFAULT_PORT } from "./serverConfig";
import { ServiceManager } from "./serviceManager";

const serviceManager: ServiceManager = new ServiceManager();

// tslint:disable-next-line:max-func-body-length
const onSocketData = (socket: Socket) => (data: string) => {
    try {
        const parsedData: IData = JSON.parse(data);

        switch (parsedData.type) {

            case DataType.AUTH:
                const authRequest: IAuthRequest = parsedData as IAuthRequest;
                if (!serviceManager.publicChat.addClient(authRequest.id, socket)) {
                    const authError: IErrorResponse = {
                        type: DataType.ERROR,
                        message: "ID is already in use",
                    };
                    socket.write(JSON.stringify(authError));
                    process.stdout.write(`[${socket.remoteAddress}:${socket.remotePort}:${authRequest.id}] Username already in use!\n`);
                } else {
                    serviceManager.publicChat.broadcastEvent(authRequest.id, "clientConnection");
                }
                process.stdout.write(`[${socket.remoteAddress}:${socket.remotePort}:${authRequest.id}] Connected\n`);
                break;

            case DataType.MSG:
                const message = parsedData as IMessage;
                serviceManager.publicChat.broadcastMessage(message);
                process.stdout.write(`[${socket.remoteAddress}:${socket.remotePort}:${message.sender}] Message Broadcasted\n`);
                break;

            default:
                process.stdout.write(`[${socket.remoteAddress}:${socket.remotePort}] Invalid data type received\n`);
        }
    } catch (error) {
        process.stdout.write(`Unexpected packet type: ${error.message}\n`);
    }
};

const onSocketError = (socket: Socket) => () => {
    const clientId = serviceManager.publicChat.getClientId(socket);
    if (clientId) {
        serviceManager.publicChat.broadcastEvent(clientId, "clientDisconnection");
    }
    serviceManager.publicChat.removeClient(socket);
    process.stdout.write(`[${socket.remoteAddress}:${socket.remotePort}:${clientId}] Connection Lost : Client Disconnected\n`);
};

const onSocketEnd = (socket: Socket) => () => {
    const clientId = serviceManager.publicChat.getClientId(socket);
    if (clientId) {
        serviceManager.publicChat.broadcastEvent(clientId, "clientDisconnection");
    }
    serviceManager.publicChat.removeClient(socket);
    process.stdout.write(`[${socket.remoteAddress}:${socket.remotePort}:${clientId}] Client Disconnected\n`);
};

const onSocketConnection = (socket: Socket) => {
    process.stdout.write(`[${socket.remoteAddress}:${socket.remotePort}] Client connection started\n`);
    socket.setEncoding("utf8");
    socket.on("data", onSocketData(socket));
    socket.on("error", onSocketError(socket));
    socket.on("end", onSocketEnd(socket));
};

const server = createServer(onSocketConnection);

server.on("error", (error: Error) => {
    process.stdout.write(`Server Error: ${error.message} \nServer Restarting\n\n`);
    promptServerIp();
});

const onReadlineInput = (rl: ReadLine) => (inputIp: string) => {
    let serverIp;
    if (inputIp) {
        if (!isValidIp(inputIp)) {
            process.stdout.write("Invalid IP address\n");
            rl.prompt();

            return;
        }

        serverIp = inputIp;
    } else {
        const publicIp: string | undefined = getPublicIp();
        serverIp = inputIp ? inputIp as string : (publicIp ? publicIp : DEFAULT_IP);
    }
    try {
        server.listen(DEFAULT_PORT, serverIp);
    } catch (error) {
        process.stdout.write(`Unable to listen on ${serverIp}:${DEFAULT_PORT}\n`);
        rl.prompt();
    }
    process.stdout.write(`Server Started on ${serverIp}:${DEFAULT_PORT}\n`);

    rl.close();
};

const promptServerIp = () => {
    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout,
    });

    const SERVER_IP_PROMPT: string =
    `Enter Server IP address:
    \tDefault if nothing is entered (in order of priority):
    \t1. Automatic search for external IPv4 IP
    \t2. 127.0.0.1\n`;

    rl.setPrompt(SERVER_IP_PROMPT);
    rl.prompt();

    rl.on("line", onReadlineInput(rl));
};

promptServerIp();
