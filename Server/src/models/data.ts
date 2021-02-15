export interface IData {
    type: DataType;
}

export enum DataType {
    AUTH   = 0,  // Authentication
    MSG    = 1,  // Chat message
    ERROR  = 2,  // Error
    EVENT  = 3,  // Event
}
