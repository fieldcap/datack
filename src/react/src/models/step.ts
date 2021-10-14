import { Job } from './job';
import { Server } from './server';

export type Step = {
    stepId: string;
    jobId: string;
    type: string;
    name: string;
    description: string;
    order: number;
    settings: StepSettings;
    job?: Job;
    serverId: string;
    server?: Server;
};

export type StepSettings = {
    createBackup?: StepCreateBackupSettings;
};

export type StepCreateBackupSettings = {
    fileName: string;
    backupDefaultExclude: boolean;
    backupIncludeRegex: string;
    backupExcludeRegex: string;
    backupExcludeSystemDatabases: boolean;
    backupIncludeManual: string;
    backupExcludeManual: string;
};
