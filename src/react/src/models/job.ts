export type Job = {
  jobId: string;
  name: string;
  isActive: boolean;
  group: string;
  priority: number;
  cron: string;
  description: string;
  deleteLogsTimeSpanAmount: number | null;
  deleteLogsTimeSpanType: string | null;
  settings: JobSettings;
};

export type JobSettings = {
  emailOnError: boolean;
  emailOnSuccess: boolean;
  emailTo: string | null;
};
