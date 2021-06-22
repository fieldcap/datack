import { TriangleDownIcon, TriangleUpIcon } from '@chakra-ui/icons';
import {
    Button,
    chakra,
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
import { useHistory } from 'react-router-dom';
import { Column, useExpanded, useSortBy, useTable } from 'react-table';
import { v4 } from 'uuid';
import { Job } from '../../models/job';
import { Step } from '../../models/step';
import Steps from '../../services/steps';

type JobStepsTabProps = {
    job: Job | null;
};

const JobStepsTab: FC<JobStepsTabProps> = (props) => {
    let [steps, setSteps] = useState<Step[]>([]);
    let [isLoaded, setIsLoaded] = useState<boolean>(false);

    const history = useHistory();

    useEffect(() => {
        if (props.job == null) {
            return;
        }

        const getByIdCancelToken = axios.CancelToken.source();

        (async () => {
            const result = await Steps.getForJob(
                props.job!.jobId,
                getByIdCancelToken
            );
            setSteps(result);
            setIsLoaded(true);
        })();

        return () => {
            getByIdCancelToken.cancel();
        };
    }, [props.job]);

    const handleAddNewStepClick = () => {
        if (props.job == null) {
            return;
        }

        const newStep: Step = {
            stepId: v4(),
            jobId: props.job.jobId,
            name: 'New Step',
            description: '',
            type: '',
            job: props.job,
            order: 0,
            settings: {},

            forceExpandRow: true,
        };

        setSteps([...steps, newStep]);
    };

    const rowClick = (jobId: string): void => {
        history.push(`/step/${jobId}`);
    };

    const columns = React.useMemo(() => {
        const columns: Column<Step>[] = [
            {
                Header: 'Name',
                accessor: 'name',
            },
        ];
        return columns;
    }, []);

    const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } =
        useTable<Step>({ columns, data: steps }, useSortBy, useExpanded);

    return (
        <>
            <Skeleton isLoaded={isLoaded}>
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
                                    onClick={() =>
                                        rowClick(row.original.stepId)
                                    }
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
                <Button
                    marginTop="24px"
                    onClick={() => handleAddNewStepClick()}
                >
                    Add new step
                </Button>
            </Skeleton>
        </>
    );
};

export default JobStepsTab;
