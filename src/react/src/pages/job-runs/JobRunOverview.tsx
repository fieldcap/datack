import { Box, Button, Flex, Heading } from '@chakra-ui/react';
import * as signalR from '@microsoft/signalr';
import { HubConnectionState } from '@microsoft/signalr';
import React, { FC, useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import Loader from '../../components/loader';
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
    const [jobRun, setJobRun] = useState<JobRun | null>(null);
    const [jobRunTasks, setJobRunTasks] = useState<JobRunTask[]>([]);
    const [jobRunTaskLogs, setJobRunTaskLogs] = useState<JobRunTaskLog[]>([]);
    const [activeJobRunTask, setActiveJobRunTask] = useState<JobRunTask | null>(null);
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [error, setError] = useState<string | null>(null);

    const cancelToken = useCancellationToken();

    useEffect(() => {
        (async () => {
            try {
                const result = await JobRuns.getById(props.match.params.id, cancelToken);
                setJobRun(result);
            } catch (err: any) {
                setError(err);
            }
        })();

        (async () => {
            try {
                const result = await JobRuns.getTasks(props.match.params.id, cancelToken);
                setJobRunTasks(result);
            } catch (err: any) {
                setError(err);
            }
        })();
    }, [props.match.params.id, cancelToken]);

    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder().withUrl('/hubs/web').withAutomaticReconnect().build();

        setConnection(newConnection);

        return () => {
            if (connection != null) {
                connection?.stop();
            }
        };
        // eslint-disable-next-line
    }, []);

    useEffect(() => {
        if (connection == null) {
            return;
        }

        if (connection.state !== HubConnectionState.Disconnected) {
            return;
        }

        connection
            .start()
            .then(() => {})
            .catch((err) => console.error(err));
    }, [connection]);

    useEffect(() => {
        if (connection == null) {
            return;
        }

        if (connection.state !== HubConnectionState.Disconnected) {
            connection.on('JobRun', (updatedJobRun: JobRun) => {
                if (updatedJobRun.jobRunId === jobRun?.jobRunId) {
                    setJobRun(jobRun);
                }
            });

            connection.on('JobRunTask', (updatedJobRunTasks: JobRunTask[]) => {
                if (updatedJobRunTasks.length === 0) {
                    return;
                }
                if (updatedJobRunTasks[0].jobRunId === jobRun?.jobRunId) {
                    setJobRunTasks(jobRunTasks);
                }
            });

            connection.on('JobRunTaskLog', (updatedJobRunTaskLog: JobRunTaskLog) => {
                if (activeJobRunTask != null && activeJobRunTask.jobRunTaskId === updatedJobRunTaskLog.jobRunTaskId) {
                    setJobRunTaskLogs((v) => [...v, updatedJobRunTaskLog]);
                }
            });
        }
    }, [connection, connection?.state, activeJobRunTask, jobRun, jobRunTasks]);

    const handleJobRunTaskClick = async (jobRunTask: JobRunTask) => {
        setJobRunTaskLogs([]);
        setActiveJobRunTask(null);

        const result = await JobRuns.getTaskLogs(jobRunTask.jobRunTaskId, cancelToken);

        setActiveJobRunTask(jobRunTask);
        setJobRunTaskLogs(result);
    };

    const stop = async () => {
        await JobRuns.stop(props.match.params.id, cancelToken);
    };

    return (
        <Loader isLoaded={jobRun != null} error={error}>
            <Flex>
                <Flex flex="1" flexDirection="column" style={{ height: 'calc(100vh - 48px)' }}>
                    {jobRun != null ? (
                        <>
                            <Box marginBottom={4}>
                                <Heading>Run for job: {jobRun.job.name}</Heading>
                            </Box>
                            {jobRun.completed == null ? (
                                <Box marginBottom={4}>
                                    <Button onClick={() => stop()}>Stop Job Run</Button>
                                </Box>
                            ) : null}
                        </>
                    ) : null}
                    <Box>
                        <Box marginBottom={4}>
                            <JobRunOverviewHeader jobRun={jobRun!} />
                        </Box>
                    </Box>
                    <Box overflowY="auto" overflowX="hidden">
                        <JobRunOverviewTasks jobRunTasks={jobRunTasks} onRowClick={handleJobRunTaskClick} />
                    </Box>
                </Flex>
                <Flex flex="1" flexDirection="column" style={{ height: 'calc(100vh - 48px)' }}>
                    <Box overflowY="auto" overflowX="hidden">
                        {activeJobRunTask != null ? (
                            <JobRunOverviewTaskLogs jobRunTaskLogs={jobRunTaskLogs} jobRunTask={activeJobRunTask} />
                        ) : null}
                    </Box>
                </Flex>
            </Flex>
        </Loader>
    );
};

export default JobRunOverview;
