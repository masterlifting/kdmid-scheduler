/** @format */

import React, { useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { KdmidIdentifier } from '../features/kdmid/identifier/KdmidIdentifierComponent';
import { useGetCitiesQuery } from '../features/kdmid/kdmidApi';
import { useAppActions } from '../hooks/useAppActions';

export const KdmidIdentifierPage = () => {
  const query = new URLSearchParams(useLocation().search);
  const chatId = query.get('chatId');
  const cityCode = query.get('cityCode');

  const { setCities } = useAppActions();

  const { data: getCities, isError: isGetCitiesError } = useGetCitiesQuery();

  useEffect(() => {
    if (!isGetCitiesError && getCities) {
      setCities(getCities);
    }
  }, [getCities, isGetCitiesError, setCities]);

  return !(chatId && cityCode) ? <div>Invalid URL parameters</div> : <KdmidIdentifier chatId={chatId} cityCode={cityCode} />;
};
