import axios, { CancelTokenSource } from 'axios';
import { DatabaseListTestResult } from '../models/database-list-test-result';
import { JobTask } from '../models/job-task';

export namespace JobTasks {
  export const map = (name: string | null | undefined): string => {
    if (name == null) {
      return '';
    }
    switch (name) {
      case 'createBackup':
        return 'Create Database Backup';
      case 'compress':
        return 'Compress File';
      case 'deleteFile':
        return 'Delete File from filesystem';
      case 'deleteS3':
        return 'Delete from AWS S3';
      case 'downloadS3':
        return 'Download File from AWS S3';
      case 'downloadAzure':
        return 'Download File from Azure Blobs';
      case 'extract':
        return 'Extract File';
      case 'restoreBackup':
        return 'Restore Database Backup';
      case 'uploadAzure':
        return 'Upload File to Azure Blobs';
      case 'uploadS3':
        return 'Upload File to AWS S3';
      default:
        return name;
    }
  };

  export const getForJob = async (jobId: string, cancelToken: CancelTokenSource): Promise<JobTask[]> => {
    const config = { cancelToken: cancelToken.token };
    const result = await axios.get<JobTask[]>(`/api/JobTasks/GetForJob/${jobId}`, config);
    return result.data;
  };

  export const getById = async (jobTaskId: string, cancelToken: CancelTokenSource): Promise<JobTask> => {
    const config = { cancelToken: cancelToken.token };
    const result = await axios.get<JobTask>(`/api/JobTasks/GetById/${jobTaskId}`, config);
    return result.data;
  };

  export const add = async (jobTask: JobTask, cancelToken: CancelTokenSource): Promise<JobTask> => {
    const config = { cancelToken: cancelToken.token };
    const result = await axios.post<JobTask>(`/api/JobTasks/Add/`, jobTask, config);
    return result.data;
  };

  export const update = async (jobTask: JobTask, cancelToken: CancelTokenSource): Promise<void> => {
    const config = { cancelToken: cancelToken.token };
    await axios.put(`/api/JobTasks/Update/`, jobTask, config);
  };

  export const deleteJobTask = async (jobTaskId: string, cancelToken: CancelTokenSource): Promise<void> => {
    const config = { cancelToken: cancelToken.token };
    await axios.delete(`/api/JobTasks/Delete/${jobTaskId}`, config);
  };

  export const reOrder = async (jobId: string, jobTaskIds: string[], cancelToken: CancelTokenSource): Promise<void> => {
    const config = { cancelToken: cancelToken.token };
    await axios.post<DatabaseListTestResult[]>(
      `/api/JobTasks/ReOrder/`,
      {
        jobId,
        jobTaskIds,
      },
      config
    );
  };

  export const testDatabaseRegex = async (
    backupDefaultExclude: boolean,
    backupIncludeRegex: string,
    backupExcludeRegex: string,
    backupExcludeSystemDatabases: boolean,
    backupIncludeManual: string,
    backupExcludeManual: string,
    backupType: string,
    agentId: string,
    jobTaskId: string,
    databaseType: string,
    connectionString: string,
    connectionStringPassword: string | null,
    cancelToken: CancelTokenSource
  ): Promise<DatabaseListTestResult[]> => {
    const config = { cancelToken: cancelToken.token };
    const result = await axios.post<DatabaseListTestResult[]>(
      `/api/JobTasks/TestDatabaseRegex/`,
      {
        backupDefaultExclude,
        backupIncludeRegex,
        backupExcludeRegex,
        backupExcludeSystemDatabases,
        backupIncludeManual,
        backupExcludeManual,
        backupType,
        agentId,
        jobTaskId,
        databaseType,
        connectionString,
        connectionStringPassword,
      },
      config
    );
    return result.data;
  };

  export const testDatabaseConnection = async (
    agentId: string,
    jobTaskId: string,
    databaseType: string,
    connectionString: string,
    connectionStringPassword: string | null,
    cancelToken: CancelTokenSource
  ): Promise<string> => {
    const config = { cancelToken: cancelToken.token };
    const result = await axios.post<string>(
      `/api/JobTasks/TestDatabaseConnection/`,
      {
        agentId,
        jobTaskId,
        databaseType,
        connectionString,
        connectionStringPassword,
      },
      config
    );
    return result.data;
  };

  export const testFilesRegex = async (
    restoreDefaultExclude: boolean,
    restoreIncludeRegex: string,
    restoreExcludeRegex: string,
    restoreIncludeManual: string,
    restoreExcludeManual: string,
    agentId: string,
    jobTaskId: string,
    containerName: string,
    blob: string,
    connectionString: string,
    cancelToken: CancelTokenSource
  ): Promise<DatabaseListTestResult[]> => {
    const config = { cancelToken: cancelToken.token };
    const result = await axios.post<DatabaseListTestResult[]>(
      `/api/JobTasks/TestFilesRegex/`,
      {
        restoreDefaultExclude: restoreDefaultExclude ?? false,
        restoreIncludeRegex: restoreIncludeRegex ?? '',
        restoreExcludeRegex: restoreExcludeRegex ?? '',
        restoreIncludeManual: restoreIncludeManual ?? '',
        restoreExcludeManual: restoreExcludeManual ?? '',
        agentId,
        jobTaskId,
        containerName,
        blob,
        connectionString,
      },
      config
    );
    return result.data;
  };
}

export default JobTasks;
