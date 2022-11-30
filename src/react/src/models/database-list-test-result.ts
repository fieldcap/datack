export type DatabaseListTestResult = {
  databaseName: string;
  hasNoAccess: boolean;
  isManualIncluded: boolean;
  isManualExcluded: boolean;
  isSystemDatabase: boolean;
  isRegexIncluded: boolean;
  isRegexExcluded: boolean;
  isBackupDefaultExcluded: boolean;
};
