/** @format */

import { PayloadAction, createSlice } from '@reduxjs/toolkit';
import { IChatCommand, IIdentifierState } from './identifierTypes';

const initialState: IIdentifierState = {
  chatCommands: [],
};

export const identifierSlice = createSlice({
  name: 'identifierSlice',
  initialState,
  reducers: {
    setChatCommand: (state, action: PayloadAction<IChatCommand>) => {
      const chatCommandIndex = state.chatCommands.findIndex(x => x.chatId === action.payload.chatId);

      if (chatCommandIndex === -1) {
        state.chatCommands.push(action.payload);
      } else {
        state.chatCommands[chatCommandIndex] = action.payload;
      }
    },
  },
});

export const identifierActions = identifierSlice.actions;
export const identifierReducer = identifierSlice.reducer;
