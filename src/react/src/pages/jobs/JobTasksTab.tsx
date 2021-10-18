import { TriangleDownIcon, TriangleUpIcon } from '@chakra-ui/icons';
import {
    Button,
    chakra,
    Skeleton,
    Table,
    Tbody,
    Td,
    Th,
    Thead,
    Tr
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { DndProvider } from 'react-dnd';
import HTML5Backend from 'react-dnd-html5-backend';
import { useHistory } from 'react-router-dom';
import { Column, useExpanded, useSortBy, useTable } from 'react-table';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Job } from '../../models/job';
import { JobTask } from '../../models/job-task';
import JobTasks from '../../services/jobTasks';

type JobTasksTabProps = {
    job: Job | null;
};

const JobTasksTab: FC<JobTasksTabProps> = (props) => {
    let [jobTasks, setJobTasks] = useState<JobTask[]>([]);
    let [isLoaded, setIsLoaded] = useState<boolean>(false);

    const history = useHistory();

    const cancelToken = useCancellationToken();

    useEffect(() => {
        if (props.job == null) {
            return;
        }

        (async () => {
            const result = await JobTasks.getForJob(
                props.job!.jobId,
                cancelToken
            );
            setJobTasks(result);
            setIsLoaded(true);
        })();
    }, [props.job, cancelToken]);

    const handleAddNewJobTaskClick = () => {
        if (props.job == null) {
            return;
        }

        history.push(`/job/${props.job?.jobId}/task/add`);
    };

    const rowClick = (jobTaskId: string): void => {
        history.push(`/job/${props.job?.jobId}/task/${jobTaskId}`);
    };

    const columns = React.useMemo(() => {
        const columns: Column<JobTask>[] = [
            {
                Header: 'Name',
                accessor: 'name',
            },
        ];
        return columns;
    }, []);

    const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } =
        useTable<JobTask>({ columns, data: jobTasks }, useSortBy, useExpanded);

    return (
        <>
            <Skeleton isLoaded={isLoaded}>
                <DndProvider backend={HTML5Backend}>
                    <Table {...getTableProps()}>
                        <Thead>
                            {headerGroups.map((headerGroup) => (
                                <Tr {...headerGroup.getHeaderGroupProps()}>
                                    {headerGroup.headers.map((column) => (
                                        <Th
                                            {...column.getHeaderProps(
                                                column.getSortByToggleProps()
                                            )}
                                        >
                                            {column.render('Header')}
                                            <chakra.span pl="4">
                                                {column.isSorted ? (
                                                    column.isSortedDesc ? (
                                                        <TriangleDownIcon aria-label="sorted descending" />
                                                    ) : (
                                                        <TriangleUpIcon aria-label="sorted ascending" />
                                                    )
                                                ) : null}
                                            </chakra.span>
                                        </Th>
                                    ))}
                                </Tr>
                            ))}
                        </Thead>
                        <Tbody {...getTableBodyProps()}>
                            {rows.map((row) => {
                                prepareRow(row);
                                return (
                                    <Tr
                                        {...row.getRowProps()}
                                        onClick={() =>
                                            rowClick(row.original.jobTaskId)
                                        }
                                        style={{ cursor: 'pointer' }}
                                    >
                                        {row.cells.map((cell) => (
                                            <Td {...cell.getCellProps()}>
                                                {cell.render('Cell')}
                                            </Td>
                                        ))}
                                    </Tr>
                                );
                            })}
                        </Tbody>
                    </Table>
                </DndProvider>
                <Button
                    marginTop="24px"
                    onClick={() => handleAddNewJobTaskClick()}
                >
                    Add new task
                </Button>
            </Skeleton>
        </>
    );
};

export default JobTasksTab;
