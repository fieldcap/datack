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
import axios from 'axios';
import React, { FC, useEffect } from 'react';
import { RouteComponentProps } from 'react-router-dom';
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

    const run = async (type: 'Full' | 'Diff' | 'Log') => {
        if (job == null) {
            return;
        }

        await Jobs.run(job.jobId, type);
    };

    return (
        <Skeleton isLoaded={job != null}>
            <Box marginBottom="24px">
                <Heading>{job?.name}</Heading>
                {/* <Link href={`/#/server/${job?.serverId}`}>
                    {job?.server?.name}
                </Link> */}
            </Box>
            <Box marginBottom="24px">
                <Button onClick={() => run('Full')}>Run Full Backup</Button>
                <Button marginLeft="12px" onClick={() => run('Diff')}>
                    Run Diff Backup
                </Button>
                <Button marginLeft="12px" onClick={() => run('Log')}>
                    Run Transaction Log Backup
                </Button>
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
