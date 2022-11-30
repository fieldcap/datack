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
  Textarea,
} from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { v4 } from 'uuid';
import { Job } from '../../models/job';
import Jobs from '../../services/jobs';

type RouteParams = {};

const JobAdd: FC = () => {
  const [name, setName] = useState<string>('');
  const [description, setDescription] = useState<string>('');
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const history = useNavigate();

  const handleSave = async (event: React.FormEvent<HTMLButtonElement>) => {
    event.preventDefault();
    setIsSaving(false);
    setError(null);

    const job: Job = {
      jobId: v4(),
      name,
      isActive: true,
      cron: '',
      group: '',
      priority: 1,
      description,
      deleteLogsTimeSpanAmount: null,
      deleteLogsTimeSpanType: null,
      settings: {
        emailOnError: false,
        emailOnSuccess: false,
        emailTo: null,
      },
    };

    try {
      setIsSaving(true);
      const newJobId = await Jobs.add(job);
      history(`/job/${newJobId}`);
    } catch (err: any) {
      setIsSaving(false);
      setError(err);
    }
  };

  const handleCancel = (event: React.FormEvent<HTMLButtonElement>) => {
    event.preventDefault();
    history(`/job`);
  };

  return (
    <>
      <Heading marginBottom={4}>Add new job</Heading>
      <form>
        <FormControl id="name" marginBottom={4} isRequired>
          <FormLabel>Job Name</FormLabel>
          <Input type="text" maxLength={100} value={name} onChange={(e) => setName(e.target.value)} />
          <FormHelperText>The name of the job.</FormHelperText>
        </FormControl>
        <FormControl id="description" marginBottom={4}>
          <FormLabel>Description</FormLabel>
          <Textarea lines={4} value={description} onChange={(e) => setDescription(e.target.value)} />
          <FormHelperText>A description for this job.</FormHelperText>
        </FormControl>
        {error != null ? (
          <Alert marginBottom={4} status="error">
            <AlertIcon />
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        ) : null}
        <HStack>
          <Button onClick={(evt) => handleSave(evt)} isLoading={isSaving}>
            Add new job
          </Button>
          <Button onClick={handleCancel} isLoading={isSaving} variant="outline">
            Cancel
          </Button>
        </HStack>
      </form>
    </>
  );
};

export default JobAdd;
