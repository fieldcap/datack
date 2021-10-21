import {
    Alert,
    AlertDescription,
    AlertIcon,
    Button,
    FormControl,
    FormHelperText,
    FormLabel,
    Heading,
    HStack,
    Input,
    Textarea
} from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { RouteComponentProps, useHistory } from 'react-router-dom';
import { v4 } from 'uuid';
import { Job } from '../../models/job';
import Jobs from '../../services/jobs';

type RouteParams = {};

const JobAdd: FC<RouteComponentProps<RouteParams>> = () => {
    const [name, setName] = useState<string>('');
    const [description, setDescription] = useState<string>('');
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const history = useHistory();

    const handleSave = async (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();
        setIsSaving(false);
        setError(null);

        const job: Job = {
            jobId: v4(),
            name,
            cron: '',
            group: '',
            priority: 1,
            description,
            settings: {},
        };

        try {
            setIsSaving(true);
            const newJobId = await Jobs.add(job);
            history.push(`/job/${newJobId}`);
        } catch (err: any) {
            setIsSaving(false);
            setError(err);
        }
    };

    const handleCancel = (event: React.FormEvent<HTMLButtonElement>) => {
        event.preventDefault();
        history.push(`/job`);
    };

    return (
        <>
            <Heading marginBottom="24px">Add new job</Heading>
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
                        Add new job
                    </Button>
                    <Button
                        onClick={handleCancel}
                        isLoading={isSaving}
                        variant="outline"
                    >
                        Cancel
                    </Button>
                </HStack>
            </form>
        </>
    );
};

export default JobAdd;
