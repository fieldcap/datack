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
import { Server } from '../../models/server';
import Servers from '../../services/servers';

type RouteParams = {};

const ServerAdd: FC<RouteComponentProps<RouteParams>> = (props) => {
    const [name, setName] = useState<string>('');
    const [key, setKey] = useState<string>('');
    const [description, setDescription] = useState<string>('');
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const history = useHistory();

    const handleSave = async (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setIsSaving(false);
        setError(null);

        const server: Server = {
            serverId: v4(),
            name,
            description,
            key,
            dbSettings: {},
            settings: {},
        };

        try {
            setIsSaving(true);
            const newServer = await Servers.add(server);
            history.push(`/server/${newServer.serverId}`);
        } catch (err) {
            setIsSaving(false);
            setError(err);
        }
    };

    const handleCancel = (event: React.FormEvent<HTMLButtonElement>) => {
        history.push(`/server`);
    };

    return (
        <>
            <Heading marginBottom="24px">Add new server</Heading>
            <form>
                <FormControl id="name" marginBottom={4} isRequired>
                    <FormLabel>Server Name</FormLabel>
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
                        The key can be found when installing the agent on the
                        server.
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

export default ServerAdd;
