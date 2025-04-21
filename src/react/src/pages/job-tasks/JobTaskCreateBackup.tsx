import {
  Alert,
  AlertDescription,
  AlertIcon,
  Box,
  Button,
  Checkbox,
  FormControl,
  FormHelperText,
  FormLabel,
  Heading,
  Input,
  Select,
  Table,
  Tbody,
  Td,
  Text,
  Thead,
  Tr,
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { FaMinus, FaPlus } from 'react-icons/fa';
import useCancellationToken from '../../hooks/useCancellationToken';
import { DatabaseListTestResult } from '../../models/database-list-test-result';
import { JobTaskCreateDatabaseSettings } from '../../models/job-task';
import JobTasks from '../../services/jobTasks';

type Props = {
  agentId: string;
  jobTaskId: string;
  settings: JobTaskCreateDatabaseSettings | undefined | null;
  onSettingsChanged: (settings: JobTaskCreateDatabaseSettings) => void;
};

const JobTaskCreateBackup: FC<Props> = (props) => {
  const { onSettingsChanged } = props;

  const [isTesting, setIsTesting] = useState<boolean>(false);
  const [testingSuccess, setTestingSuccess] = useState<string | null>(null);
  const [testingError, setTestingError] = useState<string | null>(null);

  const [testResult, setTestResult] = useState<DatabaseListTestResult[]>([]);

  const cancelToken = useCancellationToken();

  useEffect(() => {
    if (props.settings == null) {
      onSettingsChanged({
        databaseType: 'sqlServer',
        fileName: '',
        options: '',
        backupType: 'Full',
        backupDefaultExclude: false,
        backupExcludeSystemDatabases: true,
        backupExcludeRegex: '',
        backupIncludeRegex: '',
        backupIncludeManual: '',
        backupExcludeManual: '',
        connectionString: '',
        connectionStringPassword: null,
      });
    }
  }, [props.settings, onSettingsChanged]);

  useEffect(() => {
    if (
      props.agentId == null ||
      props.agentId === '' ||
      props.agentId === '00000000-0000-0000-0000-000000000000' ||
      props.settings == null
    ) {
      return;
    }

    (async () => {
      const result = await JobTasks.testDatabaseRegex(
        props.settings!.backupDefaultExclude,
        props.settings!.backupIncludeRegex,
        props.settings!.backupExcludeRegex,
        props.settings!.backupExcludeSystemDatabases,
        props.settings!.backupIncludeManual,
        props.settings!.backupExcludeManual,
        props.settings!.backupType,
        props.agentId,
        props.jobTaskId,
        props.settings!.databaseType,
        props.settings!.connectionString,
        props.settings!.connectionStringPassword,
        cancelToken
      );
      setTestResult(result);
    })();
  }, [
    props.settings?.backupDefaultExclude,
    props.settings?.backupExcludeSystemDatabases,
    props.settings?.backupExcludeRegex,
    props.settings?.backupIncludeRegex,
    props.settings?.backupIncludeManual,
    props.settings?.backupExcludeManual,
    props.agentId,
    testingSuccess,
  ]);

  const set = (settingName: keyof JobTaskCreateDatabaseSettings, newValue: string | number | boolean): void => {
    if (props.settings == null) {
      return;
    }
    props.onSettingsChanged({
      ...props.settings,
      [settingName]: newValue,
    });
  };

  const handleTestDatabaseConnection = async (event: React.FormEvent<HTMLButtonElement>) => {
    event.preventDefault();
    setIsTesting(true);
    setTestingError(null);
    setTestingSuccess(null);

    try {
      const testResult = await JobTasks.testDatabaseConnection(
        props.agentId,
        props.jobTaskId,
        props.settings!.databaseType,
        props.settings!.connectionString,
        props.settings!.connectionStringPassword,
        cancelToken
      );

      if (testResult !== 'Success') {
        setTestingError(testResult);
      } else {
        setTestingSuccess(testResult);
      }
    } catch (err: any) {
      setTestingError(err);
    }
    setIsTesting(false);
  };

  const getCheckBoxIncludeValue = (name: string): boolean => {
    if (props.settings == null) {
      return false;
    }

    if (props.settings.backupIncludeManual == null) {
      props.settings.backupIncludeManual = '';
    }

    let includedDatabases = props.settings.backupIncludeManual.split(',');

    return includedDatabases.indexOf(name) > -1;
  };

  const onCheckBoxIncludeChange = (name: string, checked: boolean) => {
    if (props.settings == null) {
      return;
    }

    if (props.settings.backupIncludeManual == null) {
      props.settings.backupIncludeManual = '';
    }

    let includedDatabases = props.settings.backupIncludeManual.split(',');
    includedDatabases = includedDatabases.filter((m) => m !== name && m !== '');
    if (checked) {
      includedDatabases.push(name);
    }

    props.onSettingsChanged({
      ...props.settings,
      backupIncludeManual: includedDatabases.join(','),
    });
  };

  const getCheckBoxExcludeValue = (name: string): boolean => {
    if (props.settings == null) {
      return false;
    }

    if (props.settings.backupExcludeManual == null) {
      props.settings.backupExcludeManual = '';
    }

    let excludedDatabases = props.settings.backupExcludeManual.split(',');

    return excludedDatabases.indexOf(name) > -1;
  };

  const onCheckBoxExcludeChange = (name: string, checked: boolean) => {
    if (props.settings == null) {
      return;
    }

    if (props.settings.backupExcludeManual == null) {
      props.settings.backupExcludeManual = '';
    }

    let excludedDatabases = props.settings.backupExcludeManual.split(',');
    excludedDatabases = excludedDatabases.filter((m) => m !== name && m !== '');
    if (checked) {
      excludedDatabases.push(name);
    }

    props.onSettingsChanged({
      ...props.settings,
      backupExcludeManual: excludedDatabases.join(','),
    });
  };

  const getDatabaseTestResult = (database: DatabaseListTestResult) => {
    if (
      database.hasNoAccess ||
      database.hasNoFullBackup ||
      database.isManualExcluded ||
      database.isRegexExcluded ||
      database.isSystemDatabase ||
      database.isBackupDefaultExcluded
    ) {
      return <span style={{ textDecoration: 'line-through' }}>{database.databaseName}</span>;
    }

    return <span>{database.databaseName}</span>;
  };

  const getDatabaseTestResult2 = (database: DatabaseListTestResult) => {
    if (database.hasNoAccess) {
      return 'Excluded because user has no access to database';
    }
    if (database.hasNoFullBackup) {
      return 'Excluded because database has no FULL backup';
    }
    if (database.isManualExcluded) {
      return 'Excluded because database is manually excluded';
    }
    if (database.isManualIncluded) {
      return 'Included because database is manually included';
    }
    if (database.isSystemDatabase) {
      return 'Excluded because database is a system database';
    }
    if (database.isRegexExcluded) {
      return 'Excluded because database matches "Exclude Regex"';
    }
    if (database.isRegexIncluded) {
      return 'Included because database matches "Include Regex"';
    }
    if (database.isBackupDefaultExcluded) {
      return 'Excluded because database does not match any rules"';
    }
    return 'Included because database does not match any rules';
  };

  return (
    <>
      <FormControl id="backupType" marginBottom={4}>
        <FormLabel>Backup Type</FormLabel>
        <Select value={props.settings?.backupType || 'Full'} onChange={(e) => set('backupType', e.target.value)}>
          <option value="Full">Full</option>
          <option value="Differential">Differential</option>
          <option value="TransactionLog">Transaction Log</option>
        </Select>
        <FormHelperText>The type of backup that will be made of the database.</FormHelperText>
      </FormControl>
      <FormControl id="fileName" marginBottom={4}>
        <FormLabel>File name</FormLabel>
        <Input
          type="text"
          value={props.settings?.fileName || ''}
          onChange={(evt) => set('fileName', evt.target.value)}
        ></Input>
        <FormHelperText>
          The full file path to write the backup to. The following tokens are supported:
          <br />
          &#123;ItemName&#125; The item name of the job task
          <br />
          &#123;FileName&#125; The filename of the file to download without the path
          <br />
          &#123;Started:yyyyMMddHHmm&#125; The date and time of the start date of the job task
          <br />
          Example:
          <br />
          C:\Temp\Backups\&#123;ItemName&#125;\&#123;ItemName&#125;-&#123;Started:yyyyMMddHHmm&#125;-Full.bak
        </FormHelperText>
      </FormControl>

      <Heading size="md" marginBottom={4}>
        Database Connection
      </Heading>
      <FormControl id="databaseType" isRequired marginBottom={4}>
        <FormLabel>Database Type</FormLabel>
        <Select
          placeholder="Select a database type"
          value={props.settings?.databaseType || ''}
          onChange={(e) => set('databaseType', e.target.value)}
        >
          <option value="sqlServer">SQL Server</option>
          <option value="postgreSql">PostgreSQL</option>
        </Select>
        <FormHelperText>The type of the task.</FormHelperText>
      </FormControl>
      <FormControl id="databaseConnectionString" marginBottom={4}>
        <FormLabel>Database Connection String</FormLabel>
        <Input
          type="text"
          value={props.settings?.connectionString || ''}
          onChange={(evt) => set('connectionString', evt.target.value)}
        ></Input>
        <FormHelperText>
          The connection string to connect to the database. When adding the token &#123;Password&#125; it will be
          replaced with the password below.
          <br />
          Example for SQL Server:
          <br />
          Data Source=127.0.0.1;Persist Security Info=True;User Id=Backup;Password=&#123;Password&#125;;Connect
          Timeout=30;Encrypt=False
          <br />
          Example for PostreSQL:
          <br />
          Server=127.0.0.1;Database=databasename;Username=backup;Password=&#123;Password&#125;
        </FormHelperText>
      </FormControl>
      <FormControl id="databaseConnectionStringPassword" marginBottom={4}>
        <FormLabel>Database Connection String Password</FormLabel>
        <Input
          type="password"
          value={props.settings?.connectionStringPassword || ''}
          onChange={(evt) => set('connectionStringPassword', evt.target.value)}
        ></Input>
        <FormHelperText>
          The password token value for the connection string. This setting is stored encrypted.
        </FormHelperText>
      </FormControl>
      <FormControl id="options" marginBottom={4}>
        <FormLabel>Options</FormLabel>
        <Input
          type="text"
          value={props.settings?.options || ''}
          onChange={(evt) => set('options', evt.target.value)}
        ></Input>
        <FormHelperText>
          If this setting is set the given value will be passed as the options for the parameters. When empty, the
          default parameters are used.
          <br />
          For SQL Server the default parameters are:
          <br />
          NAME = &#123;ItemName&#125; &#123;BackupType&#125; Backup, SKIP, STATS = 10
          <br />
          For PostgreSQL the default parameters are:
          <br />
          --format c -Z 5
        </FormHelperText>
      </FormControl>
      <Box marginBottom={4}>
        <Button onClick={handleTestDatabaseConnection} isLoading={isTesting} marginBottom={4}>
          Test database connection
        </Button>
        {testingError != null ? (
          <Alert status="error">
            <AlertIcon />
            <AlertDescription>{testingError}</AlertDescription>
          </Alert>
        ) : null}
        {testingSuccess != null ? (
          <Alert status="success">
            <AlertIcon />
            <AlertDescription>{testingSuccess}</AlertDescription>
          </Alert>
        ) : null}
      </Box>
      <Heading size="md" marginBottom={4}>
        Item generation settings
      </Heading>
      <Text marginBottom={4}>
        The following settings determine for which databases a backup is created. Each backup will result in a separate
        job run task. The artifact of the task will be the filename specified above.
      </Text>
      <FormControl id="backupDefaultExclude" marginBottom={4} isRequired>
        <Checkbox
          isChecked={props.settings?.backupDefaultExclude}
          onChange={(evt) => set('backupDefaultExclude', evt.target.checked)}
        >
          By default exclude all non matched databases
        </Checkbox>
      </FormControl>
      <FormControl id="backupAllNonSystemDatabases" marginBottom={4} isRequired>
        <Checkbox
          isChecked={props.settings?.backupExcludeSystemDatabases}
          onChange={(evt) => set('backupExcludeSystemDatabases', evt.target.checked)}
        >
          Exclude all system databases
        </Checkbox>
      </FormControl>
      <FormControl id="backupIncludeRegex" marginBottom={4}>
        <FormLabel>Include Regex</FormLabel>
        <Input
          type="text"
          value={props.settings?.backupIncludeRegex || ''}
          onChange={(evt) => set('backupIncludeRegex', evt.target.value)}
        ></Input>
      </FormControl>
      <FormControl id="backupExcludeRegex" marginBottom={4}>
        <FormLabel>Exclude Regex</FormLabel>
        <Input
          type="text"
          value={props.settings?.backupExcludeRegex || ''}
          onChange={(evt) => set('backupExcludeRegex', evt.target.value)}
        ></Input>
      </FormControl>
      <Table width="100%">
        <Thead>
          <Tr>
            <Td>Database</Td>
            <Td>Include</Td>
            <Td>Exclude</Td>
            <Td></Td>
          </Tr>
        </Thead>
        <Tbody>
          {testResult.map((m) => (
            <Tr key={m.databaseName}>
              <Td>{getDatabaseTestResult(m)}</Td>
              <Td>
                <Checkbox
                  isChecked={getCheckBoxIncludeValue(m.databaseName)}
                  onChange={(e) => onCheckBoxIncludeChange(m.databaseName, e.currentTarget.checked)}
                >
                  <FaPlus></FaPlus>
                </Checkbox>
              </Td>
              <Td>
                <Checkbox
                  isChecked={getCheckBoxExcludeValue(m.databaseName)}
                  onChange={(e) => onCheckBoxExcludeChange(m.databaseName, e.currentTarget.checked)}
                >
                  <FaMinus></FaMinus>
                </Checkbox>
              </Td>
              <Td>{getDatabaseTestResult2(m)}</Td>
            </Tr>
          ))}
        </Tbody>
      </Table>
    </>
  );
};

export default JobTaskCreateBackup;
