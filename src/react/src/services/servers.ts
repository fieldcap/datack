import axios, { CancelTokenSource } from 'axios';
import { Server } from '../models/server';
import { ErrorHelper } from './error';

export namespace Servers {
    export const getList = async (
        cancelToken: CancelTokenSource
    ): Promise<Server[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Server[]>(`/api/Servers/List`, config);
        return result.data;
    };

    export const getById = async (
        serverId: string,
        cancelToken: CancelTokenSource
    ): Promise<Server> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Server>(
            `/api/Servers/GetById/${serverId}`,
            config
        );
        return result.data;
    };

    export const add = async (server: Server): Promise<Server> => {
        try {
            const result = await axios.post<Server>(
                `/api/Servers/Add/`,
                server
            );
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const update = async (server: Server): Promise<void> => {
        try {
            await axios.put(`/api/Servers/Update/`, server);
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const testSqlServerConnection = async (
        server: Server
    ): Promise<string> => {
        try {
            const result = await axios.post<string>(
                `/api/Servers/TestSqlServerConnection/`,
                server
            );
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const getDatabaseList = async (
        serverId: string
    ): Promise<string[]> => {
        try {
            const result = await axios.post<string[]>(
                `/api/Servers/GetDatabaseList/${serverId}`
            );
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };
}

export default Servers;
