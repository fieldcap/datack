import { Job } from './job';

export type Step = {
    stepId: string;
    jobId: string;
    type: string;
    name: string;
    description: string;
    order: number;
    settings: StepSettings;
    job: Job;

    forceExpandRow: boolean;
};

export type StepSettings = {
    createBackup?: StepCreateBackupSettings;
};

export type StepCreateBackupSettings = {
    backupAllNonSystemDatabases: boolean;
};
