import { Heading, Table, Tbody, Td, Tr } from '@chakra-ui/react';
import { format, formatDuration, intervalToDuration, parseISO } from 'date-fns';
import React, { FC, useEffect, useState } from 'react';
import { JobRunTask } from '../../models/job-run-task';
import { JobTask } from '../../models/job-task';

type Props = {
    jobRunTasks: JobRunTask[];
};

type TaskSummary = {
    jobTask: JobTask;
    success: number;
    errors: number;
    totalRunTime: number;
    firstStart: Date | null;
    lastStart: Date | null;
    firstComplete: Date | null;
    lastComplete: Date | null;
    longestItem: string | null;
    longestRunTime: number | null;
};

const JobRunOverviewQueues: FC<Props> = (props) => {
    const { jobRunTasks } = props;

    const [queues, setQueues] = useState<TaskSummary[]>([]);

    useEffect(() => {
        const jobTaskGroups = jobRunTasks
            .orderBy((m) => m.taskOrder)
            .thenBy((m) => m.itemOrder)
            .groupBy((m) => m.jobTaskId);

        const queues: TaskSummary[] = [];

        for (let jobTaskId in jobTaskGroups) {
            const jobTasks = jobTaskGroups[jobTaskId];

            let queue = queues.firstOrDefault((m) => m.jobTask.jobTaskId === jobTaskId);

            if (queue == null) {
                queue = {
                    jobTask: jobTasks[0].jobTask!,
                    errors: 0,
                    success: 0,
                    totalRunTime: 0,
                    firstStart: null,
                    lastStart: null,
                    firstComplete: null,
                    lastComplete: null,
                    longestItem: null,
                    longestRunTime: null,
                };
                queues.push(queue);
            }

            jobTasks.forEach((jobTask) => {
                if (jobTask.isError) {
                    queue.errors += 1;
                } else {
                    queue.success += 1;
                }
                queue.totalRunTime += jobTask.runTime ?? 0;

                if (jobTask.runTime != null) {
                    if (queue.longestRunTime == null || jobTask.runTime > queue.longestRunTime) {
                        queue.longestItem = jobTask.itemName;
                        queue.longestRunTime = jobTask.runTime;
                    }
                }

                if (jobTask.started != null) {
                    const started = parseISO(jobTask.started);
                    if (queue.firstStart == null) {
                        queue.firstStart = started;
                    } else if (started < queue.firstStart) {
                        queue.firstStart = started;
                    }
                    if (queue.lastStart == null) {
                        queue.lastStart = started;
                    } else if (started > queue.lastStart) {
                        queue.lastStart = started;
                    }
                }

                if (jobTask.completed != null) {
                    const completed = parseISO(jobTask.completed);
                    if (queue.firstComplete == null) {
                        queue.firstComplete = completed;
                    } else if (completed < queue.firstComplete) {
                        queue.firstComplete = completed;
                    }
                    if (queue.lastComplete == null) {
                        queue.lastComplete = completed;
                    } else if (completed > queue.lastComplete) {
                        queue.lastComplete = completed;
                    }
                }
            });
        }
        setQueues(queues);
        console.log(queues);
    }, [jobRunTasks]);

    return (
        <>
            {queues.map((queue, index) => (
                <div key={queue.jobTask.jobTaskId}>
                    <Heading size="md" marginBottom={4}>
                        {index + 1}: {queue.jobTask.name}
                    </Heading>
                    <Table variant="simple" marginBottom={8}>
                        <Tbody>
                            <Tr>
                                <Td style={{ fontWeight: 'bold', width: '200px' }}>Successful items</Td>
                                <Td>{queue.success}</Td>
                            </Tr>
                            <Tr>
                                <Td style={{ fontWeight: 'bold' }}>Error items</Td>
                                <Td>{queue.errors}</Td>
                            </Tr>
                            <Tr>
                                <Td style={{ fontWeight: 'bold' }}>Total run time</Td>
                                <Td>
                                    {formatDuration(intervalToDuration({ start: 0, end: queue.totalRunTime * 1000 }))}
                                </Td>
                            </Tr>
                            <Tr>
                                <Td style={{ fontWeight: 'bold' }}>Average run time</Td>
                                <Td>
                                    {formatDuration(
                                        intervalToDuration({
                                            start: 0,
                                            end: (queue.totalRunTime * 1000) / (queue.success + queue.errors),
                                        })
                                    )}
                                </Td>
                            </Tr>
                            <Tr>
                                <Td style={{ fontWeight: 'bold' }}>First Start</Td>
                                <Td>
                                    {queue.firstStart != null ? format(queue.firstStart, 'd MMMM yyyy HH:mm') : 'Never'}
                                </Td>
                            </Tr>
                            <Tr>
                                <Td style={{ fontWeight: 'bold' }}>Last Start</Td>
                                <Td>
                                    {queue.lastStart != null ? format(queue.lastStart, 'd MMMM yyyy HH:mm') : 'Never'}
                                </Td>
                            </Tr>
                            <Tr>
                                <Td style={{ fontWeight: 'bold' }}>First Complete</Td>
                                <Td>
                                    {queue.firstStart != null ? format(queue.firstStart, 'd MMMM yyyy HH:mm') : 'Never'}
                                </Td>
                            </Tr>
                            <Tr>
                                <Td style={{ fontWeight: 'bold' }}>Last Complete</Td>
                                <Td>
                                    {queue.lastStart != null ? format(queue.lastStart, 'd MMMM yyyy HH:mm') : 'Never'}
                                </Td>
                            </Tr>
                            {queue.longestItem != null && queue.longestRunTime != null ? (
                                <Tr>
                                    <Td style={{ fontWeight: 'bold' }}>Longest Runtime</Td>
                                    <Td>
                                        {formatDuration(
                                            intervalToDuration({ start: 0, end: queue.longestRunTime * 1000 })
                                        )}{' '}
                                        ({queue.longestItem})
                                    </Td>
                                </Tr>
                            ) : null}
                        </Tbody>
                    </Table>
                </div>
            ))}
        </>
    );
};

export default JobRunOverviewQueues;
