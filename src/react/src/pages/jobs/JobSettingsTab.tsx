import {
    Alert,
    AlertDescription,
    AlertIcon,
    Button,
    Checkbox,
    FormControl,
    FormHelperText,
    FormLabel,
    HStack,
    Input,
    ListItem,
    Modal,
    ModalBody,
    ModalCloseButton,
    ModalContent,
    ModalFooter,
    ModalHeader,
    ModalOverlay,
    Radio,
    RadioGroup,
    Stack,
    Textarea,
    UnorderedList
} from '@chakra-ui/react';
import { format } from 'date-fns';
import _ from 'lodash';
import React, { FC, useCallback, useEffect, useState } from 'react';
import { useHistory } from 'react-router';
import Loader from '../../components/loader';
import { Job, JobSettings } from '../../models/job';
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
    const [deleteLogsTimeSpanAmount, setDeleteLogsTimeSpanAmount] = useState<
        number | null
    >(props.job?.deleteLogsTimeSpanAmount ?? null);
    const [deleteLogsTimeSpanType, setDeleteLogsTimeSpanType] = useState<
        string | null
    >(props.job?.deleteLogsTimeSpanType ?? null);
    const [cron, setCron] = useState<string>(props.job?.cron ?? '');
    const [cronOccurrences, setCronOccurrences] = useState<Date[]>([]);
    const [settings, setSettings] = useState<JobSettings | null>(null);

    const [testResult, setTestResult] = useState<TestCronResult | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [occurrencesMax, setOccurrencesMax] = useState<number>(20);

    const [showDeleteModal, setShowDeleteModal] = useState<boolean>(false);

    const history = useHistory();

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

    const handleMoreOccurrences = () => {
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
                deleteLogsTimeSpanAmount,
                deleteLogsTimeSpanType,
                settings: settings!,
            };

            await Jobs.update(newJob);
            setIsSaving(false);
        } catch (err: any) {
            setError(err);
            setIsSaving(false);
        }
    };

    const handleDuplicate = async () => {
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

    const handleDeleteJob = (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setShowDeleteModal(true);
    };

    const handleDeleteOk = async () => {
        setShowDeleteModal(false);

        setIsSaving(true);
        setError(null);

        try {
            await Jobs.deleteJob(props.job!.jobId);
            setIsSaving(false);

            history.push('/jobs');
        } catch (err: any) {
            setError(err);
            setIsSaving(false);
        }
    };

    const handleDeleteCancel = () => {
        setShowDeleteModal(false);
    };

    return (
        <Loader isLoaded={props.job != null} error={null}>
            <form>
                <FormControl id="name" marginBottom={4} isRequired>
                    <FormLabel>Job Name</FormLabel>
                    <Input
                        type="text"
                        maxLength={100}
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                    />
                    <FormHelperText>The name of the job.</FormHelperText>
                </FormControl>
                <FormControl id="description" marginBottom={4}>
                    <FormLabel>Description</FormLabel>
                    <Textarea
                        lines={4}
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                    />
                    <FormHelperText>A description for this job.</FormHelperText>
                </FormControl>
                <FormControl id="group" marginBottom={4}>
                    <FormLabel>Job Group</FormLabel>
                    <Input
                        type="text"
                        maxLength={100}
                        value={group}
                        onChange={(e) => setGroup(e.target.value)}
                    />
                    <FormHelperText>
                        The group for this job. When multiple jobs in the same
                        group try to execute at the same time, only the job with
                        the highest priority will start.
                        <br />
                        Only a single job in the same group can be running at
                        the same time.
                    </FormHelperText>
                </FormControl>
                <FormControl id="priority" marginBottom={4}>
                    <FormLabel>Group Priority</FormLabel>
                    <Input
                        type="number"
                        min="0"
                        max="9999"
                        value={priority}
                        onChange={(e) => setPriority(+e.target.value)}
                    />
                    <FormHelperText>
                        The priority of this job within the group.
                    </FormHelperText>
                </FormControl>

                <FormControl id="timeSpan1" marginBottom={4}>
                    <FormLabel>Delete runs older than</FormLabel>
                    {!!deleteLogsTimeSpanType ? (
                        <Input
                            type="number"
                            min="0"
                            max="9999999"
                            step="1"
                            value={deleteLogsTimeSpanAmount || ''}
                            onChange={(evt) => {
                                if (
                                    evt.target.value == null ||
                                    evt.target.value === ''
                                ) {
                                    setDeleteLogsTimeSpanAmount(null);
                                } else {
                                    setDeleteLogsTimeSpanAmount(
                                        +evt.target.value
                                    );
                                }
                            }}
                            marginBottom="12px"
                        ></Input>
                    ) : null}
                    <RadioGroup
                        value={deleteLogsTimeSpanType || ''}
                        onChange={(value) => setDeleteLogsTimeSpanType(value)}
                    >
                        <Stack direction="column">
                            <Radio value="">Never</Radio>
                            <Radio value="Year">Years</Radio>
                            <Radio value="Month">Months</Radio>
                            <Radio value="Day">Days</Radio>
                            <Radio value="Hour">Hours</Radio>
                            <Radio value="Minute">Minutes</Radio>
                        </Stack>
                    </RadioGroup>
                </FormControl>

                <FormControl id="cron" marginBottom={4}>
                    <FormLabel>Backup Schedule</FormLabel>
                    <Input
                        type="text"
                        maxLength={100}
                        value={cron}
                        onChange={(e) => setCron(e.target.value)}
                        onBlur={() => handleChangeCrons()}
                    />
                    <FormHelperText>
                        The schedule based on cron expressions. Use{' '}
                        <a
                            href="https://crontab.guru/"
                            target="_blank"
                            rel="noreferrer"
                        >
                            https://crontab.guru/
                        </a>{' '}
                        as a cheat sheet.
                    </FormHelperText>
                    <FormHelperText>{testResult?.description}</FormHelperText>
                </FormControl>

                <FormLabel>Occurrences for next 2 weeks</FormLabel>
                <UnorderedList
                    overflowY="auto"
                    maxHeight="300px"
                    marginBottom={4}
                >
                    {cronOccurrences.map((m) => (
                        <ListItem key={m.toISOString()}>
                            {format(m, 'd MMMM yyyy HH:mm')}
                        </ListItem>
                    ))}
                </UnorderedList>

                <FormControl id="emailOnSuccess" marginBottom={4} isRequired>
                    <Checkbox
                        isChecked={settings?.emailOnSuccess}
                        onChange={(evt) =>
                            setSettings({
                                ...settings!,
                                emailOnSuccess: evt.target.checked,
                            })
                        }
                    >
                        Send e-mail when a run completes succesfully.
                    </Checkbox>
                </FormControl>
                <FormControl id="emailOnErrors" marginBottom={4} isRequired>
                    <Checkbox
                        isChecked={settings?.emailOnError}
                        onChange={(evt) =>
                            setSettings({
                                ...settings!,
                                emailOnError: evt.target.checked,
                            })
                        }
                    >
                        Send e-mail when a run completes with errors.
                    </Checkbox>
                </FormControl>
                <FormControl id="name" marginBottom={4} isRequired>
                    <FormLabel>Send e-mail to</FormLabel>
                    <Input
                        type="text"
                        maxLength={1000}
                        value={settings?.emailTo || ''}
                        onChange={(evt) => {
                            setSettings({
                                ...settings!,
                                emailTo: evt.target.value,
                            });
                        }}
                    />
                    <FormHelperText>
                        The recipient of the emails sent for this job, can be
                        comma separated for multiple recipients.
                    </FormHelperText>
                </FormControl>

                {error != null ? (
                    <Alert status="error" marginBottom={4}>
                        <AlertIcon />
                        <AlertDescription>{error}</AlertDescription>
                    </Alert>
                ) : null}
                <HStack>
                    <Button
                        onClick={(evt) => handleSave(evt)}
                        isLoading={isSaving}
                    >
                        Save
                    </Button>
                    {occurrencesMax < cronOccurrences.length ? (
                        <Button onClick={() => handleMoreOccurrences()}>
                            Show more occurrences
                        </Button>
                    ) : null}
                    <Button
                        onClick={() => handleDuplicate()}
                        isLoading={isSaving}
                        colorScheme="blue"
                    >
                        Duplicate
                    </Button>
                    <Button
                        onClick={handleDeleteJob}
                        isLoading={isSaving}
                        colorScheme="red"
                    >
                        Delete
                    </Button>
                </HStack>
            </form>
            <Modal
                isOpen={showDeleteModal}
                onClose={handleDeleteCancel}
                size="lg"
            >
                <ModalOverlay />
                <ModalContent>
                    <ModalHeader>Delete job</ModalHeader>
                    <ModalCloseButton />
                    <ModalBody>
                        <p>
                            When deleting this job, all tasks, runs and logs
                            will be deleted associated with this job.
                        </p>
                        <p>Are you sure you want to delete this job?</p>
                    </ModalBody>

                    <ModalFooter>
                        <HStack>
                            <Button
                                onClick={() => handleDeleteOk()}
                                colorScheme="red"
                            >
                                Delete
                            </Button>
                            <Button onClick={() => handleDeleteCancel()}>
                                Cancel
                            </Button>
                        </HStack>
                    </ModalFooter>
                </ModalContent>
            </Modal>
        </Loader>
    );
};

export default JobSettingsTab;
