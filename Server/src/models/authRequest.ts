import { DataType, IData } from "./data";

export interface IAuthRequest extends IData {
    type: DataType.AUTH;
    id: string;
}
