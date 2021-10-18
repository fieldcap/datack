import axios, { CancelTokenSource } from 'axios';
import { JobRun } from '../models/job-run';
import { JobRunTask } from '../models/job-run-task';
import { JobRunTaskLog } from '../models/job-run-task-log';

export namespace JobRuns {
    export const getList = async (
        cancelToken: CancelTokenSource
    ): Promise<JobRun[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<JobRun[]>(
            `/api/JobRuns/GetList`,
            config
        );
        return result.data;
    };

    export const getForJob = async (
        jobId: string,
        cancelToken: CancelTokenSource
    ): Promise<JobRun[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<JobRun[]>(
            `/api/JobRuns/GetForJob/${jobId}`,
            config
        );
        return result.data;
    };

    export const getById = async (
        jobRunId: string,
        cancelToken: CancelTokenSource
    ): Promise<JobRun> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<JobRun>(
            `/api/JobRuns/GetById/${jobRunId}`,
            config
        );
        return result.data;
    };

    export const getTasks = async (
        jobRunId: string,
        cancelToken: CancelTokenSource
    ): Promise<JobRunTask[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<JobRunTask[]>(
            `/api/JobRuns/GetTasks/${jobRunId}`,
            config
        );
        return result.data;
    };

    export const getTaskLogs = async (
        jobRunId: string,
        cancelToken: CancelTokenSource
    ): Promise<JobRunTaskLog[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<JobRunTaskLog[]>(
            `/api/JobRuns/GetTaskLogs/${jobRunId}`,
            config
        );
        return result.data;
    };

    export const stop = async (
        jobRunId: string,
        cancelToken: CancelTokenSource
    ): Promise<void> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.post<void>(
            `/api/JobRuns/Stop/`,
            {
                jobRunId,
            },
            config
        );
        return result.data;
    };
}

export default JobRuns;
