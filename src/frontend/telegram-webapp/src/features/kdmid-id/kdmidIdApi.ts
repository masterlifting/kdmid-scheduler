/** @format */

import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { constants } from '../../_constants';
import { ICommandGetRequest, ICommandPostRequest } from './kdmidIdTypes';

const controller = 'chats';

export const kdmidIdApi = createApi({
  reducerPath: 'kdmidIdApi',
  baseQuery: fetchBaseQuery({ baseUrl: constants.config.backendBaseUrl }),
  endpoints: builder => ({
    getCommand: builder.query<any, ICommandGetRequest>({
      query: ({ chatId, commandId }) => ({
        url: `${controller}/${chatId}/commands/${commandId}`,
        method: constants.http.methods.GET,
      }),
    }),
    updateCommand: builder.mutation<void, ICommandPostRequest>({
      query: ({ chatId, command }) => ({
        url: `${controller}/${chatId}`,
        method: constants.http.methods.POST,
        body: command,
      }),
    }),
  }),
});

export const { useGetCommandQuery, useUpdateCommandMutation } = kdmidIdApi;
