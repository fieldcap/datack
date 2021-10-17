import {
    Box,
    Button,
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
import { Job } from '../../models/job';
import Jobs from '../../services/jobs';
import JobHistoryTab from './JobHistoryTab';
import JobSettingsTab from './JobSettingsTab';
import JobTasksTab from './JobTasksTab';

type RouteParams = {
    id: string;
};

const JobOverview: FC<RouteComponentProps<RouteParams>> = (props) => {
    let [job, setJob] = React.useState<Job | null>(null);

    const cancelToken = useCancellationToken();

    useEffect(() => {
        (async () => {
            const result = await Jobs.getById(
                props.match.params.id,
                cancelToken
            );
            setJob(result);
        })();
    }, [props.match.params.id]);

    const run = async () => {
        if (job == null) {
            return;
        }

        await Jobs.run(job.jobId);
    };

    return (
        <Skeleton isLoaded={job != null}>
            <Box marginBottom="24px">
                <Heading>{job?.name}</Heading>
            </Box>
            <Box marginBottom="24px">
                <Button onClick={() => run()}>Run Backup</Button>
            </Box>
            <Tabs>
                <TabList>
                    <Tab>History</Tab>
                    <Tab>Tasks</Tab>
                    <Tab>Configuration</Tab>
                </TabList>

                <TabPanels>
                    <TabPanel>
                        <JobHistoryTab job={job}></JobHistoryTab>
                    </TabPanel>
                    <TabPanel>
                        <JobTasksTab job={job}></JobTasksTab>
                    </TabPanel>
                    <TabPanel>
                        <JobSettingsTab job={job}></JobSettingsTab>
                    </TabPanel>
                </TabPanels>
            </Tabs>
        </Skeleton>
    );
};

export default JobOverview;
