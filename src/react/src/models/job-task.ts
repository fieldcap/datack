import { Agent } from './agent';
import { Job } from './job';

export type JobTask = {
    jobTaskId: string;
    jobId: string;
    job?: Job | null;
    isActive: boolean;
    type: string;
    parallel: number;
    maxItemsToKeep: number;
    name: string;
    description: string;
    order: number;
    timeout: number | null;
    usePreviousTaskArtifactsFromJobTaskId: string | null;
    usePreviousTaskArtifactsFromJobTask?: JobTask | null;
    settings: JobTaskSettings;
    agentId: string;
    agent?: Agent | null;
};

export type JobTaskSettings = {
    createBackup?: JobTaskCreateDatabaseSettings;
    compress?: JobTaskCompressSettings;
    deleteFile?: JobTaskDeleteSettings;
    deleteS3?: JobTaskDeleteS3Settings;
    uploadAzure?: JobTaskUploadAzureSettings;
    uploadS3?: JobTaskUploadS3Settings;
};

export type JobTaskCreateDatabaseSettings = {
    connectionString: string;
    connectionStringPassword: string | null;
    fileName: string;
    options: string;
    backupType: string;
    backupDefaultExclude: boolean;
    backupIncludeRegex: string;
    backupExcludeRegex: string;
    backupExcludeSystemDatabases: boolean;
    backupIncludeManual: string;
    backupExcludeManual: string;
};

export type JobTaskCompressSettings = {
    fileName: string;
    archiveType: string;
    compressionLevel: string;
    multithreadMode: string;
    password: string | null;
};

export type JobTaskDeleteS3Settings = {
    fileName: string;
    region: string;
    bucket: string;
    accessKey: string;
    secret: string;
    tag: string;
    timeSpanType: string;
    timeSpanAmount: number;
};

export type JobTaskDeleteSettings = {
    ignoreIfFileDoesNotExist: boolean;
};

export type JobTaskUploadS3Settings = {
    fileName: string;
    region: string;
    bucket: string;
    accessKey: string;
    secret: string;
    tag: string;
};

export type JobTaskUploadAzureSettings = {
    fileName: string;
    containerName: string;
    connectionString: string;
    tag: string;
};
