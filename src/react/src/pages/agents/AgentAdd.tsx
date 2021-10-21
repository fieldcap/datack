import {
    Alert,
    AlertDescription,
    AlertIcon,
    Button,
    FormControl,
    FormHelperText,
    FormLabel,
    Heading,
    HStack,
    Input,
    Textarea
} from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { RouteComponentProps, useHistory } from 'react-router-dom';
import { v4 } from 'uuid';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Agent } from '../../models/agent';
import Agents from '../../services/agents';

type RouteParams = {};

const AgentAdd: FC<RouteComponentProps<RouteParams>> = () => {
    const [name, setName] = useState<string>('');
    const [key, setKey] = useState<string>('');
    const [description, setDescription] = useState<string>('');
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const cancelToken = useCancellationToken();

    const history = useHistory();

    const handleSave = async (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();

        setIsSaving(false);
        setError(null);

        const agent: Agent = {
            agentId: v4(),
            name,
            description,
            key,
            settings: {},
        };

        try {
            setIsSaving(true);
            const newAgentId = await Agents.add(agent, cancelToken);
            history.push(`/agent/${newAgentId}`);
        } catch (err: any) {
            setIsSaving(false);
            setError(err);
        }
    };

    const handleCancel = (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();
        history.push(`/agents`);
    };

    return (
        <>
            <Heading marginBottom="24px">Add new agent</Heading>
            <form>
                <FormControl id="name" marginBottom={4} isRequired>
                    <FormLabel>Agent Name</FormLabel>
                    <Input
                        type="text"
                        maxLength={100}
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                    />
                    <FormHelperText>A name for the agent.</FormHelperText>
                </FormControl>
                <FormControl id="description" marginBottom={4}>
                    <FormLabel>Description</FormLabel>
                    <Textarea
                        lines={4}
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                    />
                    <FormHelperText>Description for this agent.</FormHelperText>
                </FormControl>
                <FormControl id="key" marginBottom={4} isRequired>
                    <FormLabel>Key</FormLabel>
                    <Input
                        type="text"
                        maxLength={40}
                        value={key}
                        onChange={(e) => setKey(e.target.value)}
                    />
                    <FormHelperText>
                        The key can be found when installing the agent.
                    </FormHelperText>
                </FormControl>

                {error != null ? (
                    <Alert marginTop="24px" status="error">
                        <AlertIcon />
                        <AlertDescription>{error}</AlertDescription>
                    </Alert>
                ) : null}

                <HStack marginTop="24px">
                    <Button
                        onClick={(evt) => handleSave(evt)}
                        isLoading={isSaving}
                    >
                        Save
                    </Button>
                    <Button
                        onClick={handleCancel}
                        isLoading={isSaving}
                        variant="outline"
                    >
                        Cancel
                    </Button>
                </HStack>
            </form>
        </>
    );
};

export default AgentAdd;
