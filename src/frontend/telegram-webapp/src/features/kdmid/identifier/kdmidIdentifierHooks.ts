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
      setKdmidIds(
        getCommandsResponse.map(command => {
          const cityParam = command?.parameters['KdmidScheduler.Abstractions.Models.Core.v1.City'];

          const city = JSON.parse(cityParam);

          const kdmidIdParam = command.parameters['KdmidScheduler.Abstractions.Models.Core.v1.KdmidId'];

          const result: IUserKdmidId = kdmidIdParam
            ? {
                commandId: command.id,
                city: city.name,
                identifier: JSON.parse(kdmidIdParam),
              }
            : {
                commandId: command.id,
                city: city.name,
                identifier: {
                  id: '',
                  cd: '',
                  ems: '',
                },
              };

          return result;
        }),
      );
    }
  }, [getCommandsResponse]);

  const onSubmit = (e: React.FormEvent<HTMLFormElement>, commandId: string) => {
    e.preventDefault();

    if (kdmidId.id === '') {
      alert('Id is required');
      return;
    }

    if (kdmidId.cd === '') {
      alert('Cd is required');
      return;
    }

    var command = getCommandsResponse?.find(x => x.id === commandId);

    if (command) {
      command = {
        ...command,
        parameters: {
          ...command.parameters,
          'KdmidScheduler.Abstractions.Models.Core.v1.KdmidId': JSON.stringify(kdmidId),
        },
      };

      setCommand({
        chatId,
        command,
      });
    }
  };

  const onChangeId = (e: React.ChangeEvent<HTMLInputElement>, commandId: string) => {
    const kdmidId = { ...kdmidIds.find(x => x.commandId === commandId)?.identifier, id: e.target.value };
  };

  const onChangeCd = (e: React.ChangeEvent<HTMLInputElement>, commandId: string) => {
    setKdmidId({ ...kdmidId, cd: e.target.value });
  };

  const onChangeEms = (e: React.ChangeEvent<HTMLInputElement>, commandId: string) => {
    setKdmidId({ ...kdmidId, ems: e.target.value });
  };

  return {
    kdmidIds,
    onSubmit,
    onChangeId,
    onChangeCd,
    onChangeEms,
  };
};
