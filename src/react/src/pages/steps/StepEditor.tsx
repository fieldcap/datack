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
import { Server } from '../../models/server';
import { Step, StepSettings } from '../../models/step';
import Servers from '../../services/servers';
import Steps from '../../services/steps';
import StepCreateBackup from './StepCreateBackup';

type RouteParams = {
    id?: string;
    jobId: string;
};

const StepEditor: FC<RouteComponentProps<RouteParams>> = (props) => {
    const [step, setStep] = React.useState<Step | null>(null);

    const [isAdd, setIsAdd] = useState<boolean>(false);

    const [servers, setServers] = useState<Server[]>([]);

    const [name, setName] = useState<string>('');
    const [description, setDescription] = useState<string>('');
    const [type, setType] = useState<string>('');
    const [settings, setSettings] = useState<StepSettings | null>(null);
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
                const result = await Steps.getById(
                    props.match.params.id!,
                    getByIdCancelToken
                );
                setStep(result);
                setName(result.name || '');
                setDescription(result.description || '');
                setType(result.type || '');
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
                const newStep: Step = {
                    stepId: v4(),
                    jobId: props.match.params.jobId,
                    name: name,
                    description: description,
                    type: type,
                    order: 0,
                    settings: settings || {},
                    serverId: serverId,
                };

                const result = await Steps.add(newStep);

                history.push(`/job/${result}`);
            } else if (step != null) {
                step.name = name;
                step.description = description;
                step.type = type;
                step.settings = settings || {};
                step.serverId = serverId;

                await Steps.update(step);

                history.push(`/job/${step.jobId}`);
            }
        } catch (err: any) {
            setIsSaving(false);
            setError(err);
        }
    };

    const cancel = () => {
        history.push(`/job/${step?.jobId}`);
    };

    const getStepType = () => {
        if (type == null || settings == null) {
            return null;
        }

        switch (type) {
            case 'create_backup':
                return (
                    <StepCreateBackup
                        settings={settings.createBackup}
                        serverId={serverId}
                        onSettingsChanged={(newSettings) => {
                            setSettings({
                                ...settings,
                                createBackup: newSettings,
                            });
                        }}
                    ></StepCreateBackup>
                );
            default:
                return <Alert>Unkown type {type}</Alert>;
        }
    };

    return (
        <Skeleton isLoaded={step != null || isAdd}>
            <Box marginBottom="24px">
                {isAdd ? (
                    <Heading>Add step</Heading>
                ) : (
                    <Heading>Edit step</Heading>
                )}
                {/* <Link href={`/#/job/${step?.jobId}`}>{step?.job?.name}</Link>
                <br /> */}
                {/* <Link href={`/#/server/${step?.job?.serverId}`}>
                    {step?.job?.server?.name}
                </Link> */}
            </Box>
            <form>
                <FormControl id="name" marginBottom={4} isRequired>
                    <FormLabel>Step Name</FormLabel>
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
                {getStepType()}
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
                            Delete step
                        </Button>
                    ) : null}
                </HStack>
            </form>
        </Skeleton>
    );
};

export default StepEditor;
