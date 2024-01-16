/** @format */

import React from 'react';
import { useLocation } from 'react-router-dom';
import { KdmidIdentifier } from '../features/kdmid/identifier/KdmidIdentifierComponent';

export const KdmidIdentifierPage = () => {
  const query = new URLSearchParams(useLocation().search);
  const chatId = query.get('chatId');
  const commandId = query.get('commandId');

  return !chatId || !commandId ? <div>Invalid URL parameters</div> : <KdmidIdentifier chatId={chatId} commandId={commandId} />;
};
