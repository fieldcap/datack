import {
    Alert,
    AlertDescription,
    AlertIcon,
    Button,
    FormControl,
    FormHelperText,
    FormLabel, HStack,
    Input,
    Skeleton,
    Textarea
} from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { Agent } from '../../models/agent';
import Agents from '../../services/agents';

type Props = {
    agent: Agent;
};

const AgentSettingsTab: FC<Props> = (props) => {
    const [name, setName] = useState<string>(props.agent.name ?? '');

    const [description, setDescription] = useState<string>(
        props.agent.description ?? ''
    );

    const [key, setKey] = useState<string>(props.agent.key ?? '');

    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState<boolean>(false);

    const handleSave = async (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setIsSaving(true);
        setError(null);
        setSuccess(null);

        try {
            const newAgent: Agent = {
                agentId: props.agent!.agentId,
                name,
                description,
                key,
                settings: {},
            };

            await Agents.update(newAgent);
            setIsSaving(false);
        } catch (err: any) {
            setError(err);
            setIsSaving(false);
        }
    };

    return (
        <Skeleton isLoaded={props.agent != null}>
            <form>
                <FormControl id="name" marginBottom={4} isRequired>
                    <FormLabel>Agent Name</FormLabel>
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
                <FormControl id="key" marginBottom={4} isRequired>
                    <FormLabel>Key</FormLabel>
                    <Input
                        type="text"
                        maxLength={40}
                        value={key}
                        onChange={(e) => setKey(e.target.value)}
                    />
                    <FormHelperText>
                        Only change the key when installing a new agent.
                    </FormHelperText>
                </FormControl>
                {error != null ? (
                    <Alert marginTop="24px" status="error">
                        <AlertIcon />
                        <AlertDescription>{error}</AlertDescription>
                    </Alert>
                ) : null}
                {success != null ? (
                    <Alert marginTop="24px" status="success">
                        <AlertIcon />
                        <AlertDescription>{success}</AlertDescription>
                    </Alert>
                ) : null}
                <HStack marginTop="24px">
                    <Button
                        onClick={(evt) => handleSave(evt)}
                        isLoading={isSaving}
                    >
                        Save
                    </Button>
                </HStack>
            </form>
        </Skeleton>
    );
};

export default AgentSettingsTab;
