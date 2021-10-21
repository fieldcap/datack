import { TriangleDownIcon, TriangleUpIcon } from '@chakra-ui/icons';
import {
    Button,
    chakra,
    Heading, Table,
    Tbody,
    Td,
    Th,
    Thead,
    Tr
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { RouteComponentProps, useHistory } from 'react-router-dom';
import { Column, useSortBy, useTable } from 'react-table';
import Loader from '../../components/loader';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Job } from '../../models/job';
import Jobs from '../../services/jobs';

const JobList: FC<RouteComponentProps> = () => {
    const [jobs, setJobs] = useState<Job[]>([]);
    const [isLoaded, setIsLoaded] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const history = useHistory();

    const cancelToken = useCancellationToken();

    useEffect(() => {
        (async () => {
            setError(null);
            try {
                const jobs = await Jobs.getList(cancelToken);
                setJobs(jobs);
                setIsLoaded(true);
            } catch (err: any) {
                setError(err);
            }
        })();
    }, [cancelToken]);

    const rowClick = (jobId: string): void => {
        history.push(`/job/${jobId}`);
    };

    const handleAddNewJobClick = () => {
        history.push(`/job/new`);
    };

    const columns = React.useMemo(() => {
        const columns: Column<Job>[] = [
            {
                Header: 'Name',
                accessor: 'name',
            },
            {
                Header: 'Group',
                accessor: 'group',
            },
            {
                Header: 'Priority',
                accessor: 'priority',
            },
        ];
        return columns;
    }, []);

    const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } =
        useTable<Job>({ columns, data: jobs }, useSortBy);

    return (
        <Loader isLoaded={isLoaded} error={error}>
            <Heading marginBottom="24px">Jobs</Heading>
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

            <Button marginTop="24px" onClick={handleAddNewJobClick}>
                Add new job
            </Button>
        </Loader>
    );
};

export default JobList;
