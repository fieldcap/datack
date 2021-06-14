import axios, { CancelTokenSource } from 'axios';
import { Server, ServerDbSettings } from '../models/server';
import { ErrorHelper } from './error';

export namespace Servers {
    export const getList = async (): Promise<Server[]> => {
        const result = await axios.get<Server[]>(`/api/Servers/List`);
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

    export const updateDbSettings = async (
        serverId: string,
        dbSettings: ServerDbSettings
    ): Promise<void> => {
        try {
            await axios.put(
                `/api/Servers/UpdateDbSettings/${serverId}`,
                dbSettings
            );
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };
}

export default Servers;
