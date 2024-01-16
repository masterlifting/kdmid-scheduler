/** @format */

import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { constants } from '../../_constants';
import { ICommand, ICommandGetRequest, ICommandPostRequest, ICommandsGetRequest } from './kdmidTypes';

const controller = 'bot';

export const kdmidApi = createApi({
  reducerPath: 'KdmidApi',
  baseQuery: fetchBaseQuery({ baseUrl: constants.config.backendBaseUrl }),
  endpoints: builder => ({
    getCommand: builder.query<ICommand, ICommandGetRequest>({
      query: ({ chatId, commandId }) => ({
        url: `${controller}/chats/${chatId}/commands/${commandId}`,
        method: constants.http.methods.GET,
      }),
    }),
    getCommands: builder.query<ICommand[], ICommandsGetRequest>({
      query: ({ chatId, filter }) => ({
        url: `${controller}/chats/${chatId}/commands`,
        method: constants.http.methods.GET,
        params: filter,
      }),
    }),
    setCommand: builder.mutation<void, ICommandPostRequest>({
      query: ({ chatId, command }) => ({
        url: `${controller}/chats/${chatId}/command`,
        method: constants.http.methods.POST,
        body: command,
      }),
    }),
  }),
});

export const { useGetCommandQuery, useLazyGetCommandQuery, useGetCommandsQuery, useLazyGetCommandsQuery, useSetCommandMutation } =
  kdmidApi;
