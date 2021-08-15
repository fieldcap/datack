import {
    Alert,
    AlertDescription,
    AlertIcon,
    Box,
    Button,
    FormControl,
    FormHelperText,
    FormLabel,
    HStack,
    Input,
    ListItem,
    Skeleton,
    Textarea,
    UnorderedList
} from '@chakra-ui/react';
import { format, parseISO } from 'date-fns';
import _ from 'lodash';
import React, { FC, useEffect, useRef, useState } from 'react';
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
    const [cronOccurences, setCronOccurences] = useState<
        { date: Date; type: string }[]
    >([]);

    const [testResult, setTestResult] = useState<TestCronResult | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [occurencesMax, setOccurencesMax] = useState<number>(20);

    const occurenceList = useRef(null);

    useEffect(() => {
        handleChangeCrons();
    }, [props.job]);

    useEffect(() => {
        let occurences: { date: Date; type: string }[] = [];

        if (testResult == null) {
            return;
        }

        testResult.next.forEach((d) => {
            occurences.push({ date: parseISO(d.dateTime), type: d.backupType });
        });

        occurences = _.orderBy(occurences, (m) => m.date);

        occurences = _.take(occurences, occurencesMax);

        setCronOccurences(occurences);
    }, [testResult, occurencesMax]);

    useEffect(() => {
        (occurenceList.current as any)?.scrollIntoView({ behavior: 'smooth' });
    }, [cronOccurences]);

    const handleChangeCrons = async () => {
        const result = await Jobs.testCrons(cronFull, cronDiff, cronLog);
        setTestResult(result);
    };

    const showMoreOccurences = () => {
        setOccurencesMax((value) => (value += 20));
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

                <HStack spacing="12px" align="stretch">
                    <Box flex="1">
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
                                {testResult?.resultFull}
                            </FormHelperText>
                        </FormControl>
                        <FormControl id="cronDiff" marginBottom={4} isRequired>
                            <FormLabel>Diff Backup Schedule</FormLabel>
                            <Input
                                type="text"
                                maxLength={100}
                                value={cronDiff}
                                onChange={(e) => setCronDiff(e.target.value)}
                                onBlur={() => handleChangeCrons()}
                            />
                            <FormHelperText>
                                {testResult?.resultDiff}
                            </FormHelperText>
                        </FormControl>
                        <FormControl id="cronLog" marginBottom={4} isRequired>
                            <FormLabel>
                                Transaction Log Backup Schedule
                            </FormLabel>
                            <Input
                                type="text"
                                maxLength={100}
                                value={cronLog}
                                onChange={(e) => setCronLog(e.target.value)}
                                onBlur={() => handleChangeCrons()}
                            />
                            <FormHelperText>
                                {testResult?.resultLog}
                            </FormHelperText>
                        </FormControl>
                    </Box>
                    <Box flex="1">
                        <FormLabel>Occurences for next 2 weeks</FormLabel>
                        <UnorderedList overflowY="scroll" maxHeight="262px">
                            {cronOccurences.map((m) => (
                                <ListItem key={`${m.date}${m.type}`}>
                                    {format(m.date, 'd MMMM yyyy HH:mm xxx')} (
                                    {m.type})
                                </ListItem>
                            ))}
                            <div ref={occurenceList}></div>
                        </UnorderedList>
                        <Button onClick={() => showMoreOccurences()}>
                            More
                        </Button>
                    </Box>
                </HStack>

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
