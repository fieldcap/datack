import { JobRun } from "./job-run";
import { JobTask, JobTaskSettings } from "./job-task";

export type JobRunTask = {
    jobRunTaskId: string;
    jobTaskId: string;
    jobTask?: JobTask | null;
    jobRunId: string;
    jobRun?: JobRun | null;
    started: Date | null;
    completed: Date | null;
    runTime: number | null;
    itemName: string;
    type: string;
    parallel: number;
    order: number;
    isError: boolean;
    result: string | null;
    resultArtifact: string | null;
    settings: JobTaskSettings;
};
