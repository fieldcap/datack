import axios, { CancelTokenSource } from 'axios';
import { Agent } from '../models/agent';

export namespace Agents {
  export const getList = async (cancelToken: CancelTokenSource): Promise<Agent[]> => {
    const config = { cancelToken: cancelToken.token };
    const result = await axios.get<Agent[]>(`/api/Agents/List`, config);
    return result.data;
  };

  export const getById = async (agentId: string, cancelToken: CancelTokenSource): Promise<Agent> => {
    const config = { cancelToken: cancelToken.token };
    const result = await axios.get<Agent>(`/api/Agents/GetById/${agentId}`, config);
    return result.data;
  };

  export const add = async (agent: Agent, cancelToken: CancelTokenSource): Promise<string> => {
    const config = { cancelToken: cancelToken.token };
    const result = await axios.post<string>(`/api/Agents/Add/`, agent, config);
    return result.data;
  };

  export const update = async (agent: Agent, cancelToken: CancelTokenSource): Promise<void> => {
    const config = { cancelToken: cancelToken.token };
    await axios.put(`/api/Agents/Update/`, agent, config);
  };

  export const deleteAgent = async (agentId: string, cancelToken: CancelTokenSource): Promise<void> => {
    const config = { cancelToken: cancelToken.token };
    await axios.delete(`/api/Agents/Delete/${agentId}`, config);
  };

  export const upgradeAgent = async (agentId: string, cancelToken: CancelTokenSource): Promise<void> => {
    const config = { cancelToken: cancelToken.token };
    await axios.get(`/api/Agents/UpgradeAgent/${agentId}`, config);
  };
}

export default Agents;
