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
    Link,
    Select,
    Skeleton,
    Textarea
} from '@chakra-ui/react';
import axios from 'axios';
import React, { FC, useEffect, useState } from 'react';
import { RouteComponentProps, useHistory } from 'react-router-dom';
import { Step, StepSettings } from '../../models/step';
import Steps from '../../services/steps';
import StepCreateBackup from './StepCreateBackup';

type RouteParams = {
    id: string;
};

const StepEditor: FC<RouteComponentProps<RouteParams>> = (props) => {
    let [step, setStep] = React.useState<Step | null>(null);

    const [name, setName] = useState<string>('');
    const [description, setDescription] = useState<string>('');
    const [type, setType] = useState<string>('');
    const [settings, setSettings] = useState<StepSettings>({});

    const [error, setError] = useState<string | null>(null);

    const [isSaving, setIsSaving] = useState<boolean>(false);

    const history = useHistory();

    useEffect(() => {
        const getByIdCancelToken = axios.CancelToken.source();

        (async () => {
            const result = await Steps.getById(
                props.match.params.id,
                getByIdCancelToken
            );
            setStep(result);
            setName(result.name || '');
            setDescription(result.description || '');
            setType(result.type || '');
            setSettings(result.settings || {});
        })();

        return () => {
            getByIdCancelToken.cancel();
        };
    }, [props.match.params.id]);

    const save = async () => {
        setIsSaving(true);

        if (step == null) {
            return null;
        }

        step.name = name;
        step.description = description;
        step.type = type;
        step.settings = settings;

        try {
            await Steps.update(step);

            history.push(`/job/${step?.jobId}`);
        } catch (err) {
            setIsSaving(false);
            setError(err);
        }
    };

    const cancel = () => {
        history.push(`/job/${step?.jobId}`);
    };

    const getStepType = () => {
        if (!type) {
            return null;
        }
        switch (type) {
            case 'create_backup':
                return (
                    <StepCreateBackup
                        settings={settings || {}}
                        onSettingsChanged={(newSettings) =>
                            setSettings(newSettings)
                        }
                    ></StepCreateBackup>
                );
            default:
                return <Alert>Unkown type {type}</Alert>;
        }
    };

    return (
        <Skeleton isLoaded={step != null}>
            <Box marginBottom="24px">
                <Heading>Edit step</Heading>
                <Link href={`/#/job/${step?.jobId}`}>{step?.job?.name}</Link>
                <br />
                <Link href={`/#/server/${step?.job?.serverId}`}>
                    {step?.job?.server?.name}
                </Link>
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
                    <Button
                        onClick={() => cancel()}
                        isLoading={isSaving}
                        colorScheme="red"
                    >
                        Delete step
                    </Button>
                </HStack>
            </form>
        </Skeleton>
    );
};

export default StepEditor;
