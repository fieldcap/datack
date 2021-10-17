import { Job, JobSettings } from './job';

export type JobRun = {
    jobRunId: string;
    jobId: string;
    job: Job;
    backupType: 'Full' | 'Diff' | 'Log';
    started: Date;
    completed: Date | null;
    runTime: number | null;
    isError: boolean;
    result: string | null;
    settings: JobSettings;
};
