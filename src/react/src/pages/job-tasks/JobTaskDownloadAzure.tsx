import { FormControl, FormHelperText, FormLabel, Input, Select } from '@chakra-ui/react';
import React, { FC, useEffect } from 'react';
import { JobTaskDownloadAzureSettings } from '../../models/job-task';

type Props = {
  agentId: string;
  settings: JobTaskDownloadAzureSettings | undefined | null;
  onSettingsChanged: (settings: JobTaskDownloadAzureSettings) => void;
};

const JobTaskDownloadAzure: FC<Props> = (props) => {
  const { onSettingsChanged } = props;

  useEffect(() => {
    if (props.settings == null) {
      onSettingsChanged({
        blob: '',
        fileName: '',
        containerName: '',
        connectionString: '',
      });
    }
  }, [props.settings, onSettingsChanged]);

  const set = (settingName: keyof JobTaskDownloadAzureSettings, newValue: string | number | boolean): void => {
    if (props.settings == null) {
      return;
    }
    props.onSettingsChanged({
      ...props.settings,
      [settingName]: newValue,
    });
  };

  return (
    <>
      <FormControl id="containerName" marginBottom={4}>
        <FormLabel>Container</FormLabel>
        <Input
          type="text"
          value={props.settings?.containerName || ''}
          onChange={(evt) => set('containerName', evt.target.value)}
        ></Input>
        <FormHelperText>The Azure Blob storage container.</FormHelperText>
      </FormControl>
      <FormControl id="blob" marginBottom={4}>
        <FormLabel>Blob</FormLabel>
        <Input type="text" value={props.settings?.blob || ''} onChange={(evt) => set('blob', evt.target.value)}></Input>
        <FormHelperText>
          The Azure blob root to download a file from. Is used to fetch the list of files and not in the actual job.
        </FormHelperText>
      </FormControl>
      <FormControl id="fileName" marginBottom={4}>
        <FormLabel>Destination Filename</FormLabel>
        <Input
          type="text"
          value={props.settings?.fileName || ''}
          onChange={(evt) => set('fileName', evt.target.value)}
        ></Input>
        <FormHelperText>
          The filename to download the Azure file to. The following tokens are supported:
          <br />
          &#123;ItemName&#125; The item name of the job task
          <br />
          &#123;FileName&#125; The filename of the file to download without the path
          <br />
          &#123;Started:yyyyMMddHHmm&#125; The date and time of the start date of the job task
          <br />
          Example:
          <br />
          /&#123;ItemName&#125;/&#123;ItemName&#125;-&#123;Started:yyyyMMddHHmm&#125;-Full.7z
        </FormHelperText>
      </FormControl>
      <FormControl id="connectionString" marginBottom={4}>
        <FormLabel>Connection String</FormLabel>
        <Input
          type="password"
          value={props.settings?.connectionString || ''}
          onChange={(evt) => set('connectionString', evt.target.value)}
        ></Input>
        <FormHelperText>The Azure connection string. This setting is stored encrypted.</FormHelperText>
      </FormControl>
    </>
  );
};

export default JobTaskDownloadAzure;
