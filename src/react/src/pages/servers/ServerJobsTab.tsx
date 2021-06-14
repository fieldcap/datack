import axios from 'axios';
import React, { FC, useEffect, useState } from 'react';
import DataTable from 'react-data-table-component';
import { useHistory } from 'react-router-dom';
import { Job } from '../../models/job';
import { Server } from '../../models/server';
import Jobs from '../../services/jobs';

type Props = {
    server: Server;
};

const ServerJobsTab: FC<Props> = (props) => {
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

    const rowClick = (serverId: string): void => {
        history.push(`/server/${serverId}`);
    };

    const columns = [
        {
            name: 'Job Name',
            selector: 'name',
            sortable: true,
        },
    ];

    return (
        <DataTable
            keyField="jobId"
            columns={columns}
            data={jobs}
            onRowClicked={(row) => rowClick(row.jobId)}
            pointerOnHover={true}
            highlightOnHover={true}
            noHeader={true}
        />
    );
};

export default ServerJobsTab;
