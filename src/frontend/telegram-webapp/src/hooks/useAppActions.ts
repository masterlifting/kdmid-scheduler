/** @format */

import { bindActionCreators } from '@reduxjs/toolkit';
import { useMemo } from 'react';
import { useDispatch } from 'react-redux';
import { kdmidActions } from '../features/kdmid/kdmidSlice';

const actions = {
  ...kdmidActions,
};

export const useAppActions = () => {
  const dispatch = useDispatch();
  return useMemo(() => bindActionCreators(actions, dispatch), [dispatch]);
};
