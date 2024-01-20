/** @format */

export interface IKdmidId {
  id: string;
  cd: string;
  ems?: string;
}

export interface ICommand {
  id?: string;
  name: string;
  cityName: string;
  identifier: IKdmidId;
}

export interface IChat {
  id: string;
  command: ICommand;
}

export interface IChatsState {
  chats: IChat[];
}

export type CommandGetDto = {
  id: string;
  name: string;
  city: string;
  kdmidId: string;
  kdmidCd: string;
  kdmidEms?: string;
  attempts: number;
};

export type CommandSetDto = {
  name: string;
  cityCode: string;
  kdmidId: string;
  kdmidCd: string;
  kdmidEms?: string;
};

export interface ICommandGetRequest {
  chatId: string;
  commandId: string;
}
export interface ICommandsGetRequest {
  chatId: string;
  names?: string;
  cityCode?: string;
}
export interface ICommandPostRequest {
  chatId: string;
  command: CommandSetDto;
}
export interface ICommandPutRequest {
  chatId: string;
  commandId: string;
  command: CommandSetDto;
}
export interface IcommandDeleteRequest {
  chatId: string;
  commandId: string;
}
