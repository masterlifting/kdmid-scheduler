import React from "react";
import { useLocation } from "react-router-dom";
import { Identifier } from "../features/identifiers/components/IdentifierComponents";

export const IdentifierPage = () => {
  const query = new URLSearchParams(useLocation().search);
  const chatId = query.get("chatId");
  const commandId = query.get("commandId");

  return !chatId || !commandId ? (
    <div>Invalid URL</div>
  ) : (
    <Identifier chatId={chatId} commandId={commandId} />
  );
};
