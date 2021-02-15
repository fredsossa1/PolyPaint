import { Chat } from "./chat";

export class ServiceManager {
    private _publicChat: Chat;

    constructor() {
        this._publicChat = new Chat();
    }

    get publicChat(): Chat {
        return this._publicChat;
    }
}
