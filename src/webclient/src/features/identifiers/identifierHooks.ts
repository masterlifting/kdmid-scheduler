import { useState } from "react";
import {
  ICity,
  ICommand,
  ICommandGetRequest,
  ICommandPostRequest,
  IIdentifier,
} from "./identifierTypes";
import { useGetCommandQuery, useUpdateCommandMutation } from "./identifierApi";

export const useIdentifier = (commandParams: ICommandGetRequest) => {
  const [identifier, setIdentifier] = useState<IIdentifier>({
    id: "",
    cd: "",
    ems: "",
  });

  const {
    data: getCommandResponse,
    isLoading: isGetCommandLoading,
    isError: isGetCommandError,
    error: getCommandError,
  } = useGetCommandQuery({
    chatId: commandParams.chatId,
    commandId: commandParams.commandId,
  });

  const [
    updateCommand,
    {
      data: updateCommandResponse,
      isLoading: isUpdateCommandLoading,
      isError: isUpdateCommandError,
      error: updateCommandError,
    },
  ] = useUpdateCommandMutation();

  let city: ICity | undefined = undefined;
  let command: ICommand | undefined = undefined;

  if (!isGetCommandError && getCommandResponse) {
    if (getCommandResponse.isSuccess) {
      command = getCommandResponse.data;
      city = JSON.parse(
        command.parameters["KdmidScheduler.Abstractions.Models.v1.City"]
      );
    }
  }

  const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (command) {
      command.parameters["KdmidScheduler.Abstractions.Models.v1.Identifier"] =
        JSON.stringify(identifier);

      const updatedCommand: ICommandPostRequest = {
        chatId: commandParams.chatId,
        command,
      };

      updateCommand(updatedCommand);
    }
  };

  const onChangeId = (e: React.ChangeEvent<HTMLInputElement>) => {
    setIdentifier({ ...identifier, id: e.target.value });
  };

  const onChangeCd = (e: React.ChangeEvent<HTMLInputElement>) => {
    setIdentifier({ ...identifier, cd: e.target.value });
  };

  const onChangeEms = (e: React.ChangeEvent<HTMLInputElement>) => {
    setIdentifier({ ...identifier, ems: e.target.value });
  };

  return {
    city,
    identifier,
    onSubmit,
    onChangeId,
    onChangeCd,
    onChangeEms,
  };
};
