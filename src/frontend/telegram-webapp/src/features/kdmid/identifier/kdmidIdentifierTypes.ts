/** @format */

import { ICity, ICommand } from '../kdmidTypes';

export interface IIdentifierCommand {
  key: string;
  command: ICommand;
}

export interface IIdentifier {
  city?: ICity;
  commandsMap: Map<string, IIdentifierCommand>;
}
