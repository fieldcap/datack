import {
    CheckIcon,
    TimeIcon,
    TriangleDownIcon,
    TriangleUpIcon,
    WarningIcon
} from '@chakra-ui/icons';
import {
    Alert,
    AlertDescription,
    AlertIcon,
    Box,
    chakra,
    Heading,
    Skeleton,
    Spinner,
    Table,
    Tbody,
    Td,
    Th,
    Thead,
    Tr
} from '@chakra-ui/react';
import { format, formatDistanceStrict } from 'date-fns';
import React, { FC, useEffect, useMemo, useState } from 'react';
import { RouteComponentProps, useHistory } from 'react-router-dom';
import { Column, useSortBy, useTable } from 'react-table';
import useCancellationToken from '../../hooks/useCancellationToken';
import { JobRun } from '../../models/job-run';
import JobRuns from '../../services/job-runs';

type RouteParams = {};

const History: FC<RouteComponentProps<RouteParams>> = (props) => {
    const [isLoaded, setIsLoaded] = useState<boolean>(false);
    const [jobRuns, setJobRuns] = useState<JobRun[]>([]);
    const [error, setError] = useState<string | null>(null);

    const history = useHistory();

    const cancelToken = useCancellationToken();

    useEffect(() => {
        (async () => {
            setError(null);
            try {
                const result = await JobRuns.getList(cancelToken);
                setJobRuns(result);
                setIsLoaded(true);
            } catch (err: any) {
                setError(err);
            }
        })();
    }, [cancelToken]);

    const rowClick = (jobRunId: string): void => {
        history.push(`/run/${jobRunId}`);
    };

    const columns = useMemo(() => {
        const columns: Column<JobRun>[] = [
            {
                Header: 'Job',
                accessor: (r) => r.job.name,
            },
            {
                Header: 'Started',
                accessor: 'started',
                sortType: 'datetime',
                Cell: ({ cell: { value } }) =>
                    format(value, 'd MMMM yyyy HH:mm'),
            },
            {
                Header: 'Completed',
                accessor: 'completed',
                sortType: 'datetime',
                Cell: ({ cell: { value } }) => {
                    if (!value) {
                        return '';
                    }
                    return format(value, 'd MMMM yyyy HH:mm');
                },
            },
            {
                Header: 'Runtime',
                accessor: 'runTime',
                sortType: 'datetime',
                Cell: ({ cell: { value } }) => {
                    if (value == null) {
                        return '';
                    }
                    return formatDistanceStrict(0, value * 1000);
                },
            },
            {
                Header: 'Result',
                accessor: 'isError',
                Cell: (c) => {
                    if (c.cell.value) {
                        return <WarningIcon style={{ color: 'red' }} />;
                    }

                    if (
                        c.row.original.completed == null &&
                        c.row.original.started == null
                    ) {
                        return <TimeIcon style={{ color: 'blue' }} />;
                    }

                    if (
                        c.row.original.completed == null &&
                        c.row.original.started != null
                    ) {
                        return <Spinner />;
                    }
                    return <CheckIcon style={{ color: 'green' }} />;
                },
            },
        ];
        return columns;
    }, []);

    const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } =
        useTable<JobRun>({ columns, data: jobRuns }, useSortBy);

    if (error) {
        return (
            <Alert marginTop="24px" status="error">
                <AlertIcon />
                <AlertDescription>{error}</AlertDescription>
            </Alert>
        );
    }

    return (
        <Skeleton isLoaded={isLoaded}>
            <Box marginBottom="24px">
                <Heading>History</Heading>
            </Box>
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
                                onClick={() => rowClick(row.original.jobRunId)}
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
        </Skeleton>
    );
};

export default History;
