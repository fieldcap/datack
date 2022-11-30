export type Agent = {
  agentId: string;
  name: string;
  description: string;
  key: string;
  settings: AgentSettings;

  status?: string | null;
  version?: string | null;
};

export type AgentSettings = {};
