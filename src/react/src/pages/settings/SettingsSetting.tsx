import { Checkbox, FormControl, FormLabel, Input } from '@chakra-ui/react';
import React, { FC } from 'react';
import { Setting } from '../../models/setting';

type SettingProps = {
    settingId: string;
    label: string;
    settings: Setting[];
    onChangeValue: (setting: Setting) => void;
    type?: 'checkbox' | 'text' | 'password';
};

const SettingsSetting: FC<SettingProps> = (props) => {
    const getValue = (): string => {
        const setting = props.settings.find((m) => m.settingId === props.settingId);
        if (setting == null) {
            return '';
        }
        if (setting.value === null && setting.secure) {
            return '******';
        }
        return setting.value;
    };

    const set = (value: string): void => {
        if (value === '******') {
            return;
        }
        const setting = props.settings.find((m) => m.settingId === props.settingId);
        if (setting == null) {
            return;
        }

        setting.value = value;

        props.onChangeValue(setting);
    };

    if (props.type === 'checkbox') {
        return (
            <FormControl id={props.settingId} marginBottom={4} isRequired>
                <Checkbox
                    isChecked={getValue() === 'True'}
                    onChange={(evt) => set(evt.target.checked ? 'True' : 'False')}
                >
                    {props.label}
                </Checkbox>
            </FormControl>
        );
    }

    if (props.type === 'password') {
        return (
            <FormControl id={props.settingId} marginBottom={4}>
                <FormLabel>{props.label}</FormLabel>
                <Input type="password" value={getValue()} onChange={(evt) => set(evt.target.value)}></Input>
            </FormControl>
        );
    }

    return (
        <FormControl id={props.settingId} marginBottom={4}>
            <FormLabel>{props.label}</FormLabel>
            <Input type="text" value={getValue()} onChange={(evt) => set(evt.target.value)}></Input>
        </FormControl>
    );
};

export default SettingsSetting;
