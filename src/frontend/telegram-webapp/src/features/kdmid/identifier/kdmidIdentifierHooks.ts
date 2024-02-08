/** @format */

import { useEffect, useState } from 'react';
import { useCreateCommandMutation, useDeleteCommandMutation, useGetCommandsQuery, useUpdateCommandMutation } from '../kdmidApi';
import { v4 as guid } from 'uuid';
import { IIdentifier, IIdentifierCommand } from './kdmidIdentifierTypes';
import { ICity, ICommand } from '../kdmidTypes';
import { useAppSelector } from '../../../hooks/useAppSelector';
import { IPagination } from '../../../_types';

export const useKdmidIdentifier = (chatId: string, cityCode: string) => {
  const { cities } = useAppSelector(x => x.kdmidState);
  const city: ICity | undefined = cities.find(x => x.code === cityCode);

  const [paginationState, setPaginstionState] = useState<IPagination>({ pageNumber: 1 });

  const [identifierData, setIdentifierData] = useState<IIdentifier>({
    city,
    commandsMap: new Map<string, IIdentifierCommand>(),
  });

  const [commands, setCommands] = useState<IIdentifierCommand[]>([]);
  const commandsTotalCount = identifierData.commandsMap.size;

  const { data: getCommandsResponse, isError: isGetCommandsError } = useGetCommandsQuery({
    chatId: chatId,
    names: 'commandInProcess',
    cityCode: cityCode,
  });

  const [createCommand] = useCreateCommandMutation();
  const [updateCommand] = useUpdateCommandMutation();
  const [deleteCommand] = useDeleteCommandMutation();

  useEffect(() => {
    if (!isGetCommandsError && getCommandsResponse) {
      const commandsMap = new Map<string, IIdentifierCommand>();

      for (const item of getCommandsResponse) {
        const command: ICommand = {
          id: item.id,
          identifier: {
            id: item.kdmidId,
            cd: item.kdmidCd,
            ems: item.kdmidEms,
          },
        };

        const key = guid();
        commandsMap.set(key, { key, command });
      }

      setIdentifierData({ city, commandsMap });
    }
  }, [city, getCommandsResponse, isGetCommandsError]);

  useEffect(() => {
    const paginatedCommands: IIdentifierCommand[] = [];
    const isentifierCommands = Array.from(identifierData.commandsMap.values()).sort((a, b) => {
      if (a.command.identifier.id < b.command.identifier.id) {
        return -1;
      }
      if (a.command.identifier.id > b.command.identifier.id) {
        return 1;
      }
      return 0;
    });

    let pageNumber = 1;

    for (const command of isentifierCommands) {
      if (pageNumber === paginationState.pageNumber) {
        paginatedCommands.push(command);
        break;
      } else {
        pageNumber++;
      }
    }
    setCommands(paginatedCommands);
  }, [identifierData, paginationState.pageNumber]);

  const onNewCommand = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
    e.preventDefault();

    const key = guid();
    identifierData.commandsMap.set(key, {
      key: key,
      command: {
        identifier: {
          id: '',
          cd: '',
          ems: '',
        },
      },
    });

    setIdentifierData(prev => ({ ...prev, commandsMap: identifierData.commandsMap }));
  };

  const onSetCommand = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>, key: string) => {
    e.preventDefault();

    const commandsMapValue = identifierData.commandsMap.get(key);
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
          cityCode,
          kdmidId: command.identifier.id,
          kdmidCd: command.identifier.cd,
          kdmidEms: command.identifier.ems,
        },
      })
        .unwrap()
        .catch(e => {
          alert(JSON.stringify(e.data?.message));
        })
        .then(() => {
          setIdentifierData(prev => ({ ...prev, commandsMap: identifierData.commandsMap }));
        });
    } else {
      createCommand({
        chatId,
        command: {
          cityCode,
          kdmidId: command.identifier.id,
          kdmidCd: command.identifier.cd,
          kdmidEms: command.identifier.ems,
        },
      })
        .unwrap()
        .catch(e => {
          alert(JSON.stringify(e.data?.message));
        })
        .then(response => {
          if (!response) {
            return;
          }
          command.id = response;
          setIdentifierData(prev => ({ ...prev, commandsMap: identifierData.commandsMap }));
        });
    }
  };

  const onRemoveCommand = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>, key: string) => {
    e.preventDefault();

    var command = identifierData.commandsMap.get(key).command;

    if (!command.id) {
      identifierData.commandsMap.delete(key);
      setIdentifierData(prev => ({ ...prev, commandsMap: identifierData.commandsMap }));
    } else {
      deleteCommand({ chatId, commandId: command.id })
        .unwrap()
        .catch(e => {
          alert(JSON.stringify(e.data?.message));
        })
        .then(() => {
          identifierData.commandsMap.delete(key);
          setIdentifierData(prev => ({ ...prev, commandsMap: identifierData.commandsMap }));
        });
    }
  };

  const onChangeKdmidId = (e: React.ChangeEvent<HTMLInputElement>, key: string) => {
    identifierData.commandsMap.get(key).command.identifier.id = e.target.value;
    setIdentifierData(prev => ({ ...prev, commandsMap: identifierData.commandsMap }));
  };

  const onChangeKdmidCd = (e: React.ChangeEvent<HTMLInputElement>, key: string) => {
    identifierData.commandsMap.get(key).command.identifier.cd = e.target.value;
    setIdentifierData(prev => ({ ...prev, commandsMap: identifierData.commandsMap }));
  };

  const onChangeKdmidEms = (e: React.ChangeEvent<HTMLInputElement>, key: string) => {
    identifierData.commandsMap.get(key).command.identifier.ems = e.target.value;
    setIdentifierData(prev => ({ ...prev, commandsMap: identifierData.commandsMap }));
  };

  return {
    city: identifierData.city,
    commands,
    commandsTotalCount,
    onNewCommand,
    onSetCommand,
    onRemoveCommand,
    onChangeKdmidId,
    onChangeKdmidCd,
    onChangeKdmidEms,
    paginationState,
    setPaginstionState,
  };
};
