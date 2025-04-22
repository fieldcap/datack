import {
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
import { JobTaskDownloadAzureSettings } from '../../models/job-task';
import { DatabaseListTestResult } from '../../models/database-list-test-result';
import JobTasks from '../../services/jobTasks';
import { FaMinus, FaPlus } from 'react-icons/fa';
import useCancellationToken from '../../hooks/useCancellationToken';

type Props = {
  agentId: string;
  jobTaskId: string;
  settings: JobTaskDownloadAzureSettings | undefined | null;
  onSettingsChanged: (settings: JobTaskDownloadAzureSettings) => void;
};

const JobTaskDownloadAzure: FC<Props> = (props) => {
  const { onSettingsChanged } = props;

  const [testResult, setTestResult] = useState<DatabaseListTestResult[]>([]);

  const cancelToken = useCancellationToken();

  useEffect(() => {
    if (props.settings == null) {
      onSettingsChanged({
        blob: '',
        fileName: '',
        containerName: '',
        connectionString: '',
        restoreDefaultExclude: false,
        restoreIncludeRegex: '',
        restoreExcludeRegex: '',
        restoreIncludeManual: '',
        restoreExcludeManual: '',
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
      const result = await JobTasks.testFilesRegex(
        props.settings!.restoreDefaultExclude,
        props.settings!.restoreIncludeRegex,
        props.settings!.restoreExcludeRegex,
        props.settings!.restoreIncludeManual,
        props.settings!.restoreExcludeManual,
        props.agentId,
        props.jobTaskId,
        props.settings!.containerName,
        props.settings!.blob,
        props.settings!.connectionString,
        cancelToken
      );
      setTestResult(result);
    })();
  }, [
    props.settings?.restoreDefaultExclude,
    props.settings?.restoreExcludeRegex,
    props.settings?.restoreIncludeRegex,
    props.settings?.restoreIncludeManual,
    props.settings?.restoreExcludeManual,
    props.agentId,
  ]);

  const set = (settingName: keyof JobTaskDownloadAzureSettings, newValue: string | number | boolean): void => {
    if (props.settings == null) {
      return;
    }
    props.onSettingsChanged({
      ...props.settings,
      [settingName]: newValue,
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

  const getCheckBoxIncludeValue = (name: string): boolean => {
    if (props.settings == null) {
      return false;
    }

    if (props.settings.restoreIncludeManual == null) {
      props.settings.restoreIncludeManual = '';
    }

    let includedDatabases = props.settings.restoreIncludeManual.split(',');

    return includedDatabases.indexOf(name) > -1;
  };

  const onCheckBoxIncludeChange = (name: string, checked: boolean) => {
    if (props.settings == null) {
      return;
    }

    if (props.settings.restoreIncludeManual == null) {
      props.settings.restoreIncludeManual = '';
    }

    let includedDatabases = props.settings.restoreIncludeManual.split(',');
    includedDatabases = includedDatabases.filter((m) => m !== name && m !== '');
    if (checked) {
      includedDatabases.push(name);
    }

    props.onSettingsChanged({
      ...props.settings,
      restoreIncludeManual: includedDatabases.join(','),
    });
  };

  const getCheckBoxExcludeValue = (name: string): boolean => {
    if (props.settings == null) {
      return false;
    }

    if (props.settings.restoreExcludeManual == null) {
      props.settings.restoreExcludeManual = '';
    }

    let excludedDatabases = props.settings.restoreExcludeManual.split(',');

    return excludedDatabases.indexOf(name) > -1;
  };

  const onCheckBoxExcludeChange = (name: string, checked: boolean) => {
    if (props.settings == null) {
      return;
    }

    if (props.settings.restoreExcludeManual == null) {
      props.settings.restoreExcludeManual = '';
    }

    let excludedDatabases = props.settings.restoreExcludeManual.split(',');
    excludedDatabases = excludedDatabases.filter((m) => m !== name && m !== '');
    if (checked) {
      excludedDatabases.push(name);
    }

    props.onSettingsChanged({
      ...props.settings,
      restoreExcludeManual: excludedDatabases.join(','),
    });
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
      return 'Excluded because database does not match any rules';
    }
    return 'Included because database does not match any rules';
  };

  return (
    <>
      <FormControl id="containerName" marginBottom={4}>
        <FormLabel>Container</FormLabel>
        <Input
          type="text"
          value={props.settings?.containerName || ''}
          onChange={(evt) => set('containerName', evt.target.value)}
        ></Input>
        <FormHelperText>The Azure Blob storage container.</FormHelperText>
      </FormControl>
      <FormControl id="blob" marginBottom={4}>
        <FormLabel>Blob</FormLabel>
        <Input type="text" value={props.settings?.blob || ''} onChange={(evt) => set('blob', evt.target.value)}></Input>
        <FormHelperText>
          The Azure blob root to download a file from. Is used to fetch the list of files and not in the actual job.
        </FormHelperText>
      </FormControl>
      <FormControl id="fileName" marginBottom={4}>
        <FormLabel>Destination Filename</FormLabel>
        <Input
          type="text"
          value={props.settings?.fileName || ''}
          onChange={(evt) => set('fileName', evt.target.value)}
        ></Input>
        <FormHelperText>
          The filename to download the Azure file to. The following tokens are supported:
          <br />
          &#123;ItemName&#125; The item name of the job task
          <br />
          &#123;FileName&#125; The filename of the file to download without the path
          <br />
          &#123;Started:yyyyMMddHHmm&#125; The date and time of the start date of the job task
          <br />
          Example:
          <br />
          /&#123;ItemName&#125;/&#123;ItemName&#125;-&#123;Started:yyyyMMddHHmm&#125;-Full.7z
        </FormHelperText>
      </FormControl>
      <FormControl id="connectionString" marginBottom={4}>
        <FormLabel>Connection String</FormLabel>
        <Input
          type="password"
          value={props.settings?.connectionString || ''}
          onChange={(evt) => set('connectionString', evt.target.value)}
        ></Input>
        <FormHelperText>The Azure connection string. This setting is stored encrypted.</FormHelperText>
      </FormControl>
      <Heading size="md" marginBottom={4}>
        Item generation settings
      </Heading>
      <Text marginBottom={4}>
        The following settings determine for which files a backup is restored. Each backup will result in a separate job
        run task. The artifact of the task will be the filename specified above.
      </Text>
      <FormControl id="restoreDefaultExclude" marginBottom={4} isRequired>
        <Checkbox
          isChecked={props.settings?.restoreDefaultExclude}
          onChange={(evt) => set('restoreDefaultExclude', evt.target.checked)}
        >
          By default exclude all non matched databases
        </Checkbox>
      </FormControl>
      <FormControl id="restoreIncludeRegex" marginBottom={4}>
        <FormLabel>Include Regex</FormLabel>
        <Input
          type="text"
          value={props.settings?.restoreIncludeRegex || ''}
          onChange={(evt) => set('restoreIncludeRegex', evt.target.value)}
        ></Input>
      </FormControl>
      <FormControl id="restoreExcludeRegex" marginBottom={4}>
        <FormLabel>Exclude Regex</FormLabel>
        <Input
          type="text"
          value={props.settings?.restoreExcludeRegex || ''}
          onChange={(evt) => set('restoreExcludeRegex', evt.target.value)}
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

export default JobTaskDownloadAzure;
