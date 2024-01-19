/** @format */

import React from 'react';
import { useLocation } from 'react-router-dom';
import { KdmidIdentifier } from '../features/kdmid/identifier/KdmidIdentifierComponent';

export const KdmidIdentifierPage = () => {
  const query = new URLSearchParams(useLocation().search);
  const chatId = query.get('chatId');
  const cityCode = query.get('cityCode');
  return !(chatId && cityCode) ? <div>Invalid URL parameters</div> : <KdmidIdentifier chatId={chatId} cityCode={cityCode} />;
};
