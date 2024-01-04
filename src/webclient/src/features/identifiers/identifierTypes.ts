export interface IIdentifier {
  id: string;
  cd: string;
  ems?: string;
}

export interface ICity {
  code: string;
  name: string;
}

export interface ICommand {
  id: string;
  name: string;
  parameters: Map<string, string>;
}

export interface ICommandGetRequest {
  chatId: string;
  commandId: string;
}

export interface ICommandPostRequest {
  chatId: string;
  commandId: string;
  command: ICommand;
}

export interface IChatCommand {
  chatId: string;
  command: ICommand;
}

export interface IIdentifierState {
  chatCommands: IChatCommand[];
}
