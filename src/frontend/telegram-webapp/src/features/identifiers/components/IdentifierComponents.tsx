import React from "react";
import { InputClass } from "../../../styles/input";
import { ButtonClass } from "../../../styles/button";
import { useIdentifier } from "../../../features/identifiers/identifierHooks";

interface IdentifierProps {
  chatId: string;
  commandId: string;
}

export const Identifier = ({ chatId, commandId }: IdentifierProps) => {
  const { city, identifier, onSubmit, onChangeId, onChangeCd, onChangeEms } =
    useIdentifier({ chatId, commandId });

  return (
    <form
      onSubmit={onSubmit}
      className="w-80 absolute rounded-md left-1/2 top-1/3 transform -translate-x-1/2 -translate-y-1/3"
    >
      <h1 className="text-2xl font-bold mb-2">Embassy of {city?.name}</h1>
      <div className="flex flex-col items-center">
        <input
          className={InputClass.Text}
          name="id"
          title="id"
          type="text"
          placeholder="id"
          autoComplete="id"
          value={identifier.id}
          onChange={onChangeId}
        />
        <input
          className={InputClass.Text}
          name="cd"
          title="cd"
          type="text"
          placeholder="cd"
          autoComplete="cd"
          value={identifier.cd}
          onChange={onChangeCd}
        />
        <input
          className={InputClass.Text}
          name="ems"
          title="ems"
          type="text"
          placeholder="ems"
          autoComplete="ems"
          value={identifier.ems}
          onChange={onChangeEms}
        />
      </div>
      <div className="flex justify-end">
        <button type="submit" className={ButtonClass.Success}>
          Submit
        </button>
      </div>
    </form>
  );
};
