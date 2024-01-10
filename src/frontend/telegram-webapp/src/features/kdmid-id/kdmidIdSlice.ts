/** @format */

import { PayloadAction, createSlice } from '@reduxjs/toolkit';
import { IChatCommand, IKdmidIdState } from './kdmidIdTypes';

const initialState: IKdmidIdState = {
  chatCommands: [],
};

export const kdmidIdSlice = createSlice({
  name: 'kdmidIdSlice',
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

export const kdmidIdActions = kdmidIdSlice.actions;
export const kdmidIdReducer = kdmidIdSlice.reducer;
