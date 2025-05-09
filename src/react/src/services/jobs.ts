import axios, { CancelTokenSource } from 'axios';
import { Job } from '../models/job';
import { BackupFile } from '../models/backup-file';

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

  export const run = async (jobId: string, itemList: string): Promise<string> => {
    const result = await axios.post<string>(`/api/Jobs/Run/`, {
      jobId,
      itemList,
    });
    return result.data;
  };

  export const getDatabaseList = async (jobId: string, cancelToken: CancelTokenSource): Promise<string[]> => {
    const config = { cancelToken: cancelToken.token };
    const result = await axios.post<string[]>(
      `/api/Jobs/GetDatabaseList/`,
      {
        jobId,
      },
      config
    );
    return result.data;
  };

  export const getAzureFileList = async (
    jobId: string,
    path: string | null,
    cancelToken: CancelTokenSource
  ): Promise<BackupFile[]> => {
    const config = { cancelToken: cancelToken.token };
    const result = await axios.post<BackupFile[]>(
      `/api/Jobs/GetAzureFileList/`,
      {
        jobId,
        path,
      },
      config
    );
    return result.data;
  };
}

export type TestCronResult = {
  description: string;
  next: string[];
};

export default Jobs;
