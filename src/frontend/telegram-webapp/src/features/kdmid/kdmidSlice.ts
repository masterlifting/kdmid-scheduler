/** @format */

import { PayloadAction, createSlice } from '@reduxjs/toolkit';
import { IChat, IChatsState } from './kdmidTypes';

const initialState: IChatsState = {
  chats: [],
};

export const kdmidSlice = createSlice({
  name: 'kdmidSlice',
  initialState,
  reducers: {
    setChat: (state, action: PayloadAction<IChat>) => {
      const chatIndex = state.chats.findIndex(x => x.id === action.payload.id);

      if (chatIndex === -1) {
        state.chats.push(action.payload);
      } else {
        state.chats[chatIndex] = action.payload;
      }
    },
  },
});

export const kdmidActions = kdmidSlice.actions;
export const kdmidReducer = kdmidSlice.reducer;
