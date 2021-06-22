import {
    Box,
    Heading,
    Link,
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
import { Job } from '../../models/job';
import Jobs from '../../services/jobs';
import JobStepsTab from './JobStepsTab';

type RouteParams = {
    id: string;
};

const JobOverview: FC<RouteComponentProps<RouteParams>> = (props) => {
    let [job, setJob] = React.useState<Job | null>(null);

    useEffect(() => {
        const getByIdCancelToken = axios.CancelToken.source();

        (async () => {
            const result = await Jobs.getById(
                props.match.params.id,
                getByIdCancelToken
            );
            setJob(result);
        })();

        return () => {
            getByIdCancelToken.cancel();
        };
    }, [props.match.params.id]);

    return (
        <Skeleton isLoaded={job != null}>
            <Box marginBottom="24px">
                <Heading>{job?.name}</Heading>
                <Link href={`/#/server/${job?.serverId}`}>
                    {job?.server?.name}
                </Link>
            </Box>
            <Tabs>
                <TabList>
                    <Tab>History</Tab>
                    <Tab>Steps</Tab>
                </TabList>

                <TabPanels>
                    <TabPanel></TabPanel>
                    <TabPanel>
                        <JobStepsTab job={job}></JobStepsTab>
                    </TabPanel>
                </TabPanels>
            </Tabs>
        </Skeleton>
    );
};

export default JobOverview;
