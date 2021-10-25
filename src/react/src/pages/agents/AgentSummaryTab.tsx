import { Table, Tbody, Td, Tr } from '@chakra-ui/react';
import React, { FC } from 'react';
import { Agent } from '../../models/agent';

type Props = {
    agent: Agent;
};

const AgentSummaryTab: FC<Props> = (props) => {
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
                </Tbody>
            </Table>
            <a href={`/Api/Agents/Logs/${props.agent.agentId}`} target="_blank" rel="noreferrer">View last 100 lines of agent log</a>
        </>
    );
};

export default AgentSummaryTab;
