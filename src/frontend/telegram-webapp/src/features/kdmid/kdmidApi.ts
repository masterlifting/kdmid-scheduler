/** @format */

import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { constants } from '../../_constants';
import {
  CityGetDto,
  CommandGetDto,
  ICommandGetRequest,
  ICommandPostRequest,
  ICommandPutRequest,
  ICommandsGetRequest,
} from './kdmidTypes';

const controller = 'bot';

export const kdmidApi = createApi({
  reducerPath: 'KdmidApi',
  baseQuery: fetchBaseQuery({ baseUrl: constants.config.backendBaseUrl }),
  endpoints: builder => ({
    getCities: builder.query<CityGetDto[], void>({
      query: () => ({
        url: `${controller}/cities`,
        method: constants.http.methods.GET,
      }),
    }),
    getCommand: builder.query<CommandGetDto, ICommandGetRequest>({
      query: ({ chatId, commandId }) => ({
        url: `${controller}/chats/${chatId}/commands/${commandId}`,
        method: constants.http.methods.GET,
      }),
    }),
    getCommands: builder.query<CommandGetDto[], ICommandsGetRequest>({
      query: ({ chatId, names, cityCode }) => ({
        url: `${controller}/chats/${chatId}/commands`,
        method: constants.http.methods.GET,
        params: { names, cityCode },
      }),
    }),
    createCommand: builder.mutation<string, ICommandPostRequest>({
      query: ({ chatId, command }) => ({
        url: `${controller}/chats/${chatId}/commands`,
        method: constants.http.methods.POST,
        body: command,
      }),
    }),
    updateCommand: builder.mutation<void, ICommandPutRequest>({
      query: ({ chatId, commandId, command }) => ({
        url: `${controller}/chats/${chatId}/commands/${commandId}`,
        method: constants.http.methods.PUT,
        body: command,
      }),
    }),
    deleteCommand: builder.mutation<void, ICommandGetRequest>({
      query: ({ chatId, commandId }) => ({
        url: `${controller}/chats/${chatId}/commands/${commandId}`,
        method: constants.http.methods.DELETE,
      }),
    }),
  }),
});

export const {
  useGetCitiesQuery,
  useLazyGetCitiesQuery,
  useGetCommandQuery,
  useLazyGetCommandQuery,
  useGetCommandsQuery,
  useLazyGetCommandsQuery,
  useCreateCommandMutation,
  useUpdateCommandMutation,
  useDeleteCommandMutation,
} = kdmidApi;
