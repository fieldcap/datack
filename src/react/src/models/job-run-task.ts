import { JobRun } from './job-run';
import { JobTask, JobTaskSettings } from './job-task';

export type JobRunTask = {
  jobRunTaskId: string;
  jobTaskId: string;
  jobTask?: JobTask | null;
  jobRunId: string;
  jobRun?: JobRun | null;
  started: string | null;
  completed: string | null;
  runTime: number | null;
  type: string;
  itemName: string;
  taskOrder: number;
  itemOrder: number;
  isError: boolean;
  result: string | null;
  resultArtifact: string | null;
  settings: JobTaskSettings;
};
