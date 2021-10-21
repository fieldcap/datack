import { FormControl, FormLabel, Input, Select } from '@chakra-ui/react';
import React, { FC, useEffect } from 'react';
import { JobTaskUploadS3Settings } from '../../models/job-task';

type Props = {
    agentId: string;
    settings: JobTaskUploadS3Settings | undefined | null;
    onSettingsChanged: (settings: JobTaskUploadS3Settings) => void;
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

    const handleRegionChanged = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            region: value,
        });
    };

    const handleBucketChanged = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            bucket: value,
        });
    };

    const handleAccessKeyChanged = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            accessKey: value,
        });
    };

    const handleSecretChanged = (value: string) => {
        if (props.settings == null) {
            return;
        }
        props.onSettingsChanged({
            ...props.settings,
            secret: value,
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
            <FormControl id="region" marginBottom={4}>
                <FormLabel>Region</FormLabel>
                <Select
                    value={props.settings?.region || ''}
                    onChange={(e) => handleRegionChanged(e.target.value)}
                >
                    <option value="af-south-1">Africa (Cape Town)</option>
                    <option value="ap-east-1">Asia Pacific (Hong Kong)</option>
                    <option value="ap-northeast-1">Asia Pacific (Tokyo)</option>
                    <option value="ap-northeast-2">Asia Pacific (Seoul)</option>
                    <option value="ap-northeast-3">Asia Pacific (Osaka)</option>
                    <option value="ap-south-1">Asia Pacific (Mumbai)</option>
                    <option value="ap-southeast-1">
                        Asia Pacific (Singapore)
                    </option>
                    <option value="ap-southeast-2">
                        Asia Pacific (Sydney)
                    </option>
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
                    <option value="us-gov-east-1">
                        AWS GovCloud (US-East)
                    </option>
                    <option value="us-gov-west-1">
                        AWS GovCloud (US-West)
                    </option>
                    <option value="us-iso-east-1">US ISO East</option>
                    <option value="us-iso-west-1">US ISO WEST</option>
                    <option value="us-isob-east-1">US ISOB East (Ohio)</option>
                </Select>
            </FormControl>
            <FormControl id="bucket" marginBottom={4}>
                <FormLabel>Bucket</FormLabel>
                <Input
                    type="text"
                    value={props.settings?.bucket || ''}
                    onChange={(evt) => handleBucketChanged(evt.target.value)}
                ></Input>
            </FormControl>
            <FormControl id="fileName" marginBottom={4}>
                <FormLabel>Key</FormLabel>
                <Input
                    type="text"
                    value={props.settings?.fileName || ''}
                    onChange={(evt) => handleFilenameChanged(evt.target.value)}
                ></Input>
            </FormControl>
            <FormControl id="accessKey" marginBottom={4}>
                <FormLabel>Access Key</FormLabel>
                <Input
                    type="text"
                    value={props.settings?.accessKey || ''}
                    onChange={(evt) => handleAccessKeyChanged(evt.target.value)}
                ></Input>
            </FormControl>
            <FormControl id="secret" marginBottom={4}>
                <FormLabel>Secret</FormLabel>
                <Input
                    type="password"
                    value={props.settings?.secret || ''}
                    onChange={(evt) => handleSecretChanged(evt.target.value)}
                ></Input>
            </FormControl>
            <FormControl id="tag" marginBottom={4}>
                <FormLabel>Tag</FormLabel>
                <Input
                    type="text"
                    value={props.settings?.tag || ''}
                    onChange={(evt) => handleTagChanged(evt.target.value)}
                ></Input>
            </FormControl>
        </>
    );
};

export default JobTaskUploadS3;
