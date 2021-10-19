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
import ServerJobList from './ServerJobList';
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
            {server != null ? (
                <>
                    <Heading marginBottom="24px">{server?.name}</Heading>
                    <Tabs>
                        <TabList>
                            <Tab>Summary</Tab>
                            <Tab>Jobs</Tab>
                            <Tab>Settings</Tab>
                        </TabList>

                        <TabPanels>
                            <TabPanel>
                                <ServerSummaryTab server={server} />
                            </TabPanel>
                            <TabPanel>
                                <ServerJobList server={server} />
                            </TabPanel>
                            <TabPanel>
                                <ServerSettingsTab server={server} />
                            </TabPanel>
                        </TabPanels>
                    </Tabs>
                </>
            ) : null}
        </Skeleton>
    );
};

export default ServerOverview;
