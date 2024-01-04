/** @format */

import { configureStore } from "@reduxjs/toolkit";
import { setupListeners } from "@reduxjs/toolkit/query";
import { identifierApi } from "./features/identifiers/identifierApi";
import { identifierReducer } from "./features/identifiers/identifierSlice";

export const store = configureStore({
  reducer: {
    [identifierApi.reducerPath]: identifierApi.reducer,
    identifierState: identifierReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(identifierApi.middleware),
});

setupListeners(store.dispatch);

export type RootState = ReturnType<typeof store.getState>;
