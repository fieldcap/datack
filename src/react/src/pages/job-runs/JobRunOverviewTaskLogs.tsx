import { TriangleDownIcon, TriangleUpIcon } from '@chakra-ui/icons';
import { Th, Thead } from '@chakra-ui/react';
import { chakra } from '@chakra-ui/system';
import { Table, Tbody, Td, Tr } from '@chakra-ui/table';
import { format } from 'date-fns';
import React, { FC, useMemo } from 'react';
import { Column, useSortBy, useTable } from 'react-table';
import { JobRunTaskLog } from '../../models/job-run-task-log';

type Props = {
    jobRunTaskLogs: JobRunTaskLog[];
};

const JobRunOverviewTaskLogs: FC<Props> = (props) => {
    const { jobRunTaskLogs } = props;

    const columns = useMemo(() => {
        const columns: Column<JobRunTaskLog>[] = [
            {
                Header: 'Date',
                accessor: 'dateTime',
                sortType: 'datetime',
                Cell: ({ cell: { value } }) => {
                    return format(value, 'HH:mm:ss');
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
        <Table {...getTableProps()} style={{ width: 'auto' }} size="sm">
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
    );
};

export default JobRunOverviewTaskLogs;
