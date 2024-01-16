/** @format */

import { useGetCommandsQuery, useSetCommandMutation } from '../kdmidApi';
import { useTelegramWebApp } from '../../../hooks/useTelegramWebApp';
import { useEffect, useState } from 'react';
import { IUserEmbassy } from './kdmidUserEmbassiesTypes';
import { IAttempts, ICity, IKdmidId } from '../kdmidTypes';

export const useKdmidUserEmbassies = (chatId: string) => {
  const { close: closeTelegramWebApp } = useTelegramWebApp();
  const [userEmbassies, setUserEmbassies] = useState<IUserEmbassy[]>([]);

  const {
    data: getCommandsResponse,
    isLoading: isGetCommandsLoading,
    isError: isGetCommandsError,
    error: getCommandsError,
  } = useGetCommandsQuery({
    chatId: chatId,
    filter: {
      name: 'sendAvailableDates',
    },
  });

  useEffect(() => {
    if (getCommandsResponse && !isGetCommandsLoading && !isGetCommandsError) {
      setUserEmbassies(
        getCommandsResponse.map(x => {
          const kdmidId = JSON.parse(x.parameters['KdmidScheduler.Abstractions.Models.Core.v1.KdmidId']) as IKdmidId;
          const city = JSON.parse(x.parameters['KdmidScheduler.Abstractions.Models.Core.v1.City']) as ICity;
          const attempts = JSON.parse(x.parameters['KdmidScheduler.Abstractions.Models.Core.v1.Attempts']) as IAttempts;

          return {
            commandId: x.id,
            city: city.name,
            identifierId: kdmidId.id,
            attempts: attempts ? attempts.count : 0,
          };
        }),
      );
    }
  }, [getCommandsResponse, isGetCommandsLoading, isGetCommandsError]);

  const [
    setCommand,
    { data: setCommandResponse, isLoading: isSetCommandLoading, isError: isSetCommandError, error: setCommandError },
  ] = useSetCommandMutation();

  const onSubmit = (commandId: string) => {
    const command = getCommandsResponse?.find(x => x.id === commandId);

    if (!command) {
      alert('Command not found');
      return;
    }

    alert('Waiting for response from Kdmid');
    setCommand({
      chatId,
      command,
    }).then(() => closeTelegramWebApp());
  };

  return {
    embassies: userEmbassies,
    onSubmit,
  };
};
