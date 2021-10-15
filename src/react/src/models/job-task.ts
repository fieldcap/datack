import { Job } from './job';
import { Server } from './server';

export type JobTask = {
    jobTaskId: string;
    jobId: string;
    job?: Job | null;
    type: string;
    parallel: number;
    name: string;
    description: string;
    order: number;
    settings: JobTaskSettings;
    serverId: string;
    server?: Server | null;
};

export type JobTaskSettings = {
    createBackup?: JobTaskCreateDatabaseSettings;
};

export type JobTaskCreateDatabaseSettings = {
    fileName: string;
    backupDefaultExclude: boolean;
    backupIncludeRegex: string;
    backupExcludeRegex: string;
    backupExcludeSystemDatabases: boolean;
    backupIncludeManual: string;
    backupExcludeManual: string;
};
