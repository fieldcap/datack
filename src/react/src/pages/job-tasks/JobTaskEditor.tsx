import {
    Alert,
    AlertDescription,
    AlertIcon,
    Box,
    Button,
    FormControl,
    FormLabel,
    Heading,
    HStack,
    Input,
    Select,
    Skeleton,
    Textarea
} from '@chakra-ui/react';
import axios from 'axios';
import React, { FC, useEffect, useState } from 'react';
import { RouteComponentProps, useHistory } from 'react-router-dom';
import { v4 } from 'uuid';
import { JobTask, JobTaskSettings } from '../../models/job-task';
import { Server } from '../../models/server';
import JobTasks from '../../services/jobTasks';
import Servers from '../../services/servers';
import JobTaskCreateBackup from './JobTaskCreateBackup';

type RouteParams = {
    id?: string;
    jobId: string;
};

const JobTaskEditor: FC<RouteComponentProps<RouteParams>> = (props) => {
    const [jobTask, setJobTask] = React.useState<JobTask | null>(null);

    const [isAdd, setIsAdd] = useState<boolean>(false);

    const [servers, setServers] = useState<Server[]>([]);

    const [name, setName] = useState<string>('');
    const [description, setDescription] = useState<string>('');
    const [type, setType] = useState<string>('');
    const [parallel, setParallel] = useState<number>(1);
    const [settings, setSettings] = useState<JobTaskSettings | null>(null);
    const [serverId, setServerId] = useState<string>('');

    const [error, setError] = useState<string | null>(null);

    const [isSaving, setIsSaving] = useState<boolean>(false);

    const history = useHistory();

    useEffect(() => {
        const getByIdCancelToken = axios.CancelToken.source();

        if (props.match.params.id == null) {
            setIsAdd(true);
        }

        if (props.match.params.id != null) {
            (async () => {
                const result = await JobTasks.getById(
                    props.match.params.id!,
                    getByIdCancelToken
                );
                setJobTask(result);
                setName(result.name || '');
                setDescription(result.description || '');
                setType(result.type || '');
                setParallel(result.parallel);
                setSettings(result.settings);
                setServerId(result.serverId || '');
            })();
        }

        (async () => {
            const servers = await Servers.getList(getByIdCancelToken);
            setServers(servers);
        })();

        return () => {
            getByIdCancelToken.cancel();
        };
    }, [props.match.params.id]);

    const save = async () => {
        setIsSaving(true);

        try {
            if (isAdd) {
                const newJobTask: JobTask = {
                    jobTaskId: v4(),
                    jobId: props.match.params.jobId,
                    name: name,
                    description: description,
                    type: type,
                    parallel: parallel,
                    order: 0,
                    settings: settings || {},
                    serverId: serverId,
                };

                const result = await JobTasks.add(newJobTask);

                history.push(`/job/${result}`);
            } else if (jobTask != null) {
                jobTask.name = name;
                jobTask.description = description;
                jobTask.type = type;
                jobTask.settings = settings || {};
                jobTask.serverId = serverId;
                jobTask.parallel = parallel;

                await JobTasks.update(jobTask);

                history.push(`/job/${jobTask.jobId}`);
            }
        } catch (err: any) {
            setIsSaving(false);
            setError(err);
        }
    };

    const cancel = () => {
        history.push(`/job/${jobTask?.jobId}`);
    };

    const getTaskType = () => {
        if (type == null || settings == null) {
            return null;
        }

        switch (type) {
            case 'create_backup':
                return (
                    <JobTaskCreateBackup
                        settings={settings.createBackup}
                        serverId={serverId}
                        onSettingsChanged={(newSettings) => {
                            setSettings({
                                ...settings,
                                createBackup: newSettings,
                            });
                        }}
                    ></JobTaskCreateBackup>
                );
            default:
                return <Alert>Unkown type {type}</Alert>;
        }
    };

    return (
        <Skeleton isLoaded={jobTask != null || isAdd}>
            <Box marginBottom="24px">
                {isAdd ? (
                    <Heading>Add task</Heading>
                ) : (
                    <Heading>Edit task</Heading>
                )}
                {/* <Link href={`/#/job/${jobTask?.jobId}`}>{jobTask?.job?.name}</Link>
                <br /> */}
                {/* <Link href={`/#/server/${jobTask?.job?.serverId}`}>
                    {jobTask?.job?.server?.name}
                </Link> */}
            </Box>
            <form>
                <FormControl id="name" marginBottom={4} isRequired>
                    <FormLabel>Task Name</FormLabel>
                    <Input
                        type="text"
                        maxLength={100}
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                    />
                </FormControl>
                <FormControl id="description" marginBottom={4}>
                    <FormLabel>Description</FormLabel>
                    <Textarea
                        lines={4}
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                    />
                </FormControl>
                <FormControl id="type" isRequired marginBottom={4}>
                    <FormLabel>Server</FormLabel>
                    <Select
                        placeholder="Select a server"
                        value={serverId}
                        onChange={(e) => setServerId(e.target.value)}
                    >
                        {servers.map((server) => (
                            <option
                                value={server.serverId}
                                key={server.serverId}
                            >
                                {server.name}
                            </option>
                        ))}
                    </Select>
                </FormControl>
                <FormControl id="type" isRequired marginBottom={4}>
                    <FormLabel>Type</FormLabel>
                    <Select
                        placeholder="Select a type"
                        value={type}
                        onChange={(e) => setType(e.target.value)}
                    >
                        <option value="create_backup">Create Backup</option>
                    </Select>
                </FormControl>
                <FormControl id="type" isRequired marginBottom={4}>
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
                    <Button
                        onClick={() => cancel()}
                        isLoading={isSaving}
                        variant="outline"
                    >
                        Cancel
                    </Button>
                    {!isAdd ? (
                        <Button
                            onClick={() => cancel()}
                            isLoading={isSaving}
                            colorScheme="red"
                        >
                            Delete task
                        </Button>
                    ) : null}
                </HStack>
            </form>
        </Skeleton>
    );
};

export default JobTaskEditor;
