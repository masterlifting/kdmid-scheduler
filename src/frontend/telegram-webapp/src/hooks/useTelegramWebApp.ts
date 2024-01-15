/** @format */

import { useEffect } from 'react';
import { ITelegramWebApp } from '../_types';

export const useTelegramWebApp = () => {
  const telegram: ITelegramWebApp = window['Telegram'].WebApp;

  useEffect(() => {
    telegram.ready();
  }, [telegram]);

  return {
    close: () => telegram.close(),
  };
};
