import { Alert, AlertDescription, AlertIcon, Button, Table, Tbody, Td, Tr } from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Agent } from '../../models/agent';
import Agents from '../../services/agents';

type Props = {
    agent: Agent;
};

const AgentSummaryTab: FC<Props> = (props) => {
    const [error, setError] = useState<string | null>(null);
    const [upgrading, setUpgrading] = useState<boolean>(false);

    const cancelToken = useCancellationToken();

    const getState = (): string => {
        switch (props.agent.status) {
            case 'offline':
                return 'Offline';
            case 'online':
                return 'Online';
            case 'versionmismatch':
                return 'Update required';
            default:
                return props.agent.status || '';
        }
    };

    const handleUpgradeClick = async () => {
        try {
            setUpgrading(true);
            await Agents.upgradeAgent(props.agent.agentId, cancelToken);
        } catch (err: any) {
            setError(err);
        }

        setUpgrading(false);
    };

    return (
        <>
            <Table variant="simple" marginBottom={4}>
                <Tbody>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>Name</Td>
                        <Td>{props.agent.name}</Td>
                    </Tr>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>Key</Td>
                        <Td>{props.agent.key}</Td>
                    </Tr>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>State</Td>
                        <Td>{getState()}</Td>
                    </Tr>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>Version</Td>
                        <Td>{props.agent.version}</Td>
                    </Tr>
                    <Tr>
                        <Td></Td>
                        <Td>
                            {error != null ? (
                                <Alert marginBottom={4} status="error">
                                    <AlertIcon />
                                    <AlertDescription>{error}</AlertDescription>
                                </Alert>
                            ) : null}
                            <Button onClick={() => handleUpgradeClick()} isLoading={upgrading}>
                                Upgrade Agent
                            </Button>
                        </Td>
                    </Tr>
                </Tbody>
            </Table>
            <a href={`/Api/Agents/Logs/${props.agent.agentId}`} target="_blank" rel="noreferrer">
                View last 100 lines of agent log
            </a>
        </>
    );
};

export default AgentSummaryTab;
