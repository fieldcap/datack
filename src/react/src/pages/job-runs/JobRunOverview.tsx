import { Box, Button, Flex, Heading, Tab, TabList, TabPanel, TabPanels, Tabs } from '@chakra-ui/react';
import * as signalR from '@microsoft/signalr';
import { HubConnectionState } from '@microsoft/signalr';
import React, { FC, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import Loader from '../../components/loader';
import useCancellationToken from '../../hooks/useCancellationToken';
import { JobRun } from '../../models/job-run';
import { JobRunTask } from '../../models/job-run-task';
import { JobRunTaskLog } from '../../models/job-run-task-log';
import JobRuns from '../../services/job-runs';
import JobRunOverviewHeader from './JobRunOverviewHeader';
import JobRunOverviewQueues from './JobRunOverviewQueues';
import JobRunOverviewTaskLogs from './JobRunOverviewTaskLogs';
import JobRunOverviewTasks from './JobRunOverviewTasks';

type JobRunOverviewParams = {
  id: string;
};

const JobRunOverview: FC = () => {
  const params = useParams<JobRunOverviewParams>();

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
        const result = await JobRuns.getById(params.id!, cancelToken);
        setJobRun(result);
      } catch (err: any) {
        setError(err);
      }
    })();

    (async () => {
      try {
        const result = await JobRuns.getTasks(params.id!, cancelToken);
        setJobRunTasks(result);
      } catch (err: any) {
        setError(err);
      }
    })();
  }, [params.id, cancelToken]);

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
      connection.off('JobRun');
      connection.off('JobRunTask');
      connection.off('JobRunTaskLog');

      connection.on('JobRun', (updatedJobRun: JobRun) => {
        if (updatedJobRun.jobRunId === jobRun?.jobRunId) {
          setJobRun(updatedJobRun);
        }
      });

      connection.on('JobRunTask', (updatedJobRunTask: JobRunTask) => {
        setJobRunTasks((m) =>
          m.map((m) => (m.jobRunTaskId === updatedJobRunTask.jobRunTaskId ? { ...updatedJobRunTask } : m))
        );
        if (activeJobRunTask != null && activeJobRunTask.jobRunTaskId === updatedJobRunTask.jobRunTaskId) {
          setActiveJobRunTask(updatedJobRunTask);
        }
      });

      connection.on('JobRunTaskLog', (updatedJobRunTaskLog: JobRunTaskLog) => {
        if (activeJobRunTask != null && activeJobRunTask.jobRunTaskId === updatedJobRunTaskLog.jobRunTaskId) {
          setJobRunTaskLogs((v) => [...v, updatedJobRunTaskLog]);
        }
      });
    }
  }, [connection, connection?.state, activeJobRunTask, jobRun]);

  const handleJobRunTaskClick = async (jobRunTask: JobRunTask) => {
    setJobRunTaskLogs([]);
    setActiveJobRunTask(null);

    const result = await JobRuns.getTaskLogs(jobRunTask.jobRunTaskId, cancelToken);

    setActiveJobRunTask(jobRunTask);
    setJobRunTaskLogs(result);
  };

  const stop = async () => {
    await JobRuns.stop(params.id!, cancelToken);
  };

  return (
    <Loader isLoaded={jobRun != null} error={error}>
      <Flex>
        <Flex flex="1" flexDirection="column">
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
          <Box>
            <Tabs>
              <TabList>
                <Tab>Task Log</Tab>
                <Tab>Queue Log</Tab>
              </TabList>

              <TabPanels>
                <TabPanel>
                  <Box overflowY="auto" overflowX="hidden" style={{ height: 'calc(100vh - 362px)' }}>
                    <JobRunOverviewTasks jobRunTasks={jobRunTasks} onRowClick={handleJobRunTaskClick} />
                  </Box>
                </TabPanel>
                <TabPanel>
                  <JobRunOverviewQueues jobRunTasks={jobRunTasks} />
                </TabPanel>
              </TabPanels>
            </Tabs>
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
