import { FormControl, FormHelperText, FormLabel, Input, Radio, RadioGroup, Select, Stack } from '@chakra-ui/react';
import React, { FC, useEffect } from 'react';
import { JobTaskDeleteS3Settings } from '../../models/job-task';

type Props = {
  agentId: string;
  settings: JobTaskDeleteS3Settings | undefined | null;
  onSettingsChanged: (settings: JobTaskDeleteS3Settings) => void;
};

const JobTaskUploadS3: FC<Props> = (props) => {
  const { onSettingsChanged } = props;

  useEffect(() => {
    if (props.settings == null) {
      onSettingsChanged({
        fileName: '',
        region: '',
        bucket: '',
        accessKey: '',
        secret: '',
        tag: '',
        timeSpanType: 'Month',
        timeSpanAmount: 1,
      });
    }
  }, [props.settings, onSettingsChanged]);

  const set = (settingName: keyof JobTaskDeleteS3Settings, newValue: string | number | boolean): void => {
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
        <FormLabel>Root Key</FormLabel>
        <Input
          type="text"
          value={props.settings?.fileName || ''}
          onChange={(evt) => set('fileName', evt.target.value)}
        ></Input>
        <FormHelperText>
          The S3 root path to delete objects from. The following tokens are supported:
          <br />
          &#123;ItemName&#125; The item name of the job task
          <br />
          &#123;0:yyyyMMddHHmm&#125; The date and time of the start date of the job task
          <br />
          Example:
          <br />
          /&#123;ItemName&#125;/
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
      <FormControl id="tag" marginBottom={4}>
        <FormLabel>Tag</FormLabel>
        <Input type="text" value={props.settings?.tag || ''} onChange={(evt) => set('tag', evt.target.value)}></Input>
        <FormHelperText>
          The tag used to identify objects for deletion. When set, only objects found with this tag value set will be
          eligable for deletion.
        </FormHelperText>
      </FormControl>
      <FormControl id="timeSpan1" marginBottom={4}>
        <FormLabel>Delete objects older than</FormLabel>
        <Input
          type="number"
          min="0"
          max="9999999"
          step="1"
          value={props.settings?.timeSpanAmount || ''}
          onChange={(evt) => set('timeSpanAmount', +evt.target.value)}
          marginBottom={2}
        ></Input>
        <RadioGroup value={props.settings?.timeSpanType || ''} onChange={(value) => set('timeSpanType', value)}>
          <Stack direction="column">
            <Radio value="Year">Years</Radio>
            <Radio value="Month">Months</Radio>
            <Radio value="Day">Days</Radio>
            <Radio value="Hour">Hours</Radio>
            <Radio value="Minute">Minutes</Radio>
          </Stack>
        </RadioGroup>
        <FormHelperText>The tag "Datack:JobDate" is used to determine the backup date.</FormHelperText>
      </FormControl>
    </>
  );
};

export default JobTaskUploadS3;
