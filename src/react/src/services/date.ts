import { formatDuration, intervalToDuration, parseISO } from 'date-fns';
import { JobRun } from '../models/job-run';
import { JobRunTask } from '../models/job-run-task';

export const formatRuntime = (jobRun: JobRun): string => {
    if (jobRun == null) {
        return '';
    }
    if (jobRun.completed == null) {
        return formatDuration(intervalToDuration({ start: parseISO(jobRun.started), end: new Date() }));
    }

    if (jobRun.runTime != null) {
        if (jobRun.runTime <= 0) {
            return '< 0 seconds';
        }
        return formatDuration(intervalToDuration({ start: 0, end: jobRun.runTime * 1000 }));
    }

    return '';
};

export const formatRuntimeTask = (jobRunTask: JobRunTask): string => {
    if (jobRunTask == null) {
        return '';
    }

    if (jobRunTask.started != null && jobRunTask.completed != null) {
        formatDuration(intervalToDuration({ start: parseISO(jobRunTask.started), end: new Date() }));
    }

    if (jobRunTask.runTime != null) {
        if (jobRunTask.runTime <= 0) {
            return '< 0 seconds';
        }
        return formatDuration(intervalToDuration({ start: 0, end: jobRunTask.runTime * 1000 }));
    }
    return '';
};
