import { TriangleDownIcon, TriangleUpIcon } from '@chakra-ui/icons';
import { chakra, Table, Tbody, Td, Th, Thead, Tr } from '@chakra-ui/react';
import axios from 'axios';
import React, { FC, useEffect, useState } from 'react';
import { useHistory } from 'react-router-dom';
import { Column, useSortBy, useTable } from 'react-table';
import { Job } from '../../models/job';
import { Server } from '../../models/server';
import Jobs from '../../services/jobs';

type Props = {
    server: Server;
};

const ServerSummaryTab: FC<Props> = (props) => {
    const [jobs, setJobs] = useState<Job[]>([]);

    const history = useHistory();

    useEffect(() => {
        if (!props.server) {
            return;
        }

        const cancelToken = axios.CancelToken.source();

        (async () => {
            const jobs = await Jobs.getForServer(
                props.server.serverId,
                cancelToken
            );

            setJobs(jobs);
        })();

        return () => {
            cancelToken.cancel();
        };
    }, [props.server]);

    const rowClick = (jobId: string): void => {
        history.push(`/job/${jobId}`);
    };

    const columns = React.useMemo(() => {
        const columns: Column<Job>[] = [
            {
                Header: 'Name',
                accessor: 'name',
            },
        ];
        return columns;
    }, []);

    const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } =
        useTable<Job>({ columns, data: jobs }, useSortBy);

    return (
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
                            onClick={() => rowClick(row.original.jobId)}
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
    );
};

export default ServerSummaryTab;
