import axios from 'axios';
import { ErrorHelper } from './error';

export namespace Auth {
    export const hasAuthToken = (): boolean => {
        const tokenId = localStorage.getItem('isAuthenticated');
        return tokenId != null;
    };

    export const isSetup = async () => {
        try {
            const result = await axios.get<boolean>(
                `/api/Authentication/IsSetup`
            );
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const login = async (
        userName: string,
        password: string,
        rememberMe: boolean
    ): Promise<void> => {
        try {
            const result = await axios.post(`/api/Authentication/Login`, {
                userName,
                password,
                rememberMe,
            });
            localStorage.setItem('isAuthenticated', result.data);
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const logout = async (): Promise<void> => {
        localStorage.removeItem('isAuthenticated');
        await axios.post(`/api/Authentication/Logout`, {});
    };
}

export default Auth;
