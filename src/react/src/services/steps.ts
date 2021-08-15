import axios, { CancelTokenSource } from 'axios';
import { DatabaseListTestResult } from '../models/database-list-test-result';
import { Step, StepCreateBackupSettings } from '../models/step';
import { ErrorHelper } from './error';

export namespace Steps {
    export const getForJob = async (
        jobId: string,
        cancelToken: CancelTokenSource
    ): Promise<Step[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Step[]>(
            `/api/Steps/GetForJob/${jobId}`,
            config
        );
        return result.data;
    };

    export const getById = async (
        stepId: string,
        cancelToken: CancelTokenSource
    ): Promise<Step> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Step>(
            `/api/Steps/GetById/${stepId}`,
            config
        );
        return result.data;
    };

    export const add = async (step: Step): Promise<Step> => {
        try {
            const result = await axios.post<Step>(`/api/Steps/Add/`, step);
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const update = async (step: Step): Promise<void> => {
        try {
            await axios.put(`/api/Steps/Update/`, step);
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const testDatabaseRegex = async (
        settings: StepCreateBackupSettings,
        serverId: string
    ): Promise<DatabaseListTestResult[]> => {
        try {
            const result = await axios.post<DatabaseListTestResult[]>(
                `/api/Steps/TestDatabaseRegex/`,
                {
                    settings,
                    serverId,
                }
            );
            return result.data;
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };
}

export default Steps;
