/** @format */

export interface IKdmidId {
  id: string;
  cd: string;
  ems?: string;
}

export interface ICity {
  code: string;
  name: string;
  timeShift: string;
}

export interface IAttempts {
  day: number;
  count: number;
}

export interface ICommand {
  id: string;
  name: string;
  parameters: { [key: string]: string };
}

export interface IChat {
  id: string;
  command: ICommand;
}

export interface IChatsState {
  chats: IChat[];
}

export interface ICommandGetRequest {
  chatId: string;
  commandId: string;
}

export interface ICommandsGetRequest {
  chatId: string;
  names: string;
  cityCode: string;
}

export interface ICommandPostRequest {
  chatId: string;
  command: ICommand;
}
