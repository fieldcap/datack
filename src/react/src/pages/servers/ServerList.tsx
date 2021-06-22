import { TriangleDownIcon, TriangleUpIcon } from '@chakra-ui/icons';
import {
    Button,
    chakra,
    Heading,
    Skeleton,
    Table,
    Tbody,
    Td,
    Th,
    Thead,
    Tr
} from '@chakra-ui/react';
import axios from 'axios';
import React, { FC, useEffect, useState } from 'react';
import { RouteComponentProps, useHistory } from 'react-router-dom';
import { Column, useSortBy, useTable } from 'react-table';
import { Server } from '../../models/server';
import Servers from '../../services/servers';

const ServerList: FC<RouteComponentProps> = () => {
    const [servers, setServers] = useState<Server[]>([]);
    const [isLoaded, setIsLoaded] = useState<boolean>(false);

    const history = useHistory();

    useEffect(() => {
        const getByIdCancelToken = axios.CancelToken.source();

        (async () => {
            const servers = await Servers.getList(getByIdCancelToken);
            setServers(servers);
            setIsLoaded(true);
        })();

        return () => {
            getByIdCancelToken.cancel();
        };
    }, []);

    const rowClick = (serverId: string): void => {
        history.push(`/server/${serverId}`);
    };

    const handleAddNewServerClick = () => {
        history.push(`/server/new`);
    }

    const columns = React.useMemo(() => {
        const columns: Column<Server>[] = [
            {
                Header: 'Name',
                accessor: 'name',
            },
        ];
        return columns;
    }, []);

    const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } =
        useTable<Server>({ columns, data: servers }, useSortBy);

    return (
        <Skeleton isLoaded={isLoaded}>
            <Heading marginBottom="24px">Servers</Heading>

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
                            <Tr {...row.getRowProps()} onClick={() => rowClick(row.original.serverId)} style={{ cursor: 'pointer' }}>
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

            <Button marginTop="24px" onClick={handleAddNewServerClick}>Add new server</Button>
        </Skeleton>
    );
};

export default ServerList;
