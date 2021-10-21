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
    Modal,
    ModalBody,
    ModalCloseButton,
    ModalContent,
    ModalFooter,
    ModalHeader,
    ModalOverlay,
    Select,
    Textarea
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { RouteComponentProps, useHistory } from 'react-router-dom';
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
import JobTaskUploadAzure from './JobTaskUploadAzure';
import JobTaskUploadS3 from './JobTaskUploadS3';

type RouteParams = {
    id?: string;
    jobId: string;
};

const JobTaskEditor: FC<RouteComponentProps<RouteParams>> = (props) => {
    const [jobTask, setJobTask] = useState<JobTask | null>(null);

    const [allJobTasks, setAllJobTasks] = useState<JobTask[]>([]);

    const [agents, setAgents] = useState<Agent[]>([]);

    const [name, setName] = useState<string>('');
    const [description, setDescription] = useState<string>('');
    const [type, setType] = useState<string>('');
    const [parallel, setParallel] = useState<number>(1);
    const [timeout, setTimeout] = useState<number | null>(null);
    const [usePreviousTaskArtifacts, setUsePreviousTaskArtifacts] = useState<string | null>(null);
    const [settings, setSettings] = useState<JobTaskSettings | null>(null);
    const [agentId, setAgentId] = useState<string>('00000000-0000-0000-0000-000000000000');

    const [error, setError] = useState<string | null>(null);

    const [isSaving, setIsSaving] = useState<boolean>(false);

    const [showDeleteModal, setShowDeleteModal] = useState<boolean>(false);

    const history = useHistory();

    const cancelToken = useCancellationToken();

    useEffect(() => {
        (async () => {
            const result = await JobTasks.getById(props.match.params.id!, cancelToken);
            setJobTask(result);
            setName(result.name);
            setDescription(result.description);
            setType(result.type);
            setParallel(result.parallel);
            setUsePreviousTaskArtifacts(result.usePreviousTaskArtifactsFromJobTaskId);
            setSettings(result.settings);
            setAgentId(result.agentId);
            setTimeout(result.timeout);
        })();

        (async () => {
            try {
                const result = await JobTasks.getForJob(props.match.params.jobId, cancelToken);
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
    }, [props.match.params.id, props.match.params.jobId, cancelToken]);

    const save = async () => {
        setIsSaving(true);

        if (jobTask == null) {
            return;
        }

        try {
            jobTask.name = name;
            jobTask.description = description;
            jobTask.type = type;
            jobTask.settings = settings || {};
            jobTask.agentId = agentId;
            jobTask.parallel = parallel;
            jobTask.timeout = timeout;

            if (!usePreviousTaskArtifacts) {
                jobTask.usePreviousTaskArtifactsFromJobTaskId = null;
            } else {
                jobTask.usePreviousTaskArtifactsFromJobTaskId = usePreviousTaskArtifacts;
            }

            await JobTasks.update(jobTask, cancelToken);

            history.push(`/job/${jobTask.jobId}`);
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

            history.push('/jobs');
        } catch (err: any) {
            setError(err);
            setIsSaving(false);
        }
    };

    const handleDeleteCancel = () => {
        setShowDeleteModal(false);
    };

    const cancel = () => {
        history.push(`/job/${props.match.params.jobId}`);
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
            <Box marginBottom="24px">
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
                <FormControl id="usePreviousTaskArtifacts" marginBottom={4}>
                    <FormLabel>Use artifact results from previous task</FormLabel>
                    <Select
                        value={usePreviousTaskArtifacts || ''}
                        onChange={(e) => setUsePreviousTaskArtifacts(e.target.value)}
                    >
                        <option value="">Don't use artifactes from previous tasks</option>
                        {allJobTasks.map((jobTask) => (
                            <option value={jobTask.jobTaskId} key={jobTask.jobTaskId}>
                                Task: {jobTask.name}
                            </option>
                        ))}
                    </Select>
                    <FormHelperText>
                        The output artifacts from this task will be used as inputs for this task. You can only uses
                        tasks that are executed before this task.
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
                    <FormHelperText>The timeout in seconds.</FormHelperText>
                </FormControl>
                {getTaskType()}
                {error != null ? (
                    <Alert marginTop="24px" status="error">
                        <AlertIcon />
                        <AlertDescription>{error}</AlertDescription>
                    </Alert>
                ) : null}
                <HStack marginTop="24px">
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
