import axios, { CancelTokenSource } from 'axios';
import { Job } from '../models/job';
import { ErrorHelper } from './error';

export namespace Jobs {
    export const getList = async (
        cancelToken: CancelTokenSource
    ): Promise<Job[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Job[]>(`/api/Jobs/List`, config);
        return result.data;
    };

    export const getForServer = async (
        serverId: string,
        cancelToken: CancelTokenSource
    ): Promise<Job[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Job[]>(
            `/api/Jobs/GetForServer/${serverId}`,
            config
        );
        return result.data;
    };

    export const getById = async (
        jobId: string,
        cancelToken: CancelTokenSource
    ): Promise<Job> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Job>(
            `/api/Jobs/GetById/${jobId}`,
            config
        );
        return result.data;
    };

    export const add = async (job: Job): Promise<Job> => {
        try {
            const result = await axios.post<Job>(`/api/Jobs/Add/`, job);
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const update = async (job: Job): Promise<void> => {
        try {
            await axios.put(`/api/Jobs/Update/`, job);
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const testCron = async (cron: string) => {
        try {
            const result = await axios.post<TestCronResult>(
                `/api/Jobs/ParseCron/`,
                {
                    cron,
                }
            );
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const run = async (jobId: string): Promise<string> => {
        try {
            const result = await axios.post<string>(`/api/Jobs/Run/`, {
                jobId,
            });
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };
}

export type TestCronResult = {
    result: string;
    next: Date[];
};

export default Jobs;
