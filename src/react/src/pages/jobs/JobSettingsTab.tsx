import {
    Alert,
    AlertDescription,
    AlertIcon,
    Button,
    FormControl,
    FormHelperText,
    FormLabel,
    HStack,
    Input,
    Skeleton,
    Textarea
} from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { Job } from '../../models/job';
import Jobs, { TestCronResult } from '../../services/jobs';

type Props = {
    job: Job | null;
};

const JobSettingsTab: FC<Props> = (props) => {
    const [name, setName] = useState<string>(props.job?.name ?? '');

    const [description, setDescription] = useState<string>(
        props.job?.description ?? ''
    );

    const [cronFull, setCronFull] = useState<string>(
        props.job?.settings.cronFull ?? ''
    );
    const [cronDiff, setCronDiff] = useState<string>(
        props.job?.settings.cronDiff ?? ''
    );
    const [cronLog, setCronLog] = useState<string>(
        props.job?.settings.cronLog ?? ''
    );

    const [testResult, setTestResult] = useState<TestCronResult | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState<boolean>(false);

    const handleChangeCrons = async () => {
        const result = await Jobs.testCrons(cronFull, cronDiff, cronLog);
        setTestResult(result);
    };

    const handleSave = async (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setIsSaving(true);
        setError(null);

        try {
            const newJob: Job = {
                jobId: props.job!.jobId,
                name,
                description,
                settings: {
                    cronFull,
                    cronDiff,
                    cronLog,
                },
            };

            await Jobs.update(newJob);
            setIsSaving(false);
        } catch (err) {
            setError(err);
            setIsSaving(false);
        }
    };

    return (
        <Skeleton isLoaded={props.job != null}>
            <form>
                <FormControl id="name" marginBottom={4} isRequired>
                    <FormLabel>Job Name</FormLabel>
                    <Input
                        type="text"
                        maxLength={100}
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                    />
                </FormControl>
                <FormControl id="description" marginBottom={4}>
                    <FormLabel>Description</FormLabel>
                    <Textarea
                        lines={4}
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                    />
                </FormControl>
                <FormControl id="cronFull" marginBottom={4} isRequired>
                    <FormLabel>Full Backup Schedule</FormLabel>
                    <Input
                        type="text"
                        maxLength={100}
                        value={cronFull}
                        onChange={(e) => setCronFull(e.target.value)}
                        onBlur={() => handleChangeCrons()}
                    />
                    <FormHelperText>
                        {testResult?.resultFull?.description}
                    </FormHelperText>
                </FormControl>

                <FormControl id="cronDiff" marginBottom={4} isRequired>
                    <FormLabel>Diff Backup Schedule</FormLabel>
                    <Input
                        type="text"
                        maxLength={100}
                        value={cronDiff}
                        onChange={(e) => setCronDiff(e.target.value)}
                    />
                    <FormHelperText>
                        {testResult?.resultDiff?.description}
                    </FormHelperText>
                </FormControl>
                <FormControl id="cronLog" marginBottom={4} isRequired>
                    <FormLabel>Transaction Log Backup Schedule</FormLabel>
                    <Input
                        type="text"
                        maxLength={100}
                        value={cronLog}
                        onChange={(e) => setCronLog(e.target.value)}
                    />
                    <FormHelperText>
                        {testResult?.resultLog?.description}
                    </FormHelperText>
                </FormControl>

                {error != null ? (
                    <Alert marginTop="24px" status="error">
                        <AlertIcon />
                        <AlertDescription>{error}</AlertDescription>
                    </Alert>
                ) : null}
                <HStack marginTop="24px">
                    <Button
                        onClick={(evt) => handleSave(evt)}
                        isLoading={isSaving}
                    >
                        Save
                    </Button>
                </HStack>
            </form>
        </Skeleton>
    );
};

export default JobSettingsTab;
