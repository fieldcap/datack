import {
  Alert,
  AlertDescription,
  AlertIcon,
  Box,
  Button,
  FormControl,
  FormLabel,
  Heading,
  HStack,
  Input,
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import Loader from '../../components/loader';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Setting } from '../../models/setting';
import Settings from '../../services/settings';
import SettingsSetting from './SettingsSetting';

const SettingsOverview: FC = () => {
  const [isLoaded, setIsLoaded] = useState<boolean>(false);
  const [settings, setSettings] = useState<Setting[]>([]);
  const [testEmailTo, setTestEmailTo] = useState<string>('');

  const [error, setError] = useState<string | null>(null);
  const [saveError, setSaveError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState<boolean>(false);

  const cancelToken = useCancellationToken();

  useEffect(() => {
    (async () => {
      setError(null);
      setIsLoaded(false);
      try {
        const result = await Settings.getList(cancelToken);
        setSettings(result);
        setIsLoaded(true);
      } catch (err: any) {
        setError(err);
      }
    })();
  }, [cancelToken]);

  const handleChangeValue = (setting: Setting): void => {
    const newSettings = [...settings];
    newSettings.map((s) => {
      if (s.settingId === setting.settingId) {
        s.value = setting.value;
      }
      return s;
    });
    setSettings(newSettings);
  };

  const handleSave = async (event: React.FormEvent<HTMLButtonElement>) => {
    event.preventDefault();
    setIsSaving(true);
    setSaveError(null);

    try {
      await Settings.update(settings);
      setIsSaving(false);
    } catch (err: any) {
      setSaveError(err);
      setIsSaving(false);
    }
  };

  const handleTestEmail = async (event: React.FormEvent<HTMLButtonElement>) => {
    event.preventDefault();
    setIsSaving(true);
    setSaveError(null);

    try {
      await Settings.testEmail(testEmailTo);
      setIsSaving(false);
    } catch (err: any) {
      setSaveError(err);
      setIsSaving(false);
    }
  };

  return (
    <Loader isLoaded={isLoaded} error={error}>
      <Box marginBottom={4}>
        <Heading>Settings</Heading>
      </Box>
      <Box marginBottom={4}>
        <Heading size="md">Server Logging</Heading>
      </Box>
      <Box marginBottom={4}>
        Server version <span id="version">1.0.20</span>
      </Box>
      <Box marginBottom={4}>
        <SettingsSetting
          settingId="LogLevel"
          settings={settings}
          label="Log Level"
          onChangeValue={handleChangeValue}
          type="select"
          options="Verbose,Debug,Information,Warning,Error,Fatal"
        />
      </Box>
      <Box marginBottom={4}>
        <Heading size="md">E-Mail SMTP</Heading>
      </Box>
      <Box marginBottom={4}>
        <SettingsSetting
          settingId="Email:Smtp:Host"
          settings={settings}
          label="Host"
          onChangeValue={handleChangeValue}
        />
        <SettingsSetting
          settingId="Email:Smtp:Port"
          settings={settings}
          label="Port"
          onChangeValue={handleChangeValue}
        />
        <SettingsSetting
          settingId="Email:Smtp:UserName"
          settings={settings}
          label="Username"
          onChangeValue={handleChangeValue}
        />
        <SettingsSetting
          settingId="Email:Smtp:Password"
          settings={settings}
          label="Password"
          onChangeValue={handleChangeValue}
          type="password"
        />
        <SettingsSetting
          settingId="Email:Smtp:UseSsl"
          settings={settings}
          label="Use SSL"
          onChangeValue={handleChangeValue}
          type="checkbox"
        />
        <SettingsSetting
          settingId="Email:Smtp:From"
          settings={settings}
          label="From E-mail Address"
          onChangeValue={handleChangeValue}
        />
      </Box>
      <Box marginBottom={4}>
        <Heading size="md">Test E-mail</Heading>
      </Box>
      <FormControl id="TestEmailTo" marginBottom={4}>
        <FormLabel>E-Mail address</FormLabel>
        <Input type="email" value={testEmailTo} onChange={(evt) => setTestEmailTo(evt.target.value)}></Input>
      </FormControl>
      <Button onClick={handleTestEmail} isLoading={isSaving} marginBottom={4}>
        Test E-mail
      </Button>
      {saveError != null ? (
        <Alert status="error" marginBottom={4}>
          <AlertIcon />
          <AlertDescription>{saveError}</AlertDescription>
        </Alert>
      ) : null}
      <HStack>
        <Button onClick={handleSave} isLoading={isSaving}>
          Save
        </Button>
      </HStack>
    </Loader>
  );
};

export default SettingsOverview;
