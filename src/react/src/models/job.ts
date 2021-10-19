export type Job = {
    jobId: string;
    name: string;
    group: string;
    priority: number;
    cron: string;
    description: string;
    settings: JobSettings;
};

export type JobSettings = {};
