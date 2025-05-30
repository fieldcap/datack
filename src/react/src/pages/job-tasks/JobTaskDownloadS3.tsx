import { FormControl, FormHelperText, FormLabel, Input, Select } from '@chakra-ui/react';
import React, { FC, useEffect } from 'react';
import { JobTaskDownloadS3Settings } from '../../models/job-task';

type Props = {
  agentId: string;
  settings: JobTaskDownloadS3Settings | undefined | null;
  onSettingsChanged: (settings: JobTaskDownloadS3Settings) => void;
};

const JobTaskDownloadS3: FC<Props> = (props) => {
  const { onSettingsChanged } = props;

  useEffect(() => {
    if (props.settings == null) {
      onSettingsChanged({
        fileName: '',
        region: '',
        bucket: '',
        accessKey: '',
        secret: '',
      });
    }
  }, [props.settings, onSettingsChanged]);

  const set = (settingName: keyof JobTaskDownloadS3Settings, newValue: string | number | boolean): void => {
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
      <FormControl id="region" marginBottom={4}>
        <FormLabel>Region</FormLabel>
        <Select value={props.settings?.region || ''} onChange={(e) => set('region', e.target.value)}>
          <option value="af-south-1">Africa (Cape Town)</option>
          <option value="ap-east-1">Asia Pacific (Hong Kong)</option>
          <option value="ap-northeast-1">Asia Pacific (Tokyo)</option>
          <option value="ap-northeast-2">Asia Pacific (Seoul)</option>
          <option value="ap-northeast-3">Asia Pacific (Osaka)</option>
          <option value="ap-south-1">Asia Pacific (Mumbai)</option>
          <option value="ap-southeast-1">Asia Pacific (Singapore)</option>
          <option value="ap-southeast-2">Asia Pacific (Sydney)</option>
          <option value="ca-central-1">Canada (Central)</option>
          <option value="eu-central-1">Europe (Frankfurt)</option>
          <option value="eu-north-1">Europe (Stockholm)</option>
          <option value="eu-south-1">Europe (Milan)</option>
          <option value="eu-west-1">Europe (Ireland)</option>
          <option value="eu-west-2">Europe (London)</option>
          <option value="eu-west-3">Europe (Paris)</option>
          <option value="me-south-1">Middle East (Bahrain)</option>
          <option value="sa-east-1">South America (Sao Paulo)</option>
          <option value="us-east-1">US East (N. Virginia)</option>
          <option value="us-east-2">US East (Ohio)</option>
          <option value="us-west-1">US West (N. California)</option>
          <option value="us-west-2">US West (Oregon)</option>
          <option value="cn-north-1">China (Beijing)</option>
          <option value="cn-northwest-1">China (Ningxia)</option>
          <option value="us-gov-east-1">AWS GovCloud (US-East)</option>
          <option value="us-gov-west-1">AWS GovCloud (US-West)</option>
          <option value="us-iso-east-1">US ISO East</option>
          <option value="us-iso-west-1">US ISO WEST</option>
          <option value="us-isob-east-1">US ISOB East (Ohio)</option>
        </Select>
        <FormHelperText>The AWS S3 bucket region.</FormHelperText>
      </FormControl>
      <FormControl id="bucket" marginBottom={4}>
        <FormLabel>Bucket</FormLabel>
        <Input
          type="text"
          value={props.settings?.bucket || ''}
          onChange={(evt) => set('bucket', evt.target.value)}
        ></Input>
        <FormHelperText>The AWS S3 bucket name.</FormHelperText>
      </FormControl>
      <FormControl id="fileName" marginBottom={4}>
        <FormLabel>Destination Filename</FormLabel>
        <Input
          type="text"
          value={props.settings?.fileName || ''}
          onChange={(evt) => set('fileName', evt.target.value)}
        ></Input>
        <FormHelperText>
          The filename to download the S3 file to. The following tokens are supported:
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
      <FormControl id="accessKey" marginBottom={4}>
        <FormLabel>Access Key</FormLabel>
        <Input
          type="text"
          value={props.settings?.accessKey || ''}
          onChange={(evt) => set('accessKey', evt.target.value)}
        ></Input>
        <FormHelperText>The AWS S3 access key.</FormHelperText>
      </FormControl>
      <FormControl id="secret" marginBottom={4}>
        <FormLabel>Secret</FormLabel>
        <Input
          type="password"
          value={props.settings?.secret || ''}
          onChange={(evt) => set('secret', evt.target.value)}
        ></Input>
        <FormHelperText>The AWS S3 access key secret. This setting is stored encrypted.</FormHelperText>
      </FormControl>
    </>
  );
};

export default JobTaskDownloadS3;
