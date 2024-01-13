/** @format */

import { useState } from 'react';
import { ICity, ICommand, ICommandGetRequest, IKdmidId } from './kdmidIdTypes';
import { useGetCommandQuery, useUpdateCommandMutation } from './kdmidIdApi';
import { useTelegramWebApp } from '../../hooks/useTelegramWebApp';

export const useKdmidId = ({ chatId, commandId }: ICommandGetRequest) => {
  const { close: closeTelegramWebApp } = useTelegramWebApp();
  const [kdmidId, setKdmidId] = useState<IKdmidId>({
    id: '',
    cd: '',
    ems: '',
  });

  const {
    data: getCommandResponse,
    isLoading: isGetCommandLoading,
    isError: isGetCommandError,
    error: getCommandError,
  } = useGetCommandQuery({
    chatId: chatId,
    commandId: commandId,
  });

  const [
    updateCommand,
    { data: updateCommandResponse, isLoading: isUpdateCommandLoading, isError: isUpdateCommandError, error: updateCommandError },
  ] = useUpdateCommandMutation();

  let city: ICity | undefined = undefined;
  let command: ICommand | undefined = undefined;

  if (!isGetCommandError) {
    command = getCommandResponse;
    const cityParam = command?.parameters['KdmidScheduler.Abstractions.Models.Core.v1.City'];

    if (cityParam) {
      city = JSON.parse(cityParam);
    }
  }

  const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (kdmidId.id === '') {
      alert('Id is required');
      return;
    }

    if (kdmidId.cd === '') {
      alert('Cd is required');
      return;
    }

    if (command) {
      command = {
        ...command,
        parameters: {
          ...command.parameters,
          'KdmidScheduler.Abstractions.Models.Core.v1.KdmidId': JSON.stringify(kdmidId),
        },
      };

      updateCommand({
        chatId: chatId,
        command,
      });

      closeTelegramWebApp();
    }
  };

  const onChangeId = (e: React.ChangeEvent<HTMLInputElement>) => {
    setKdmidId({ ...kdmidId, id: e.target.value });
  };

  const onChangeCd = (e: React.ChangeEvent<HTMLInputElement>) => {
    setKdmidId({ ...kdmidId, cd: e.target.value });
  };

  const onChangeEms = (e: React.ChangeEvent<HTMLInputElement>) => {
    setKdmidId({ ...kdmidId, ems: e.target.value });
  };

  return {
    city,
    kdmidId,
    onSubmit,
    onChangeId,
    onChangeCd,
    onChangeEms,
  };
};
