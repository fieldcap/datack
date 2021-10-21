import axios, { CancelTokenSource } from 'axios';
import { Agent } from '../models/agent';
import { ErrorHelper } from './error';

export namespace Agents {
    export const getList = async (
        cancelToken: CancelTokenSource
    ): Promise<Agent[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Agent[]>(`/api/Agents/List`, config);
        return result.data;
    };

    export const getById = async (
        agentId: string,
        cancelToken: CancelTokenSource
    ): Promise<Agent> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Agent>(
            `/api/Agents/GetById/${agentId}`,
            config
        );
        return result.data;
    };

    export const add = async (agent: Agent): Promise<Agent> => {
        try {
            const result = await axios.post<Agent>(`/api/Agents/Add/`, agent);
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const update = async (agent: Agent): Promise<void> => {
        try {
            await axios.put(`/api/Agents/Update/`, agent);
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const testDatabaseConnection = async (
        agentId: string,
        connectionString: string
    ): Promise<string> => {
        try {
            const result = await axios.post<string>(
                `/api/Agents/TestDatabaseConnection/`,
                {
                    agentId,
                    connectionString,
                }
            );
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };
}

export default Agents;
