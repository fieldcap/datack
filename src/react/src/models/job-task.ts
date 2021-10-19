import { Job } from './job';
import { Server } from './server';

export type JobTask = {
    jobTaskId: string;
    jobId: string;
    job?: Job | null;
    type: string;
    parallel: number;
    name: string;
    description: string;
    order: number;
    timeout: number | null;
    usePreviousTaskArtifactsFromJobTaskId: string | null;
    settings: JobTaskSettings;
    serverId: string;
    server?: Server | null;
};

export type JobTaskSettings = {
    createBackup?: JobTaskCreateDatabaseSettings;
    compress?: JobTaskCompressSettings;
    delete?: JobTaskDeleteSettings;
    uploadS3?: JobTaskUploadS3Settings;
    uploadAzure?: JobTaskUploadAzureSettings;
};

export type JobTaskCreateDatabaseSettings = {
    fileName: string;
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

export type JobTaskDeleteSettings = {
    ignoreIfFileDoesNotExist: boolean;
};

export type JobTaskUploadS3Settings = {
    fileName: string;
    region: string;
    bucket: string;
    accessKey: string;
    secret: string;
};

export type JobTaskUploadAzureSettings = {
    fileName: string;
    containerName: string;
    connectionString: string;
};
