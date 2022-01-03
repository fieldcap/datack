import { ArrowUpDownIcon, TriangleDownIcon, TriangleUpIcon } from '@chakra-ui/icons';
import { Button, chakra, Table, Tbody, Td, Th, Thead, Tr } from '@chakra-ui/react';
import React, { FC, useEffect, useMemo, useState } from 'react';
import { DragDropContext, Draggable, Droppable, DropResult } from 'react-beautiful-dnd';
import { useNavigate } from 'react-router-dom';
import { Column, useExpanded, useSortBy, useTable } from 'react-table';
import Loader from '../../components/loader';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Job } from '../../models/job';
import { JobTask } from '../../models/job-task';
import JobTasks from '../../services/jobTasks';

type JobTasksTabProps = {
    job: Job | null;
};

const JobTasksTab: FC<JobTasksTabProps> = (props) => {
    const [jobTasks, setJobTasks] = useState<JobTask[]>([]);
    const [isLoaded, setIsLoaded] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const history = useNavigate();

    const cancelToken = useCancellationToken();

    useEffect(() => {
        if (props.job == null) {
            return;
        }

        (async () => {
            try {
                setJobTasks([]);
                setIsLoaded(false);
                setError(null);
                const result = await JobTasks.getForJob(props.job!.jobId, cancelToken);
                setJobTasks(result);
                setIsLoaded(true);
            } catch (err: any) {
                setError(err);
            }
        })();
    }, [props.job, cancelToken]);

    const handleAddNewJobTaskClick = () => {
        if (props.job == null) {
            return;
        }

        history(`/job/${props.job?.jobId}/task/add`);
    };

    const rowClick = (jobTaskId: string): void => {
        history(`/job/${props.job?.jobId}/task/${jobTaskId}`);
    };

    const columns = useMemo(() => {
        const columns: Column<JobTask>[] = [
            {
                Header: '',
                id: 'click',
                Cell: (c: any) => (
                    <span {...c.dragHandleProps}>
                        <ArrowUpDownIcon />
                    </span>
                ),
            },
            {
                Header: 'Name',
                accessor: 'name',
            },
            {
                Header: 'Type',
                accessor: 'type',
                Cell: ({ cell: { value } }) => {
                    return JobTasks.map(value);
                },
            },
            {
                Header: 'Use artifact',
                accessor: (a) => JobTasks.map(a.usePreviousTaskArtifactsFromJobTask?.type),
            },
            {
                Header: 'Agent',
                accessor: (a) => a.agent?.name,
            },
        ];
        return columns;
    }, []);

    const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } = useTable<JobTask>(
        { columns, data: jobTasks },
        useSortBy,
        useExpanded
    );

    const handleDragEnd = async (result: DropResult) => {
        const { source, destination } = result;
        if (!destination) {
            return;
        }
        const newData = [...jobTasks];
        const [movedRow] = newData.splice(source.index, 1);
        newData.splice(destination.index, 0, movedRow);
        setJobTasks(newData);

        await JobTasks.reOrder(
            props.job!.jobId,
            newData.map((m) => m.jobTaskId),
            cancelToken
        );
    };

    return (
        <Loader isLoaded={isLoaded} error={error}>
            <Table {...getTableProps()} marginBottom={4}>
                <Thead>
                    {headerGroups.map((headerGroup) => (
                        <Tr {...headerGroup.getHeaderGroupProps()}>
                            {headerGroup.headers.map((column) => (
                                <Th {...column.getHeaderProps(column.getSortByToggleProps())}>
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
                <DragDropContext onDragEnd={handleDragEnd}>
                    <Droppable droppableId="table-body">
                        {(provided, snapshot) => (
                            <Tbody {...getTableBodyProps()} {...provided.droppableProps} ref={provided.innerRef}>
                                {rows.map((row) => {
                                    prepareRow(row);
                                    return (
                                        <Draggable
                                            draggableId={row.original.jobTaskId}
                                            key={row.original.jobTaskId}
                                            index={row.index}
                                        >
                                            {(provided, snapshot) => {
                                                return (
                                                    <Tr
                                                        {...row.getRowProps()}
                                                        {...provided.draggableProps}
                                                        ref={provided.innerRef}
                                                    >
                                                        {row.cells.map((cell, index) => (
                                                            <Td
                                                                onClick={() => rowClick(cell.row.original.jobTaskId)}
                                                                {...cell.getCellProps({
                                                                    style: {
                                                                        cursor: index > 0 ? 'pointer' : 'move',
                                                                    },
                                                                })}
                                                            >
                                                                {cell.render('Cell', {
                                                                    dragHandleProps: provided.dragHandleProps,
                                                                    isSomethingDragging: snapshot.isDragging,
                                                                })}
                                                            </Td>
                                                        ))}
                                                    </Tr>
                                                );
                                            }}
                                        </Draggable>
                                    );
                                })}
                                {provided.placeholder}
                            </Tbody>
                        )}
                    </Droppable>
                </DragDropContext>
            </Table>
            <Button onClick={() => handleAddNewJobTaskClick()}>
                Add new task
            </Button>
        </Loader>
    );
};

export default JobTasksTab;
