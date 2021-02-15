import { DataType, IData } from "./data";

export interface IMessage extends IData {
    type: DataType.MSG;
    sender: string;
    content: string;
    created: Date;
}
