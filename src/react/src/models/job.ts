export type Job = {
    jobId: string;
    name: string;
    description: string;
    settings: JobSettings;
};

export type JobSettings = {
    cronFull?: string;
    cronDiff?: string;
    cronLog?: string;
};
