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

export interface ICommand {
  id: string;
  name: string;
  parameters: { [key: string]: string };
}

export interface ICommandGetRequest {
  chatId: string;
  commandId: string;
}

export interface ICommandPostRequest {
  chatId: string;
  command: ICommand;
}

export interface IChatCommand {
  chatId: string;
  command: ICommand;
}

export interface IKdmidIdState {
  chatCommands: IChatCommand[];
}
