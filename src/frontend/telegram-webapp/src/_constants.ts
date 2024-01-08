/** @format */

export const constants = {
  config: {
    backendBaseUrl:
      process.env.REACT_APP_BACKEND_URL ||
      (() => {
        throw new Error('REACT_APP_BACKEND_URL is not defined');
      })(),
  },
  http: {
    methods: {
      GET: 'GET',
      POST: 'POST',
      PUT: 'PUT',
      DELETE: 'DELETE',
    },
    defaultErrorMessage: 'Something went wrong',
  },
};
