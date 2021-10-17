import { Box, Flex, Heading, Skeleton } from '@chakra-ui/react';
import React, { FC, useEffect } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import useCancellationToken from '../../hooks/useCancellationToken';
import { JobRun } from '../../models/job-run';
import { JobRunTask } from '../../models/job-run-task';
import { JobRunTaskLog } from '../../models/job-run-task-log';
import JobRuns from '../../services/job-runs';
import JobRunOverviewHeader from './JobRunOverviewHeader';
import JobRunOverviewTaskLogs from './JobRunOverviewTaskLogs';
import JobRunOverviewTasks from './JobRunOverviewTasks';

type RouteParams = {
    id: string;
};

const JobRunOverview: FC<RouteComponentProps<RouteParams>> = (props) => {
    let [jobRun, setJobRun] = React.useState<JobRun | null>(null);
    let [jobRunTasks, setJobRunTasks] = React.useState<JobRunTask[]>([]);
    let [jobRunTaskLogs, setJobRunTaskLogs] = React.useState<JobRunTaskLog[]>(
        []
    );

    const cancelToken = useCancellationToken();

    useEffect(() => {
        (async () => {
            const result = await JobRuns.getById(
                props.match.params.id,
                cancelToken
            );
            setJobRun(result);
        })();

        (async () => {
            const result = await JobRuns.getTasks(
                props.match.params.id,
                cancelToken
            );
            setJobRunTasks(result);
        })();
    }, [props.match.params.id, cancelToken]);

    const handleJobRunTaskClick = async (jobRunTaskId: string) => {
        const result = await JobRuns.getTaskLogs(jobRunTaskId, cancelToken);

        setJobRunTaskLogs(result);
    };

    return (
        <Skeleton isLoaded={jobRun != null}>
            {jobRun != null ? (
                <Flex>
                    <Flex
                        flex="1"
                        flexDirection="column"
                        style={{ height: 'calc(100vh - 48px)' }}
                    >
                        <Box marginBottom="24px">
                            <Heading>Run for job: {jobRun.job.name}</Heading>
                        </Box>
                        <Box>
                            <JobRunOverviewHeader jobRun={jobRun} />
                        </Box>
                        <Box marginTop={6} overflowY="auto">
                            <JobRunOverviewTasks
                                jobRunTasks={jobRunTasks}
                                onRowClick={handleJobRunTaskClick}
                            />
                        </Box>
                    </Flex>
                    <Flex
                        flex="2"
                        flexDirection="column"
                        style={{ height: 'calc(100vh - 48px)' }}
                    >
                        <Box overflowY="auto">
                            <JobRunOverviewTaskLogs
                                jobRunTaskLogs={jobRunTaskLogs}
                            />
                        </Box>
                    </Flex>
                </Flex>
            ) : null}
        </Skeleton>
    );
};

export default JobRunOverview;
