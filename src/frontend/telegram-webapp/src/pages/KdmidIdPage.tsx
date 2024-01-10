/** @format */

import React from 'react';
import { useLocation } from 'react-router-dom';
import { KdmidId } from '../features/kdmid-id/components/KdmidIdComponents';

export const KdmidIdPage = () => {
  const query = new URLSearchParams(useLocation().search);
  const chatId = query.get('chatId');
  const commandId = query.get('commandId');

  return !chatId || !commandId ? <div>Invalid URL parameters</div> : <KdmidId chatId={chatId} commandId={commandId} />;
};
