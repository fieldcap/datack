import { AxiosError } from 'axios';

export namespace ErrorHelper {
  export const getError = (error: any): string => {
    if (isAxiosError(error)) {
      if (error.response == null) {
        return error.message;
      }
      error = error.response;
    }

    if (isError(error)) {
      return error.message;
    }

    if (error.data) {
      return error.data;
    }

    if (error.status) {
      return error.statusText;
    }

    return error.toString();
  };

  function isAxiosError(error: any): error is AxiosError {
    return (error as AxiosError).isAxiosError !== undefined;
  }

  function isError(error: any): error is Error {
    return (error as Error).message !== undefined;
  }
}
