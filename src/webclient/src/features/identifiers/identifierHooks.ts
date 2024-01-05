import { useEffect, useState } from "react";
import {
  ICity,
  ICommand,
  ICommandGetRequest,
  IIdentifier,
} from "./identifierTypes";
import { useGetCommandQuery, useUpdateCommandMutation } from "./identifierApi";
import { constants } from "../../_constants";

//const telegram = window.Telegram.WebApp;

export const useIdentifier = (commandParams: ICommandGetRequest) => {
  const [identifier, setIdentifier] = useState<IIdentifier>({
    id: "",
    cd: "",
    ems: "",
  });

  // useEffect(() => {
  //   telegram.ready();
  // }, []);

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

  if (!isGetCommandError) {
    command = getCommandResponse;
    const cityParam = command?.parameters[constants.command.parameterKeys.city];

    if (cityParam) {
      city = JSON.parse(cityParam);
    }
  }

  const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (identifier.id === "" || identifier.cd === "" || identifier.ems === "") {
      alert("Fill all fields");
      return;
    }

    if (command) {
      command = {
        ...command,
        parameters: {
          ...command.parameters,
          "KdmidScheduler.Abstractions.Models.v1.Identifier":
            JSON.stringify(identifier),
        },
      };

      updateCommand({
        chatId: commandParams.chatId,
        command,
      });

      //telegram.close();
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
