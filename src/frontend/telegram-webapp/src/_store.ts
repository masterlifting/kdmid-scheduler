/** @format */

import { configureStore } from '@reduxjs/toolkit';
import { setupListeners } from '@reduxjs/toolkit/query';
import { kdmidIdApi } from './features/kdmid-id/kdmidIdApi';
import { kdmidIdReducer } from './features/kdmid-id/kdmidIdSlice';

export const store = configureStore({
  reducer: {
    [kdmidIdApi.reducerPath]: kdmidIdApi.reducer,
    identifierState: kdmidIdReducer,
  },
  middleware: getDefaultMiddleware => getDefaultMiddleware().concat(kdmidIdApi.middleware),
});

setupListeners(store.dispatch);

export type RootState = ReturnType<typeof store.getState>;
