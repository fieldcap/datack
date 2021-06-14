import { Button, Heading, Skeleton } from '@chakra-ui/react';
import axios from 'axios';
import React, { FC, useEffect, useState } from 'react';
import DataTable from 'react-data-table-component';
import { RouteComponentProps, useHistory } from 'react-router-dom';
import { Server } from '../../models/server';
import Servers from '../../services/servers';

const ServerList: FC<RouteComponentProps> = (props) => {
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

    const columns = [
        {
            name: 'Server Name',
            selector: 'name',
            sortable: true,
        },
    ];

    return (
        <Skeleton isLoaded={isLoaded}>
            <Heading marginBottom="24px">Servers</Heading>
            <DataTable
                keyField="serverId"
                columns={columns}
                data={servers}
                onRowClicked={(row) => rowClick(row.serverId)}
                pointerOnHover={true}
                highlightOnHover={true}
                noHeader={true}
            />
            <Button marginTop="24px">Add new server</Button>
        </Skeleton>
    );
};

export default ServerList;
