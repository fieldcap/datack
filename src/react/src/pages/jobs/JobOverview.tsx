import { Box, Button, Heading, Tab, TabList, TabPanel, TabPanels, Tabs } from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { RouteComponentProps, useHistory } from 'react-router-dom';
import Loader from '../../components/loader';
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
    const [job, setJob] = useState<Job | null>(null);
    const [error, setError] = useState<string | null>(null);

    const history = useHistory();

    const cancelToken = useCancellationToken();

    useEffect(() => {
        (async () => {
            try {
                setError(null);
                const result = await Jobs.getById(props.match.params.id, cancelToken);
                setJob(result);
            } catch (err: any) {
                setError(err);
            }
        })();
    }, [props.match.params.id, cancelToken]);

    const run = async () => {
        var jobRunId = await Jobs.run(job!.jobId);

        history.push(`/run/${jobRunId}`);
    };

    return (
        <Loader isLoaded={job != null} error={error}>
            <Box marginBottom={4}>
                <Heading>{job?.name}</Heading>
            </Box>
            <Box marginBottom={4}>
                <Button onClick={() => run()}>Run Job</Button>
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
        </Loader>
    );
};

export default JobOverview;
