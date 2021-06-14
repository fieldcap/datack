import { AxiosError } from 'axios';

export namespace ErrorHelper {
    export const getError = (error: any): string => {
        if (isAxiosError(error)) {
            return error.message;
        } else if (isError(error)) {
            return error.message;
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
