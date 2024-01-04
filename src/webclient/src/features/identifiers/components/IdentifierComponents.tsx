import React from "react";
import { useGetCommandQuery } from "../identifierApi";
import { ICity, IIdentifier } from "../identifierTypes";

interface IdentifierProps {
  chatId: string;
  commandId: string;
}

export const Identifier = ({ chatId, commandId }: IdentifierProps) => {
  const {
    data: response,
    isLoading,
    isError,
    error,
  } = useGetCommandQuery({
    chatId,
    commandId,
  });

  if (!isError && response) {
    if (response.isSuccess) {
      const command = response.data;
      const city: ICity = JSON.parse(
        command.parameters["KdmidScheduler.Abstractions.Models.v1.City"]
      );

      const identifier: IIdentifier = {
        id: "",
        cd: "",
      };

      return (
        <form>
          <div className="form-group">
            <label htmlFor="identifier">Identifier for the {city?.name}</label>
            <input
              type="text"
              className="form-control"
              id="identifier"
              aria-describedby="identifierHelp"
              placeholder="Enter identifier"
              value={identifier.id}
            />
            <small id="identifierHelp" className="form-text text-muted">
              Identifier
            </small>
          </div>
          <div className="form-group">
            <label htmlFor="cd">CD</label>
            <input
              type="text"
              className="form-control"
              id="cd"
              aria-describedby="cdHelp"
              placeholder="Enter cd"
              value={identifier.cd}
            />
            <small id="cdHelp" className="form-text text-muted">
              CD
            </small>
          </div>
        </form>
      );
    }
  } else
    return (
      <div>
        <h1>Identifier</h1>
      </div>
    );
};
