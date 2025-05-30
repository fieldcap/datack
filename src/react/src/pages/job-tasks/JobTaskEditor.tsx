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
  HStack,
  Input,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  Select,
  Textarea,
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Loader from '../../components/loader';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Agent } from '../../models/agent';
import { JobTask, JobTaskSettings } from '../../models/job-task';
import Agents from '../../services/agents';
import JobTasks from '../../services/jobTasks';
import JobTaskCompress from './JobTaskCompress';
import JobTaskCreateBackup from './JobTaskCreateBackup';
import JobTaskDeleteFile from './JobTaskDeleteFile';
import JobTaskDeleteS3 from './JobTaskDeleteS3';
import JobTaskDownloadS3 from './JobTaskDownloadS3';
import JobTaskDownloadAzure from './JobTaskDownloadAzure';
import JobTaskRestoreBackup from './JobTaskRestoreBackup';
import JobTaskUploadAzure from './JobTaskUploadAzure';
import JobTaskUploadS3 from './JobTaskUploadS3';
import JobTaskExtract from './JobTaskExtract';

type JobTaskEditorParams = {
  id: string;
  jobId: string;
};

const JobTaskEditor: FC = () => {
  const params = useParams<JobTaskEditorParams>();

  const [jobTask, setJobTask] = useState<JobTask | null>(null);

  const [allJobTasks, setAllJobTasks] = useState<JobTask[]>([]);

  const [agents, setAgents] = useState<Agent[]>([]);

  const [name, setName] = useState<string>('');
  const [description, setDescription] = useState<string>('');
  const [isActive, setIsActive] = useState<boolean>(false);
  const [type, setType] = useState<string>('');
  const [parallel, setParallel] = useState<number>(1);
  const [maxItemsToKeep, setMaxItemsToKeep] = useState<number>(0);
  const [timeout, setTimeout] = useState<number | null>(null);
  const [usePreviousTaskArtifacts, setUsePreviousTaskArtifacts] = useState<string | null>(null);
  const [settings, setSettings] = useState<JobTaskSettings | null>(null);
  const [agentId, setAgentId] = useState<string>('00000000-0000-0000-0000-000000000000');

  const [error, setError] = useState<string | null>(null);

  const [isSaving, setIsSaving] = useState<boolean>(false);

  const [showDeleteModal, setShowDeleteModal] = useState<boolean>(false);

  const history = useNavigate();

  const cancelToken = useCancellationToken();

  useEffect(() => {
    (async () => {
      const result = await JobTasks.getById(params.id!, cancelToken);
      setJobTask(result);
      setName(result.name);
      setIsActive(result.isActive);
      setDescription(result.description);
      setType(result.type);
      setParallel(result.parallel);
      setMaxItemsToKeep(result.maxItemsToKeep);
      setUsePreviousTaskArtifacts(result.usePreviousTaskArtifactsFromJobTaskId);
      setSettings(result.settings);
      setAgentId(result.agentId);
      setTimeout(result.timeout);
    })();

    (async () => {
      try {
        const result = await JobTasks.getForJob(params.jobId!, cancelToken);
        setAllJobTasks(result);
      } catch (err: any) {
        setError(`Cannot get job tasks: ${err}`);
      }
    })();

    (async () => {
      try {
        const agents = await Agents.getList(cancelToken);
        setAgents(agents);
      } catch (err: any) {
        setError(`Cannot get agents: ${err}`);
      }
    })();
  }, [params.id!, params.jobId!, cancelToken]);

  const save = async () => {
    setIsSaving(true);

    if (jobTask == null) {
      return;
    }

    try {
      jobTask.name = name;
      jobTask.description = description;
      jobTask.type = type;
      jobTask.isActive = isActive;
      jobTask.settings = settings || {};
      jobTask.agentId = agentId;
      jobTask.parallel = parallel;
      jobTask.maxItemsToKeep = maxItemsToKeep;
      jobTask.timeout = timeout;

      if (!usePreviousTaskArtifacts) {
        jobTask.usePreviousTaskArtifactsFromJobTaskId = null;
      } else {
        jobTask.usePreviousTaskArtifactsFromJobTaskId = usePreviousTaskArtifacts;
      }

      await JobTasks.update(jobTask, cancelToken);

      history(`/job/${jobTask.jobId}`);
    } catch (err: any) {
      setIsSaving(false);
      setError(err);
    }
  };

  const handleDeleteTask = (event: React.FormEvent<HTMLButtonElement>) => {
    event.preventDefault();
    setShowDeleteModal(true);
  };

  const handleDeleteOk = async () => {
    setShowDeleteModal(false);

    setIsSaving(true);
    setError(null);

    try {
      await JobTasks.deleteJobTask(jobTask!.jobTaskId, cancelToken);
      setIsSaving(false);

      history('/jobs');
    } catch (err: any) {
      setError(err);
      setIsSaving(false);
    }
  };

  const handleDeleteCancel = () => {
    setShowDeleteModal(false);
  };

  const cancel = () => {
    history(`/job/${params.jobId!}`);
  };

  const getTaskType = () => {
    if (type == null || settings == null) {
      return null;
    }

    switch (type) {
      case 'createBackup':
        return (
          <JobTaskCreateBackup
            settings={settings.createBackup}
            agentId={agentId}
            jobTaskId={jobTask!.jobTaskId}
            onSettingsChanged={(newSettings) => {
              setSettings({
                createBackup: newSettings,
              });
            }}
          ></JobTaskCreateBackup>
        );
      case 'compress':
        return (
          <JobTaskCompress
            settings={settings.compress}
            agentId={agentId}
            onSettingsChanged={(newSettings) => {
              setSettings({
                compress: newSettings,
              });
            }}
          ></JobTaskCompress>
        );
      case 'deleteFile':
        return (
          <JobTaskDeleteFile
            settings={settings.deleteFile}
            agentId={agentId}
            onSettingsChanged={(newSettings) => {
              setSettings({
                deleteFile: newSettings,
              });
            }}
          ></JobTaskDeleteFile>
        );
      case 'deleteS3':
        return (
          <JobTaskDeleteS3
            settings={settings.deleteS3}
            agentId={agentId}
            onSettingsChanged={(newSettings) => {
              setSettings({
                deleteS3: newSettings,
              });
            }}
          ></JobTaskDeleteS3>
        );
      case 'downloadS3':
        return (
          <JobTaskDownloadS3
            settings={settings.downloadS3}
            agentId={agentId}
            onSettingsChanged={(newSettings) => {
              setSettings({
                downloadS3: newSettings,
              });
            }}
          ></JobTaskDownloadS3>
        );
      case 'downloadAzure':
        return (
          <JobTaskDownloadAzure
            settings={settings.downloadAzure}
            agentId={agentId}
            jobTaskId={jobTask!.jobTaskId}
            onSettingsChanged={(newSettings) => {
              setSettings({
                downloadAzure: newSettings,
              });
            }}
          ></JobTaskDownloadAzure>
        );
      case 'extract':
        return (
          <JobTaskExtract
            settings={settings.extract}
            agentId={agentId}
            onSettingsChanged={(newSettings) => {
              setSettings({
                extract: newSettings,
              });
            }}
          ></JobTaskExtract>
        );
      case 'restoreBackup':
        return (
          <JobTaskRestoreBackup
            settings={settings.restoreBackup}
            agentId={agentId}
            jobTaskId={jobTask!.jobTaskId}
            onSettingsChanged={(newSettings) => {
              setSettings({
                restoreBackup: newSettings,
              });
            }}
          ></JobTaskRestoreBackup>
        );
      case 'uploadS3':
        return (
          <JobTaskUploadS3
            settings={settings.uploadS3}
            agentId={agentId}
            onSettingsChanged={(newSettings) => {
              setSettings({
                uploadS3: newSettings,
              });
            }}
          ></JobTaskUploadS3>
        );
      case 'uploadAzure':
        return (
          <JobTaskUploadAzure
            settings={settings.uploadAzure}
            agentId={agentId}
            onSettingsChanged={(newSettings) => {
              setSettings({
                ...settings,
                uploadAzure: newSettings,
              });
            }}
          ></JobTaskUploadAzure>
        );
      default:
        return <></>;
    }
  };

  return (
    <Loader isLoaded={jobTask != null} error={null}>
      <Box marginBottom={4}>
        <Heading>Edit task</Heading>
      </Box>
      <form>
        <FormControl id="name" marginBottom={4} isRequired>
          <FormLabel>Task Name</FormLabel>
          <Input type="text" maxLength={100} value={name} onChange={(e) => setName(e.target.value)} />
          <FormHelperText>The name of the task.</FormHelperText>
        </FormControl>
        <FormControl id="description" marginBottom={4}>
          <FormLabel>Description</FormLabel>
          <Textarea lines={4} value={description} onChange={(e) => setDescription(e.target.value)} />
          <FormHelperText>A description of what the task does.</FormHelperText>
        </FormControl>
        <FormControl id="isActive" marginBottom={4}>
          <Checkbox isChecked={isActive} onChange={(evt) => setIsActive(evt.target.checked)}>
            Is Active
          </Checkbox>
        </FormControl>
        <FormControl id="agentId" isRequired marginBottom={4}>
          <FormLabel>Agent</FormLabel>
          <Select placeholder="Select an agent" value={agentId} onChange={(e) => setAgentId(e.target.value)}>
            {agents.map((agent) => (
              <option value={agent.agentId} key={agent.agentId}>
                {agent.name}
              </option>
            ))}
          </Select>
          <FormHelperText>The agent this task should execute on.</FormHelperText>
        </FormControl>
        <FormControl id="type" isRequired marginBottom={4}>
          <FormLabel>Type</FormLabel>
          <Select placeholder="Select a type" value={type} onChange={(e) => setType(e.target.value)}>
            <option value="createBackup">{JobTasks.map('createBackup')}</option>
            <option value="compress">{JobTasks.map('compress')}</option>
            <option value="deleteFile">{JobTasks.map('deleteFile')}</option>
            <option value="deleteS3">{JobTasks.map('deleteS3')}</option>
            <option value="downloadS3">{JobTasks.map('downloadS3')}</option>
            <option value="downloadAzure">{JobTasks.map('downloadAzure')}</option>
            <option value="restoreBackup">{JobTasks.map('restoreBackup')}</option>
            <option value="uploadAzure">{JobTasks.map('uploadAzure')}</option>
            <option value="uploadS3">{JobTasks.map('uploadS3')}</option>
          </Select>
          <FormHelperText>The type of the task.</FormHelperText>
        </FormControl>
        <FormControl id="parallel" isRequired marginBottom={4}>
          <FormLabel>Parallel execution</FormLabel>
          <Input
            type="number"
            min={0}
            max={99}
            value={parallel}
            onChange={(e) => {
              setParallel(Number(e.target.value));
            }}
          />
          <FormHelperText>The amount of items that will be executed in parallel.</FormHelperText>
        </FormControl>
        <FormControl id="maxItemsToKeep" isRequired marginBottom={4}>
          <FormLabel>Max Items To Keep</FormLabel>
          <Input
            type="number"
            min={0}
            max={999999}
            value={maxItemsToKeep}
            onChange={(e) => {
              setMaxItemsToKeep(Number(e.target.value));
            }}
          />
          <FormHelperText>
            The maximum amount of items it will run the task for that have not been moved to the next task yet. This
            prevents 1 running far ahead of the other tasks. When 0 don't put a limit.
          </FormHelperText>
        </FormControl>
        <FormControl id="usePreviousTaskArtifacts" marginBottom={4}>
          <FormLabel>Use artifact results from previous task</FormLabel>
          <Select value={usePreviousTaskArtifacts || ''} onChange={(e) => setUsePreviousTaskArtifacts(e.target.value)}>
            <option value="">Don't use artifactes from previous tasks</option>
            {allJobTasks.map((jobTask) => (
              <option value={jobTask.jobTaskId} key={jobTask.jobTaskId}>
                Task: {jobTask.name}
              </option>
            ))}
          </Select>
          <FormHelperText>
            The output artifacts from this task will be used as inputs for this task. You can only uses tasks that are
            executed before this task.
          </FormHelperText>
        </FormControl>
        <FormControl id="timeout" marginBottom={4}>
          <FormLabel>Timeout</FormLabel>
          <Input
            type="number"
            value={timeout || ''}
            onChange={(e) => {
              const n = parseInt(e.target.value);
              if (isNaN(n)) {
                setTimeout(null);
              } else {
                setTimeout(n);
              }
            }}
          />
          <FormHelperText>
            The timeout in seconds. When no value is given, the timeout defaults to 3600 seconds.
          </FormHelperText>
        </FormControl>
        <Box marginBottom={4}>{getTaskType()}</Box>
        {error != null ? (
          <Alert mmarginBottom={4} status="error">
            <AlertIcon />
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        ) : null}
        <HStack>
          <Button onClick={() => save()} isLoading={isSaving}>
            Save
          </Button>
          <Button onClick={() => cancel()} isLoading={isSaving} variant="outline">
            Cancel
          </Button>
          <Button onClick={handleDeleteTask} isLoading={isSaving} colorScheme="red">
            Delete task
          </Button>
        </HStack>
      </form>
      <Modal isOpen={showDeleteModal} onClose={handleDeleteCancel} size="lg">
        <ModalOverlay />
        <ModalContent>
          <ModalHeader>Delete task</ModalHeader>
          <ModalCloseButton />
          <ModalBody>
            <p>When deleting this task, all runs and logs will be deleted associated with this task.</p>
            <p>Are you sure you want to delete this task?</p>
          </ModalBody>

          <ModalFooter>
            <HStack>
              <Button onClick={() => handleDeleteOk()} colorScheme="red">
                Delete
              </Button>
              <Button onClick={() => handleDeleteCancel()}>Cancel</Button>
            </HStack>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </Loader>
  );
};

export default JobTaskEditor;
