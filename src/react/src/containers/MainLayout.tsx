import { ChevronRightIcon } from '@chakra-ui/icons';
import { Box, Flex, IconButton, useBreakpointValue } from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import AgentAdd from '../pages/agents/AgentAdd';
import AgentList from '../pages/agents/AgentList';
import AgentOverview from '../pages/agents/AgentOverview';
import History from '../pages/history/History';
import JobRunOverview from '../pages/job-runs/JobRunOverview';
import JobTaskAdd from '../pages/job-tasks/JobTaskAdd';
import JobTaskEditor from '../pages/job-tasks/JobTaskEditor';
import JobAdd from '../pages/jobs/JobAdd';
import JobList from '../pages/jobs/JobList';
import JobOverview from '../pages/jobs/JobOverview';
import SettingsOverview from '../pages/settings/SettingsOverview';
import Auth from '../services/auth';
import NavLayout from './NavLayout';

const smVariant = { navigation: 'drawer', navigationButton: true };
const mdVariant = { navigation: 'sidebar', navigationButton: false };

const MainLayout: FC = () => {
    const [isSidebarOpen, setSidebarOpen] = useState(false);
    const variants = useBreakpointValue({ base: smVariant, md: mdVariant });
    const toggleSidebar = () => setSidebarOpen(!isSidebarOpen);

    if (!Auth.hasAuthToken()) {
        return (
            <Navigate
                to={{
                    pathname: '/login',
                }}
            />
        );
    }

    return (
        <Flex height="100%" alignItems="stretch">
            {variants?.navigationButton ? (
                <IconButton
                    icon={<ChevronRightIcon w={8} h={8} />}
                    colorScheme="blackAlpha"
                    variant="outline"
                    onClick={() => toggleSidebar()}
                    position="fixed"
                    top="12px"
                    left="12px"
                    aria-label=""
                    zIndex="100"
                    background="white"
                    _hover={{
                        background: 'white',
                    }}
                />
            ) : null}
            <NavLayout variant={variants?.navigation} isOpen={isSidebarOpen} onClose={toggleSidebar}></NavLayout>

            <Box flex="1" padding="24px">
                <Routes>
                    <Route path="/settings" element={<SettingsOverview />} />
                    <Route path="/agents" element={<AgentList />} />
                    <Route path="/agent/new" element={<AgentAdd />} />
                    <Route path="/agent/:id" element={<AgentOverview />} />
                    <Route path="/jobs" element={<JobList />} />
                    <Route path="/job/new" element={<JobAdd />} />
                    <Route path="/job/:jobId/task/add" element={<JobTaskAdd />} />
                    <Route path="/job/:jobId/task/:id" element={<JobTaskEditor />} />
                    <Route path="/job/:id" element={<JobOverview />} />
                    <Route path="/run/:id" element={<JobRunOverview />} />
                    <Route path="/history" element={<History />} />
                </Routes>
            </Box>
        </Flex>
    );
};

export default MainLayout;
