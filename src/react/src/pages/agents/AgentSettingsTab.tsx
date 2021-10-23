import {
    Alert,
    AlertDescription,
    AlertIcon,
    Button,
    FormControl,
    FormHelperText,
    FormLabel,
    HStack,
    Input,
    Modal,
    ModalBody,
    ModalCloseButton,
    ModalContent,
    ModalFooter,
    ModalHeader,
    ModalOverlay,
    Textarea
} from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { useHistory } from 'react-router';
import Loader from '../../components/loader';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Agent } from '../../models/agent';
import Agents from '../../services/agents';

type Props = {
    agent: Agent;
};

const AgentSettingsTab: FC<Props> = (props) => {
    const [name, setName] = useState<string>(props.agent.name ?? '');

    const [description, setDescription] = useState<string>(props.agent.description ?? '');

    const [key, setKey] = useState<string>(props.agent.key ?? '');

    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState<boolean>(false);

    const [showDeleteModal, setShowDeleteModal] = useState<boolean>(false);

    const history = useHistory();

    const cancelToken = useCancellationToken();

    const handleSave = async (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setIsSaving(true);
        setError(null);
        setSuccess(null);

        try {
            const agent: Agent = {
                agentId: props.agent!.agentId,
                name,
                description,
                key,
                settings: {},
            };

            await Agents.update(agent, cancelToken);
            setIsSaving(false);
        } catch (err: any) {
            setError(err);
            setIsSaving(false);
        }
    };

    const handleDelete = (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setShowDeleteModal(true);
    };

    const handleDeleteOk = async () => {
        setShowDeleteModal(false);

        setIsSaving(true);
        setError(null);
        setSuccess(null);

        try {
            await Agents.deleteAgent(props.agent!.agentId, cancelToken);
            setIsSaving(false);

            history.push('/agents');
        } catch (err: any) {
            setError(err);
            setIsSaving(false);
        }
    };

    const handleDeleteCancel = () => {
        setShowDeleteModal(false);
    };

    return (
        <Loader isLoaded={props.agent != null} error={null}>
            <form>
                <FormControl id="name" marginBottom={4} isRequired>
                    <FormLabel>Agent Name</FormLabel>
                    <Input type="text" maxLength={100} value={name} onChange={(e) => setName(e.target.value)} />
                    <FormHelperText>A name for the agent.</FormHelperText>
                </FormControl>
                <FormControl id="description" marginBottom={4}>
                    <FormLabel>Description</FormLabel>
                    <Textarea lines={4} value={description} onChange={(e) => setDescription(e.target.value)} />
                    <FormHelperText>Description for this agent.</FormHelperText>
                </FormControl>
                <FormControl id="key" marginBottom={4} isRequired>
                    <FormLabel>Key</FormLabel>
                    <Input type="text" maxLength={40} value={key} onChange={(e) => setKey(e.target.value)} />
                    <FormHelperText>Only change the key when installing a new agent.</FormHelperText>
                </FormControl>
                {error != null ? (
                    <Alert marginBottom={4} status="error">
                        <AlertIcon />
                        <AlertDescription>{error}</AlertDescription>
                    </Alert>
                ) : null}
                {success != null ? (
                    <Alert marginBottom={4} status="success">
                        <AlertIcon />
                        <AlertDescription>{success}</AlertDescription>
                    </Alert>
                ) : null}
                <HStack>
                    <Button onClick={(evt) => handleSave(evt)} isLoading={isSaving}>
                        Save
                    </Button>
                    <Button onClick={(evt) => handleDelete(evt)} isLoading={isSaving} colorScheme="red">
                        Delete
                    </Button>
                </HStack>
            </form>
            <Modal isOpen={showDeleteModal} onClose={handleDeleteCancel} size="lg">
                <ModalOverlay />
                <ModalContent>
                    <ModalHeader>Delete agent</ModalHeader>
                    <ModalCloseButton />
                    <ModalBody>
                        <p>Before deleting the agent you need to unassign all job tasks from this agent.</p>
                        <p>Are you sure you want to delete this agent?</p>
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

export default AgentSettingsTab;
