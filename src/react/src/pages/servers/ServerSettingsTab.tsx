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
    Skeleton,
    Textarea
} from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { Server } from '../../models/server';
import Servers from '../../services/servers';

type Props = {
    server?: Server;
};

const ServerSettingsTab: FC<Props> = (props) => {
    const [name, setName] = useState<string>(props.server?.name ?? '');

    const [description, setDescription] = useState<string>(
        props.server?.description ?? ''
    );

    const [key, setKey] = useState<string>(props.server?.key ?? '');

    const [tempPath, setTempPath] = useState<string>(
        props.server?.settings?.tempPath ?? ''
    );

    const [server, setServer] = useState<string>(
        props.server?.dbSettings?.server ?? ''
    );
    const [userName, setUserName] = useState<string>(
        props.server?.dbSettings?.userName ?? ''
    );
    const [password, setPassword] = useState<string>(
        props.server?.dbSettings?.password ?? ''
    );

    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState<boolean>(false);

    const handleSave = async (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setIsSaving(true);
        setError(null);
        setSuccess(null);

        try {
            const newServer: Server = {
                serverId: props.server!.serverId,
                name,
                description,
                key,
                settings: {
                    tempPath,
                },
                dbSettings: {
                    password,
                    server,
                    userName,
                },
            };

            await Servers.update(newServer);
            setIsSaving(false);
        } catch (err) {
            setError(err);
            setIsSaving(false);
        }
    };

    const handleTestConnection = async (
        event: React.FormEvent<HTMLButtonElement>
    ) => {
        event.preventDefault();
        setIsSaving(true);
        setError(null);
        setSuccess(null);

        try {
            const newServer: Server = {
                serverId: props.server!.serverId,
                name,
                description,
                key,
                settings: {
                    tempPath,
                },
                dbSettings: {
                    password,
                    server,
                    userName,
                },
            };

            const testResult = await Servers.testSqlServerConnection(newServer);

            setSuccess(testResult);
            setIsSaving(false);
        } catch (err) {
            setError(err);
            setIsSaving(false);
        }
    };

    return (
        <Skeleton isLoaded={props.server != null}>
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
                        Only change the key when installing a new agent on the
                        same server.
                    </FormHelperText>
                </FormControl>
                <Heading size="md" marginBottom={2} marginTop={6}>
                    Server settings
                </Heading>

                <FormControl id="key" marginBottom={4} isRequired>
                    <FormLabel>Temp Path</FormLabel>
                    <Input
                        type="text"
                        value={tempPath}
                        onChange={(e) => setTempPath(e.target.value)}
                    />
                    <FormHelperText>
                        Leave empty to use the default windows temp path.
                    </FormHelperText>
                </FormControl>

                <Heading size="md" marginBottom={2} marginTop={6}>
                    SQL Connection settings
                </Heading>

                <FormControl id="key" marginBottom={4} isRequired>
                    <FormLabel>Server Name</FormLabel>
                    <Input
                        type="text"
                        value={server}
                        onChange={(e) => setServer(e.target.value)}
                    />
                </FormControl>

                <FormControl id="key" marginBottom={4} isRequired>
                    <FormLabel>Username</FormLabel>
                    <Input
                        type="text"
                        value={userName}
                        onChange={(e) => setUserName(e.target.value)}
                    />
                </FormControl>

                <FormControl id="key" marginBottom={4} isRequired>
                    <FormLabel>Password</FormLabel>
                    <Input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                </FormControl>
                <HStack marginTop="24px">
                    <Button
                        onClick={(evt) => handleTestConnection(evt)}
                        isLoading={isSaving}
                        variant="outline"
                    >
                        Test
                    </Button>
                </HStack>

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

export default ServerSettingsTab;
