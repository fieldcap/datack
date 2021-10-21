import {
    Heading,
    Skeleton,
    Tab,
    TabList,
    TabPanel,
    TabPanels,
    Tabs
} from '@chakra-ui/react';
import React, { FC, useEffect } from 'react';
import { RouteComponentProps } from 'react-router-dom';
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
    let [agent, setAgent] = React.useState<Agent | null>(null);

    const cancelToken = useCancellationToken();

    useEffect(() => {
        const fetchData = async () => {
            const result = await Agents.getById(
                props.match.params.id,
                cancelToken
            );
            setAgent(result);
        };
        fetchData();
    }, [props.match.params.id, cancelToken]);

    return (
        <Skeleton isLoaded={agent != null}>
            {agent != null ? (
                <>
                    <Heading marginBottom="24px">{agent?.name}</Heading>
                    <Tabs>
                        <TabList>
                            <Tab>Summary</Tab>
                            <Tab>Jobs</Tab>
                            <Tab>Settings</Tab>
                        </TabList>

                        <TabPanels>
                            <TabPanel>
                                <AgentSummaryTab agent={agent} />
                            </TabPanel>
                            <TabPanel>
                                <AgentJobList agent={agent} />
                            </TabPanel>
                            <TabPanel>
                                <AgentSettingsTab agent={agent} />
                            </TabPanel>
                        </TabPanels>
                    </Tabs>
                </>
            ) : null}
        </Skeleton>
    );
};

export default AgentOverview;
