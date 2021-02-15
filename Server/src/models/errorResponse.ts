import { DataType, IData } from "./data";

export interface IErrorResponse extends IData {
    type: DataType.ERROR;
    message: string;
}
