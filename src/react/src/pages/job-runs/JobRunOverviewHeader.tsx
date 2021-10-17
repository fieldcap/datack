import { Table, Tbody, Td, Tr } from '@chakra-ui/table';
import { format, formatDistanceStrict } from 'date-fns';
import { FC } from 'react';
import { JobRun } from '../../models/job-run';

type Props = {
    jobRun: JobRun;
};

const JobRunOverviewHeader: FC<Props> = (props) => {
    const { jobRun } = props;
    return (
        <Table variant="simple" size="sm" style={{ width: 'auto' }}>
            <Tbody>
                <Tr>
                    <Td style={{ fontWeight: 'bold' }}>Job</Td>
                    <Td>{jobRun.job.name}</Td>
                </Tr>
                <Tr>
                    <Td style={{ fontWeight: 'bold' }}>Backup Type</Td>
                    <Td>{jobRun.backupType}</Td>
                </Tr>
                <Tr>
                    <Td style={{ fontWeight: 'bold' }}>Started</Td>
                    <Td>{format(jobRun.started, 'd MMMM yyyy HH:mm')}</Td>
                </Tr>
                <Tr>
                    <Td style={{ fontWeight: 'bold' }}>Completed</Td>
                    <Td>
                        {jobRun.completed == null
                            ? ''
                            : format(jobRun.completed, 'd MMMM yyyy HH:mm')}
                    </Td>
                </Tr>
                <Tr>
                    <Td style={{ fontWeight: 'bold' }}>Run time</Td>
                    <Td>
                        {jobRun.runTime == null
                            ? ''
                            : formatDistanceStrict(0, jobRun.runTime * 1000)}
                    </Td>
                </Tr>
                <Tr>
                    <Td style={{ fontWeight: 'bold' }}>Result</Td>
                    <Td>{jobRun.result}</Td>
                </Tr>
            </Tbody>
        </Table>
    );
};

export default JobRunOverviewHeader;
