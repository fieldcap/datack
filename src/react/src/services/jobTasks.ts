import axios, { CancelTokenSource } from 'axios';
import { DatabaseListTestResult } from '../models/database-list-test-result';
import { JobTask } from '../models/job-task';
import { ErrorHelper } from './error';

export namespace JobTasks {
    export const getForJob = async (
        jobId: string,
        cancelToken: CancelTokenSource
    ): Promise<JobTask[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<JobTask[]>(
            `/api/JobTasks/GetForJob/${jobId}`,
            config
        );
        return result.data;
    };

    export const getById = async (
        jobTaskId: string,
        cancelToken: CancelTokenSource
    ): Promise<JobTask> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<JobTask>(
            `/api/JobTasks/GetById/${jobTaskId}`,
            config
        );
        return result.data;
    };

    export const add = async (jobTask: JobTask): Promise<JobTask> => {
        try {
            const result = await axios.post<JobTask>(
                `/api/JobTasks/Add/`,
                jobTask
            );
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const update = async (jobTask: JobTask): Promise<void> => {
        try {
            await axios.put(`/api/JobTasks/Update/`, jobTask);
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const reOrder = async (
        jobId: string,
        jobTaskIds: string[]
    ): Promise<void> => {
        await axios.post<DatabaseListTestResult[]>(`/api/JobTasks/ReOrder/`, {
            jobId,
            jobTaskIds,
        });
    };

    export const testDatabaseRegex = async (
        backupDefaultExclude: boolean,
        backupIncludeRegex: string,
        backupExcludeRegex: string,
        backupExcludeSystemDatabases: boolean,
        backupIncludeManual: string,
        backupExcludeManual: string,
        agentId: string
    ): Promise<DatabaseListTestResult[]> => {
        try {
            const result = await axios.post<DatabaseListTestResult[]>(
                `/api/JobTasks/TestDatabaseRegex/`,
                {
                    backupDefaultExclude,
                    backupIncludeRegex,
                    backupExcludeRegex,
                    backupExcludeSystemDatabases,
                    backupIncludeManual,
                    backupExcludeManual,
                    agentId,
                }
            );
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const testDatabaseConnection = async (
        agentId: string,
        connectionString: string
    ): Promise<string> => {
        try {
            const result = await axios.post<string>(
                `/api/JobTasks/TestDatabaseConnection/`,
                {
                    agentId,
                    connectionString,
                }
            );
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };
}

export default JobTasks;
