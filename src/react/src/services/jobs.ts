import axios, { CancelTokenSource } from 'axios';
import { Job, JobSettings } from '../models/job';
import { ErrorHelper } from './error';

export namespace Jobs {
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

    export const updateSettings = async (
        jobId: string,
        settings: JobSettings
    ): Promise<void> => {
        try {
            await axios.put(`/api/Jobs/UpdateSettings/${jobId}`, settings);
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };
}

export default Jobs;
