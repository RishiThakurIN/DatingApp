export interface Message {
    id: number;
    senderId: number;
    senderUername: string;
    senderPhotUrl: string;
    recipientId: number;
    recipientUsername: string;
    recipientPhotUrl: string;
    content: string;
    dateRead?: Date;
    messageSent: Date;
}