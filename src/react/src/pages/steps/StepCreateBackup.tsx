import {
    Checkbox,
    CheckboxGroup,
    FormControl,
    FormLabel,
    Input,
    Table,
    Tbody,
    Td,
    Thead,
    Tr,
    VStack
} from '@chakra-ui/react';
import axios from 'axios';
import React, { FC, useCallback, useEffect, useState } from 'react';
import { FaMinus, FaPlus } from 'react-icons/fa';
import { StepCreateBackupSettings } from '../../models/step';
import Servers from '../../services/servers';
import Steps, { TestDatabaseRegexResponse } from '../../services/steps';

type Props = {
    serverId: string;
    settings: StepCreateBackupSettings | undefined | null;
    onSettingsChanged: (settings: StepCreateBackupSettings) => void;
};

const StepCreateBackup: FC<Props> = (props) => {
    const { onSettingsChanged } = props;

    const [databases, setDatabases] = useState<string[]>([]);

    const [testResult, setTestResult] =
        useState<TestDatabaseRegexResponse | null>(null);

    const handleChangeRegex = useCallback(async () => {
        if (props.settings == null) {
            return;
        }

        const result = await Steps.testDatabaseRegex(props.settings, databases);
        setTestResult(result);
    }, [props.settings, databases]);

    useEffect(() => {
        if (props.settings == null) {
            onSettingsChanged({
                backupDefaultExclude: false,
                backupExcludeSystemDatabases: true,
                backupExcludeRegex: '',
                backupIncludeRegex: '',
                backupIncludeManual: '',
                backupExcludeManual: '',
            });
        } else {
            handleChangeRegex();
        }
    }, [props.settings, onSettingsChanged]);

    useEffect(() => {
        if (!props.serverId) {
            return;
        }

        const getByIdCancelToken = axios.CancelToken.source();
        (async () => {
            var databases = await Servers.getDatabaseList(props.serverId);
            setDatabases(databases);
        })();

        return () => {
            getByIdCancelToken.cancel();
        };
    }, [props.serverId]);

    useEffect(() => {
        if (props.settings?.backupExcludeSystemDatabases != null) {
            (async () => {
                await handleChangeRegex();
            })();
        }
        // eslint-disable-next-line
    }, [
        props.settings?.backupDefaultExclude,
        props.settings?.backupExcludeSystemDatabases,
        props.settings?.backupExcludeManual,
        props.settings?.backupIncludeManual,
        databases,
    ]);

    const handleBackupDefaultExclude = async (checked: boolean) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            backupDefaultExclude: checked,
        });
    };

    const handleBackupExcludeSystemDatabases = async (checked: boolean) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            backupExcludeSystemDatabases: checked,
        });
    };

    const handleBackupIncludeRegex = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            backupIncludeRegex: value,
        });
    };

    const handleBackupExcludeRegex = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            backupExcludeRegex: value,
        });
    };

    const getCheckBoxIncludeValue = (name: string): boolean => {
        if (props.settings == null) {
            return false;
        }

        if (props.settings.backupIncludeManual == null) {
            props.settings.backupIncludeManual = '';
        }

        let includedDatabases = props.settings.backupIncludeManual.split(',');

        return includedDatabases.indexOf(name) > -1;
    };

    const onCheckBoxIncludeChange = (name: string, checked: boolean) => {
        if (props.settings == null) {
            return;
        }

        if (props.settings.backupIncludeManual == null) {
            props.settings.backupIncludeManual = '';
        }

        let includedDatabases = props.settings.backupIncludeManual.split(',');
        includedDatabases = includedDatabases.filter(
            (m) => m !== name && m !== ''
        );
        if (checked) {
            includedDatabases.push(name);
        }

        props.onSettingsChanged({
            ...props.settings,
            backupIncludeManual: includedDatabases.join(','),
        });
    };

    const getCheckBoxExcludeValue = (name: string): boolean => {
        if (props.settings == null) {
            return false;
        }

        if (props.settings.backupExcludeManual == null) {
            props.settings.backupExcludeManual = '';
        }

        let excludedDatabases = props.settings.backupExcludeManual.split(',');

        return excludedDatabases.indexOf(name) > -1;
    };

    const onCheckBoxExcludeChange = (name: string, checked: boolean) => {
        if (props.settings == null) {
            return;
        }

        if (props.settings.backupExcludeManual == null) {
            props.settings.backupExcludeManual = '';
        }

        let excludedDatabases = props.settings.backupExcludeManual.split(',');
        excludedDatabases = excludedDatabases.filter(
            (m) => m !== name && m !== ''
        );
        if (checked) {
            excludedDatabases.push(name);
        }

        props.onSettingsChanged({
            ...props.settings,
            backupExcludeManual: excludedDatabases.join(','),
        });
    };

    const getDatabaseName = (name: string) => {
        if (testResult == null) {
            return null;
        }
        if (testResult.excludeManualList.indexOf(name) > -1) {
            return (
                <span style={{ textDecoration: 'line-through' }}>{name}</span>
            );
        }
        if (testResult.includeManualList.indexOf(name) > -1) {
            return <span>{name}</span>;
        }
        if (testResult.systemList.indexOf(name) > -1) {
            return (
                <span style={{ textDecoration: 'line-through' }}>{name}</span>
            );
        }
        if (testResult.excludeRegexList.indexOf(name) > -1) {
            return (
                <span style={{ textDecoration: 'line-through' }}>{name}</span>
            );
        }
        if (testResult.includeRegexList.indexOf(name) > -1) {
            return <span>{name}</span>;
        }

        if (props.settings?.backupDefaultExclude) {
            return (
                <span style={{ textDecoration: 'line-through' }}>{name}</span>
            );
        }

        return <span>{name}</span>;
    };

    const getDatabaseStatus = (name: string) => {
        if (testResult == null) {
            return null;
        }
        if (testResult.excludeManualList.indexOf(name) > -1) {
            return 'Excluded because database is manually excluded';
        }
        if (testResult.includeManualList.indexOf(name) > -1) {
            return 'Included because database is manually included';
        }
        if (testResult.systemList.indexOf(name) > -1) {
            return 'Excluded because database is a system database';
        }
        if (testResult.excludeRegexList.indexOf(name) > -1) {
            return 'Excluded because database matches "Exclude Regex"';
        }
        if (testResult.includeRegexList.indexOf(name) > -1) {
            return 'Included because database matches "Include Regex"';
        }

        if (props.settings?.backupDefaultExclude) {
            return 'Excluded because database does not match any rules"';
        }
        return 'Included because database does not match any rules';
    };

    return (
        <>
            <FormControl id="backupDefaultExclude" marginBottom={4} isRequired>
                <Checkbox
                    isChecked={props.settings?.backupDefaultExclude}
                    onChange={(evt) =>
                        handleBackupDefaultExclude(evt.target.checked)
                    }
                >
                    By default exclude all non matched databases
                </Checkbox>
            </FormControl>
            <FormControl
                id="backupAllNonSystemDatabases"
                marginBottom={4}
                isRequired
            >
                <Checkbox
                    isChecked={props.settings?.backupExcludeSystemDatabases}
                    onChange={(evt) =>
                        handleBackupExcludeSystemDatabases(evt.target.checked)
                    }
                >
                    Exclude all system databases
                </Checkbox>
            </FormControl>
            <FormControl id="backupIncludeRegex" marginBottom={4}>
                <FormLabel>Include Regex</FormLabel>
                <Input
                    type="text"
                    value={props.settings?.backupIncludeRegex || ''}
                    onChange={(evt) =>
                        handleBackupIncludeRegex(evt.target.value)
                    }
                    onBlur={() => handleChangeRegex()}
                ></Input>
            </FormControl>
            <FormControl id="backupExcludeRegex" marginBottom={4}>
                <FormLabel>Exclude Regex</FormLabel>
                <Input
                    type="text"
                    value={props.settings?.backupExcludeRegex || ''}
                    onChange={(evt) =>
                        handleBackupExcludeRegex(evt.target.value)
                    }
                    onBlur={() => handleChangeRegex()}
                ></Input>
            </FormControl>
            <Table width="50%">
                <Thead>
                    <Tr>
                        <Td>Database</Td>
                        <Td>Include</Td>
                        <Td>Exclude</Td>
                        <Td></Td>
                    </Tr>
                </Thead>
                <Tbody>
                    {databases.map((m) => (
                        <Tr key={m}>
                            <Td>{getDatabaseName(m)}</Td>
                            <Td>
                                <Checkbox
                                    isChecked={getCheckBoxIncludeValue(m)}
                                    onChange={(e) =>
                                        onCheckBoxIncludeChange(
                                            m,
                                            e.currentTarget.checked
                                        )
                                    }
                                >
                                    <FaPlus></FaPlus>
                                </Checkbox>
                            </Td>
                            <Td>
                                <Checkbox
                                    isChecked={getCheckBoxExcludeValue(m)}
                                    onChange={(e) =>
                                        onCheckBoxExcludeChange(
                                            m,
                                            e.currentTarget.checked
                                        )
                                    }
                                >
                                    <FaMinus></FaMinus>
                                </Checkbox>
                            </Td>
                            <Td>{getDatabaseStatus(m)}</Td>
                        </Tr>
                    ))}
                </Tbody>
            </Table>
            <CheckboxGroup>
                <VStack align="left"></VStack>
            </CheckboxGroup>
        </>
    );
};

export default StepCreateBackup;
