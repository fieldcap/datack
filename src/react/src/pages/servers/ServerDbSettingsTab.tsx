import React, { FC, useState } from 'react';
import { Alert, Button, Form } from 'react-bootstrap';
import { Server, ServerDbSettings } from '../../models/server';
import Servers from '../../services/servers';
import './ServerOverview.scss';

type Props = {
    server: Server;
};

const ServerDbSettingsTab: FC<Props> = (props) => {
    const [server, setServer] = useState<string>(
        props.server.dbSettings.server
    );
    const [userName, setUserName] = useState<string>(
        props.server.dbSettings.userName
    );
    const [password, setPassword] = useState<string>(
        props.server.dbSettings.password
    );

    const [saveError, setSaveError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState<boolean>(false);

    const validateForm = () => {
        return server.length > 0;
    };

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setIsSaving(true);
        setSaveError(null);

        try {
            const newSettings: ServerDbSettings = {
                server,
                userName,
                password,
            };
            await Servers.updateDbSettings(props.server.serverId, newSettings);
            setIsSaving(false);
        } catch (err) {
            setSaveError(err);
            setIsSaving(false);
        }
    };

    return (
        <Form onSubmit={handleSubmit}>
            <Form.Group controlId="server">
                <Form.Label>Server</Form.Label>
                <Form.Control
                    type="text"
                    value={server}
                    onChange={(e) => setServer(e.target.value)}
                />
            </Form.Group>
            <Form.Group controlId="userName">
                <Form.Label>Username</Form.Label>
                <Form.Control
                    type="text"
                    value={userName}
                    onChange={(e) => setUserName(e.target.value)}
                />
            </Form.Group>
            <Form.Group controlId="password">
                <Form.Label>Password</Form.Label>
                <Form.Control
                    type="text"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />
            </Form.Group>
            {saveError != null ? (
                <Alert variant={'danger'}>
                    There was an error updating the settings: {saveError}
                </Alert>
            ) : (
                ''
            )}
            <Button
                block
                size="lg"
                type="submit"
                disabled={!validateForm() || isSaving}
            >
                Save changes
            </Button>
        </Form>
    );
};

export default ServerDbSettingsTab;
