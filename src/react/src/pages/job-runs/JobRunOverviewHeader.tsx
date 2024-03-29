import { Table, Tbody, Td, Tr } from '@chakra-ui/table';
import { format, parseISO } from 'date-fns';
import { FC } from 'react';
import { JobRun } from '../../models/job-run';
import { formatRuntime } from '../../services/date';

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
          <Td style={{ fontWeight: 'bold' }}>Started</Td>
          <Td>{format(parseISO(jobRun.started), 'd MMMM yyyy HH:mm')}</Td>
        </Tr>
        <Tr>
          <Td style={{ fontWeight: 'bold' }}>Completed</Td>
          <Td>{jobRun.completed == null ? '' : format(parseISO(jobRun.completed), 'd MMMM yyyy HH:mm')}</Td>
        </Tr>
        <Tr>
          <Td style={{ fontWeight: 'bold' }}>Run time</Td>
          <Td>{formatRuntime(jobRun)}</Td>
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
