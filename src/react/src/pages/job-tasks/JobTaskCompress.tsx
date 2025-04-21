import { FormControl, FormHelperText, FormLabel, Input, Select } from '@chakra-ui/react';
import React, { FC, useEffect } from 'react';
import { JobTaskCompressSettings } from '../../models/job-task';

type Props = {
  agentId: string;
  settings: JobTaskCompressSettings | undefined | null;
  onSettingsChanged: (settings: JobTaskCompressSettings) => void;
};

const JobTaskCompress: FC<Props> = (props) => {
  const { onSettingsChanged } = props;

  useEffect(() => {
    if (props.settings == null) {
      onSettingsChanged({
        fileName: '',
        archiveType: '7z',
        compressionLevel: '5',
        multithreadMode: 'on',
        password: null,
      });
    }
  }, [props.settings, onSettingsChanged]);

  const set = (settingName: keyof JobTaskCompressSettings, newValue: string | number | boolean): void => {
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
      <FormControl id="fileName" marginBottom={4}>
        <FormLabel>File name</FormLabel>
        <Input
          type="text"
          value={props.settings?.fileName || ''}
          onChange={(evt) => set('fileName', evt.target.value)}
        ></Input>
        <FormHelperText>
          The full file path to write the compressed file to. The following tokens are supported:
          <br />
          &#123;ItemName&#125; The item name of the job task
          <br />
          &#123;FileName&#125; The filename of the file to download without the path
          <br />
          &#123;Started:yyyyMMddHHmm&#125; The date and time of the start date of the job task
          <br />
          Example:
          <br />
          C:\Temp\Backups\&#123;ItemName&#125;\&#123;ItemName&#125;-&#123;Started:yyyyMMddHHmm&#125;-Full.7z
        </FormHelperText>
      </FormControl>
      <FormControl id="archiveType" marginBottom={4}>
        <FormLabel>Archive Type</FormLabel>
        <Select value={props.settings?.archiveType || '7z'} onChange={(e) => set('archiveType', e.target.value)}>
          <option value="7z">7zip</option>
          <option value="xz">xz</option>
          <option value="split">split</option>
          <option value="zip">zip</option>
          <option value="gzip">gzip</option>
          <option value="bzip2">bzip2</option>
          <option value="tar">tar</option>
        </Select>
        <FormHelperText>The archive type used by 7zip.</FormHelperText>
      </FormControl>
      <FormControl id="compressionLevel" marginBottom={4}>
        <FormLabel>Compression Level</FormLabel>
        <Select
          value={props.settings?.compressionLevel || '5'}
          onChange={(e) => set('compressionLevel', e.target.value)}
        >
          <option value="0">Copy mode (no compression)</option>
          <option value="1">Fastest</option>
          <option value="3">Fast</option>
          <option value="5">Normal</option>
          <option value="7">Maximum</option>
          <option value="9">Ultra</option>
        </Select>
        <FormHelperText>
          The the higher the compression level the longer it takes. Higher than 5 (normal) is not recommended.
        </FormHelperText>
      </FormControl>
      <FormControl id="multithreadMode" marginBottom={4}>
        <FormLabel>Multithread Mode</FormLabel>
        <Select
          value={props.settings?.multithreadMode || 'on'}
          onChange={(e) => set('multithreadMode', e.target.value)}
        >
          <option value="on">On</option>
          <option value="off">Off</option>
          {Array.from(Array(63).keys()).map((v: any) => (
            <option value={v + 1} key={v + 1}>
              {v + 1}
            </option>
          ))}
        </Select>
        <FormHelperText>
          On: use all cores on the agent
          <br />
          Off: use a single core on the agent
          <br />
          Or specify the amount of cores allowed to be used.
        </FormHelperText>
      </FormControl>
      <FormControl id="password" marginBottom={4}>
        <FormLabel>Password</FormLabel>
        <Input
          type="password"
          value={props.settings?.password || ''}
          onChange={(evt) => set('password', evt.target.value)}
        ></Input>
        <FormHelperText>
          Password to protect the archive. On 7z AES-256 is used. This setting is stored encrypted.
        </FormHelperText>
      </FormControl>
    </>
  );
};

export default JobTaskCompress;
