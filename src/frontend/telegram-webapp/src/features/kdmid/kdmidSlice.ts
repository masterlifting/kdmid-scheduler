/** @format */

import { PayloadAction, createSlice } from '@reduxjs/toolkit';
import { CityGetDto, IChat, IKdmidState } from './kdmidTypes';

const initialState: IKdmidState = {
  cities: [],
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
    setCities: (state, action: PayloadAction<CityGetDto[]>) => {
      state.cities = action.payload.map(x => ({ code: x.code, name: x.name }));
    },
  },
});

export const kdmidActions = kdmidSlice.actions;
export const kdmidReducer = kdmidSlice.reducer;
