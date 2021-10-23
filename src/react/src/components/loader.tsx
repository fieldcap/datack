import { Alert, AlertDescription, AlertIcon, Skeleton } from '@chakra-ui/react';
import React, { FC, PropsWithChildren } from 'react';

type LoaderProps = {
    isLoaded: boolean;
    error: string | null;
};

const Loader: FC<PropsWithChildren<LoaderProps>> = (props) => {
    if (props.error) {
        return (
            <Alert marginBottom={4} status="error">
                <AlertIcon />
                <AlertDescription>{props.error}</AlertDescription>
            </Alert>
        );
    }
    if (props.isLoaded) {
        return <>{props.children}</>;
    }
    return (
        <Skeleton isLoaded={false}>
            <div style={{ height: '250px' }}>&nbsp;</div>
        </Skeleton>
    );
};

export default Loader;
