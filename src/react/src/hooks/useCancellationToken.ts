import axios, { CancelTokenSource } from 'axios';
import { useEffect, useState } from 'react';

export const useCancellationToken = (): CancelTokenSource => {
    const [cancellationToken] = useState<CancelTokenSource>(
        axios.CancelToken.source()
    );

    useEffect(() => {
        return () => {
            cancellationToken.cancel();
        };
    }, [cancellationToken]);

    return cancellationToken;
};

export default useCancellationToken;
