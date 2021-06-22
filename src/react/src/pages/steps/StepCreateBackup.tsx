import { Checkbox, FormControl } from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { StepCreateBackupSettings, StepSettings } from '../../models/step';

type Props = {
    settings: StepSettings;
    onSettingsChanged: (settings: StepSettings) => void;
};

const StepCreateBackup: FC<Props> = (props) => {
    const [settings, setSettings] = useState<StepCreateBackupSettings | null>();

    useEffect(() => {
        if (props.settings.createBackup == null) {
            setSettings({ backupAllNonSystemDatabases: false });
        } else {
            setSettings({ ...props.settings.createBackup });
        }
    }, [props.settings]);

    const onChange = (settings: StepCreateBackupSettings) => {
        props.onSettingsChanged({ ... { createBackup: settings } });
    };

    const handleBackupAllNonSystemDatabases = (checked: boolean) => {
        setSettings((settings) => {
            if (settings == null) {
                return null;
            }
            settings.backupAllNonSystemDatabases = checked;
            onChange(settings);
            return settings;
        });
    };

    return (
        <>
            <FormControl
                id="backupAllNonSystemDatabases"
                marginBottom={4}
                isRequired
            >
                <Checkbox
                    isChecked={settings?.backupAllNonSystemDatabases}
                    onChange={(evt) =>
                        handleBackupAllNonSystemDatabases(evt.target.checked)
                    }
                >
                    Backup all non-system databases
                </Checkbox>
            </FormControl>
        </>
    );
};

export default StepCreateBackup;
