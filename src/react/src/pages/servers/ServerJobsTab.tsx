import { format } from 'date-fns';
import React, { FC, useMemo } from 'react';
import { Column, useTable } from 'react-table';
import { Server } from '../../models/server';
import './ServerOverview.scss';

type Props = {
    server: Server;
};

const ServerJobsTab: FC<Props> = (props) => {
    interface Data {
        date: Date;
        objectCount: number;
        size: number;
    }

    const memoColumns: Column<Data>[] = useMemo(
        () => [
            {
                Header: 'Date',
                accessor: (d) => format(d.date, 'MM/dd/yyyy'),
            },
            {
                Header: 'Backup Objects',
                accessor: 'objectCount',
            },
            {
                Header: 'Archive Size',
                accessor: 'size',
            },
        ],
        []
    );

    const memoData: Data[] = useMemo(
        () => [
            {
                date: new Date(),
                objectCount: 100,
                size: 1024,
            },
        ],
        []
    );

    const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } =
        useTable<Data>({ columns: memoColumns, data: memoData });

    return (
        <table {...getTableProps()} className="table">
            <thead>
                {headerGroups.map((headerGroup) => (
                    <tr {...headerGroup.getHeaderGroupProps()}>
                        {headerGroup.headers.map((column) => (
                            <th {...column.getHeaderProps()}>
                                {column.render('Header')}
                            </th>
                        ))}
                    </tr>
                ))}
            </thead>
            <tbody {...getTableBodyProps()}>
                {rows.map((row) => {
                    prepareRow(row);
                    return (
                        <tr {...row.getRowProps()}>
                            {row.cells.map((cell) => {
                                return (
                                    <td {...cell.getCellProps()}>
                                        {cell.render('Cell')}
                                    </td>
                                );
                            })}
                        </tr>
                    );
                })}
            </tbody>
        </table>
    );
};

export default ServerJobsTab;
