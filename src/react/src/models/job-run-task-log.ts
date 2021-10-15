import { JobRunTask } from './job-run-task';

export type JobRunTaskLog = {
    jobRunTaskLogId: number;
    jobRunTaskId: string;
    jobRunTask?: JobRunTask | null;
    dateTime: Date;
    isError: boolean;
    message: string;
};
