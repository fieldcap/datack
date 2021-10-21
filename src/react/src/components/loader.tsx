import { Skeleton } from '@chakra-ui/react';
import React, { FC, PropsWithChildren } from 'react';

type LoaderProps = {
    isLoaded: boolean;
};

const Loader: FC<PropsWithChildren<LoaderProps>> = (props) => {
    if (props.isLoaded) {
        return <>{props.children}</>;
    }
    return <Skeleton isLoaded={false}></Skeleton>;
};

export default Loader;
