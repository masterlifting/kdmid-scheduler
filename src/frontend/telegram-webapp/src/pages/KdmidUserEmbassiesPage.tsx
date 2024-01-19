/** @format */

import React from 'react';
import { useLocation } from 'react-router-dom';
import { KdmidUserEmbassies } from '../features/kdmid/embassies/KdmidUserEmbassiesComponent';

export const KdmidUserEmbassiesPage = () => {
  const query = new URLSearchParams(useLocation().search);
  const chatId = query.get('chatId');

  return !chatId ? <div>Invalid URL parameters</div> : <KdmidUserEmbassies chatId={chatId} />;
};
