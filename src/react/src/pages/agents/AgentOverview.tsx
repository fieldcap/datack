import {
    Alert,
    AlertDescription,
    AlertIcon,
    Heading,
    Tab,
    TabList,
    TabPanel,
    TabPanels,
    Tabs
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import Loader from '../../components/loader';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Agent } from '../../models/agent';
import Agents from '../../services/agents';
import AgentJobList from './AgentJobList';
import AgentSettingsTab from './AgentSettingsTab';
import AgentSummaryTab from './AgentSummaryTab';

type RouteParams = {
    id: string;
};

const AgentOverview: FC<RouteComponentProps<RouteParams>> = (props) => {
    const [agent, setAgent] = useState<Agent | null>(null);
    const [error, setError] = useState<string | null>(null);

    const cancelToken = useCancellationToken();

    useEffect(() => {
        const fetchData = async () => {
            setError(null);
            try {
                const result = await Agents.getById(
                    props.match.params.id,
                    cancelToken
                );
                setAgent(result);
            } catch (err: any) {
                setError(err);
            }
        };
        fetchData();
    }, [props.match.params.id, cancelToken]);

    if (error) {
        return (
            <Alert marginTop="24px" status="error">
                <AlertIcon />
                <AlertDescription>{error}</AlertDescription>
            </Alert>
        );
    }

    return (
        <Loader isLoaded={agent != null}>
            <Heading marginBottom="24px">{agent?.name}</Heading>
            <Tabs>
                <TabList>
                    <Tab>Summary</Tab>
                    <Tab>Jobs</Tab>
                    <Tab>Settings</Tab>
                </TabList>

                <TabPanels>
                    <TabPanel>
                        <AgentSummaryTab agent={agent!} />
                    </TabPanel>
                    <TabPanel>
                        <AgentJobList agent={agent!} />
                    </TabPanel>
                    <TabPanel>
                        <AgentSettingsTab agent={agent!} />
                    </TabPanel>
                </TabPanels>
            </Tabs>
        </Loader>
    );
};

export default AgentOverview;
