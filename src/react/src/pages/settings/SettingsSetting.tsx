import { Checkbox, FormControl, FormLabel, Input, Select } from '@chakra-ui/react';
import React, { FC } from 'react';
import { Setting } from '../../models/setting';

type SettingProps = {
  settingId: string;
  label: string;
  settings: Setting[];
  onChangeValue: (setting: Setting) => void;
  type?: 'checkbox' | 'text' | 'password' | 'select';
  options?: string | null;
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
        <Checkbox isChecked={getValue() === 'True'} onChange={(evt) => set(evt.target.checked ? 'True' : 'False')}>
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

  if (props.type === 'select') {
    return (
      <FormControl id={props.settingId} marginBottom={4}>
        <FormLabel>{props.label}</FormLabel>
        <Select value={getValue()} onChange={(evt) => set(evt.target.value)}>
          {props.options?.split(',').map((v) => (
            <option value={v} key={v}>
              {v}
            </option>
          ))}
        </Select>
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
