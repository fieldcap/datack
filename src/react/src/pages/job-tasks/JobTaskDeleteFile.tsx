import { Checkbox, FormControl, FormHelperText } from '@chakra-ui/react';
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

    const set = (settingName: keyof JobTaskDeleteSettings, newValue: boolean): void => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            [settingName]: newValue,
        });
    };

    return (
        <>
            <FormControl id="ignore" marginBottom={4} isRequired>
                <Checkbox
                    isChecked={props.settings?.ignoreIfFileDoesNotExist}
                    onChange={(evt) => set('ignoreIfFileDoesNotExist', evt.target.checked)}
                >
                    Ignore deletion of files that are not found.
                </Checkbox>
                <FormHelperText>When checked and the file is not found continue as a succesful task.</FormHelperText>
            </FormControl>
        </>
    );
};

export default JobTaskDeleteFile;
