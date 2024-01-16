/** @format */

import { configureStore } from '@reduxjs/toolkit';
import { setupListeners } from '@reduxjs/toolkit/query';
import { kdmidApi } from './features/kdmid/kdmidApi';
import { kdmidReducer } from './features/kdmid/kdmidSlice';

export const store = configureStore({
  reducer: {
    [kdmidApi.reducerPath]: kdmidApi.reducer,
    identifierState: kdmidReducer,
  },
  middleware: getDefaultMiddleware => getDefaultMiddleware().concat(kdmidApi.middleware),
});

setupListeners(store.dispatch);

export type RootState = ReturnType<typeof store.getState>;
