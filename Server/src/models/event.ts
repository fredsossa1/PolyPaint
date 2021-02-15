import { DataType, IData } from "./data";

export interface IEvent extends IData {
    type: DataType.EVENT;
    name: string;
    context: string;
    source: string;
    timestamp: Date;
}
