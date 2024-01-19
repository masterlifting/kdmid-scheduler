/** @format */

import { ICommand, IKdmidId } from '../kdmidTypes';

export interface IUserKdmidId {
  city: string;
  identifier: IKdmidId;
  command: ICommand;
}
