/** @format */

import { useEffect, useState } from 'react';
import { useGetCommandsQuery, useSetCommandMutation } from '../kdmidApi';
import { IUserKdmidId } from './kdmidIdentifierTypes';

export const useKdmidIdentifier = (chatId: string, cityCode: string) => {
  const [kdmidIds, setKdmidIds] = useState<Map<string, IUserKdmidId>>(new Map<string, IUserKdmidId>());

  const {
    data: getCommandsResponse,
    isLoading: isGetCommandsLoading,
    isError: isGetCommandsError,
    error: getCommandsError,
  } = useGetCommandsQuery({
    chatId: chatId,
    names: 'sendAvailableDates,addAvailableEmbassy',
    cityCode: cityCode,
  });

  const [
    setCommand,
    { data: setCommandResponse, isLoading: isSetCommandLoading, isError: isSetCommandError, error: setCommandError },
  ] = useSetCommandMutation();

  useEffect(() => {
    if (!isGetCommandsError && getCommandsResponse) {
      const kdmidIdsMap = new Map<string, IUserKdmidId>();

      for (const command of getCommandsResponse) {
        const cityParam = command?.parameters['KdmidScheduler.Abstractions.Models.Core.v1.City'];

        const city = JSON.parse(cityParam);

        const kdmidIdParam = command.parameters['KdmidScheduler.Abstractions.Models.Core.v1.KdmidId'];

        const result: IUserKdmidId = kdmidIdParam
          ? {
              city: city.name,
              command: command,
              identifier: JSON.parse(kdmidIdParam),
            }
          : {
              city: city.name,
              command: command,
              identifier: {
                id: '',
                cd: '',
                ems: '',
              },
            };

        kdmidIdsMap.set(command.id, result);
      }

      setKdmidIds(kdmidIdsMap);
    }
  }, [getCommandsResponse, isGetCommandsError]);

  const onSubmit = (e: React.FormEvent<HTMLFormElement>, commandId: string) => {
    e.preventDefault();

    const userKdmidId = kdmidIds.get(commandId);

    if (userKdmidId.identifier.id === '') {
      alert('Id is required');
      return;
    }

    if (userKdmidId.identifier.cd === '') {
      alert('Cd is required');
      return;
    }

    userKdmidId.command.parameters['KdmidScheduler.Abstractions.Models.Core.v1.KdmidId'] = JSON.stringify(userKdmidId.identifier);

    setCommand({
      chatId,
      command: userKdmidId.command,
    });
  };

  const onChangeId = (e: React.ChangeEvent<HTMLInputElement>, commandId: string) => {
    kdmidIds.get(commandId).identifier.id = e.target.value;
    setKdmidIds(prev => new Map(prev));
  };

  const onChangeCd = (e: React.ChangeEvent<HTMLInputElement>, commandId: string) => {
    kdmidIds.get(commandId).identifier.cd = e.target.value;
    setKdmidIds(prev => new Map(prev));
  };

  const onChangeEms = (e: React.ChangeEvent<HTMLInputElement>, commandId: string) => {
    kdmidIds.get(commandId).identifier.ems = e.target.value;
    setKdmidIds(prev => new Map(prev));
  };

  console.log(kdmidIds);
  return {
    userKdmidIds: Array.from(kdmidIds.values()),
    onSubmit,
    onChangeId,
    onChangeCd,
    onChangeEms,
    onRemove: (e: React.MouseEvent<HTMLButtonElement, MouseEvent>, commandId: string) => {
      e.preventDefault();
      kdmidIds.delete(commandId);
      setKdmidIds(prev => new Map(prev));
    },
    onAdd: (e: React.MouseEvent<HTMLButtonElement, MouseEvent>, commandId: string) => {
      e.preventDefault();

      const userKdmidId = kdmidIds.get(commandId);

      kdmidIds.set(commandId, {
        city: userKdmidId.city,
        command: userKdmidId.command,
        identifier: {
          id: '',
          cd: '',
          ems: '',
        },
      });
      setKdmidIds(prev => new Map(prev));
    },
  };
};
