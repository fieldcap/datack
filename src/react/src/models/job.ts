import { Server } from './server';

export type Job = {
    jobId: string;
    name: string;
    settings: JobSettings;
    serverId: string;
    server: Server;
};

export type JobSettings = {};
