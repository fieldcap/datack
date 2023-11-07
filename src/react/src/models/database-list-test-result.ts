export type DatabaseListTestResult = {
  databaseName: string;
  hasNoAccess: boolean;
  hasNoFullBackup: boolean;
  isManualIncluded: boolean;
  isManualExcluded: boolean;
  isSystemDatabase: boolean;
  isRegexIncluded: boolean;
  isRegexExcluded: boolean;
  isBackupDefaultExcluded: boolean;
};
