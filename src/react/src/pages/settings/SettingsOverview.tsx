import {
    Alert,
    AlertDescription,
    AlertIcon,
    Box,
    Button,
    Heading,
    HStack,
    Skeleton
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router-dom';
import useCancellationToken from '../../hooks/useCancellationToken';
import { Setting } from '../../models/setting';
import Settings from '../../services/settings';
import SettingsSetting from './SettingsSetting';

type RouteParams = {};

const SettingsOverview: FC<RouteComponentProps<RouteParams>> = (props) => {
    let [isLoaded, setIsLoaded] = useState<boolean>(false);
    let [settings, setSettings] = useState<Setting[]>([]);

    const [error, setError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState<boolean>(false);

    const cancelToken = useCancellationToken();

    useEffect(() => {
        (async () => {
            const result = await Settings.getList(cancelToken);
            setSettings(result);
            setIsLoaded(true);
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
        setError(null);

        try {
            await Settings.update(settings);
            setIsSaving(false);
        } catch (err: any) {
            setError(err);
            setIsSaving(false);
        }
    };

    const handleTestEmail = async (
        event: React.FormEvent<HTMLButtonElement>
    ) => {
        event.preventDefault();
        setIsSaving(true);
        setError(null);

        try {
            await Settings.testEmail('roger@versluis.ca');
            setIsSaving(false);
        } catch (err: any) {
            setError(err);
            setIsSaving(false);
        }
    };

    return (
        <Skeleton isLoaded={isLoaded}>
            <Box marginBottom="24px">
                <Heading>Settings</Heading>
            </Box>
            <Box marginBottom="24px">
                <Heading size="md">E-Mail SMTP</Heading>
            </Box>
            <Box>
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
            <Box marginBottom="24px" marginTop="36px">
                <Heading size="md">Cloud authentication</Heading>
            </Box>
            <Box>
                <SettingsSetting
                    settingId="AWS:S3:Secret"
                    settings={settings}
                    label="AWS S3 Secret"
                    onChangeValue={handleChangeValue}
                    type="password"
                />
                <SettingsSetting
                    settingId="Azure:Blob:ConnectionString"
                    settings={settings}
                    label="Azure Blobl Storage Connection String"
                    onChangeValue={handleChangeValue}
                    type="password"
                />
            </Box>
            {error != null ? (
                <Alert status="error">
                    <AlertIcon />
                    <AlertDescription>{error}</AlertDescription>
                </Alert>
            ) : null}
            <HStack marginTop="24px">
                <Button onClick={handleSave} isLoading={isSaving}>
                    Save
                </Button>
                <Button onClick={handleTestEmail} isLoading={isSaving}>
                    Test E-mail
                </Button>
            </HStack>
        </Skeleton>
    );
};

export default SettingsOverview;
