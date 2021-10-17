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
import { Server } from '../../models/server';
import Servers from '../../services/servers';
import ServerSettingsTab from './ServerSettingsTab';
import ServerSummaryTab from './ServerSummaryTab';

type RouteParams = {
    id: string;
};

const ServerOverview: FC<RouteComponentProps<RouteParams>> = (props) => {
    let [server, setServer] = React.useState<Server | null>(null);

    const cancelToken = useCancellationToken();

    useEffect(() => {
        const fetchData = async () => {
            const result = await Servers.getById(
                props.match.params.id,
                cancelToken
            );
            setServer(result);
        };
        fetchData();
    }, [props.match.params.id, cancelToken]);

    return (
        <Skeleton isLoaded={server != null}>
            <Heading marginBottom="24px">{server?.name}</Heading>
            <Tabs>
                <TabList>
                    <Tab>Summary</Tab>
                    <Tab>Settings</Tab>
                </TabList>

                <TabPanels>
                    <TabPanel>
                        <ServerSummaryTab server={server!}></ServerSummaryTab>
                    </TabPanel>
                    <TabPanel>
                        <ServerSettingsTab server={server!}></ServerSettingsTab>
                    </TabPanel>
                </TabPanels>
            </Tabs>
        </Skeleton>
    );
};

export default ServerOverview;
