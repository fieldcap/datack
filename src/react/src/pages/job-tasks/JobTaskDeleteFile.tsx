import { Checkbox, FormControl } from '@chakra-ui/react';
import React, { FC, useEffect } from 'react';
import { JobTaskDeleteSettings } from '../../models/job-task';

type Props = {
    agentId: string;
    settings: JobTaskDeleteSettings | undefined | null;
    onSettingsChanged: (settings: JobTaskDeleteSettings) => void;
};

const JobTaskDeleteFile: FC<Props> = (props) => {
    const { onSettingsChanged } = props;

    useEffect(() => {
        if (props.settings == null) {
            onSettingsChanged({
                ignoreIfFileDoesNotExist: true,
            });
        }
    }, [props.settings, onSettingsChanged]);

    const handleIgnoreChanged = (value: boolean) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            ignoreIfFileDoesNotExist: value,
        });
    };
    return (
        <>
            <FormControl id="ignore" marginBottom={4} isRequired>
                <Checkbox
                    isChecked={props.settings?.ignoreIfFileDoesNotExist}
                    onChange={(evt) => handleIgnoreChanged(evt.target.checked)}
                >
                    Ignore deletion of files that are not found.
                </Checkbox>
            </FormControl>
        </>
    );
};

export default JobTaskDeleteFile;
