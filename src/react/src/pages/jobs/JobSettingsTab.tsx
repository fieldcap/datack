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
    ListItem,
    Skeleton,
    Textarea,
    UnorderedList
} from '@chakra-ui/react';
import { format } from 'date-fns';
import _ from 'lodash';
import React, { FC, useCallback, useEffect, useRef, useState } from 'react';
import { useHistory } from 'react-router';
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
    const [group, setGroup] = useState<string>(props.job?.group ?? '');
    const [priority, setPriority] = useState<number>(props.job?.priority ?? 0);

    const [cron, setCron] = useState<string>(props.job?.cron ?? '');
    const [cronOccurrences, setCronOccurrences] = useState<Date[]>([]);

    const [testResult, setTestResult] = useState<TestCronResult | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [occurrencesMax, setOccurrencesMax] = useState<number>(20);

    const history = useHistory();

    const occurrenceList = useRef(null);

    const handleChangeCrons = useCallback(() => {
        (async () => {
            const result = await Jobs.testCron(cron);
            setTestResult(result);
        })();
    }, [cron]);

    useEffect(() => {
        handleChangeCrons();
    }, [props.job, handleChangeCrons]);

    useEffect(() => {
        let occurrences: Date[] = [];

        if (testResult == null) {
            return;
        }

        testResult.next.forEach((d) => {
            occurrences.push(d);
        });

        occurrences = _.orderBy(occurrences, (m) => m);

        occurrences = _.take(occurrences, occurrencesMax);

        setCronOccurrences(occurrences);
    }, [testResult, occurrencesMax]);

    useEffect(() => {
        (occurrenceList.current as any)?.scrollIntoView({ behavior: 'smooth' });
    }, [cronOccurrences]);

    const showMoreOccurrences = () => {
        setOccurrencesMax((value) => (value += 20));
    };

    const handleSave = async (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setIsSaving(true);
        setError(null);

        try {
            const newJob: Job = {
                jobId: props.job!.jobId,
                name,
                group,
                priority,
                description,
                cron,
                settings: {},
            };

            await Jobs.update(newJob);
            setIsSaving(false);
        } catch (err: any) {
            setError(err);
            setIsSaving(false);
        }
    };

    const duplicate = async () => {
        setIsSaving(true);
        setError(null);

        try {
            const newJob = await Jobs.duplicate(props.job!.jobId);
            history.push(`/job/${newJob.jobId}`);
        } catch (err: any) {
            setError(err);
            setIsSaving(false);
        }
    };

    const deleteJob = async () => {
        setIsSaving(true);
        setError(null);

        try {
            await Jobs.deleteJob(props.job!.jobId);
            history.push(`/jobs`);
        } catch (err: any) {
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
                <FormControl id="group" marginBottom={4}>
                    <FormLabel>Job Group</FormLabel>
                    <Input
                        type="text"
                        maxLength={100}
                        value={group}
                        onChange={(e) => setGroup(e.target.value)}
                    />
                </FormControl>
                <FormControl id="priority" marginBottom={4}>
                    <FormLabel>Priority</FormLabel>
                    <Input
                        type="number"
                        min="0"
                        max="9999"
                        value={priority}
                        onChange={(e) => setPriority(+e.target.value)}
                    />
                </FormControl>

                <FormControl id="cron" marginBottom={4} isRequired>
                    <FormLabel>Backup Schedule</FormLabel>
                    <Input
                        type="text"
                        maxLength={100}
                        value={cron}
                        onChange={(e) => setCron(e.target.value)}
                        onBlur={() => handleChangeCrons()}
                    />
                    <FormHelperText>{testResult?.result}</FormHelperText>
                </FormControl>

                <FormLabel>Occurrences for next 2 weeks</FormLabel>
                <UnorderedList overflowY="scroll" maxHeight="300px">
                    {cronOccurrences.map((m) => (
                        <ListItem key={m.toISOString()}>
                            {format(m, 'd MMMM yyyy HH:mm')}
                        </ListItem>
                    ))}
                    <div ref={occurrenceList}></div>
                </UnorderedList>

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
                    {occurrencesMax < cronOccurrences.length ? (
                        <Button onClick={() => showMoreOccurrences()}>
                            Show more occurrences
                        </Button>
                    ) : null}
                    <Button onClick={() => duplicate()} isLoading={isSaving}>
                        Duplicate
                    </Button>
                    <Button onClick={() => deleteJob()} isLoading={isSaving}>
                        Delete
                    </Button>
                </HStack>
            </form>
        </Skeleton>
    );
};

export default JobSettingsTab;
