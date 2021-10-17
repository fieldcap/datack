export type Job = {
    jobId: string;
    name: string;
    cron: string;
    description: string;
    settings: JobSettings;
};

export type JobSettings = {};
