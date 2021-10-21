import {
    Checkbox,
    CheckboxGroup,
    FormControl,
    FormLabel,
    Input,
    Select,
    Table,
    Tbody,
    Td,
    Thead,
    Tr,
    VStack
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { FaMinus, FaPlus } from 'react-icons/fa';
import { DatabaseListTestResult } from '../../models/database-list-test-result';
import { JobTaskCreateDatabaseSettings } from '../../models/job-task';
import JobTasks from '../../services/jobTasks';

type Props = {
    agentId: string;
    settings: JobTaskCreateDatabaseSettings | undefined | null;
    onSettingsChanged: (settings: JobTaskCreateDatabaseSettings) => void;
};

const JobTaskCreateBackup: FC<Props> = (props) => {
    const { onSettingsChanged } = props;

    const [testResult, setTestResult] = useState<DatabaseListTestResult[]>([]);

    useEffect(() => {
        if (props.settings == null) {
            onSettingsChanged({
                fileName: '',
                backupType: 'Full',
                backupDefaultExclude: false,
                backupExcludeSystemDatabases: true,
                backupExcludeRegex: '',
                backupIncludeRegex: '',
                backupIncludeManual: '',
                backupExcludeManual: '',
            });
        }
    }, [props.settings, onSettingsChanged]);

    useEffect(() => {
        if (props.agentId == null || props.agentId === '') {
            return;
        }

        (async () => {
            const result = await JobTasks.testDatabaseRegex(
                props.settings!.backupDefaultExclude,
                props.settings!.backupIncludeRegex,
                props.settings!.backupExcludeRegex,
                props.settings!.backupExcludeSystemDatabases,
                props.settings!.backupIncludeManual,
                props.settings!.backupExcludeManual,
                props.agentId
            );
            setTestResult(result);
        })();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [
        props.settings?.backupDefaultExclude,
        props.settings?.backupExcludeSystemDatabases,
        props.settings?.backupExcludeRegex,
        props.settings?.backupIncludeRegex,
        props.settings?.backupIncludeManual,
        props.settings?.backupExcludeManual,
        props.agentId,
    ]);

    const handleBackupTypeChanged = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            backupType: value,
        });
    };

    const handleFilenameChanged = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            fileName: value,
        });
    };

    const handleBackupDefaultExclude = (checked: boolean) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            backupDefaultExclude: checked,
        });
    };

    const handleBackupExcludeSystemDatabases = (checked: boolean) => {
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

    const getDatabaseTestResult = (database: DatabaseListTestResult) => {
        if (
            database.hasNoAccess ||
            database.isManualExcluded ||
            database.isRegexExcluded ||
            database.isSystemDatabase ||
            database.isBackupDefaultExcluded
        ) {
            return (
                <span style={{ textDecoration: 'line-through' }}>
                    {database.databaseName}
                </span>
            );
        }

        return <span>{database.databaseName}</span>;
    };

    const getDatabaseTestResult2 = (database: DatabaseListTestResult) => {
        if (database.hasNoAccess) {
            return 'Excluded because user has no access to database';
        }
        if (database.isManualExcluded) {
            return 'Excluded because database is manually excluded';
        }
        if (database.isManualIncluded) {
            return 'Included because database is manually included';
        }
        if (database.isSystemDatabase) {
            return 'Excluded because database is a system database';
        }
        if (database.isRegexExcluded) {
            return 'Excluded because database matches "Exclude Regex"';
        }
        if (database.isRegexIncluded) {
            return 'Included because database matches "Include Regex"';
        }
        if (database.isBackupDefaultExcluded) {
            return 'Excluded because database does not match any rules"';
        }
        return 'Included because database does not match any rules';
    };

    return (
        <>
            <FormControl id="backupType" marginBottom={4}>
                <FormLabel>Backup Type</FormLabel>
                <Select
                    value={props.settings?.backupType || 'Full'}
                    onChange={(e) => handleBackupTypeChanged(e.target.value)}
                >
                    <option value="Full">Full</option>
                    <option value="Transaction">Transaction</option>
                    <option value="Log">Log</option>
                </Select>
            </FormControl>
            <FormControl id="fileName" marginBottom={4}>
                <FormLabel>File name</FormLabel>
                <Input
                    type="text"
                    value={props.settings?.fileName || ''}
                    onChange={(evt) => handleFilenameChanged(evt.target.value)}
                ></Input>
            </FormControl>
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
                ></Input>
            </FormControl>
            <Table width="100%">
                <Thead>
                    <Tr>
                        <Td>Database</Td>
                        <Td>Include</Td>
                        <Td>Exclude</Td>
                        <Td></Td>
                    </Tr>
                </Thead>
                <Tbody>
                    {testResult.map((m) => (
                        <Tr key={m.databaseName}>
                            <Td>{getDatabaseTestResult(m)}</Td>
                            <Td>
                                <Checkbox
                                    isChecked={getCheckBoxIncludeValue(
                                        m.databaseName
                                    )}
                                    onChange={(e) =>
                                        onCheckBoxIncludeChange(
                                            m.databaseName,
                                            e.currentTarget.checked
                                        )
                                    }
                                >
                                    <FaPlus></FaPlus>
                                </Checkbox>
                            </Td>
                            <Td>
                                <Checkbox
                                    isChecked={getCheckBoxExcludeValue(
                                        m.databaseName
                                    )}
                                    onChange={(e) =>
                                        onCheckBoxExcludeChange(
                                            m.databaseName,
                                            e.currentTarget.checked
                                        )
                                    }
                                >
                                    <FaMinus></FaMinus>
                                </Checkbox>
                            </Td>
                            <Td>{getDatabaseTestResult2(m)}</Td>
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

export default JobTaskCreateBackup;
