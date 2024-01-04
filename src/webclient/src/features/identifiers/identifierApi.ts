import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { constants } from "../../_constants";
import { WebApiResponseType } from "../../_types";
import {
  ICommand,
  ICommandGetRequest,
  ICommandPostRequest,
} from "./identifierTypes";

const controller = "chats";

export const identifierApi = createApi({
  reducerPath: "identifierApi",
  baseQuery: fetchBaseQuery({ baseUrl: constants.http.baseFetchUrl }),
  endpoints: (builder) => ({
    getCommand: builder.query<WebApiResponseType<ICommand>, ICommandGetRequest>(
      {
        query: ({ chatId, commandId }) => ({
          url: `${controller}/${chatId}/commands/${commandId}`,
          method: constants.http.methods.GET,
        }),
      }
    ),
    updateCommand: builder.mutation<
      WebApiResponseType<ICommand>,
      ICommandPostRequest
    >({
      query: ({ chatId, command }) => ({
        url: `${controller}/${chatId}/commands/${command.id}`,
        method: constants.http.methods.POST,
        body: command,
      }),
    }),
  }),
});

export const { useGetCommandQuery, useUpdateCommandMutation } = identifierApi;
