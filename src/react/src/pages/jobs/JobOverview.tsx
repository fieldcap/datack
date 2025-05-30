import {
  Box,
  Button,
  Checkbox,
  FormControl,
  FormHelperText,
  FormLabel,
  Heading,
  HStack,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  Select,
  Tab,
  TabList,
  TabPanel,
  TabPanels,
  Tabs,
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Loader from '../../components/loader';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Job } from '../../models/job';
import Jobs from '../../services/jobs';
import JobHistoryTab from './JobHistoryTab';
import JobSettingsTab from './JobSettingsTab';
import JobTasksTab from './JobTasksTab';
import JobTasks from '../../services/jobTasks';

type JobOverviewRouteParams = {
  id: string;
};

const JobOverview: FC = () => {
  const params = useParams<JobOverviewRouteParams>();

  const [job, setJob] = useState<Job | null>(null);
  const [isRestoreTask, setIsRestoreTask] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const [showRunModal, setShowRunModal] = useState<boolean>(false);
  const [runType, setRunType] = useState<string>('All');
  const [runDatabases, setRunDatabases] = useState<string[]>([]);

  const [showRestoreModal, setShowRestoreModal] = useState<boolean>(false);
  const [restoreType, setRestoreType] = useState<string>('Manual');
  const [restoreDatabases, setRestoreDatabases] = useState<{ value: string; label: string }[]>([]);
  const [restoreFiles, setRestoreFiles] = useState<{ value: string; label: string }[]>([]);
  const [restoreDatabase, setRestoreDatabase] = useState<string | undefined>(undefined);
  const [restoreFile, setRestoreFile] = useState<string | undefined>(undefined);

  const [testResult, setTestResult] = useState<string[]>([]);

  const history = useNavigate();

  const cancelToken = useCancellationToken();

  useEffect(() => {
    (async () => {
      try {
        setError(null);
        const result = await Jobs.getById(params.id!, cancelToken);
        const tasks = await JobTasks.getForJob(params.id!, cancelToken);
        setJob(result);

        if (tasks.length > 0 && (tasks[0].type === 'downloadAzure' || tasks[0].type === 'downloadS3')) {
          setIsRestoreTask(tasks[0].type);
        } else {
          setIsRestoreTask(null);
        }
      } catch (err: any) {
        setError(err);
      }
    })();
  }, [params.id, showRunModal, cancelToken]);

  useEffect(() => {
    if (runType === 'All') {
      return;
    }
  }, [showRunModal, runType, params.id, cancelToken]);

  useEffect(() => {
    (async () => {
      if (isRestoreTask === 'downloadAzure') {
        const result = await Jobs.getAzureFileList(params.id!, null, cancelToken);

        const databases = result.map((m) => {
          const split = m.fileName.replace(/\/$/, '').split('/');
          return {
            value: m.fileName,
            label: split[split.length - 1],
          };
        });

        setRestoreDatabases(databases);
      } else {
        const result = await Jobs.getDatabaseList(params.id!, cancelToken);
        setTestResult(result);
      }
    })();
  }, [showRestoreModal]);

  useEffect(() => {
    if (restoreDatabase) {
      (async () => {
        let result = await Jobs.getAzureFileList(params.id!, restoreDatabase, cancelToken);

        result = result.orderByDescending((m) => m.dateTime);

        const files = result.map((m) => {
          return {
            value: m.fileName,
            label: `${m.databaseName} - ${m.dateTime} - ${m.backupType}`,
          };
        });

        setRestoreFiles(files);
      })();
    }
  }, [restoreDatabase]);

  const handleRunJob = () => {
    if (isRestoreTask === 'downloadAzure') {
      setShowRestoreModal(true);
    } else {
      setShowRunModal(true);
    }
  };

  const handleRunCancel = () => {
    setShowRunModal(false);
  };

  const handleRunOk = async () => {
    setShowRunModal(false);

    let databaseList = runDatabases.join(',');
    if (runType === 'All') {
      databaseList = '';
    }

    var jobRunId = await Jobs.run(job!.jobId, databaseList);

    history(`/run/${jobRunId}`);
  };

  const handleRestoreCancel = () => {
    setShowRestoreModal(false);
  };

  const handleRestoreOk = async () => {
    if (restoreType === 'Manual' && (!restoreDatabase || !restoreFile)) {
      return;
    }

    setShowRestoreModal(false);

    let fileList = restoreFile ?? '';
    if (restoreType === 'All') {
      fileList = '';
    }

    var jobRunId = await Jobs.run(job!.jobId, fileList);

    history(`/run/${jobRunId}`);
  };

  const getCheckBoxIncludeValue = (name: string): boolean => {
    return runDatabases.indexOf(name) > -1;
  };

  const handleCheckBoxIncludeChange = (name: string, checked: boolean): void => {
    if (checked) {
      setRunDatabases([...runDatabases, name]);
    } else {
      setRunDatabases([...runDatabases.filter((m) => m !== name)]);
    }
  };

  return (
    <>
      <Loader isLoaded={job != null} error={error}>
        <Box marginBottom={4}>
          <Heading>{job?.name}</Heading>
        </Box>
        <Box marginBottom={4}>
          <Button onClick={() => handleRunJob()}>{isRestoreTask ? 'Run Restore' : 'Run Backup'} Job</Button>
        </Box>
        <Tabs>
          <TabList>
            <Tab>History</Tab>
            <Tab>Tasks</Tab>
            <Tab>Configuration</Tab>
          </TabList>

          <TabPanels>
            <TabPanel>
              <JobHistoryTab job={job}></JobHistoryTab>
            </TabPanel>
            <TabPanel>
              <JobTasksTab job={job}></JobTasksTab>
            </TabPanel>
            <TabPanel>
              <JobSettingsTab job={job}></JobSettingsTab>
            </TabPanel>
          </TabPanels>
        </Tabs>
      </Loader>
      <Modal isOpen={showRunModal} onClose={handleRunCancel} size="lg">
        <ModalOverlay />
        <ModalContent>
          <ModalHeader>Run Backup Job</ModalHeader>
          <ModalCloseButton />
          <ModalBody>
            <FormControl id="runType" isRequired marginBottom={4}>
              <FormLabel>Run Type</FormLabel>
              <Select value={runType} onChange={(e) => setRunType(e.target.value)}>
                <option value="All">All items</option>
                <option value="Manual">Select items</option>
              </Select>
              <FormHelperText>
                Run the task for either all items generated by the first task, or select which items to run the job for.
              </FormHelperText>
            </FormControl>
            {testResult.map((m) => (
              <FormControl marginBottom={2} key={m}>
                <Checkbox
                  isChecked={getCheckBoxIncludeValue(m)}
                  onChange={(e) => handleCheckBoxIncludeChange(m, e.currentTarget.checked)}
                >
                  {m}
                </Checkbox>
              </FormControl>
            ))}
          </ModalBody>

          <ModalFooter>
            <HStack>
              <Button onClick={() => handleRunOk()} colorScheme="blue">
                Run
              </Button>
              <Button onClick={() => handleRunCancel()}>Cancel</Button>
            </HStack>
          </ModalFooter>
        </ModalContent>
      </Modal>
      <Modal isOpen={showRestoreModal} onClose={handleRunCancel} size="lg">
        <ModalOverlay />
        <ModalContent>
          <ModalHeader>Run Restore Job</ModalHeader>
          <ModalCloseButton />
          <ModalBody>
            <FormControl id="restoreType" isRequired marginBottom={4}>
              <FormLabel>Restore Type</FormLabel>
              <Select value={restoreType} onChange={(e) => setRestoreType(e.target.value)}>
                <option value="All">All items</option>
                <option value="Manual">Select items</option>
              </Select>
              <FormHelperText>
                Run the task for either all items generated by the first task, or select which items to run the job for.
              </FormHelperText>
            </FormControl>
            <FormControl id="restoreDatabase" isRequired marginBottom={4}>
              <FormLabel>Database</FormLabel>
              <Select value={restoreDatabase} onChange={(e) => setRestoreDatabase(e.target.value)}>
                <option value={undefined}>Select a database</option>
                {restoreDatabases.map((m) => (
                  <option value={m.value} key={m.value}>
                    {m.label}
                  </option>
                ))}
              </Select>
              <FormHelperText>Select which database to restore.</FormHelperText>
            </FormControl>

            <FormControl id="restoreFile" isRequired marginBottom={4}>
              <FormLabel>File</FormLabel>
              <Select value={restoreFile} onChange={(e) => setRestoreFile(e.target.value)}>
                <option value={undefined}>Select a file</option>
                {restoreFiles.map((m) => (
                  <option value={m.value} key={m.value}>
                    {m.label}
                  </option>
                ))}
              </Select>
              <FormHelperText>Select which database to restore.</FormHelperText>
            </FormControl>
          </ModalBody>

          <ModalFooter>
            <HStack>
              <Button onClick={() => handleRestoreOk()} colorScheme="blue">
                Run
              </Button>
              <Button onClick={() => handleRestoreCancel()}>Cancel</Button>
            </HStack>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
};

export default JobOverview;
