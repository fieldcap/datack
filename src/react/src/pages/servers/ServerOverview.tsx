import {
    Heading,
    Skeleton,
    Tab,
    TabList,
    TabPanel,
    TabPanels,
    Tabs
} from '@chakra-ui/react';
import axios from 'axios';
import React, { FC, useEffect } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import { Server } from '../../models/server';
import Servers from '../../services/servers';
import ServerJobsTab from './ServerJobsTab';

type RouteParams = {
    id: string;
};

const ServerOverview: FC<RouteComponentProps<RouteParams>> = (props) => {
    let [server, setServer] = React.useState<Server | null>(null);

    useEffect(() => {
        const getByIdCancelToken = axios.CancelToken.source();

        const fetchData = async () => {
            const result = await Servers.getById(
                props.match.params.id,
                getByIdCancelToken
            );
            setServer(result);
        };
        fetchData();

        return () => {
            getByIdCancelToken.cancel();
        };
    }, [props.match.params.id]);

    return (
        <Skeleton isLoaded={server != null}>
            <Heading marginBottom="24px">{server?.name}</Heading>
            <Tabs>
                <TabList>
                    <Tab>Overview</Tab>
                    <Tab>Jobs</Tab>
                    <Tab>Connection Settings</Tab>
                </TabList>

                <TabPanels>
                    <TabPanel>
                        <p>one!</p>
                    </TabPanel>
                    <TabPanel>
                        <ServerJobsTab server={server!}></ServerJobsTab>
                    </TabPanel>
                    <TabPanel>
                        <p>two!</p>
                    </TabPanel>
                </TabPanels>
            </Tabs>
        </Skeleton>
    );
};

export default ServerOverview;
