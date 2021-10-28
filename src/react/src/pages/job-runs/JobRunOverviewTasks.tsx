import { CheckIcon, TimeIcon, TriangleDownIcon, TriangleUpIcon, WarningIcon } from '@chakra-ui/icons';
import { Checkbox, FormControl, HStack, Select, Spinner, Th, Thead } from '@chakra-ui/react';
import { chakra } from '@chakra-ui/system';
import { Table, Tbody, Td, Tr } from '@chakra-ui/table';
import { format, parseISO } from 'date-fns';
import React, { FC, useEffect, useMemo, useState } from 'react';
import { Column, useSortBy, useTable } from 'react-table';
import { JobRunTask } from '../../models/job-run-task';
import { formatRuntimeTask } from '../../services/date';
import JobTasks from '../../services/jobTasks';

type Props = {
    jobRunTasks: JobRunTask[];
    onRowClick: (jobRunTask: JobRunTask) => void;
};

const JobRunOverviewTasks: FC<Props> = (props) => {
    const { onRowClick } = props;

    const [gridItems, setGridItems] = useState<JobRunTask[]>([]);
    const [filterItem, setFilterItem] = useState<string>('');
    const [filterErrors, setFilterErrors] = useState<boolean>(false);
    const [items, setItems] = useState<string[]>([]);

    useEffect(() => {
        const items = props.jobRunTasks
            .select((m) => m.itemName)
            .distinctBy((m) => m)
            .orderBy((m) => m);
        setItems(items);
        // eslint-disable-next-line
    }, [props.jobRunTasks]);

    useEffect(() => {
        let items = props.jobRunTasks || [];

        if (filterItem != null && filterItem !== '') {
            items = items.filter((m) => m.itemName === filterItem);
        }

        if (filterErrors) {
            items = items.filter(m => m.isError);
        }

        setGridItems(items);
    }, [props.jobRunTasks, filterItem, filterErrors]);

    const columns = useMemo(() => {
        const columns: Column<JobRunTask>[] = [
            {
                Header: 'Started',
                accessor: 'started',
                sortType: 'datetime',
                Cell: ({ cell: { value } }) => {
                    if (!value) {
                        return '';
                    }
                    return format(parseISO(value), 'HH:mm:ss');
                },
            },
            {
                Header: 'Completed',
                accessor: 'completed',
                sortType: 'datetime',
                Cell: ({ cell: { value } }) => {
                    if (!value) {
                        return '';
                    }
                    return format(parseISO(value), 'HH:mm:ss');
                },
            },
            {
                Header: 'Runtime',
                accessor: 'runTime',
                Cell: (c) => formatRuntimeTask(c.row.original),
            },
            {
                Header: 'Task',
                accessor: 'type',
                Cell: ({ cell: { value } }) => {
                    return JobTasks.map(value);
                },
            },
            {
                Header: 'Item',
                accessor: 'itemName',
            },
            {
                Header: 'Result',
                accessor: 'isError',
                Cell: (c) => {
                    if (c.cell.value) {
                        return <WarningIcon style={{ color: 'red' }} />;
                    }

                    if (c.row.original.completed == null && c.row.original.started == null) {
                        return <TimeIcon style={{ color: 'blue' }} />;
                    }

                    if (c.row.original.completed == null && c.row.original.started != null) {
                        return <Spinner size="sm" />;
                    }
                    return <CheckIcon style={{ color: 'green' }} />;
                },
            },
        ];
        return columns;
    }, []);

    const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } = useTable<JobRunTask>(
        { columns, data: gridItems },
        useSortBy
    );

    return (
        <>
            <form>
                <HStack marginBottom={4}>
                    <FormControl id="name" w={250}>
                        <Select
                            placeholder="Select an item to filter by"
                            value={filterItem || ''}
                            onChange={(e) => setFilterItem(e.target.value)}

                        >
                            {items.map((item) => (
                                <option value={item} key={item}>
                                    {item}
                                </option>
                            ))}
                        </Select>
                    </FormControl>
                    <Checkbox isChecked={filterErrors} onChange={(e) => setFilterErrors(e.currentTarget.checked)}>
                        Show only errors
                    </Checkbox>
                </HStack>
            </form>
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
                            <Tr
                                {...row.getRowProps()}
                                onClick={() => onRowClick(row.original)}
                                style={{ cursor: 'pointer' }}
                                id={`JobRunTaskRow-${row.original.jobRunTaskId}`}
                            >
                                {row.cells.map((cell) => (
                                    <Td {...cell.getCellProps()}>{cell.render('Cell')}</Td>
                                ))}
                            </Tr>
                        );
                    })}
                </Tbody>
            </Table>
        </>
    );
};

export default JobRunOverviewTasks;
