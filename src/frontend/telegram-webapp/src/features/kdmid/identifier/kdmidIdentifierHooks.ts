/** @format */

import { useEffect, useState } from 'react';
import { useCreateCommandMutation, useDeleteCommandMutation, useGetCommandsQuery, useUpdateCommandMutation } from '../kdmidApi';
import { v4 as guid } from 'uuid';
import { IGuidCommand } from './kdmidIdentifierTypes';
import { ICommand } from '../kdmidTypes';

export const useKdmidIdentifier = (chatId: string, cityCode: string) => {
  const [commandsMap, setCommandsMap] = useState<Map<string, IGuidCommand>>(new Map<string, IGuidCommand>());

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
    createCommand,
    { data: createCommandResponse, isLoading: isCreateCommandLoading, isError: isCreateCommandError, error: createCommandError },
  ] = useCreateCommandMutation();

  const [
    updateCommand,
    { data: updateCommandResponse, isLoading: isUpdateCommandLoading, isError: isUpdateCommandError, error: updateCommandError },
  ] = useUpdateCommandMutation();

  const [
    deleteCommand,
    { data: deleteCommandResponse, isLoading: isDeleteCommandLoading, isError: isDeleteCommandError, error: deleteCommandError },
  ] = useDeleteCommandMutation();

  useEffect(() => {
    if (!isGetCommandsError && getCommandsResponse) {
      const newCommandsMap = new Map<string, IGuidCommand>();

      for (const item of getCommandsResponse) {
        const command: ICommand = {
          id: item.id,
          name: item.name,
          cityName: item.city,
          identifier: {
            id: item.kdmidId,
            cd: item.kdmidCd,
            ems: item.kdmidEms,
          },
        };

        const key = guid();

        newCommandsMap.set(key, {
          key: key,
          command,
        });
      }

      setCommandsMap(newCommandsMap);
    }
  }, [getCommandsResponse, isGetCommandsError]);

  const onAddNewCommand = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    e.preventDefault();

    const key = guid();
    commandsMap.set(key, {
      key: key,
      command: {
        name: 'addAvailableEmbassy',
        cityName: '',
        identifier: {
          id: '',
          cd: '',
          ems: '',
        },
      },
    });

    setCommandsMap(prev => new Map(prev));
  };

  const onSetCommand = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>, key: string) => {
    e.preventDefault();

    const commandsMapValue = commandsMap.get(key);
    const command = commandsMapValue.command;

    if (command.identifier.id === '') {
      alert('Id is required');
      return;
    }

    if (command.identifier.cd === '') {
      alert('Cd is required');
      return;
    }

    if (command.id) {
      updateCommand({
        chatId,
        commandId: command.id,
        command: {
          name: command.name,
          cityCode: cityCode,
          kdmidId: command.identifier.id,
          kdmidCd: command.identifier.cd,
          kdmidEms: command.identifier.ems,
        },
      });
    } else {
      createCommand({
        chatId,
        command: {
          name: 'addToProcess',
          cityCode: cityCode,
          kdmidId: command.identifier.id,
          kdmidCd: command.identifier.cd,
          kdmidEms: command.identifier.ems,
        },
      });
    }
  };

  const onRemoveCommand = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>, key: string) => {
    e.preventDefault();

    var command = commandsMap.get(key).command;

    if (!command.id) {
      commandsMap.delete(key);
      setCommandsMap(prev => new Map(prev));
    } else {
      deleteCommand({ chatId, commandId: command.id });
    }
  };

  const onChangeKdmidId = (e: React.ChangeEvent<HTMLInputElement>, key: string) => {
    commandsMap.get(key).command.identifier.id = e.target.value;
    setCommandsMap(prev => new Map(prev));
  };

  const onChangeKdmidCd = (e: React.ChangeEvent<HTMLInputElement>, key: string) => {
    commandsMap.get(key).command.identifier.cd = e.target.value;
    setCommandsMap(prev => new Map(prev));
  };

  const onChangeKdmidEms = (e: React.ChangeEvent<HTMLInputElement>, key: string) => {
    commandsMap.get(key).command.identifier.ems = e.target.value;
    setCommandsMap(prev => new Map(prev));
  };

  return {
    commands: Array.from(commandsMap.values()),
    onAddNewCommand,
    onSetCommand,
    onRemoveCommand,
    onChangeKdmidId,
    onChangeKdmidCd,
    onChangeKdmidEms,
  };
};
