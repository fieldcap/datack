import axios, { CancelTokenSource } from 'axios';
import { Agent } from '../models/agent';

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

    export const add = async (agent: Agent): Promise<string> => {
        const result = await axios.post<string>(`/api/Agents/Add/`, agent);
        return result.data;
    };

    export const update = async (agent: Agent): Promise<void> => {
        await axios.put(`/api/Agents/Update/`, agent);
    };
}

export default Agents;
