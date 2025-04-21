import {
  Alert,
  AlertDescription,
  AlertIcon,
  Box,
  Button,
  FormControl,
  FormHelperText,
  FormLabel,
  Heading,
  HStack,
  Input,
  Select,
  Textarea,
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { v4 } from 'uuid';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Agent } from '../../models/agent';
import { JobTask } from '../../models/job-task';
import Agents from '../../services/agents';
import JobTasks from '../../services/jobTasks';

type JobTaskAddParams = {
  id: string;
  jobId: string;
};

const JobTaskAdd: FC = () => {
  const params = useParams<JobTaskAddParams>();

  const [name, setName] = useState<string>('');
  const [description, setDescription] = useState<string>('');
  const [type, setType] = useState<string>('');
  const [agentId, setAgentId] = useState<string>('00000000-0000-0000-0000-000000000000');

  const [agents, setAgents] = useState<Agent[]>([]);

  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState<boolean>(false);

  const history = useNavigate();

  const cancelToken = useCancellationToken();

  useEffect(() => {
    (async () => {
      try {
        const agents = await Agents.getList(cancelToken);
        setAgents(agents);
      } catch (err: any) {
        setError(`Cannot get agents: ${err}`);
      }
    })();
  }, [cancelToken]);

  const save = async () => {
    setIsSaving(true);

    try {
      const newJobTask: JobTask = {
        jobTaskId: v4(),
        jobId: params.jobId!,
        isActive: true,
        name: name,
        description: description,
        type: type,
        parallel: 1,
        maxItemsToKeep: 0,
        order: 0,
        timeout: 0,
        usePreviousTaskArtifactsFromJobTaskId: null,
        settings: {},
        agentId: agentId,
      };

      const result = await JobTasks.add(newJobTask, cancelToken);

      history(`/job/${params.jobId!}/task/${result.jobTaskId}`);
    } catch (err: any) {
      setError(err);
    }
    setIsSaving(false);
  };

  const cancel = () => {
    history(`/job/${params.jobId!}`);
  };

  return (
    <>
      <Box marginBottom={4}>
        <Heading>Add task</Heading>
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
            <option value="extract">{JobTasks.map('extract')}</option>
            <option value="restoreBackup">{JobTasks.map('restoreBackup')}</option>
            <option value="uploadAzure">{JobTasks.map('uploadAzure')}</option>
            <option value="uploadS3">{JobTasks.map('uploadS3')}</option>
          </Select>
          <FormHelperText>The type of the task.</FormHelperText>
        </FormControl>
        {error != null ? (
          <Alert marginBottom={4} status="error">
            <AlertIcon />
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        ) : null}
        <HStack>
          <Button onClick={() => save()} isLoading={isSaving}>
            Add task
          </Button>
          <Button onClick={() => cancel()} isLoading={isSaving} variant="outline">
            Cancel
          </Button>
        </HStack>
      </form>
    </>
  );
};

export default JobTaskAdd;
