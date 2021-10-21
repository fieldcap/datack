import axios, { CancelTokenSource } from 'axios';
import { Setting } from '../models/setting';
import { ErrorHelper } from './error';

export namespace Settings {
    export const getList = async (cancelToken: CancelTokenSource): Promise<Setting[]> => {
        const config = { cancelToken: cancelToken.token };
        const result = await axios.get<Setting[]>(`/api/Settings`, config);
        return result.data;
    };

    export const update = async (settings: Setting[]): Promise<void> => {
        try {
            await axios.put(`/api/Settings`, settings);
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };

    export const testEmail = async (to: string): Promise<void> => {
        try {
            await axios.post(`/api/Settings/TestEmail`, { to });
        } catch (err) {
            throw ErrorHelper.getError(err);
        }
    };
}

export default Settings;
