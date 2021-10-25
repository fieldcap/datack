import { TriangleDownIcon, TriangleUpIcon } from '@chakra-ui/icons';
import { Heading, Th, Thead } from '@chakra-ui/react';
import { chakra } from '@chakra-ui/system';
import { Table, Tbody, Td, Tr } from '@chakra-ui/table';
import { format, formatDistanceStrict, parseISO } from 'date-fns';
import React, { FC, useEffect, useMemo, useRef } from 'react';
import { Column, useSortBy, useTable } from 'react-table';
import { JobRunTask } from '../../models/job-run-task';
import { JobRunTaskLog } from '../../models/job-run-task-log';
import JobTasks from '../../services/jobTasks';

type Props = {
    jobRunTask: JobRunTask;
    jobRunTaskLogs: JobRunTaskLog[];
};

const JobRunOverviewTaskLogs: FC<Props> = (props) => {
    const { jobRunTaskLogs } = props;

    const bottomRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (bottomRef == null || bottomRef.current == null) {
            return;
        }
        if (props.jobRunTask.completed != null) {
            return;
        }

        bottomRef.current.scrollIntoView({ behavior: 'auto' });
    }, [props.jobRunTaskLogs, props.jobRunTask.completed]);

    const columns = useMemo(() => {
        const columns: Column<JobRunTaskLog>[] = [
            {
                Header: 'Date',
                accessor: 'dateTime',
                sortType: 'datetime',
                Cell: ({ cell: { value } }) => {
                    return format(parseISO(value), 'HH:mm:ss');
                },
            },
            {
                Header: 'Message',
                accessor: 'message',
                Cell: (cell) => {
                    if (cell.row.original.isError) {
                        <span style={{ color: 'red' }}>{cell.value}</span>;
                    }
                    return cell.value;
                },
            },
        ];
        return columns;
    }, []);

    const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } = useTable<JobRunTaskLog>(
        { columns, data: jobRunTaskLogs },
        useSortBy
    );

    return (
        <>
            <Heading marginBottom={4} size="sm">
                Summary
            </Heading>
            <Table style={{ width: '100%' }} size="sm" marginBottom={4}>
                <Tbody>
                    <Tr>
                        <Td style={{ fontWeight: 'bold', width:'110px' }}>Started</Td>
                        <Td>
                            {props.jobRunTask.started != null
                                ? format(parseISO(props.jobRunTask.started), 'd MMMM yyyy HH:mm')
                                : ''}
                        </Td>
                    </Tr>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>Completed</Td>
                        <Td>
                            {props.jobRunTask.completed != null
                                ? format(parseISO(props.jobRunTask.completed), 'd MMMM yyyy HH:mm')
                                : ''}
                        </Td>
                    </Tr>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>Runtime</Td>
                        <Td>
                            {props.jobRunTask.runTime != null
                                ? formatDistanceStrict(0, props.jobRunTask.runTime * 1000)
                                : ''}
                        </Td>
                    </Tr>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>Task Type</Td>
                        <Td>{JobTasks.map(props.jobRunTask.type)}</Td>
                    </Tr>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>Item Name</Td>
                        <Td>{props.jobRunTask.itemName || ''}</Td>
                    </Tr>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>Artifact</Td>
                        <Td>{props.jobRunTask.resultArtifact || ''}</Td>
                    </Tr>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>Task Order</Td>
                        <Td>{props.jobRunTask.taskOrder}</Td>
                    </Tr>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>Item Order</Td>
                        <Td>{props.jobRunTask.itemOrder}</Td>
                    </Tr>
                    <Tr>
                        <Td style={{ fontWeight: 'bold' }}>Result</Td>
                        <Td>{props.jobRunTask.result}</Td>
                    </Tr>
                </Tbody>
            </Table>
            <Heading marginBottom={4} size="sm">
                Message log
            </Heading>
            <Table {...getTableProps()} style={{ width: '100%' }} size="sm">
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
                <Tbody {...getTableBodyProps()}>
                    {rows.map((row) => {
                        prepareRow(row);
                        return (
                            <Tr {...row.getRowProps()}>
                                {row.cells.map((cell) => (
                                    <Td {...cell.getCellProps()}>{cell.render('Cell')}</Td>
                                ))}
                            </Tr>
                        );
                    })}
                </Tbody>
            </Table>
            <div ref={bottomRef}></div>
        </>
    );
};

export default JobRunOverviewTaskLogs;
