import { TriangleDownIcon, TriangleUpIcon } from '@chakra-ui/icons';
import { Button, chakra, Heading, Table, Tbody, Td, Th, Thead, Tr } from '@chakra-ui/react';
import React, { FC, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Column, useSortBy, useTable } from 'react-table';
import Loader from '../../components/loader';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Agent } from '../../models/agent';
import Agents from '../../services/agents';

const AgentList: FC = () => {
    const [agents, setAgents] = useState<Agent[]>([]);
    const [isLoaded, setIsLoaded] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const history = useNavigate();

    const cancelToken = useCancellationToken();

    useEffect(() => {
        (async () => {
            try {
                const agents = await Agents.getList(cancelToken);
                setAgents(agents);
            } catch (err: any) {
                setError(err);
            }
            setIsLoaded(true);
        })();
    }, [cancelToken]);

    const rowClick = (agentId: string): void => {
        history(`/agent/${agentId}`);
    };

    const handleAddNewAgentClick = () => {
        history(`/agent/new`);
    };

    const columns = useMemo(() => {
        const columns: Column<Agent>[] = [
            {
                Header: 'Name',
                accessor: 'name',
            },
            {
                Header: 'State',
                accessor: 'status',
                Cell: ({ cell: { value } }) => {
                    switch (value) {
                        case 'offline':
                            return 'Offline';
                        case 'online':
                            return 'Online';
                        case 'versionmismatch':
                            return 'Update required';
                        default:
                            return value || '';
                    }
                },
            },
        ];
        return columns;
    }, []);

    const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } = useTable<Agent>(
        { columns, data: agents },
        useSortBy
    );

    return (
        <Loader isLoaded={isLoaded} error={error}>
            <Heading marginBottom={4}>Agents</Heading>

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
                <Tbody {...getTableBodyProps()}>
                    {rows.map((row) => {
                        prepareRow(row);
                        return (
                            <Tr
                                {...row.getRowProps()}
                                onClick={() => rowClick(row.original.agentId)}
                                style={{ cursor: 'pointer' }}
                            >
                                {row.cells.map((cell) => (
                                    <Td {...cell.getCellProps()}>{cell.render('Cell')}</Td>
                                ))}
                            </Tr>
                        );
                    })}
                </Tbody>
            </Table>

            <Button onClick={handleAddNewAgentClick}>Add new agent</Button>
        </Loader>
    );
};

export default AgentList;
