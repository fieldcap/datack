import axios, { CancelTokenSource } from 'axios';
import { Job } from '../models/job';

export namespace Jobs {
    export const getList = async (cancelToken: CancelTokenSource): Promise<Job[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Job[]>(`/api/Jobs/List`, config);
        return result.data;
    };

    export const getForAgent = async (agentId: string, cancelToken: CancelTokenSource): Promise<Job[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Job[]>(`/api/Jobs/GetForAgent/${agentId}`, config);
        return result.data;
    };

    export const getById = async (jobId: string, cancelToken: CancelTokenSource): Promise<Job> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Job>(`/api/Jobs/GetById/${jobId}`, config);
        return result.data;
    };

    export const add = async (job: Job): Promise<string> => {
        const result = await axios.post<string>(`/api/Jobs/Add/`, job);
        return result.data;
    };

    export const update = async (job: Job): Promise<void> => {
        await axios.put(`/api/Jobs/Update/`, job);
    };

    export const duplicate = async (jobId: string): Promise<Job> => {
        var result = await axios.post<Job>(`/api/Jobs/Duplicate/`, {
            jobId,
        });
        return result.data;
    };

    export const deleteJob = async (jobId: string): Promise<void> => {
        await axios.delete<void>(`/api/Jobs/Delete/${jobId}`);
    };

    export const testCron = async (cron: string) => {
        const result = await axios.post<TestCronResult>(`/api/Jobs/ParseCron/`, {
            cron,
        });
        return result.data;
    };

    export const run = async (jobId: string): Promise<string> => {
        const result = await axios.post<string>(`/api/Jobs/Run/`, {
            jobId,
        });
        return result.data;
    };
}

export type TestCronResult = {
    description: string;
    next: Date[];
};

export default Jobs;
