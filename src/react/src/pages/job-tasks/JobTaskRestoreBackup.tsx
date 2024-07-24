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
import { JobTaskRestoreDatabaseSettings } from '../../models/job-task';
import JobTasks from '../../services/jobTasks';

type Props = {
  agentId: string;
  jobTaskId: string;
  settings: JobTaskRestoreDatabaseSettings | undefined | null;
  onSettingsChanged: (settings: JobTaskRestoreDatabaseSettings) => void;
};

const JobTaskRestoreBackup: FC<Props> = (props) => {
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
        databaseName: '',
        options: '',
        connectionString: '',
        connectionStringPassword: null,
      });
    }
  }, [props.settings, onSettingsChanged]);

  const set = (settingName: keyof JobTaskRestoreDatabaseSettings, newValue: string | number | boolean): void => {
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

  return (
    <>
      <FormControl id="databaseName" marginBottom={4}>
        <FormLabel>Database Name</FormLabel>
        <Input
          type="text"
          value={props.settings?.databaseName || ''}
          onChange={(evt) => set('databaseName', evt.target.value)}
        ></Input>
        <FormHelperText>
          The name of the database to restore to. The following tokens are supported:
          <br />
          &#123;ItemName&#125; The item name of the job task
          <br />
          &#123;Started:yyyyMMddHHmm&#125; The date and time of the start date of the job task
          <br />
          Example:
          <br />
          Restored-&#123;ItemName&#125;-&#123;Started:yyyyMMddHHmm&#125;
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
          If this setting is set the given value will be passed as the options for the parameters.
          <br />
          Default restore action is: RESTORE DATABASE [DatabaseName] FROM DISK = N'FileName' WITH FILE = 1
          <br />
          The following tokens are supported:
          <br />
          &#123;ItemName&#125; The item name of the job task
          <br />
          &#123;Started:yyyyMMddHHmm&#125; The date and time of the start date of the job task
          <br />
          &#123;LogicalNameData&#125; The logical name of the data file in the backup set
          <br />
          &#123;LogicalNameLog&#125; The logical name of the log file in the backup set
        </FormHelperText>
      </FormControl>
    </>
  );
};

export default JobTaskRestoreBackup;
