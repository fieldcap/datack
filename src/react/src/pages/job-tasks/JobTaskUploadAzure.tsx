import { FormControl, FormHelperText, FormLabel, Input } from '@chakra-ui/react';
import React, { FC, useEffect } from 'react';
import { JobTaskUploadAzureSettings } from '../../models/job-task';

type Props = {
    agentId: string;
    settings: JobTaskUploadAzureSettings | undefined | null;
    onSettingsChanged: (settings: JobTaskUploadAzureSettings) => void;
};

const JobTaskUploadAzure: FC<Props> = (props) => {
    const { onSettingsChanged } = props;

    useEffect(() => {
        if (props.settings == null) {
            onSettingsChanged({
                fileName: '',
                containerName: '',
                connectionString: '',
                tag: '',
            });
        }
    }, [props.settings, onSettingsChanged]);

    const handleFilenameChanged = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            fileName: value,
        });
    };

    const handleContainerNameChanged = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            containerName: value,
        });
    };

    const handleConnectionStringChanged = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            connectionString: value,
        });
    };

    const handleTagChanged = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            tag: value,
        });
    };

    return (
        <>
            <FormControl id="containerName" marginBottom={4}>
                <FormLabel>Container</FormLabel>
                <Input
                    type="text"
                    value={props.settings?.containerName || ''}
                    onChange={(evt) => handleContainerNameChanged(evt.target.value)}
                ></Input>
                <FormHelperText>The Azure Blob storage container.</FormHelperText>
            </FormControl>
            <FormControl id="fileName" marginBottom={4}>
                <FormLabel>Blob</FormLabel>
                <Input
                    type="text"
                    value={props.settings?.fileName || ''}
                    onChange={(evt) => handleFilenameChanged(evt.target.value)}
                ></Input>
                <FormHelperText>
                    The Azure blob file name to upload the file to. The following tokens are supported:
                    <br />
                    &#123;ItemName&#125; The item name of the job task
                    <br />
                    &#123;0:yyyyMMddHHmm&#125; The date and time of the start date of the job task
                    <br />
                    Example:
                    <br />
                    /&#123;ItemName&#125;/&#123;ItemName&#125;-&#123;0:yyyyMMddHHmm&#125;-Full.7z
                </FormHelperText>
            </FormControl>
            <FormControl id="connectionString" marginBottom={4}>
                <FormLabel>Connection String</FormLabel>
                <Input
                    type="password"
                    value={props.settings?.connectionString || ''}
                    onChange={(evt) => handleConnectionStringChanged(evt.target.value)}
                ></Input>
                <FormHelperText>The Azure connection string. This setting is stored encrypted.</FormHelperText>
            </FormControl>
            <FormControl id="tag" marginBottom={4}>
                <FormLabel>Tag</FormLabel>
                <Input
                    type="text"
                    value={props.settings?.tag || ''}
                    onChange={(evt) => handleTagChanged(evt.target.value)}
                ></Input>
                <FormHelperText>An optional tag to write on the object.</FormHelperText>
            </FormControl>
        </>
    );
};

export default JobTaskUploadAzure;
