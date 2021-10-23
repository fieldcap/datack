import { Job, JobSettings } from './job';

export type JobRun = {
    jobRunId: string;
    jobId: string;
    job: Job;
    started: string;
    completed: string | null;
    runTime: number | null;
    isError: boolean;
    result: string | null;
    settings: JobSettings;
};
